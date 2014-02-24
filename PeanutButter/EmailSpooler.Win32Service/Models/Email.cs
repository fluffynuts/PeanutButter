using System.Collections.Generic;

namespace EmailSpooler.Win32Service.Models
{
    public partial class Email
    {
        public Email()
        {
            this.EmailAttachments = new List<EmailAttachment>();
            this.EmailRecipients = new List<EmailRecipient>();
        }

        public long EmailID { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public System.DateTime SendAt { get; set; }
        public int SendAttempts { get; set; }
        public bool Sent { get; set; }
        public string LastError { get; set; }
        public virtual IList<EmailAttachment> EmailAttachments { get; set; }
        public virtual IList<EmailRecipient> EmailRecipients { get; set; }
    }
}
