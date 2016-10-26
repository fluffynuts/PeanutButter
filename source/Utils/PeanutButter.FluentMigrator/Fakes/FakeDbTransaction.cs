using System.Data;

namespace PeanutButter.FluentMigrator.Fakes
{
    public class FakeDbTransaction: IDbTransaction
    {
        public FakeDbTransaction(IDbConnection connection, IsolationLevel il)
        {
            Connection = connection;
            IsolationLevel = il;
        }
        public void Dispose()
        {
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }

        public IDbConnection Connection { get; }

        public IsolationLevel IsolationLevel { get; }
    }
}