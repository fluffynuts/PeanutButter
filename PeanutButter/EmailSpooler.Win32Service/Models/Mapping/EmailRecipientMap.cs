using System.Data.Entity.ModelConfiguration;

namespace EmailSpooler.Win32Service.Models.Mapping
{
    public class EmailRecipientMap : EntityTypeConfiguration<EmailRecipient>
    {
        public EmailRecipientMap()
        {
            // Primary Key
            this.HasKey(t => t.EmailRecipientID);

            // Properties
            this.Property(t => t.Recipient)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("EmailRecipient");
            this.Property(t => t.EmailRecipientID).HasColumnName("EmailRecipientID");
            this.Property(t => t.EmailID).HasColumnName("EmailID");
            this.Property(t => t.Recipient).HasColumnName("Recipient");
            this.Property(t => t.PrimaryRecipient).HasColumnName("PrimaryRecipient");
            this.Property(t => t.CC).HasColumnName("CC");
            this.Property(t => t.BCC).HasColumnName("BCC");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastModified).HasColumnName("LastModified");
            this.Property(t => t.Enabled).HasColumnName("Enabled");

            // Relationships
            this.HasRequired(t => t.Email)
                .WithMany(t => t.EmailRecipients)
                .HasForeignKey(d => d.EmailID);

        }
    }
}
