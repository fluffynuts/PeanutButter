using System;

namespace EmailSpooler.Win32Service.SMTP
{
    public class EmailAttachment
    {
        public string Name { get; private set; }
        public byte[] Data { get; private set; }
        public string MIMEType { get; private set; }
        public string ContentID { get; set; }
        public bool IsInline { get; private set; }
        public EmailAttachment(string name, byte[] data, string mimeType, bool isInline = false)
        {
            Name = name;
            Data = data;
            MIMEType = mimeType;
            ContentID = Guid.NewGuid().ToString();
            IsInline = isInline;
        }
    }
}