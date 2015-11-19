using System.Collections.Generic;
using PeanutButter.Utils.Entity;

namespace EmailSpooler.Win32Service.Entity
{
    public class Email: EntityBase
    {
        public Email()
        {
            EmailAttachments = new List<EmailAttachment>();
            EmailRecipients = new List<EmailRecipient>();
        }

        public int EmailId { get; set; }
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
