using System.Data;
using FluentMigrator.Runner.Processors;

namespace PeanutButter.FluentMigrator.Fakes
{
    public class FakeDbConnectionFactory: IDbFactory
    {
        public IDbConnection CreateConnection(string connectionString)
        {
            return new FakeDbConnection();
        }

        public IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction)
        {
            return new FakeDbCommand();
        }

        public IDbDataAdapter CreateDataAdapter(IDbCommand command)
        {
            return new FakeDbDataAdapter();
        }

        public IDbCommand CreateCommand(string commandText, IDbConnection connection)
        {
            return new FakeDbCommand();
        }
    }

}