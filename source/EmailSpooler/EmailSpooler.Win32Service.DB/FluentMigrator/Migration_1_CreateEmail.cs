using System;
using FluentMigrator;
using _Columns = EmailSpooler.Win32Service.DB.DataConstants.Tables.Emails.Columns;

namespace EmailSpooler.Win32Service.DB.FluentMigrator
{
    public class Migration_1_CreateEmail: MigrationFoundation
    {
        public override void Up()
        {
            Create.Table(DataConstants.Tables.Emails.NAME)
                        .WithColumn(_Columns.EMAILID)
                            .AsInt32().PrimaryKey().Identity()
                        .WithColumn(_Columns.SENDER)
                            .AsString(int.MaxValue).Nullable()
                        .WithColumn(_Columns.SUBJECT)
                            .AsString(int.MaxValue).NotNullable()
                        .WithColumn(_Columns.BODY)
                            .AsString(int.MaxValue).NotNullable()
                        .WithColumn(_Columns.SENDAT)
                            .AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                        .WithColumn(_Columns.SENDATTEMPTS)
                            .AsInt32().NotNullable().WithDefaultValue(0)
                        .WithColumn(_Columns.SENT)
                            .AsBoolean().NotNullable().WithDefaultValue(false)
                        .WithColumn(_Columns.LASTERROR)
                            .AsString(int.MaxValue).Nullable()
                        .WithDefaultColumns();
            AddLastUpdatedTriggerFor(DataConstants.Tables.Emails.NAME, _Columns.EMAILID);
        }

        public override void Down()
        {
        }
    }
}
