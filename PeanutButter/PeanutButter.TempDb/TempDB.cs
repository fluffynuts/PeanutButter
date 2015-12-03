using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using PeanutButter.Utils;

namespace PeanutButter.TempDb
{
    public abstract class TempDB<TDatabaseConnection> : IDisposable where TDatabaseConnection: DbConnection
    {
        public string DatabaseFile { get; private set; }
        public string ConnectionString { get; protected set; }
        private static Semaphore _lock = new Semaphore(1, 1);
        private List<DbConnection> _managedConnections;

        public TempDB(params string[] creationScripts)
        {
            Init(creationScripts);
        }

        public TempDB(Action<object> beforeInit, params string[] creationScripts)
        {
            beforeInit(this);
            Init(creationScripts);
        }

        protected virtual void Init(string[] creationScripts)
        {
            using (new AutoLocker(_lock))
            {
                do
                {
                    DatabaseFile = Path.Combine(TempDbHints.PreferredBasePath, Guid.NewGuid().ToString() + ".db");
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
                var conn = disposer.Add(CreateOpenDatabaseConnection());
                var cmd = disposer.Add(conn.CreateCommand());
                Action<string> exec = s => {
                                               cmd.CommandText = s;
                                               cmd.ExecuteNonQuery();
                };
                foreach (var script in scripts)
                    exec(script);
            }
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                DisposeOfManagedConnections();
                DeleteTemporaryDatabaseFile();
            }
        }

        protected virtual void DeleteTemporaryDatabaseFile()
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
        }

        private void Trace(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        private void DisposeOfManagedConnections()
        {
            Action<string, Action> TryDo = (heading, toRun) =>
            {
                try { toRun(); }
                catch (Exception ex) 
                {
                    Trace("Error whilst: " + heading);
                    Trace(ex.Message); 
                };
            };
            foreach (var conn in _managedConnections)
            {
                var localConnection = conn;
                TryDo("disposing of managed connections", localConnection.Dispose);
            }
        }
    }
}