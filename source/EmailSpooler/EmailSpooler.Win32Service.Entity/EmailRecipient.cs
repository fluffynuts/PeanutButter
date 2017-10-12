using PeanutButter.Utils.Entity;
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace EmailSpooler.Win32Service.Entity
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
