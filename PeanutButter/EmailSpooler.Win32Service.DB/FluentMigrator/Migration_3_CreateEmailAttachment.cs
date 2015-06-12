using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailSpooler.Win32Service.DB;
using EmailSpooler.Win32Service.DB.FluentMigrator;
using FluentMigrator;
using _EmailAttachment = EmailSpooler.Win32Service.DB.DataConstants.Tables.EmailAttachment;
using _Columns = EmailSpooler.Win32Service.DB.DataConstants.Tables.EmailAttachment.Columns;
using _Email = EmailSpooler.Win32Service.DB.DataConstants.Tables.Email;

namespace EACH.DB.Migrations.Migrations
{
    [Migration(2013120703)]
    public class Migration_3_CreateEmailAttachment: MigrationFoundation
    {
        public override void Up()
        {
            Create.Table(_EmailAttachment.NAME)
                    .WithColumn(_Columns.EMAILATTACHMENTID)
                        .AsGuid().PrimaryKey()
                    .WithColumn(_Columns.EMAILID)
                        .AsGuid().ForeignKey(_Email.NAME, _Email.Columns.EMAILID).NotNullable()
                    .WithColumn(_Columns.NAME)
                        .AsString(DataConstants.FieldSizes.MAX_PATH).NotNullable()
                    .WithColumn(_Columns.INLINE)
                        .AsBoolean().NotNullable().WithDefaultValue(false)
                    .WithColumn(_Columns.CONTENTID)
                        .AsString(DataConstants.FieldSizes.MAX_PATH).NotNullable().WithDefault(SystemMethods.NewGuid)
                    .WithColumn(_Columns.MIMETYPE)
                        .AsString(DataConstants.FieldSizes.MAX_PATH).NotNullable().WithDefaultValue("application/octet-stream")
                    .WithColumn(_Columns.DATA)
                        .AsBinary(Int32.MaxValue).NotNullable()
                    .WithDefaultColumns();
            AddLastUpdatedTriggerFor(_EmailAttachment.NAME, _Columns.EMAILATTACHMENTID);
        }

        public override void Down()
        {
        }
    }
}
