using System.Data.Entity.ModelConfiguration;

namespace EACH.Data.Models.Mapping
{
    public class EmailAttachmentMap : EntityTypeConfiguration<EmailSpooler.Win32Service.Models.EmailAttachment>
    {
        public EmailAttachmentMap()
        {
            // Primary Key
            this.HasKey(t => t.EmailAttachmentID);

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(260);

            this.Property(t => t.ContentID)
                .IsRequired()
                .HasMaxLength(260);

            this.Property(t => t.MIMEType)
                .IsRequired()
                .HasMaxLength(260);

            this.Property(t => t.Data)
                .IsRequired();

            // Table & Column Mappings
            this.ToTable("EmailAttachment");
            this.Property(t => t.EmailAttachmentID).HasColumnName("EmailAttachmentID");
            this.Property(t => t.EmailID).HasColumnName("EmailID");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Inline).HasColumnName("Inline");
            this.Property(t => t.ContentID).HasColumnName("ContentID");
            this.Property(t => t.MIMEType).HasColumnName("MIMEType");
            this.Property(t => t.Data).HasColumnName("Data");
            this.Property(t => t.Created).HasColumnName("Created");
            this.Property(t => t.LastModified).HasColumnName("LastModified");
            this.Property(t => t.Enabled).HasColumnName("Enabled");

            // Relationships
            this.HasRequired(t => t.Email)
                .WithMany(t => t.EmailAttachments)
                .HasForeignKey(d => d.EmailID);

        }
    }
}
