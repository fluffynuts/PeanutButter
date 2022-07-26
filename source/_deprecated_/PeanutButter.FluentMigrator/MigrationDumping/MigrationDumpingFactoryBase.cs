using System;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using PeanutButter.FluentMigrator.Fakes;

namespace PeanutButter.FluentMigrator.MigrationDumping
{
    public abstract class MigrationDumpingFactoryBase<T> :
            MigrationProcessorFactory
        where T : IMigrationProcessor
    {
        private readonly Func<IDbFactory, IAnnouncer, IMigrationProcessorOptions, T> _processorFactory;

        public MigrationDumpingFactoryBase(
            Func<IDbFactory, IAnnouncer, IMigrationProcessorOptions, T> processorFactory
        )
        {
            _processorFactory = processorFactory;
        }
        public override IMigrationProcessor Create(
            string connectionString,
            IAnnouncer announcer,
            IMigrationProcessorOptions options
        )
        {
            var connectionFactory = new FakeDbConnectionFactory();
            return _processorFactory(
                connectionFactory,
                announcer,
                options
            );
        }
    }
}