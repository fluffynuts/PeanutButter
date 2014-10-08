using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    public class TempDB : IDisposable
    {
        public string DatabaseFile { get; private set; }
        public string ConnectionString { get; private set; }
        private static Semaphore _lock = new Semaphore(1, 1);
        private List<DbConnection> _managedConnections;

        public TempDB(params string[] creationScripts)
        {
            using (new AutoLocker(_lock))
            {
                DatabaseFile = Path.GetTempFileName();
                if (File.Exists(DatabaseFile))
                    File.Delete(DatabaseFile);
                ConnectionString = String.Format("DataSource=\"{0}\";", DatabaseFile);
                using (var engine = new SqlCeEngine(ConnectionString))
                {
                    engine.CreateDatabase();
                }
            }
            _managedConnections = new List<DbConnection>();
            RunScripts(creationScripts);
        }

        public DbConnection CreateConnection()
        {
            var connection = new SqlCeConnection(ConnectionString);
            connection.Open();
            _managedConnections.Add(connection);
            return connection;
        }

        private void RunScripts(IEnumerable<string> scripts)
        {
            if (scripts == null || !scripts.Any())
                return;
            using (var disposer = new AutoDisposer())
            {
                var conn = disposer.Add(new SqlCeConnection(this.ConnectionString));
                conn.Open();
                var cmd = disposer.Add(conn.CreateCommand());
                Action<string> exec = s => {
                                      cmd.CommandText = s;
                                      cmd.ExecuteNonQuery();
                                  };
                foreach (var script in scripts)
                    exec(script);
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                try
                {
                    File.Delete(DatabaseFile);
                    DatabaseFile = null;
                }
                catch
                {
                }
                foreach (var conn in _managedConnections)
                {
                    try { conn.Dispose(); }
                    catch { };
                }
            }
        }
    }
}
