using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EmailSpooler.Win32Service.Entity;
using EmailSpooler.Win32Service.SMTP;
using PeanutButter.ServiceShell;
using Email = EmailSpooler.Win32Service.Entity.Email;

namespace EmailSpooler.Win32Service
{
    public interface IEmailSpoolerDependencies
    {
        IEmailContext DbContext { get; }
        Func<IEmail> EmailGenerator { get; }
        IEmailSpoolerConfig EmailSpoolerConfig { get; }
        IEmailConfiguration EmailConfig { get; }
    }

    public interface IEmailSpoolerConfig
    {
        ISimpleLogger Logger { get; }
        int MaxSendAttempts { get; }
        int BackoffIntervalInMinutes { get; }
        int BackoffMultiplier { get; }
        int PurgeMessageWithAgeInDays { get; }
    }

    public class EmailSpooler: IDisposable
    {
        private IEmailContext _context;
        private readonly Func<IEmail> _emailGenerator;
        private readonly IEmailSpoolerConfig _config;

        public EmailSpooler(IEmailSpoolerDependencies dependencies)
        {
            CheckDependencies(dependencies);
            _context = dependencies.DbContext;
            _emailGenerator = dependencies.EmailGenerator;
            _config = dependencies.EmailSpoolerConfig;
        }

        private static void CheckDependencies(IEmailSpoolerDependencies dependencies)
        {
            var missing = new List<string>();
            if (dependencies.DbContext == null) missing.Add("dbContext");
            if (dependencies.EmailGenerator == null) missing.Add("emailGenerator");
            if (dependencies.EmailSpoolerConfig == null) missing.Add("emailSpoolerConfig");
            if (missing.Any())
                throw new ArgumentNullException(string.Join(",", missing));
        }

        public void Dispose()
        {
            lock (this)
            {
                _context?.Dispose();
                _context = null;
            }
        }

        public void Spool()
        {
            AttemptToSendUnsentMail();
            PurgeOldSentAndUnsendableMail();
        }

        private void PurgeOldSentAndUnsendableMail()
        {
            var cutoff = DateTime.Now.AddDays(-_config.PurgeMessageWithAgeInDays);
            var toRemove = _context.Emails.Where(e => (e.Sent || e.SendAttempts >= _config.MaxSendAttempts)
                                                            && (e.LastModified.HasValue && 
                                                                e.LastModified.Value <= cutoff))
                                                                .ToList();
            if (!toRemove.Any()) return;
            _config.Logger.LogInfo(string.Join("", "Purging ", toRemove.Count().ToString(), " stale) messages"));
            foreach (var email in toRemove)
                _context.Emails.Remove(email);
            _context.SaveChanges();
        }

        private void AttemptToSendUnsentMail()
        {
            var unsent = _context.Emails
                .Include(e => e.EmailRecipients)
                .Include(e => e.EmailAttachments)
                .Where(e => e.Enabled
                            && !e.Sent
                            && e.SendAt <= DateTime.Now
                            && e.SendAttempts < _config.MaxSendAttempts)
                .ToList();
            foreach (var message in unsent)
            {
                SendMessageFor(message);
            }
        }

        private void SendMessageFor(Email message)
        {
            using (var email = _emailGenerator())
            {
                SetupEmailFromMessage(message, email);
                _config.Logger.LogInfo(string.Join("", "Attempting to send mail to '", email.To.FirstOrDefault()));
                try
                {
                    email.Send();
                    message.Sent = true;
                    _config.Logger.LogInfo(" => message sent");
                }
                catch (Exception ex)
                {
                    message.LastError = ex.Message;
                    message.SendAttempts++;
                    var backoffBy = _config.BackoffIntervalInMinutes * message.SendAttempts;
                    if (message.SendAttempts > 1)
                        backoffBy *= _config.BackoffMultiplier;
                    message.SendAt = DateTime.Now.AddMinutes(backoffBy);
                    message.Sent = false;
                    _config.Logger.LogInfo("** Message not sent: " + ex.Message);
                }
                _context.SaveChanges();
            }
        }

        private static void SetupEmailFromMessage(Email message, IEmail email)
        {
            AddAllRecipients(message, email);
            email.From = message.Sender;
            email.Subject = message.Subject;
            email.Body = message.Body;
            AddAttachments(message, email);
        }

        private static void AddAttachments(Email message, IEmail email)
        {
            foreach (var attachment in message.EmailAttachments.Where(a => a.Enabled))
            {
                if (string.IsNullOrEmpty(attachment.ContentID))
                    email.AddAttachment(attachment.Name, attachment.Data, attachment.MIMEType);
                else
                    email.AddAttachment(attachment.Name, attachment.Data, attachment.MIMEType, attachment.ContentID);
            }
        }

        private static void AddAllRecipients(Email message, IEmail email)
        {
            foreach (var recipient in message.EmailRecipients.Where(r => r.Enabled))
            {
                if (recipient.IsPrimaryRecipient)
                    email.AddRecipient(recipient.Recipient);
                if (recipient.IsCC)
                    email.AddCC(recipient.Recipient);
                if (recipient.IsBCC)
                    email.AddBCC(recipient.Recipient);
            }
        }
    }
}
