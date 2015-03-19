using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

namespace PeanutButter.FluentMigrator
{
    public class DBMigrationsRunner<T> where T: MigrationProcessorFactory, new()
    {
        private class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; private set; }
            public int Timeout { get; private set; }
            public string ProviderSwitches { get; private set; }
            public MigrationOptions(bool previewOnly = false, string providerSwitches = null, int timeout = 60)
            {
                PreviewOnly = previewOnly;
                ProviderSwitches = providerSwitches;
                Timeout = timeout;
            }
        }

        private readonly Assembly _assemblyToLoadMigrationsFrom;
        private readonly string _connectionString;
        private readonly Action<string> _textWriterAction;

        public DBMigrationsRunner(Assembly assemblyToLoadMigrationsFrom, string connectionString, Action<string> textWriterAction = null)
        {
            _assemblyToLoadMigrationsFrom = assemblyToLoadMigrationsFrom;
            _connectionString = connectionString;
            _textWriterAction = textWriterAction ?? (str => Debug.WriteLine(str));
        }

        public void MigrateTo(int version)
        {
            DoMigration(runner => runner.MigrateUp(version));
        }

        public void MigrateToLatest()
        {
            DoMigration(runner => runner.MigrateUp(true));
        }

        private void DoMigration(Action<MigrationRunner> call)
        {
            var announcer = new TextWriterAnnouncer(_textWriterAction);
            var context = new RunnerContext(announcer);
            var options = new MigrationOptions();
            var factory = new T();
            var processor = factory.Create(_connectionString, announcer, options);
            var runner = new MigrationRunner(_assemblyToLoadMigrationsFrom, context, processor);
            call(runner);
        }
    }
}
