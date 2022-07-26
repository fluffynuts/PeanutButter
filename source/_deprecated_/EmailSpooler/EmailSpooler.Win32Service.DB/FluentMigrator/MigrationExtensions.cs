using FluentMigrator;
using FluentMigrator.Builders.Create.Table;

namespace EmailSpooler.Win32Service.DB.FluentMigrator
{
    public static class MigrationExtensions
    {
        public static ICreateTableWithColumnSyntax WithDefaultColumns(this ICreateTableWithColumnSyntax root)
        {
            return root.WithColumn("Created").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                        .WithColumn("LastModified").AsDateTime().Nullable()
                        .WithColumn("Enabled").AsBoolean().NotNullable().WithDefaultValue(true);
        }
    }
}
