using System.Data.SQLite;

namespace PeanutButter.TempDb.Sqlite
{
    public class TempDBSqlite : TempDB<SQLiteConnection>
    {
        public TempDBSqlite(params string[] creationScripts)
            : base(creationScripts)
        {
        }
        protected override void CreateDatabase()
        {
            SQLiteConnection.CreateFile(DatabaseFile);
        }
    }
}