using System;
using System.Data.SqlServerCe;
// ReSharper disable InconsistentNaming

namespace PeanutButter.TempDb.SqlCe
{
    public class TempDBSqlCe : TempDB<SqlCeConnection>
    {
        public TempDBSqlCe(params string[] creationScripts)
            : base(creationScripts)
        {
        }

        public override string DumpSchema()
        {
            throw new NotImplementedException(
                "DumpSchema not yet implemented for LocalDb. Open a Pull Request!"
            );
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
