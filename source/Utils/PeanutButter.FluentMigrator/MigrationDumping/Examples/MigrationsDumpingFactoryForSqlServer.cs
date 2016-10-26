using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;

namespace PeanutButter.FluentMigrator.MigrationDumping.Examples
{
    public class MigrationsDumpingFactoryForSqlServer :
        MigrationDumpingFactoryBase<SqlServerProcessor>
    {
        public MigrationsDumpingFactoryForSqlServer() : base(CreateProcessor)
        {
        }

        private static SqlServerProcessor CreateProcessor(
            IDbFactory connectionFactory,
            IAnnouncer announcer,
            IMigrationProcessorOptions options)
        {
            return new SqlServerProcessor(
                connectionFactory.CreateConnection(null),
                new SqlServer2000Generator(),
                announcer,
                options,
                connectionFactory);
        }
    }
}