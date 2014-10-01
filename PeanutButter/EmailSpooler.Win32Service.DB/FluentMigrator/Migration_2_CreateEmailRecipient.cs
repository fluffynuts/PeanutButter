using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmailSpooler.Win32Service.DB.FluentMigrator;
using FluentMigrator;
using _EmailRecipient = EmailSpooler.Win32Service.DB.DataConstants.Tables.EmailRecipient;
using _Columns = EmailSpooler.Win32Service.DB.DataConstants.Tables.EmailRecipient.Columns;
using _Email = EmailSpooler.Win32Service.DB.DataConstants.Tables.Email;

namespace EACH.DB.Migrations.Migrations
{
    [Migration(2013120702)]
    public class Migration_2_CreateEmailRecipient: MigrationFoundation
    {
        public override void Up()
        {
            Create.Table(_EmailRecipient.NAME)
                        .WithColumn(_Columns.EMAILRECIPIENTID)
                            .AsGuid().PrimaryKey()
                        .WithColumn(_Columns.EMAILID)
                            .AsGuid().ForeignKey(_Email.NAME, _Email.Columns.EMAILID).NotNullable()
                        .WithColumn(_Columns.RECIPIENT)
                            .AsString(Int32.MaxValue).NotNullable()
                        .WithColumn(_Columns.PRIMARYRECIPIENT)
                            .AsBoolean().NotNullable().WithDefaultValue(true)
                        .WithColumn(_Columns.CC)
                            .AsBoolean().NotNullable().WithDefaultValue(false)
                        .WithColumn(_Columns.BCC)
                            .AsBoolean().NotNullable().WithDefaultValue(false)
                        .WithDefaultColumns();
            AddLastUpdatedTriggerFor(_EmailRecipient.NAME, _Columns.EMAILRECIPIENTID);
        }

        public override void Down()
        {
        }
    }
}
