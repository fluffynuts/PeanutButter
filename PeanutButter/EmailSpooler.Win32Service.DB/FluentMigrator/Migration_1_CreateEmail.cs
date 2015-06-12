using System;
using FluentMigrator;
using _Email = EmailSpooler.Win32Service.DB.DataConstants.Tables.Email;
using _Columns = EmailSpooler.Win32Service.DB.DataConstants.Tables.Email.Columns;

namespace EmailSpooler.Win32Service.DB.FluentMigrator
{
    public class Migration_1_CreateEmail: MigrationFoundation
    {
        public override void Up()
        {
            Create.Table(_Email.NAME)
                        .WithColumn(_Columns.EMAILID)
                            .AsGuid().PrimaryKey()
                        .WithColumn(_Columns.SENDER)
                            .AsString(Int32.MaxValue).Nullable()
                        .WithColumn(_Columns.SUBJECT)
                            .AsString(Int32.MaxValue).NotNullable()
                        .WithColumn(_Columns.BODY)
                            .AsString(Int32.MaxValue).NotNullable()
                        .WithColumn(_Columns.SENDAT)
                            .AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                        .WithColumn(_Columns.SENDATTEMPTS)
                            .AsInt32().NotNullable().WithDefaultValue(0)
                        .WithColumn(_Columns.SENT)
                            .AsBoolean().NotNullable().WithDefaultValue(false)
                        .WithColumn(_Columns.LASTERROR)
                            .AsString(Int32.MaxValue).Nullable()
                        .WithDefaultColumns();
            AddLastUpdatedTriggerFor(_Email.NAME, _Columns.EMAILID);
        }

        public override void Down()
        {
        }
    }
}
