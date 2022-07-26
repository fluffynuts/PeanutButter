using System;
// ReSharper disable InconsistentNaming

namespace EmailSpooler.Win32Service.SMTP
{
    public class EmailAttachment
    {
        public string Name { get; }
        public byte[] Data { get; }
        public string MIMEType { get; }
        public string ContentID { get; set; }
        public bool IsInline { get; }
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