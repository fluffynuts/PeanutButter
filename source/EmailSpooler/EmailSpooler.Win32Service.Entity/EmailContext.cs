using System.Data.Common;
using System.Data.Entity;
using PeanutButter.Utils.Entity;

namespace EmailSpooler.Win32Service.Entity
{
    public interface IEmailContext
    {
        IDbSet<Email> Emails { get; set; }
        IDbSet<EmailAttachment> EmailAttachments { get; set; }
        IDbSet<EmailRecipient> EmailRecipients { get; set; }
        int SaveChanges();
        void Dispose();
    }

    public class EmailContext : DbContextWithAutomaticTrackingFields, IEmailContext
    {
        static EmailContext()
        {
            Database.SetInitializer<EmailContext>(null);
        }

        public EmailContext(DbConnection connection)
            :base (connection,true)
        {
        }

        public EmailContext(string connectionString)
            : base(connectionString)
        {
        }

        public EmailContext()
            : base("Name=DefaultConnection")
        {
        }

        public IDbSet<Email> Emails { get; set; }
        public IDbSet<EmailAttachment> EmailAttachments { get; set; }
        public IDbSet<EmailRecipient> EmailRecipients { get; set; }
    }
}
