using System;
using System.Data;
using FluentMigrator;
using FluentMigrator.Runner.Processors;

namespace PeanutButter.FluentMigrator.Fakes
{
    public class FakeDbConnectionFactory: IDbFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new FakeDbConnection()
            {
                ConnectionString = connectionString
            };
        }

        [Obsolete("Obsolete")]
        public IDbCommand CreateCommand(string commandText,
            IDbConnection connection,
            IDbTransaction transaction,
            IMigrationProcessorOptions options)
        {
            return new FakeDbCommand()
            {
                CommandText = commandText,
                Connection = connection,
                Transaction = transaction,
                CommandTimeout = options.Timeout
            };
        }

        public IDbCommand CreateCommand(
            string commandText,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            return new FakeDbCommand()
            {
                CommandText = commandText,
                Connection = connection,
                Transaction = transaction
            };
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new FakeDbDataAdapter()
            {
                DeleteCommand = command,
                InsertCommand = command,
                SelectCommand = command,
                UpdateCommand = command
            };
        }

        public IDbCommand CreateCommand(string commandText, IDbConnection connection)
        {
            return new FakeDbCommand()
            {
                CommandText = commandText,
                Connection = connection,
                Transaction = new FakeDbTransaction(connection, IsolationLevel.Chaos)
            };
        }
    }
}