using EntityUtilities;

namespace EmailSpooler.Win32Service.DB.Entities
{
    public class EmailRecipient: EntityBase
    {
        public int EmailRecipientId { get; set; }
        public int EmailId { get; set; }
        public string Recipient { get; set; }
        public bool IsPrimaryRecipient { get; set; }
        public bool IsCC { get; set; }
        public bool IsBCC { get; set; }
        public virtual Email Email { get; set; }
    }
}
