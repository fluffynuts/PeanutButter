namespace EmailSpooler.Win32Service.Models
{
    public partial class EmailRecipient
    {
        public long EmailRecipientID { get; set; }
        public long EmailID { get; set; }
        public string Recipient { get; set; }
        public bool PrimaryRecipient { get; set; }
        public bool CC { get; set; }
        public bool BCC { get; set; }
        public virtual Email Email { get; set; }
    }
}
