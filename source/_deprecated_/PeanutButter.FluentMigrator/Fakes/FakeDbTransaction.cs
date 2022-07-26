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
            /* intentionally left blank */
        }

        public void Commit()
        {
            /* intentionally left blank */
        }

        public void Rollback()
        {
            /* intentionally left blank */
        }

        public IDbConnection Connection { get; }

        public IsolationLevel IsolationLevel { get; }
    }
}