using System.Data;
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