using System.Data.Entity;
using EACH.Data.Models.Mapping;
using EmailSpooler.Win32Service.Models.Mapping;

namespace EmailSpooler.Win32Service.Models
{
    public interface IEmailContext
    {
        IDbSet<Email> Emails { get; set; }
        IDbSet<EmailAttachment> EmailAttachments { get; set; }
        IDbSet<EmailRecipient> EmailRecipients { get; set; }
        int SaveChanges();
        void Dispose();
    }

    public partial class EmailContext : DbContext, IEmailContext
    {
        static EmailContext()
        {
            Database.SetInitializer<EmailContext>(null);
        }

        public EmailContext()
            : base("Name=EmailConnection")
        {
        }

        public IDbSet<Email> Emails { get; set; }
        public IDbSet<EmailAttachment> EmailAttachments { get; set; }
        public IDbSet<EmailRecipient> EmailRecipients { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new EmailMap());
            modelBuilder.Configurations.Add(new EmailAttachmentMap());
            modelBuilder.Configurations.Add(new EmailRecipientMap());
        }
    }
}
