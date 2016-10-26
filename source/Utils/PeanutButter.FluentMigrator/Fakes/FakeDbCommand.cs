using System.Data;

namespace PeanutButter.FluentMigrator.Fakes
{
    internal class FakeDbCommand: IDbCommand
    {
        public void Dispose()
        {
            /* nothing to do */
        }

        public void Prepare()
        {
            /* nothing to do */
        }

        public void Cancel()
        {
            /* nothing to do */
        }

        public IDbDataParameter CreateParameter()
        {
            return new FakeDbParameter();
        }

        public int ExecuteNonQuery()
        {
            return 0;
        }

        public IDataReader ExecuteReader()
        {
            return new FakeDbReader();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return new FakeDbReader();
        }

        public object ExecuteScalar()
        {
            return 0;
        }

        public IDbConnection Connection { get; set; }
        public IDbTransaction Transaction { get; set; }
        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDataParameterCollection Parameters { get; }
        public UpdateRowSource UpdatedRowSource { get; set; }
    }
}