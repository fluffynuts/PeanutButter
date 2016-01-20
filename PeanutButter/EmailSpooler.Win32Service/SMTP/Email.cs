using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace EmailSpooler.Win32Service.SMTP
{
    public class Email : IEmail
    {
        private IEmailConfiguration _config;
        public List<string> To { get; protected set; }
        public List<string> CC { get; protected set; }
        public List<string> BCC { get; protected set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<EmailAttachment> Attachments { get; private set; }
        protected List<IDisposable> Disposables { get; set; } = new List<IDisposable>();
        public Email(IEmailConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _config = config;
            SetDefaults();
        }

        public static Email Create()
        {
            return new Email(EmailConfiguration.CreateFromAppConfig());
        }

        public string AddPDFAttachment(string fileName, byte[] data)
        {
            return AddAttachment(fileName, data, "application/pdf");
        }

        public string AddAttachment(string fileName, byte[] data, string mimeType, bool isInline = false)
        {
            var emailAttachment = new EmailAttachment(fileName, data, mimeType, isInline);
            Attachments.Add(emailAttachment);
            return emailAttachment.ContentID;
        }

        public void AddAttachment(string fileName, byte[] data, string mimeType, string contentId)
        {
            var emailAttachment = new EmailAttachment(fileName, data, mimeType, true);
            emailAttachment.ContentID = contentId;
            Attachments.Add(emailAttachment);
        }

        public string AddInlineImageAttachment(string fileName, byte[] data)
        {
            var parts = fileName.Split(new[] { '.' });
            var extension = parts.Length > 1 ? parts[parts.Length-1].ToLower() : "jpeg";
            var mimeType = "image/" + extension;
            return AddAttachment(fileName, data, mimeType, true);
        }

        public void AddRecipient(string email)
        {
            To.Add(email);
        }

        public void AddCC(string email)
        {
            CC.Add(email);
        }

        public void AddBCC(string email)
        {
            BCC.Add(email);
        }

        public void Send()
        {
            CheckEmailParameters();
            var message = CreateMessage();
            var client = CreateSMTPClient();
            client.Send(message);
        }

        public void Dispose()
        {
            lock (this)
            {
                foreach (var d in Disposables)
                    d.Dispose();
                Disposables.Clear();
            }
        }

        private void SetDefaults()
        {
            Body = string.Empty;
            To = new List<string>();
            CC = new List<string>();
            BCC = new List<string>();
            Subject = string.Empty;
            Attachments = new List<EmailAttachment>();
        }

        protected virtual void CheckEmailParameters()
        {
            if (Body == null) throw new ArgumentException("Email body cannot be null");
            if (Subject == null) throw new ArgumentException("Email subject cannot be null");
            if (To.Count == 0) throw new ArgumentException("Email has no recipients");
            if (string.IsNullOrWhiteSpace(From)) throw new ArgumentException("Email sender cannot be empty");
        }

        protected virtual MailMessage CreateMessage()
        {
            var message = new MailMessage()
            {
                Subject = Subject,
                IsBodyHtml = DetermineIfBodyIsHTML(),
                Body = Body,
                From = new MailAddress(From),
                Sender = new MailAddress(From)
            };
            AddRecipientsTo(message);
            AddAttachmentsTo(message);
            Disposables.Add(message);
            return message;
        }

        private void AddRecipientsTo(MailMessage message)
        {
            foreach (var address in To)
                message.To.Add(address);
            foreach (var address in CC)
                message.CC.Add(address);
            foreach (var address in BCC)
                message.Bcc.Add(address);
        }

        private void AddAttachmentsTo(MailMessage message)
        {
            foreach (var attachment in Attachments)
            {
                var memStream = new MemoryStream(attachment.Data);
                var mailAttachment = new Attachment(memStream, attachment.Name, attachment.MIMEType);
                mailAttachment.ContentId = attachment.ContentID;
                if (attachment.IsInline)
                {
                    mailAttachment.ContentDisposition.Inline = true;
                    mailAttachment.ContentDisposition.DispositionType = DispositionTypeNames.Inline;
                }
                else
                {
                    mailAttachment.ContentDisposition.Inline = false;
                    mailAttachment.ContentDisposition.DispositionType = DispositionTypeNames.Attachment;
                }
                message.Attachments.Add(mailAttachment);
                Disposables.Add(memStream);
            }
        }

        protected virtual ISmtpClient CreateSMTPClient()
        {
            var client = new SmtpClientFacade()
            {
                Host = _config.Host,
                Port = _config.Port,
                EnableSsl = _config.SSLEnabled,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_config.UserName, _config.Password)
            };
            return client;
        }

        protected bool DetermineIfBodyIsHTML()
        {
            return Body.IndexOf("<html", StringComparison.Ordinal) > -1 && 
                    Body.IndexOf("</html>", StringComparison.Ordinal) > -1;
        }

    }
}