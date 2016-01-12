using System;
using FluentMigrator;
using _Columns = EmailSpooler.Win32Service.DB.DataConstants.Tables.EmailAttachments.Columns;

namespace EmailSpooler.Win32Service.DB.FluentMigrator
{
    [Migration(2013120703)]
    public class Migration_3_CreateEmailAttachment: MigrationFoundation
    {
        public override void Up()
        {
            Create.Table(DataConstants.Tables.EmailAttachments.NAME)
                    .WithColumn(_Columns.EMAILATTACHMENTID)
                        .AsInt32().PrimaryKey().Identity()
                    .WithColumn(_Columns.EMAILID)
                        .AsInt32().ForeignKey(DataConstants.Tables.Emails.NAME, DataConstants.Tables.Emails.Columns.EMAILID).NotNullable()
                    .WithColumn(_Columns.NAME)
                        .AsString(DataConstants.FieldSizes.MAX_PATH).NotNullable()
                    .WithColumn(_Columns.INLINE)
                        .AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn(_Columns.CONTENTID)
                        .AsString(DataConstants.FieldSizes.MAX_PATH).NotNullable().WithDefault(SystemMethods.NewGuid)
                    .WithColumn(_Columns.MIMETYPE)
                        .AsString(DataConstants.FieldSizes.MAX_PATH).NotNullable().WithDefaultValue("application/octet-stream")
                    .WithColumn(_Columns.DATA)
                        .AsBinary(int.MaxValue).NotNullable()
                    .WithDefaultColumns();
            AddLastUpdatedTriggerFor(DataConstants.Tables.EmailAttachments.NAME, _Columns.EMAILATTACHMENTID);
        }

        public override void Down()
        {
        }
    }
}
