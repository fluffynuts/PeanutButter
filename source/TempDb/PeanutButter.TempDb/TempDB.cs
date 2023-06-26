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
    public delegate void TempDbDisposedEventHandler(object sender, TempDbDisposedEventArgs args);

    // ReSharper disable once InconsistentNaming
    public interface ITempDB : IDisposable
    {
        /// <summary>
        /// Fired when the instance is disposed. Useful if you've set up automatic
        /// disposal and would like to act on that.
        /// </summary>
        TempDbDisposedEventHandler Disposed { get; set; }

        /// <summary>
        /// Path to the database. For single-file databases, this will be a file
        /// path. For multi-file databases like MySql, this will be a folder containing
        /// all the files for that database.
        /// </summary>
        string DatabasePath { get; }

        /// <summary>
        /// 
        /// </summary>
        string ConnectionString { get; }

        [Obsolete("Please use OpenConnection. CreateConnection will be removed in a future release.")]
        DbConnection CreateConnection();

        /// <summary>
        /// Opens a new connection to the TempDb instance with the connection string
        /// automatically set based on the current running parameters
        /// </summary>
        /// <returns></returns>
        DbConnection OpenConnection();

        /// <summary>
        /// (Where supported) dumps the current schema of the running database
        /// Currently only supported on mysql, when mysqldump is available
        /// </summary>
        /// <returns></returns>
        string DumpSchema();

        /// <summary>
        /// Set up automatic disposal of this TempDb instance (may only be done once per instance)
        /// </summary>
        /// <param name="absoluteTimeout">Absolute timeout after which this instance is automatically disposed irrespective of ongoing connections</param>
        /// <exception cref="InvalidOperationException">Will be thrown if this method is invoked more than once per instance</exception>
        void SetupAutoDispose(
            TimeSpan absoluteTimeout
        );

        /// <summary>
        /// Set up automatic disposal of this TempDb instance (may only be done once per instance)
        /// </summary>
        /// <param name="inactivityTimeout">Inactivity timeout (only supported on mysql so far)</param>
        /// <param name="absoluteTimeout">Absolute timeout after which this instance is automatically disposed irrespective of ongoing connections</param>
        /// <exception cref="InvalidOperationException">Will be thrown if this method is invoked more than once per instance</exception>
        void SetupAutoDispose(
            TimeSpan? absoluteTimeout,
            TimeSpan? inactivityTimeout);
    }

    public abstract class TempDB<TDatabaseConnection> : ITempDB where TDatabaseConnection : DbConnection
    {
        public uint DefaultTimeout { get; set; } = 30;
        public TempDbDisposedEventHandler Disposed { get; set; }
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

        public Action<string> LogAction { get; set; }

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

        protected abstract int FetchCurrentConnectionCount();

        public void SetupAutoDispose(
            TimeSpan absoluteTimeout
        )
        {
            SetupAutoDispose(absoluteTimeout, null);
        }

        public void SetupAutoDispose(
            TimeSpan? absoluteTimeout,
            TimeSpan? inactivityTimeout)
        {
            if (_inactivityWatcherThread is not null)
            {
                throw new InvalidOperationException(
                    $"Automatic disposal has already been set up for this TempDb instance"
                );
            }

            switch (inactivityTimeout)
            {
                case null when absoluteTimeout is null:
                    return;
                case null:
                    inactivityTimeout = absoluteTimeout.Value;
                    break;
            }

            _timeout = inactivityTimeout.Value;
            _eol = DateTime.Now.Add(_timeout);
            if (absoluteTimeout.HasValue)
            {
                _absoluteEol = DateTime.Now.Add(absoluteTimeout.Value);
                _absoluteLifespan = absoluteTimeout.Value;
            }
            else
            {
                _absoluteEol = DateTime.MaxValue;
            }

            Log($@"setting up inactivity watcher thread for absolute lifespan {
                _absoluteLifespan
            } and absolute EOL {
                _absoluteEol
            }");
            _inactivityWatcherThread = new Thread(CheckForInactivity);
            _inactivityWatcherThread.Start();
            _autoDisposeThread = null;
        }

        protected void CheckForInactivity(object _)
        {
            try
            {
                while (!_disposed)
                {
                    try
                    {
                        var connectionCount = TryFetchCurrentConnectionCount();
                        var now = DateTime.Now;
                        if (connectionCount != 0)
                        {
                            _eol = now.Add(_timeout);
                        }

                        var eolExceeded = now > _eol;
                        var absoluteLifespanExceeded = now > _absoluteEol;

                        if (eolExceeded || absoluteLifespanExceeded)
                        {
                            var message = absoluteLifespanExceeded
                                ? $"absolute lifespan of {_absoluteLifespan} exceeded; shutting down"
                                : $"inactivity timeout of {_timeout} exceeded; shutting down";
                            Log(message);

                            _autoDisposeInformation = new TempDbDisposedEventArgs(
                                message,
                                true,
                                _timeout,
                                _absoluteLifespan
                            );
                            _autoDisposeThread = new Thread(Dispose);
                            _autoDisposeThread.Start();
                            return;
                        }
                    }
                    catch
                    {
                        // Suppress; this is a background thread; nothing we can really do about it anyway
                    }

                    Thread.Sleep(100);
                }
            }
            catch (ThreadAbortException)
            {
                // just exit cleanly
            }
        }

        /// <summary>
        /// Provides a convenience logging mechanism which outputs via
        /// the established LogAction
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        protected void Log(string message, params object[] parameters)
        {
            var logAction = LogAction;
            if (logAction == null)
            {
                return;
            }

            try
            {
                logAction(string.Format(message, parameters));
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        private Thread _inactivityWatcherThread;
        private DateTime _eol;
        private TimeSpan _timeout;
        private Thread _autoDisposeThread;
        private DateTime _absoluteEol;
        private TimeSpan _absoluteLifespan;
        private bool _haveReportedIdleTimeoutNotSupported;

        public int TryFetchCurrentConnectionCount()
        {
            try
            {
                return FetchCurrentConnectionCount();
            }
            catch (Exception ex)
            {
                if (ex is NotImplementedException &&
                    !_haveReportedIdleTimeoutNotSupported)
                {
                    _haveReportedIdleTimeoutNotSupported = true;
                    Log(ex.Message);
                }
                else
                {
                    Log(
                        $"Error whilst trying to retrieve active database connection count: {ex.Message}\n{ex.StackTrace}");
                }

                return -1;
            }
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
                    if (string.IsNullOrWhiteSpace(script))
                    {
                        continue;
                    }

                    try
                    {
                        Exec(script);
                    }
                    catch (Exception ex)
                    {
                        var foo = script;
                        throw;
                    }
                }
            }
        }

        private readonly object _disposeLock = new object();

        public virtual void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
            }

            if (_inactivityWatcherThread is not null)
            {
                Log("Waiting on inactivity watcher thread to complete");
                if (!(_inactivityWatcherThread?.Join(10000) ?? true))
                {
                    _inactivityWatcherThread.Abort();
                }

                _inactivityWatcherThread = null;
            }

            DisposeOfManagedConnections();
            DeleteTemporaryDataArtifacts();
            var handlers = Disposed;
            try
            {
                handlers?.Invoke(this, _autoDisposeInformation ??
                    new TempDbDisposedEventArgs(
                        "TempDb instance was disposed",
                        false,
                        _timeout,
                        _absoluteLifespan
                    ));
            }
            catch
            {
                // suppress
            }
        }

        private bool _disposed;
        private TempDbDisposedEventArgs _autoDisposeInformation;

        protected virtual void DeleteTemporaryDataArtifacts()
        {
            if (KeepTemporaryDatabaseArtifactsForDiagnostics)
            {
                return;
            }

            Log("Deleting temporary database artifacts");
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
            Log("Disposing of managed connections");
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