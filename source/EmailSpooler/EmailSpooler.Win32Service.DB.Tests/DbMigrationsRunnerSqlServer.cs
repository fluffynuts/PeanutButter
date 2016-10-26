using System;
using EmailSpooler.Win32Service.DB.Tests.FluentMigrator;
using FluentMigrator.Runner.Processors.SqlServer;
using PeanutButter.FluentMigrator;

namespace EmailSpooler.Win32Service.DB.Tests
{
    public class DbMigrationsRunnerSqlServer : DBMigrationsRunner<SqlServer2000ProcessorFactory>
    {
        public DbMigrationsRunnerSqlServer(string connectionString) 
            : this(connectionString, null)
        {
        }

        public DbMigrationsRunnerSqlServer(string connectionString, Action<string> textWriterAction) 
            : base(typeof(Migration_1_CreateEmail).Assembly, connectionString, textWriterAction)
        {
        }
    }
}