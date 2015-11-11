using EntityUtilities;

namespace EmailSpooler.Win32Service.DB.Entities
{
    public class EmailAttachment: EntityBase
    {
        public int EmailAttachmentId { get; set; }
        public int EmailId { get; set; }
        public string Name { get; set; }
        public bool Inline { get; set; }
        public string ContentID { get; set; }
        public string MIMEType { get; set; }
        public byte[] Data { get; set; }
        public virtual Email Email { get; set; }
    }
}
