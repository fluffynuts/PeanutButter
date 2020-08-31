using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using PeanutButter.Utils;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable PublicConstructorInAbstractClass

namespace PeanutButter.TempDb
{
    // ReSharper disable once InconsistentNaming
    public interface ITempDB : IDisposable
    {
        EventHandler Disposed { get; set; }
        string DatabasePath { get; }
        string ConnectionString { get; }

        [Obsolete("Please use OpenConnection. CreateConnection will be removed in a future release.")]
        DbConnection CreateConnection();

        DbConnection OpenConnection();

        string DumpSchema();
    }

    public abstract class TempDB<TDatabaseConnection> : ITempDB where TDatabaseConnection : DbConnection
    {
        public uint DefaultTimeout { get; set; } = 30;
        public EventHandler Disposed { get; set; }
        public bool KeepTemporaryDatabaseArtifactsForDiagnostics { get; set; }

        /// <summary>
        /// Path to where the temporary database resides. May be a file
        /// for single-file databases or a folder.
        /// </summary>
        public string DatabasePath { get; private set; }

        public string ConnectionString => GenerateConnectionString();

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Semaphore _lock = new Semaphore(1, 1);
        private readonly List<DbConnection> _managedConnections = new List<DbConnection>();

        public TempDB(params string[] creationScripts)
        {
            Init(creationScripts);
        }

        public TempDB(Action<object> beforeInit, params string[] creationScripts)
        {
            beforeInit(this);
            Init(creationScripts);
        }

        public abstract string DumpSchema();

        protected virtual void Init(string[] creationScripts)
        {
            try
            {
                if (!Directory.Exists(TempDbHints.PreferredBasePath))
                {
                    Directory.CreateDirectory(TempDbHints.PreferredBasePath);
                }

                AttemptToCreateDatabaseWith(TempDbHints.PreferredBasePath);
            }
            catch (FatalTempDbInitializationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (TempDbHints.UsingOverrideBasePath)
                {
                    Trace(
                        "An error was encountered whilst attempting to use the configured TempDbHints.PreferredBasePath: " +
                        ex.Message);
                    Trace(" -> falling back on using %TEMP%");
                    AttemptToCreateDatabaseWith(Path.GetTempPath());
                    TempDbHints.PreferredBasePath = TempDbHints.DefaultBasePath;
                }
                else
                {
                    throw;
                }
            }

            RunScripts(creationScripts);
        }

        private void AttemptToCreateDatabaseWith(string basePath)
        {
            using (new AutoLocker(_lock))
            {
                do
                {
                    DatabasePath = Path.Combine(basePath, $"{Guid.NewGuid()}.db");
                } while (File.Exists(DatabasePath) || Directory.Exists(DatabasePath));

                CreateDatabase();
            }
        }

        protected virtual string GenerateConnectionString()
        {
            return $"DataSource=\"{DatabasePath}\"";
        }

        protected abstract void CreateDatabase();

        [Obsolete("Please use 'OpenConnection' instead")]
        public DbConnection CreateConnection()
        {
            return OpenConnection();
        }

        public DbConnection OpenConnection()
        {
            var connection = CreateOpenDatabaseConnection();
            _managedConnections.Add(connection);
            return connection;
        }

        private DbConnection CreateOpenDatabaseConnection()
        {
            var connection = Activator.CreateInstance(
                typeof(TDatabaseConnection),
                ConnectionString
            ) as DbConnection;

            if (connection == null)
                throw new InvalidOperationException(
                    $"Unable to instantiate connection of type {typeof(TDatabaseConnection)} as a DbConnection"
                );

            connection.Open();
            return connection;
        }

        public void RunScripts(IEnumerable<string> scripts)
        {
            if (scripts == null || !scripts.Any())
            {
                return;
            }

            using (var disposer = new AutoDisposer())
            {
                var conn = disposer.Add(CreateOpenDatabaseConnection());
                var cmd = disposer.Add(conn.CreateCommand());

                void Exec(string s)
                {
                    cmd.CommandText = s;
                    cmd.ExecuteNonQuery();
                }

                foreach (var script in scripts)
                {
                    Exec(script);
                }
            }
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                DisposeOfManagedConnections();
                DeleteTemporaryDataArtifacts();
                var handlers = Disposed;
                try
                {
                    handlers.Invoke(this, new EventArgs());
                }
                catch
                {
                    // suppress
                }
            }
        }

        protected virtual void DeleteTemporaryDataArtifacts()
        {
            if (KeepTemporaryDatabaseArtifactsForDiagnostics)
            {
                return;
            }

            try
            {
                if (Directory.Exists(DatabasePath))
                {
                    Directory.Delete(DatabasePath, true);
                }
                else if (File.Exists(DatabasePath))
                {
                    File.Delete(DatabasePath);
                }

                DatabasePath = null;
            }
            catch
            {
                Trace(
                    $"WARNING: Unable to delete temporary database at: {DatabasePath}; probably still locked. Artifact will remain on disk."
                );
            }
        }

        private static void Trace(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        private void DisposeOfManagedConnections()
        {
            foreach (var conn in _managedConnections)
            {
                var localConnection = conn;
                TryDo("disposing of managed connections", localConnection.Dispose);
            }

            void TryDo(string heading, Action toRun)
            {
                try
                {
                    toRun();
                }
                catch (Exception ex)
                {
                    Trace("Error whilst: " + heading);
                    Trace(ex.Message);
                }
            }
        }
    }
}