using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    public abstract class TempDB<TDatabaseConnection> : IDisposable where TDatabaseConnection: DbConnection
    {
        public string DatabaseFile { get; private set; }
        public string ConnectionString { get; private set; }
        private static Semaphore _lock = new Semaphore(1, 1);
        private List<DbConnection> _managedConnections;

        public TempDB(params string[] creationScripts)
        {
            using (new AutoLocker(_lock))
            {
                do
                {
                    DatabaseFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".db");
                } while (File.Exists(DatabaseFile));
                ConnectionString = String.Format("DataSource=\"{0}\";", DatabaseFile);
                CreateDatabase();
            }
            _managedConnections = new List<DbConnection>();
            RunScripts(creationScripts);
        }

        protected abstract void CreateDatabase();

        public DbConnection CreateConnection()
        {
            var connection = CreateOpenDatabaseConnection();
            _managedConnections.Add(connection);
            return connection;
        }

        private DbConnection CreateOpenDatabaseConnection()
        {
            var connection = Activator.CreateInstance(typeof (TDatabaseConnection), ConnectionString) as DbConnection;
            connection.Open();
            return connection;
        }

        private void RunScripts(IEnumerable<string> scripts)
        {
            if (scripts == null || !scripts.Any())
                return;
            using (var disposer = new AutoDisposer())
            {
                var conn = CreateOpenDatabaseConnection();
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
                    System.Diagnostics.Trace.WriteLine(string.Join("", new[]
                    {
                        "WARNING: Unable to delete temporary database at: ",
                        DatabaseFile,
                        "; probably still locked. Artifact will remain on disk."
                    }));
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