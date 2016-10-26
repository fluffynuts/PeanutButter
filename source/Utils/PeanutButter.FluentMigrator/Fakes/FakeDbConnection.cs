using System.Data;

namespace PeanutButter.FluentMigrator.Fakes
{
    internal class FakeDbConnection: IDbConnection
    {
        private ConnectionState _state;

        public void Dispose()
        {
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
        }

        public IDbCommand CreateCommand()
        {
            return new FakeDbCommand();
        }

        public void Open()
        {
            _state = ConnectionState.Open;
        }

        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get; set; }
        public string Database { get; set; }
        public ConnectionState State => _state;
    }
}