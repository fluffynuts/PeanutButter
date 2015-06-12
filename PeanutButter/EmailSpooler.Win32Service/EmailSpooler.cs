using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailSpooler.Win32Service.Models;
using PeanutButter.ServiceShell;
using ServiceShell;

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
        private IEmailContext _dbContext;
        private Func<IEmail> _emailGenerator;
        private IEmailSpoolerConfig _config;

        public EmailSpooler(IEmailSpoolerDependencies dependencies)
        {
            CheckDependencies(dependencies);
            this._dbContext = dependencies.DbContext;
            this._emailGenerator = dependencies.EmailGenerator;
            this._config = dependencies.EmailSpoolerConfig;
        }

        private static void CheckDependencies(IEmailSpoolerDependencies dependencies)
        {
            var missing = new List<string>();
            if (dependencies.DbContext == null) missing.Add("dbContext");
            if (dependencies.EmailGenerator == null) missing.Add("emailGenerator");
            if (dependencies.EmailSpoolerConfig == null) missing.Add("emailSpoolerConfig");
            if (missing.Any())
                throw new ArgumentNullException(String.Join(",", missing));
        }

        public void Dispose()
        {
            lock (this)
            {
                if (this._dbContext != null)
                    this._dbContext.Dispose();
                this._dbContext = null;
            }
        }

        public void Spool()
        {
            AttemptToSendUnsentMail();
            PurgeOldSentAndUnsendableMail();
        }

        private void PurgeOldSentAndUnsendableMail()
        {
            var cutoff = DateTime.Now.AddDays(-this._config.PurgeMessageWithAgeInDays);
            var toRemove = this._dbContext.Emails.Where(e => (e.Sent || e.SendAttempts >= this._config.MaxSendAttempts)
                                                            && (e.LastModified.HasValue && 
                                                                e.LastModified.Value <= cutoff))
                                                                .ToList();
            if (!toRemove.Any()) return;
            this._config.Logger.LogInfo(String.Join("", new[] {
                "Purging ", toRemove.Count().ToString(), " stale) messages"
            }));
            foreach (var email in toRemove)
                this._dbContext.Emails.Remove(email);
            this._dbContext.SaveChanges();
        }

        private void AttemptToSendUnsentMail()
        {
            var unsent = this._dbContext.Emails
                .Include(e => e.EmailRecipients)
                .Include(e => e.EmailAttachments)
                .Where(e => e.Enabled
                            && !e.Sent
                            && e.SendAt <= DateTime.Now
                            && e.SendAttempts < this._config.MaxSendAttempts)
                .ToList();
            foreach (var message in unsent)
            {
                this.SendMessageFor(message);
            }
        }

        private void SendMessageFor(Models.Email message)
        {
            using (var email = this._emailGenerator())
            {
                SetupEmailFromMessage(message, email);
                this._config.Logger.LogInfo(String.Join("", new[] { "Attempting to send mail to '", email.To.FirstOrDefault() }));
                try
                {
                    email.Send();
                    message.Sent = true;
                    this._config.Logger.LogInfo(" => message sent");
                }
                catch (Exception ex)
                {
                    message.LastError = ex.Message;
                    message.SendAttempts++;
                    var backoffBy = this._config.BackoffIntervalInMinutes * message.SendAttempts;
                    if (message.SendAttempts > 1)
                        backoffBy *= this._config.BackoffMultiplier;
                    message.SendAt = DateTime.Now.AddMinutes(backoffBy);
                    message.Sent = false;
                    this._config.Logger.LogInfo("** Message not sent: " + ex.Message);
                }
                this._dbContext.SaveChanges();
            }
        }

        private static void SetupEmailFromMessage(Models.Email message, IEmail email)
        {
            AddAllRecipients(message, email);
            email.From = message.Sender;
            email.Subject = message.Subject;
            email.Body = message.Body;
            AddAttachments(message, email);
        }

        private static void AddAttachments(Models.Email message, IEmail email)
        {
            foreach (var attachment in message.EmailAttachments.Where(a => a.Enabled))
            {
                if (String.IsNullOrEmpty(attachment.ContentID))
                    email.AddAttachment(attachment.Name, attachment.Data, attachment.MIMEType);
                else
                    email.AddAttachment(attachment.Name, attachment.Data, attachment.MIMEType, attachment.ContentID);
            }
        }

        private static void AddAllRecipients(Models.Email message, IEmail email)
        {
            foreach (var recipient in message.EmailRecipients.Where(r => r.Enabled))
            {
                if (recipient.PrimaryRecipient)
                    email.AddRecipient(recipient.Recipient);
                if (recipient.CC)
                    email.AddCC(recipient.Recipient);
                if (recipient.BCC)
                    email.AddBCC(recipient.Recipient);
            }
        }
    }
}
