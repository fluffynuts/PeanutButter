using System;
using System.Reflection;
using FluentMigrator.Runner.Processors.SqlServer;

namespace PeanutButter.FluentMigrator.Tests.Shared
{
    public class RunningDbMigrationsRunnerForSqlServer :
        DBMigrationsRunner<SqlServer2000ProcessorFactory>
    {
        public RunningDbMigrationsRunnerForSqlServer(
            Assembly assemblyToLoadMigrationsFrom,
            string connectionString,
            Action<string> textWriterAction = null)
            : base(assemblyToLoadMigrationsFrom,
                connectionString,
                textWriterAction)
        {
        }
    }
}