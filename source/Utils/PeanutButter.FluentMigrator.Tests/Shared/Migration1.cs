using FluentMigrator;

namespace PeanutButter.FluentMigrator.Tests.Shared
{
    [Migration(1)]
    public class Migration1 : MigrationBase
    {
        public override void Up()
        {
            Create.Table("Cows")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("MooDialect").AsString().NotNullable()
                .WithColumn("IsDairy").AsBoolean().NotNullable();
        }

        public override void Down()
        {
            /* do nothing */
        }
    }
}