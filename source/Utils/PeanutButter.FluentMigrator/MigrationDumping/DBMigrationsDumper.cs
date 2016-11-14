using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Runner.Processors;

namespace PeanutButter.FluentMigrator.MigrationDumping
{
    public interface IMigrationDumperOptions
    {
        bool IncludeComments { get; }
        bool IncludeFluentMigratorStructures { get; }
    }

    internal class DefaultMigrationDumperOptions : IMigrationDumperOptions
    {
        public bool IncludeComments => true;
        public bool IncludeFluentMigratorStructures => true;
    }


    // ReSharper disable once InconsistentNaming    // to match other inconsistent naming. At least I'm consistent about being inconsistent.
#pragma warning disable S101 // Types should be named in camel case
    public class DBMigrationsDumper<T> where T : MigrationProcessorFactory, new()
#pragma warning restore S101 // Types should be named in camel case
    {
        private readonly IMigrationDumperOptions _dumpOptions;

        public DBMigrationsDumper() : this(new DefaultMigrationDumperOptions())
        {
        }
        public DBMigrationsDumper(IMigrationDumperOptions options)
        {
            _dumpOptions = options;
        }

        public IEnumerable<string> DumpMigrationScript(
            Assembly migrationsAssembly
        )
        {
            var recorder = new AnnouncerMessageRecorder(_dumpOptions);
            var runner = new DBMigrationsRunner<T>(
                migrationsAssembly, string.Empty, recorder.Record
            );
            runner.MigrateToLatest();
            return recorder.Statements;
        }
    }
}