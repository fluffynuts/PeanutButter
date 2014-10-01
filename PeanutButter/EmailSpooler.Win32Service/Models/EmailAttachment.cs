using System;

namespace EmailSpooler.Win32Service.Models
{
    public partial class EmailAttachment
    {
        public Guid EmailAttachmentID { get; set; }
        public Guid EmailID { get; set; }
        public string Name { get; set; }
        public bool Inline { get; set; }
        public string ContentID { get; set; }
        public string MIMEType { get; set; }
        public byte[] Data { get; set; }
        public virtual Email Email { get; set; }
    }
}
