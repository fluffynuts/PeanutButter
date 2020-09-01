using System;
using System.Data.SQLite;

// ReSharper disable InconsistentNaming

namespace PeanutButter.TempDb.Sqlite
{
    public class TempDBSqlite : TempDB<SQLiteConnection>
    {
        /// <summary>
        /// Creates a new TempDBSqlite
        /// </summary>
        public TempDBSqlite()
        {
        }

        /// <summary>
        /// Creates a new TempDbSqlite and runs in all provides scripts
        /// </summary>
        /// <param name="creationScripts"></param>
        public TempDBSqlite(params string[] creationScripts)
            : base(creationScripts)
        {
        }

        public override string DumpSchema()
        {
            throw new NotImplementedException(
                "DumpSchema not yet implemented for LocalDb. Open a Pull Request!"
            );
        }

        protected override int FetchCurrentConnectionCount()
        {
            throw new NotImplementedException(
                "Inactivity monitoring for Sqlite not yet implemented"
            );
        }

        protected override void CreateDatabase()
        {
            SQLiteConnection.CreateFile(DatabasePath);
        }
    }
}