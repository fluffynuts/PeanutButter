using FluentMigrator;

namespace PeanutButter.FluentMigrator.Tests.Shared
{
    [Migration(2)]
    public class Migration2 : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("Cows")
                .AddColumn("IsLactoseIntolerant").AsBoolean()
                .Nullable().WithDefaultValue(false);
        }

        public override void Down()
        {
            /* do nothing */
        }
    }
}