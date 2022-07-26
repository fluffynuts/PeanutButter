using System.Data;

namespace PeanutButter.FluentMigrator.Fakes
{
    internal class FakeDbConnection: IDbConnection
    {
        private ConnectionState _state;

        public void Dispose()
        {
            /* intentionally left blank */
        }

        public IDbTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.ReadUncommitted);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return new FakeDbTransaction(this, il);
        }

        public void Close()
        {
            _state = ConnectionState.Closed;
        }

        public void ChangeDatabase(string databaseName)
        {
            /* intentionally left blank */
        }

        public IDbCommand CreateCommand()
        {
            return new FakeDbCommand()
            {
                Connection = this
            };
        }

        public void Open()
        {
            _state = ConnectionState.Open;
        }

        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get; set; } = 30;
        public string Database { get; set; }
        public ConnectionState State => _state;
    }
}