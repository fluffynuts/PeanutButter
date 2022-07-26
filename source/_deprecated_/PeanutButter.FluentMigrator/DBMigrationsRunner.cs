using System;
using System.Diagnostics;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;

// ReSharper disable InconsistentNaming

namespace PeanutButter.FluentMigrator
{
    public interface IDBMigrationsRunner
    {
        void MigrateToLatest();
    }

    public class DBMigrationsRunner<T> : IDBMigrationsRunner where T : MigrationProcessorFactory, new()
    {
        private class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; }
            public int Timeout { get; }
            public string ProviderSwitches { get; }

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

        public DBMigrationsRunner(
            Assembly assemblyToLoadMigrationsFrom,
            string connectionString,
            Action<string> textWriterAction = null)
        {
            _assemblyToLoadMigrationsFrom = assemblyToLoadMigrationsFrom;
            _connectionString = connectionString;
            _textWriterAction = textWriterAction ?? (str => Debug.WriteLine(str));
        }

        // ReSharper disable once UnusedMember.Global
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
            var options = new MigrationOptions();
            DoMigrationsWithOptions(options, call);
        }

        private void DoMigrationsWithOptions(MigrationOptions options, Action<MigrationRunner> call)
        {
            var announcer = new TextWriterAnnouncer(_textWriterAction)
            {
                ShowSql = true
            };
            var context = new RunnerContext(announcer);
            var factory = new T();
            var processor = factory.Create(_connectionString, announcer, options);
            var runner = new MigrationRunner(_assemblyToLoadMigrationsFrom, context, processor);
            call(runner);
        }
    }
}