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
        protected List<IDisposable> _disposables = new List<IDisposable>();
        public Email(IEmailConfiguration config)
        {
            if (config == null) throw new ArgumentNullException("config");
            this._config = config;
            this.SetDefaults();
        }

        public static Email Create()
        {
            return new Email(EmailConfiguration.CreateFromAppConfig());
        }

        public string AddPDFAttachment(string fileName, byte[] data)
        {
            return this.AddAttachment(fileName, data, "application/pdf");
        }

        public string AddAttachment(string fileName, byte[] data, string mimeType, bool isInline = false)
        {
            var emailAttachment = new EmailAttachment(fileName, data, mimeType, isInline);
            this.Attachments.Add(emailAttachment);
            return emailAttachment.ContentID;
        }

        public void AddAttachment(string fileName, byte[] data, string mimeType, string contentId)
        {
            var emailAttachment = new EmailAttachment(fileName, data, mimeType, true);
            emailAttachment.ContentID = contentId;
            this.Attachments.Add(emailAttachment);
        }

        public string AddInlineImageAttachment(string fileName, byte[] data)
        {
            var parts = fileName.Split(new[] { '.' });
            var extension = parts.Length > 1 ? parts[parts.Length-1].ToLower() : "jpeg";
            var mimeType = "image/" + extension;
            return this.AddAttachment(fileName, data, mimeType, true);
        }

        public void AddRecipient(string email)
        {
            this.To.Add(email);
        }

        public void AddCC(string email)
        {
            this.CC.Add(email);
        }

        public void AddBCC(string email)
        {
            this.BCC.Add(email);
        }

        public void Send()
        {
            this.CheckEmailParameters();
            var message = CreateMessage();
            var client = CreateSMTPClient();
            client.Send(message);
        }

        public void Dispose()
        {
            lock (this)
            {
                foreach (var d in this._disposables)
                    d.Dispose();
                this._disposables.Clear();
            }
        }

        private void SetDefaults()
        {
            this.Body = "";
            this.To = new List<string>();
            this.CC = new List<string>();
            this.BCC = new List<string>();
            this.Subject = "";
            this.Attachments = new List<EmailAttachment>();
        }

        protected virtual void CheckEmailParameters()
        {
            if (this.Body == null) throw new ArgumentException("Email body cannot be null");
            if (this.Subject == null) throw new ArgumentException("Email subject cannot be null");
            if (this.To.Count == 0) throw new ArgumentException("Email has no recipients");
            if (String.IsNullOrWhiteSpace(this.From)) throw new ArgumentException("Email sender cannot be empty");
        }

        protected virtual MailMessage CreateMessage()
        {
            var message = new MailMessage()
            {
                Subject = this.Subject,
                IsBodyHtml = this.DetermineIfBodyIsHTML(),
                Body = this.Body,
                From = new MailAddress(this.From),
                Sender = new MailAddress(this.From)
            };
            this.AddRecipientsTo(message);
            this.AddAttachmentsTo(message);
            this._disposables.Add(message);
            return message;
        }

        private void AddRecipientsTo(MailMessage message)
        {
            foreach (var address in this.To)
                message.To.Add(address);
            foreach (var address in this.CC)
                message.CC.Add(address);
            foreach (var address in this.BCC)
                message.Bcc.Add(address);
        }

        private void AddAttachmentsTo(MailMessage message)
        {
            foreach (var attachment in this.Attachments)
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
                this._disposables.Add(memStream);
            }
        }

        protected virtual ISmtpClient CreateSMTPClient()
        {
            var client = new SmtpClientFacade()
            {
                Host = this._config.Host,
                Port = this._config.Port,
                EnableSsl = this._config.SSLEnabled,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(this._config.UserName, this._config.Password)
            };
            return client;
        }

        protected bool DetermineIfBodyIsHTML()
        {
            return (this.Body.IndexOf("<html") > -1 && this.Body.IndexOf("</html>") > -1);
        }

    }
}