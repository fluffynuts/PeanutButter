using System;
using EmailSpooler.Win32Service.DB.Tests.Migrations;
using FluentMigrator.Runner.Processors.SqlServer;
using PeanutButter.FluentMigrator;

namespace EmailSpooler.Win32Service.DB.Tests
{
    public class DbMigrationsRunnerSqlServer : DBMigrationsRunner<SqlServer2000ProcessorFactory>
    {
        public DbMigrationsRunnerSqlServer(string connectionString, Action<string> textWriterAction = null) 
            : base(typeof(Migration_1_CreateEmail).Assembly, connectionString, textWriterAction)
        {
        }
    }
}