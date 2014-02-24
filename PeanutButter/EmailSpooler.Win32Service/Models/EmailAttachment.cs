namespace EmailSpooler.Win32Service.Models
{
    public partial class EmailAttachment
    {
        public long EmailAttachmentID { get; set; }
        public long EmailID { get; set; }
        public string Name { get; set; }
        public bool Inline { get; set; }
        public string ContentID { get; set; }
        public string MIMEType { get; set; }
        public byte[] Data { get; set; }
        public virtual Email Email { get; set; }
    }
}
