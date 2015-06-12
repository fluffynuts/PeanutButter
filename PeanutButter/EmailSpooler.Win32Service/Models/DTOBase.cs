using System;

namespace EmailSpooler.Win32Service.Models
{
    public class DTOBase
    {
        public DateTime Created { get; set; }
        public bool Enabled { get; set; }
        public DateTime? LastModified { get; set; }
        public DTOBase()
        {
            this.Created = DateTime.Now;
            this.Enabled = true;
        }
    }
    public partial class Email : DTOBase { }
    public partial class EmailAttachment : DTOBase { }
    public partial class EmailRecipient : DTOBase { }
}