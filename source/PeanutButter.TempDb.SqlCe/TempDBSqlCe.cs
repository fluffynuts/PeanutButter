using System.Data.SqlServerCe;
using PeanutButter.TempDb;

namespace PeanutButter.TestUtils.Generic
{
    public class TempDBSqlCe : TempDB<SqlCeConnection>
    {
        public TempDBSqlCe(params string[] creationScripts)
            : base(creationScripts)
        {
        }
        protected override void CreateDatabase()
        {
            using (var engine = new SqlCeEngine(ConnectionString))
            {
                engine.CreateDatabase();
            }
        }
    }
}
