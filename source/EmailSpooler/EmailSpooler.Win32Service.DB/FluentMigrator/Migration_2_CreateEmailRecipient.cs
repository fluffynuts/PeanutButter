using FluentMigrator;
using _Columns = EmailSpooler.Win32Service.DB.DataConstants.Tables.EmailRecipients.Columns;
// ReSharper disable InconsistentNaming

namespace EmailSpooler.Win32Service.DB.FluentMigrator
{
    [Migration(2013120702)]
    public class Migration_2_CreateEmailRecipient: MigrationFoundation
    {
        public override void Up()
        {
            Create.Table(DataConstants.Tables.EmailRecipients.NAME)
                        .WithColumn(_Columns.EMAILRECIPIENTID)
                            .AsInt32().PrimaryKey().Identity()
                        .WithColumn(_Columns.EMAILID)
                            .AsInt32().ForeignKey(DataConstants.Tables.Emails.NAME, DataConstants.Tables.Emails.Columns.EMAILID).NotNullable()
                        .WithColumn(_Columns.RECIPIENT)
                            .AsString(int.MaxValue).NotNullable()
                        .WithColumn(_Columns.IS_PRIMARYRECIPIENT)
                            .AsBoolean().NotNullable().WithDefaultValue(true)
                        .WithColumn(_Columns.IS_CC)
                            .AsBoolean().NotNullable().WithDefaultValue(false)
                        .WithColumn(_Columns.IS_BCC)
                            .AsBoolean().NotNullable().WithDefaultValue(false)
                        .WithDefaultColumns();
            AddLastUpdatedTriggerFor(DataConstants.Tables.EmailRecipients.NAME, _Columns.EMAILRECIPIENTID);
        }

        public override void Down()
        {
            /* intentionally left blank */
        }
    }
}
