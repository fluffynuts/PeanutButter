﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using PeanutButter.FileSystem;
using PeanutButter.Utils;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace PeanutButter.TempDb.MySql.Base
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// MySql flavor of TempDb
    /// </summary>
    public abstract class TempDBMySqlBase<T> : TempDB<T> where T : DbConnection
    {
        public string ServerCommandline =>
            _serverProcess?.Commandline;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        /// The maximum amount of time to wait for a mysqld process to be
        /// listening for connections after starting up. Defaults to 45 seconds,
        /// but left static so consumers can tweak the value as required.
        /// </summary>
        public static int DefaultStartupMaxWaitSeconds = 45;

        // ReSharper disable once StaticMemberInGenericType
        /// <summary>
        /// Maximum number of times that TempDBMySql will automatically
        /// re-attempt initial start. This may be useful in the case where
        /// MySql isn't starting up but also isn't logging anything particularly
        /// useful about _why_ it isn't starting up. Defaults to 5, but left
        /// as a static so consumers can tweak the value as required.
        /// </summary>
        public static int MaxStartupAttempts = 5;

        public static int MaxSecondsToWaitForMySqlToStart =>
            DetermineMaxSecondsToWaitForMySqlToStart();

        /// <summary>
        /// After the server has been set up and started, this will reflect
        /// the absolute path to the configuration file for the server
        /// </summary>
        public string ConfigFilePath { get; set; }

        private static int DetermineMaxSecondsToWaitForMySqlToStart()
        {
            var env = Environment.GetEnvironmentVariable("MYSQL_MAX_STARTUP_TIME_IN_SECONDS");
            if (env is null || !int.TryParse(env, out var value))
            {
                return DefaultStartupMaxWaitSeconds;
            }

            return value;
        }

        public const int PROCESS_POLL_INTERVAL = 100;

        public bool VerboseLoggingEnabled
        {
            get
            {
                return _verboseLoggingEnabled ??= DetermineIfVerboseLoggingShouldBeEnabled();
            }
            set
            {
                _verboseLoggingEnabled = value;
            }
        }

        private bool? _verboseLoggingEnabled;

        private string[] VerboseLoggingCommandLineArgs =>
            VerboseLoggingEnabled
                ? new[]
                {
                    "--log-error-verbosity=3"
                }
                : new string[0];

        private bool DetermineIfVerboseLoggingShouldBeEnabled()
        {
            var envValue = Environment.GetEnvironmentVariable("TEMPDB_VERBOSE");
            if (envValue is null)
            {
                return Settings?.Options?.EnableVerboseLogging ?? false;
            }

            return envValue.AsBoolean();
        }

        private bool ShouldAttemptGracefulShutdown
        {
            get
            {
                var envValue = Environment.GetEnvironmentVariable("TEMPDB_GRACEFUL_SHUTDOWN");
                if (envValue is null)
                {
                    return Settings?.Options?.AttemptGracefulShutdown ?? true;
                }

                return envValue.AsBoolean();
            }
        }

        private readonly object _debugLogLock = new object();

        private void DebugLog(string toLog)
        {
            if (!VerboseLoggingEnabled)
            {
                return;
            }

            lock (_debugLogLock)
            {
                File.AppendAllLines(
                    _debugLogFile,
                    new[]
                    {
                        toLog
                    }
                );
            }
        }

        /// <summary>
        /// Set to true to see trace logging about discovery of a port to
        /// instruct mysqld to bind to
        /// </summary>
        public TempDbMySqlServerSettings Settings { get; private set; }

        public int? ServerProcessId => _serverProcess?.ProcessId;

        private IProcessIO _serverProcess;
        public int Port { get; protected set; }
        public bool RootPasswordSet { get; private set; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly SemaphoreSlim MysqldLock = new(1, 1);
        private static readonly SemaphoreSlim InstanceLock = new(1, 1);

        // ReSharper disable once StaticMemberInGenericType
        private static string _mysqld;

        public Guid InstanceId =>
            TryDetermineInstanceId();

        private Guid TryDetermineInstanceId()
        {
            return ParseFirstGuidFromPath(DatabasePath);
        }

        private Guid ParseFirstGuidFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Guid.Empty;
            }

            return InstanceIdCache.FindOrAdd(
                path,
                () =>
                {
                    var parts = path.Split(
                        new[]
                        {
                            "\\",
                            "/"
                        },
                        StringSplitOptions.RemoveEmptyEntries
                    );
                    foreach (var part in parts)
                    {
                        var match = GuidMatcher.Match(part);
                        if (match.Success)
                        {
                            return Guid.Parse(match.Groups["guid"].Value);
                        }
                    }

                    throw new Exception(
                        $"Expected DatabasePath '{DatabasePath}' to contain a guid, somewhere."
                    );
                }
            );
        }

        private static ConcurrentDictionary<string, Guid> InstanceIdCache = new();

        private static readonly Regex GuidMatcher = new(
            "(?<guid>[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12})"
        );

        private int _conflictingPortRetries = 0;

#if NETSTANDARD
        private readonly FatalTempDbInitializationException _noMySqlFoundException =
            new FatalTempDbInitializationException(
                "Unable to detect an installed mysqld. Either supply a path as part of your initializing parameters or ensure that mysqld is in your PATH"
            );
#else
        private readonly FatalTempDbInitializationException _noMySqlFoundException =
            new FatalTempDbInitializationException(
                "Unable to detect an installed mysqld. Either supply a path as part of your initializing parameters or ensure that mysqld is in your PATH or install as a windows service"
            );
#endif

        protected string SchemaName;
        private AutoDeleter _autoDeleter;

        private string _debugLogFile = Path.GetTempFileName();

        /// <summary>
        /// Construct a TempDbMySql with zero or more creation scripts and default options
        /// </summary>
        /// <param name="creationScripts"></param>
        public TempDBMySqlBase(params string[] creationScripts)
            : this(new TempDbMySqlServerSettings(), creationScripts)
        {
        }

        /// <summary>
        /// Create a TempDbMySql instance with provided options and zero or more creation scripts
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="creationScripts"></param>
        public TempDBMySqlBase(
            TempDbMySqlServerSettings settings,
            params string[] creationScripts
        )
            : this(
                settings,
                o =>
                {
                },
                creationScripts
            )
        {
        }

        /// <summary>
        /// Create a TempDbMySql instance with provided options, an action to run before initializing and
        /// zero or more creation scripts
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="beforeInit"></param>
        /// <param name="creationScripts"></param>
        public TempDBMySqlBase(
            TempDbMySqlServerSettings settings,
            Action<object> beforeInit,
            params string[] creationScripts
        ) : base(
            o => BeforeInit(o as TempDBMySqlBase<T>, beforeInit, settings),
            creationScripts
        )
        {
        }

        protected static void BeforeInit(
            TempDBMySqlBase<T> self,
            Action<object> beforeInit,
            TempDbMySqlServerSettings settings
        )
        {
            self.LogAction = WrapWithDebugLogger(self, settings?.Options?.LogAction);
            self._autoDeleter = new AutoDeleter();
            self.Settings = settings;

            beforeInit?.Invoke(self);
        }

        private static bool FolderExists(string path)
        {
            return path is not null &&
                Directory.Exists(path);
        }

        private static void EnsureFolderDoesNotExist(
            string path
        )
        {
            var container = Path.GetDirectoryName(path);
            var name = Path.GetFileName(path);
            var fs = new LocalFileSystem(container);
            fs.DeleteRecursive(name);
        }

        private static Action<string> WrapWithDebugLogger(
            TempDBMySqlBase<T> tempDb,
            Action<string> optionsLogAction
        )
        {
            if (optionsLogAction is null)
            {
                return tempDb.DebugLog;
            }

            return logString =>
            {
                tempDb.DebugLog(logString);
                optionsLogAction.Invoke(logString);
            };
        }

        public string Snapshot()
        {
            return Snapshot(null);
        }

        public string Snapshot(string toNewFolder)
        {
            SetDbInstanceId("");
            DisposeManagedConnections();
            Stop();
            var baseDir = Path.GetDirectoryName(DatabasePath);
            var result = toNewFolder ?? GenerateTemplatePath();
            var fs = new LocalFileSystem(baseDir);
            EnsureFolderDoesNotExist(result);
            fs.Copy(DatabasePath, result);
            foreach (var f in DeleteDataFilesOnSnapshot)
            {
                var toDelete = Path.Combine(result, "data", f);
                if (File.Exists(toDelete))
                {
                    File.Delete(toDelete);
                }
            }

            // restarting the server after having wiped the instance
            // id should cause the instance id to be automagically rewritten
            StartServer(MySqld, null);
            return result;
        }

        private string GenerateTemplatePath()
        {
            return Path.Combine(
                Path.GetDirectoryName(DatabasePath)!,
                $"template-{InstanceId}-{CurrentTimestamp()}.db"
            );
        }

        private string CurrentTimestamp()
        {
            return DateTime.Now.ToString(
                "yyyyMMddHHmmssfff"
            );
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly string[] DeleteDataFilesOnSnapshot =
        {
            "mysql-err.log",
            "start-upinfo.log",
            "tempdb-debug.log"
        };

        private string FindInstalledMySqlD()
        {
            using (new AutoLocker(MysqldLock))
            {
                return _mysqld ??= QueryForMySqld();
            }
        }

        private string QueryForMySqld()
        {
            Log("Looking for mysqld in the PATH...");
            var mysqld = Find.InPath("mysqld");
            var shouldFindInPath = Platform.IsUnixy || Settings.Options.ForceFindMySqlInPath;
            if (shouldFindInPath && mysqld is not null)
            {
                Log($"Found mysqld in the PATH: {mysqld}");
                return mysqld;
            }

            if (!Platform.IsWindows)
            {
                Log("mysqld discovery on non-windows platforms is limited to finding within the PATH");
                throw _noMySqlFoundException;
            }

            var servicePath = MySqlWindowsServiceFinder.FindPathToMySqlD();
            // prefer the pathed mysqld, but fall back on service path if available
            var resolved = mysqld ?? servicePath;
            Log(
                $@"mysqld from path: {
                    mysqld ?? "(not found)"
                }; mysqld from service finder: {
                    servicePath ?? "(not found)"
                }"
            );
            return resolved ?? throw _noMySqlFoundException;
        }

        private string MySqld =>
            _mysqld ??= Settings?.Options?.PathToMySqlD ?? FindInstalledMySqlD();

        /// <inheritdoc />
        protected override void CreateDatabase()
        {
            MySqlVersion = QueryVersionOf(MySqld);
            Log($"mysql is version: {MySqlVersion}");
            using (var _ = new AutoLocker(InstanceLock))
            {
                if (IsMySql8())
                {
                    RemoveDeprecatedOptions();
                }

                Guid? assimilatedInstanceId = null;
                if (FolderExists(Settings?.Options?.TemplateDatabasePath))
                {
                    RootPasswordSet = true; // prior snapshot should have set the root password
                    var container = Path.GetDirectoryName(Settings!.Options!.TemplateDatabasePath);
                    EnsureFolderDoesNotExist(DatabasePath);
                    var fs = new LocalFileSystem(
                        container
                    );
                    var src = Path.GetFileName(Settings!.Options!.TemplateDatabasePath);
                    fs.Copy(src, DatabasePath);

                    Log($"Re-using data at {DatabasePath}");
                }
                else
                {
                    Log("create initial database");
                    _startAttempts = 0;
                    EnsureIsRemoved(DatabasePath);

                    // mysql 5.7 wants an empty data base dir, so we have to use
                    // a temp defaults file for init only, which 8 seems to be ok with
                    using var tempFolder = new AutoTempFolder(
                        Environment.GetEnvironmentVariable("TEMPDB_BASE_PATH")
                    );
                    Log($"temp folder created at {tempFolder}");
                    Log($"dumping defaults in temp folder {tempFolder.Path} for initialization");
                    DumpDefaultsFileAt(tempFolder.Path);
                    InitializeWith(MySqld, Path.Combine(tempFolder.Path, "my.cnf"));
                }

                RedirectDebugLogging();

                // now we need the real config file, sitting in the db dir
                Log($"dumping run-time defaults file into {DatabasePath}");
                DumpDefaultsFileAt(DatabasePath);
                ConfigFilePath = Path.Combine(DatabasePath, MYSQL_CONFIG_FILE);
                Port = DeterminePortToUse();
                StartServer(MySqld, assimilatedInstanceId);
                SetRootPassword();
                CreateInitialSchema();
                SetUpAutoDisposeIfRequired();
            }
        }


        private void RedirectDebugLogging()
        {
            lock (_debugLogLock)
            {
                var existingFile = _debugLogFile;
                if (!File.Exists(existingFile))
                {
                    return;
                }

                _debugLogFile = DataFilePath("tempdb-debug.log");
                if (File.Exists(_debugLogFile))
                {
                    // if we're working from a template,
                    // the file will already exist; if
                    // not, it doesn't matter
                    File.Delete(_debugLogFile);
                }

                var targetFolder = Path.GetDirectoryName(_debugLogFile);
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                File.Move(existingFile, _debugLogFile);
            }
        }

        private void SetUpAutoDisposeIfRequired()
        {
            var timeout = Settings?.Options?.InactivityTimeout;
            var absoluteLifespan = Settings?.Options?.AbsoluteLifespan;
            SetupAutoDispose(absoluteLifespan, timeout);
        }


        private void RemoveDeprecatedOptions()
        {
            Log("Removing NO_AUTO_CREATE_USER option (deprecated in mysql 8+)");
            Settings.SqlMode = (Settings.SqlMode ?? "").Split(',')
                .Where(p => p != "NO_AUTO_CREATE_USER")
                .JoinWith(",");
        }

        private void SetRootPassword()
        {
            Log("setting root password (default is 'root', actual password not output in case it's sensitive)");
            using var conn = base.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $@"alter user 'root'@'localhost' identified with mysql_native_password by '{
                    Settings.Options.RootUserPassword.Replace("'", "''")
                }';";
            cmd.ExecuteNonQuery();
            RootPasswordSet = true;
        }

        public void CloseAllConnections()
        {
            using var conn = base.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "show processlist";
            using var reader = cmd.ExecuteReader();
            var pids = new List<string>();
            while (reader.NextResult())
            {
                var pid = reader["Id"].ToString();
                pids.Add(pid);
            }

            foreach (var pid in pids)
            {
                cmd.CommandText = $"kill {pid}";
                TryDo(() => cmd.ExecuteNonQuery());
            }
        }

        public override string DumpSchema()
        {
            // TODO: rather query INFORMATION_SCHEMA and do the work internally
            var mysqldHome = Path.GetDirectoryName(MySqld);
            // ReSharper disable once AssignNullToNotNullAttribute
            var mysqldump = Path.Combine(mysqldHome, "mysqldump");
            if (Platform.IsWindows)
            {
                mysqldump += ".exe";
            }

            if (!File.Exists(mysqldump))
            {
                mysqldump = Find.InPath("mysqldump");
                if (mysqldump is null)
                {
                    throw new InvalidOperationException(
                        $"Unable to find mysqldump at {mysqldump}"
                    );
                }
            }

            using var io = ProcessIO.Start(
                mysqldump,
                "-u",
                "root",
                $"--password={Settings.Options.RootUserPassword}",
                "-h",
                "localhost",
                "-P",
                Port.ToString(),
                "--protocol",
                "TCP",
                SchemaName
            );
            io.WaitForExit();
            return string.Join("\n", io.StandardOutput);
        }

        private void CreateInitialSchema()
        {
            var schema = Settings?.Options.DefaultSchema ?? "tempdb";
            Log($"create initial schema: {schema}");
            SwitchToSchema(schema);
        }

        /// <summary>
        /// Switches to the provided schema name for connections from now on.
        /// Creates the schema if not already present.
        /// </summary>
        /// <param name="schema"></param>
        public void SwitchToSchema(string schema)
        {
            // last-recorded schema may no longer exist; so go schemaless for this connection
            SchemaName = "";
            if (string.IsNullOrWhiteSpace(schema))
            {
                Log("empty schema provided; not switching!");
                return;
            }

            CreateSchemaIfNotExists(schema);
            Log($"Attempting to switch to schema {schema} with connection string: {ConnectionString}");
            Execute($"use `{Escape(schema)}`");

            SchemaName = schema;
        }

        public void CreateSchemaIfNotExists(string schema)
        {
            Execute($"create schema if not exists `{Escape(schema)}`");
        }

        public void CreateUser(
            string user,
            string password,
            params string[] forSchemas
        )
        {
            Execute(
                $"create user {Quote(user)}@'%' identified with mysql_native_password by {Quote(password)}"
            );
            forSchemas.ForEach(
                schema =>
                {
                    GrantAllPermissionsFor(user, schema, "%");
                }
            );
        }

        public void GrantAllPermissionsFor(
            string user,
            string schema,
            string host
        )
        {
            Execute($"grant all privileges on {Escape(schema)}.* to {Quote(user)}@{Quote(host)}");
        }


        /// <summary>
        /// Escapes back-ticks in mysql sql strings
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public string Escape(string other)
        {
            return $"{other?.Replace("`", "``")}";
        }

        public string Quote(string other)
        {
            return $"'{Escape(other)}'";
        }

        /// <summary>
        /// Executes arbitrary sql on the current schema
        /// </summary>
        /// <param name="sql"></param>
        public void Execute(string sql)
        {
            Log($"executing: {sql}");
            using var connection = base.OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        private void EnsureIsRemoved(string databasePath)
        {
            Log($"Ensuring {databasePath} does not already exist");
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }

            if (Directory.Exists(databasePath))
            {
                Directory.Delete(databasePath, true);
            }
        }

        private const string MYSQL_CONFIG_FILE = "my.cnf";

        private string DumpDefaultsFileAt(
            string databasePath,
            string configFileName = MYSQL_CONFIG_FILE
        )
        {
            var generator = new MySqlConfigGenerator();
            var outputFile = Path.Combine(databasePath, configFileName);
            var containingFolder = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(containingFolder))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                Directory.CreateDirectory(containingFolder);
            }

            File.WriteAllBytes(
                outputFile,
                Encoding.UTF8.GetBytes(
                    generator.GenerateFor(Settings)
                )
            );
            Log($"Dumped defaults at {outputFile}");
            return outputFile;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            try
            {
                using (new AutoLocker(InstanceLock))
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _disposed = true;
                }

                try
                {
                    Stop();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"Unable to properly stop mysqld: {ex.Message}"
                    );
                }

                base.Dispose();
                _autoDeleter?.Dispose();
                _autoDeleter = null;
            }
            catch (Exception ex)
            {
                Log($"Unable to kill MySql instance {_serverProcess?.ProcessId}: {ex.Message}");
            }
        }

        private void Stop()
        {
            using (new AutoLocker(InstanceLock))
            {
                StopUnlocked();
            }
        }

        private void StopUnlocked()
        {
            StopWatcher();
            AttemptGracefulShutdown();
            ForceEndServerProcess();
            TryDo(() => File.Delete(Settings.Socket));
        }

        private void TryDo(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch
            {
                // suppress
            }
        }

        private void StopWatcher()
        {
            _running = false;
            _processWatcherThread?.Join();
            _processWatcherThread = null;
        }

        private void ForceEndServerProcess()
        {
            try
            {
                var proc = _serverProcess;
                _serverProcess = null;
                if (proc is null || proc.HasExited)
                {
                    return;
                }

                if (Platform.IsWindows)
                {
                    Console.Error.WriteLine(
                        $"WARNING: killing mysqld with process id {proc.ProcessId} - this may leave temporary files on your filesystem"
                    );
                }
                else
                {
                    Log($"Killing mysqld process {proc.ProcessId}");
                }

                proc.Kill();
                proc.WaitForExit(3000);

                if (!proc.HasExited)
                {
                    throw new Exception($"MySql process {proc.ProcessId} has not shut down!");
                }
            }
            catch (Exception ex)
            {
                var foo = ex;
                throw;
            }
        }

        private void AttemptGracefulShutdown()
        {
            if (
                _serverProcess is null
                || !ShouldAttemptGracefulShutdown
            )
            {
                return;
            }

            Log("Attempting graceful shutdown of mysql server");
            try
            {
                SwitchToSchema("mysql");
                KillAllActiveConnections();
                Execute("SHUTDOWN");
                var timeout = DateTime.Now.AddSeconds(5);
                while (DateTime.Now < timeout)
                {
                    if (!IsRunning)
                    {
                        return;
                    }

                    Thread.Sleep(100);
                }

                Log($"Unable to perform graceful shutdown: mysqld remains alive after issuing SHUTDOWN and waiting");
            }
            catch (Exception ex)
            {
                Log($"Unable to perform graceful shutdown: {ex.Message}");
            }
        }

        private void KillAllActiveConnections()
        {
            using var conn = base.OpenConnection();
            using var cmd = conn.CreateCommand();
            var toKill = new List<ulong>();
            cmd.CommandText = "select connection_id();";
            ulong myId;
            using (var reader = cmd.ExecuteReader())
            {
                reader.Read();
                myId = ulong.Parse($"{reader[0]}");
            }

            cmd.CommandText = "show processlist";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = ulong.Parse($"{reader["Id"]}");
                    if (id != myId)
                    {
                        toKill.Add(id);
                    }
                }
            }

            foreach (var id in toKill)
            {
                cmd.CommandText = $"kill {id}";
                // ReSharper disable once AccessToDisposedClosure
                TryDo(() => cmd!.ExecuteNonQuery());
            }
        }

        private int _startAttempts;

        private void PauseWatcher()
        {
            _watcherPaused = true;
        }

        private void ResumeWatcher()
        {
            _watcherPaused = false;
        }

        private bool _watcherPaused = false;

        private void StartServer(string mysqld, Guid? assimilatedInstanceId)
        {
            PauseWatcher();
            var args = new[]
            {
                $"\"--defaults-file={Path.Combine(DatabasePath, "my.cnf")}\"",
                $"\"--basedir={BaseDirOf(mysqld)}\"",
                $"\"--datadir={DataDir}\"",
                $"--port={Port}"
            };

            if (VerboseLoggingEnabled)
            {
                args = args.And(VerboseLoggingCommandLineArgs);
            }

            if (IsMySql8())
            {
                // shut down as fast as possible
                args = args.And("--innodb-fast-shutdown=2");
                // stay connected on the console
                args = args.And("--console");
            }

            _serverProcess = RunCommand(
                true,
                mysqld,
                args
            );
            try
            {
                TestIsRunning(assimilatedInstanceId);
                Log("MySql appears to be up an running! Setting up an auto-restarter in case it falls over.");
                if (_running)
                {
                    ResumeWatcher();
                }
                else
                {
                    StartProcessWatcher();
                }
            }
            catch (TryAnotherPortException)
            {
                Log($"Looks like a port conflict at {Port}. Will try another port.");
                Retry();
            }
            catch (FatalTempDbInitializationException ex)
            {
                Log($"Fatal TempDb init exception: {ex.Message}");
                // I've seen the mysqld process simply exit early
                // (5.7, win32) without anything interesting in the
                // error log; so let's just give this a few attempts
                // before giving up completely
                if (++_startAttempts >= MaxStartupAttempts)
                {
                    Log($"Giving up: have tried {_startAttempts} and limit is {MaxStartupAttempts}");
                    throw;
                }

                Log("MySql appears to not start up properly; retrying...");
                Retry();
            }

            void Retry()
            {
                if (Settings?.Options?.PortHint.HasValue ?? false)
                {
                    Log($"incrementing port hint to {Settings.Options.PortHint + 1}");
                    Settings.Options.PortHint++;
                }

                Port = DeterminePortToUse();
                ForceEndServerProcess();
                StartServer(mysqld, assimilatedInstanceId);
            }
        }

        public bool IsRunning
        {
            get
            {
                var proc = _serverProcess;
                if (proc is null)
                {
                    return false;
                }

                try
                {
                    return !proc.HasExited;
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Restart()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("This instance has already been disposed");
            }

            Stop();
            // ReSharper disable once ConvertToUsingDeclaration
            using (var _ = new AutoLocker(InstanceLock))
            {
                StartServer(MySqld, null);
            }
        }

        public string DataDir =>
            _dataDir ??= (
                MySqlVersion.Version.Major >= 8
                    ? Path.Combine(DatabasePath, "data") // mysql 8 wants a clean dir to init in
                    : DatabasePath
            );

        private string _dataDir;

        private Thread _processWatcherThread;

        private void StartProcessWatcher()
        {
            _watcherPaused = false;
            _running = true;
            _processWatcherThread?.Abort();
            _processWatcherThread?.Join();
            _processWatcherThread = new Thread(ObserveMySqlProcess);
            _processWatcherThread.Start();
        }

        private bool _disposed;
        private bool _running;

        private void ObserveMySqlProcess()
        {
            while (_running)
            {
                Thread.Sleep(PROCESS_POLL_INTERVAL);
                if (_watcherPaused)
                {
                    continue;
                }

                var serverProcessId = ServerProcessId;
                if (serverProcessId is null)
                {
                    // undefined state -- possibly never was started
                    if (_watcherPaused)
                    {
                        continue; // coming back later perhaps?
                    }

                    break;
                }

                bool shouldResurrect;
                try
                {
                    var process = Process.GetProcessById(
                        serverProcessId.Value
                    );
                    shouldResurrect = process.HasExited;
                }
                catch
                {
                    // unable to query the process
                    shouldResurrect = true;
                }

                var shouldBeRunning = _running && !_watcherPaused;
                if (shouldResurrect && shouldBeRunning)
                {
                    Log($"{MySqld} seems to have gone away; restarting on port {Port}");
                    StartServer(MySqld, null);
                }
            }

            Log("Watcher thread exiting");
        }

        private string BaseDirOf(string mysqld)
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(mysqld));
        }

        public override DbConnection OpenConnection()
        {
            if (!_running)
            {
                throw new InvalidOperationException(
                    $"Cannot open connection to the db: not running right now!"
                );
            }

            return base.OpenConnection();
        }

        protected virtual bool IsMyInstance(Guid? assimilatedInstanceId)
        {
            var existingId = FetchDbInstanceId();
            if (string.IsNullOrWhiteSpace(existingId))
            {
                TrySetDbInstanceId(InstanceId, assimilatedInstanceId);
                return true;
            }

            if (Guid.TryParse(existingId, out var guid))
            {
                return guid == InstanceId;
            }

            throw new InvalidOperationException(
                "Value stored in instance-id.txt file is not a guid"
            );
        }

        private void TrySetDbInstanceId(Guid newId, Guid? assimilatedInstanceId)
        {
            TrySetDbInstanceId(
                $"{newId}",
                assimilatedInstanceId.HasValue
                    ? $"{assimilatedInstanceId}"
                    : null
            );
        }

        private void TrySetDbInstanceId(string newId, string assimilatedId)
        {
            if (newId is null)
            {
                throw new ArgumentException(
                    "Database instance id cannot be null"
                );
            }

            var existingId = FetchDbInstanceId();
            if (!string.IsNullOrWhiteSpace(existingId))
            {
                if (existingId == newId)
                {
                    // already stored the correct id
                    return;
                }


                // if we're assimilating, we can simply overwrite the id
                if (existingId != assimilatedId)
                {
                    throw new InvalidOperationException(
                        $"Database is already assigned instance id: {existingId}"
                    );
                }
            }

            SetDbInstanceId(newId);
        }

        private void SetDbInstanceId(string newId)
        {
            newId = newId.Replace("'", "''");

            using var conn = base.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
use sys;
insert into sys.sys_config (`variable`, `value`, `set_by`)
values ('__tempdb_id__', '{newId}', 'root')
on duplicate key update `value` = '{newId}';
    ";
            cmd.ExecuteNonQuery();
            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "FLUSH TABLES;";
            cmd2.ExecuteNonQuery();
            var timeout = DateTime.Now.AddSeconds(5);
            while (FetchDbInstanceId() != newId && DateTime.Now < timeout)
            {
                cmd.ExecuteNonQuery();
                Thread.Sleep(250);
            }

            var currentId = FetchDbInstanceId();
            if (currentId != newId)
            {
                throw new InvalidOperationException(
                    $"Unable to set new instance id '{newId}' - id '{currentId}' is retained."
                );
            }
        }

        private string FetchDbInstanceId()
        {
            using var conn = base.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @$"
use sys;
select `value` 
from sys.sys_config 
where `variable` = '__tempdb_id__';";
            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return $"{reader["value"]}";
        }

        private void TestIsRunning(Guid? assimilatedInstanceId)
        {
            Log("testing to see if mysqld is running");
            var maxTime = DateTime.Now.AddSeconds(MaxSecondsToWaitForMySqlToStart);
            do
            {
                if (_serverProcess is null || _serverProcess.HasExited)
                {
                    throw new TryAnotherPortException(
                        "Server process not running - perhaps shut down of own accord? Will try a different port."
                    );
                }

                Log($"Server process is running as {_serverProcess.ProcessId}");

                if (CanConnect())
                {
                    var isAssimilated = assimilatedInstanceId is not null;
                    if (isAssimilated)
                    {
                        var currentId = FetchDbInstanceId();
                        if (currentId == $"{InstanceId}")
                        {
                            return;
                        }

                        if (!string.IsNullOrWhiteSpace(currentId))
                        {
                            throw new TryAnotherPortException(
                                $"mysqld listening on {Port} is a different instance ({currentId})"
                            );
                        }

                        TrySetDbInstanceId(InstanceId, assimilatedInstanceId);
                        return;
                    }

                    if (!IsMyInstance(InstanceId))
                    {
                        throw new TryAnotherPortException(
                            $"Encountered existing instance on port {Port}"
                        );
                    }

                    return;
                }

                Thread.Sleep(250);
            } while (DateTime.Now < maxTime);

            KeepTemporaryDatabaseArtifactsForDiagnostics = true;

            Log(
                $"Unable to establish a connection to database '{InstanceId}' within {MaxSecondsToWaitForMySqlToStart} seconds"
            );
            CleanupAndThrowFatalInitError();
        }

        private void CleanupAndThrowFatalInitError()
        {
            var stderr = "(unknown)";
            var stdout = "(unknown)";

            TryDo(() =>
            {
                if (_serverProcess is not null)
                {
                    PauseWatcher();
                    Log($"killing server process {_serverProcess?.ProcessId} - seems to be unresponsive to connect?");
                    AttemptGracefulShutdown();
                    _serverProcess?.Kill();
                    stderr = _serverProcess?.StandardError?.JoinWith("\n") ?? "(none)";
                    stdout = _serverProcess?.StandardOutput?.JoinWith("\n") ?? "(none)";
                    _serverProcess?.Dispose();
                    _serverProcess = null;
                }
            });

            if (LooksLikePortConflict() &&
                ++_conflictingPortRetries < 5)
            {
                KeepTemporaryDatabaseArtifactsForDiagnostics = false;
                throw new TryAnotherPortException($"Port conflict at {Port}, will try another...");
            }

            throw new FatalTempDbInitializationException(
                $@"MySql doesn't want to start up. Please check logging in {
                    DatabasePath
                }
stdout: {stdout}
stderr: {stderr}"
            );
        }

        private bool LooksLikePortConflict()
        {
            var filename = "mysql-err.log";
            var logFile = FindFirstExistingFile(
                // mysql 5
                Path.Combine(DatabasePath, filename),
                // mysql 8
                Path.Combine(DataDir, filename)
            );
            if (logFile is null)
            {
                Log($"no {filename} found under {DatabasePath}");
                return false;
            }

            try
            {
                var lines = File.ReadLines(logFile);
                foreach (var line in lines)
                {
                    foreach (var re in PortConflictMatches)
                    {
                        if (re.IsMatch(line))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch
            {
                Log($"Can't read from {logFile}");
                return false; // can't read the file, so can't say it might just be a port conflict
            }
        }

        private static Regex[] PortConflictMatches =
        {
            // mysql 5
            new("bind on tcp/ip port: no such file or directory", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            // mysql 8
            new(
                "do you already have another mysqld server running on port",
                RegexOptions.Compiled | RegexOptions.IgnoreCase
            )
        };

        private string FindFirstExistingFile(
            params string[] paths
        )
        {
            if (paths.Length == 0)
            {
                throw new InvalidOperationException(
                    "At least one path should be specified"
                );
            }

            foreach (var f in paths)
            {
                if (File.Exists(f))
                {
                    return f;
                }
            }

            Log($"Can't find any of:\n{paths.JoinWith("\n")}");
            return null;
        }

        protected bool CanConnect()
        {
            var cutoff = DateTime.Now.AddSeconds(
                Settings?.Options?.MaxTimeToConnectAtStartInSeconds
                ?? TempDbMySqlServerSettings.TempDbOptions.DEFAULT_MAX_TIME_TO_CONNECT_AT_START_IN_SECONDS
            );
            var testAttempts = 0;
            while (DateTime.Now < cutoff || testAttempts++ < 1)
            {
                if (_serverProcess is null || _serverProcess.HasExited)
                {
                    return false;
                }

                try
                {
                    Log($"Attempt to connect on {Port}...");
                    base.OpenConnection()?.Dispose();
                    Log("Connection established!");
                    return true;
                }
                catch (Exception ex)
                {
                    if (_serverProcess is null || _serverProcess.HasExited)
                    {
                        Log($"Unable to connect: server process has exited");
                        return false;
                    }

                    Log($"Unable to connect ({ex.Message}). Will continue to try until {cutoff}");
                    Thread.Sleep(250);
                }
            }

            Log($"Giving up on connecting to mysql on port {Port}");
            return false;
        }

        private void InitializeWith(string mysqld, string tempDefaultsFile)
        {
            Directory.CreateDirectory(DataDir);

            if (IsWindowsAndMySql56OrLower(MySqlVersion))
            {
                AttemptManualInitialization(mysqld);
                return;
            }

            Log($"Initializing MySql in {DatabasePath}");
            var args = new[]
            {
                $"\"--defaults-file={tempDefaultsFile}\"",
                "--initialize-insecure",
                $"\"--basedir={BaseDirOf(mysqld)}\"",
                $"\"--datadir={DataDir}\""
            };
            if (VerboseLoggingEnabled)
            {
                args = args.And(VerboseLoggingCommandLineArgs);
            }

            using var process = RunCommand(
                false,
                mysqld,
                args
            );
            var waitSeconds = 60;
            var exitCode = process.WaitForExit(waitSeconds * 1000);
            if (exitCode is null)
            {
                throw new Exception($"Initialize command for mysqld does not complete within {waitSeconds} seconds");
            }

            if (exitCode == 0)
            {
                return;
            }

            var stderr = process.StandardError.JoinWith("\n");
            var errors = new List<string>();
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                errors.Add($"stderr:\n{stderr}");
            }

            var errorFile = Path.Combine(DataDir, "mysql-err.log");
            if (File.Exists(errorFile))
            {
                try
                {
                    errors.Add($"{errorFile}:\n{File.ReadAllText(errorFile)}");
                }
                catch (Exception ex)
                {
                    errors.Add($"(unable to read error file at: '{errorFile}': {ex.Message})");
                }
            }


            throw new UnableToInitializeMySqlException(
                errors.JoinWith("\n")
            );
        }

        private bool IsMySql8()
        {
            return MySqlVersion?.Version?.Major >= 8;
        }

        public MySqlVersionInfo MySqlVersion { get; private set; }

        private void AttemptManualInitialization(string mysqld)
        {
            Log("Attempting manual initialization for older mysql");
            var installFolder = Path.GetDirectoryName(
                Path.GetDirectoryName(
                    mysqld
                )
            );
            var dataDir = Path.Combine(
                installFolder ??
                throw new InvalidOperationException($"Unable to determine hosting folder for {mysqld}"),
                "data"
            );
            if (!Directory.Exists(dataDir))
            {
                throw new FatalTempDbInitializationException(
                    $"Unable to manually initialize: folder not found: {dataDir}"
                );
            }

            Log($"copying files and folders from {dataDir} into {DatabasePath}");
            Directory.EnumerateDirectories(dataDir)
                .ForEach(d => CopyFolder(d, CombinePaths(DatabasePath, Path.GetFileName(d))));
            Directory.EnumerateFiles(dataDir)
                .ForEach(f => File.Copy(Path.Combine(f), CombinePaths(DatabasePath, Path.GetFileName(f))));
        }

        private static string CombinePaths(params string[] parts)
        {
            var nonNull = parts.Where(p => p != null).ToArray();
            if (nonNull.IsEmpty())
            {
                throw new InvalidOperationException($"no paths provided for {nameof(CombinePaths)}");
            }

            return Path.Combine(nonNull);
        }

        private static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            Directory.GetFiles(sourceFolder)
                .ForEach(f => CopyFileToFolder(f, destFolder));

            Directory.GetDirectories(sourceFolder)
                .ForEach(d => CopyFolderIntoFolder(d, destFolder));
        }

        private static void CopyFolderIntoFolder(string srcFolder, string targetFolder)
        {
            CopyItemToFolder(srcFolder, targetFolder, CopyFolder);
        }

        private static void CopyItemToFolder(string src, string targetFolder, Action<string, string> copyAction)
        {
            var baseName = Path.GetFileName(src);
            if (baseName == null)
            {
                return;
            }

            copyAction(src, Path.Combine(targetFolder, baseName));
        }

        private static void CopyFileToFolder(string srcFile, string targetFolder)
        {
            CopyItemToFolder(srcFile, targetFolder, File.Copy);
        }

        private bool IsWindowsAndMySql56OrLower(MySqlVersionInfo mysqlVersion)
        {
            return (mysqlVersion.Platform?.StartsWith("win") ?? false) &&
                mysqlVersion.Version.Major <= 5 &&
                mysqlVersion.Version.Minor <= 6;
        }

        public class MySqlVersionInfo
        {
            public string Platform { get; set; }
            public Version Version { get; set; }

            public override string ToString()
            {
                return $"{Version} ({Platform})";
            }
        }

        private MySqlVersionInfo QueryVersionOf(string mysqld)
        {
            using var process = RunCommand(false, mysqld, "--version");
            process.WaitForExit();

            var result = process.StandardOutput.JoinWith("\n");
            var parts = result.ToLower().Split(' ');
            var versionInfo = new MySqlVersionInfo();
            var last = "";
            parts.ForEach(
                p =>
                {
                    if (last == "ver")
                    {
                        versionInfo.Version = new Version(p.Split('-').First());
                    }
                    else if (last == "for" && versionInfo.Platform == null)
                    {
                        versionInfo.Platform = p;
                    }

                    last = p;
                }
            );
            return versionInfo;
        }

        private string DataFilePath(string relativePath)
        {
            return Path.Combine(DataDir, relativePath);
        }

        private IProcessIO RunCommand(
            bool logStartupInfo,
            string filename,
            params string[] args
        )
        {
            if (logStartupInfo)
            {
                LogProcessStartInfo(
                    DataFilePath("startup-info.log"),
                    filename,
                    args
                );
            }

            Log($"Running command:\n\"{filename}\" {args.JoinWith(" ")}");

            var result = ProcessIO.Start(
                filename,
                args
            );

            if (!result.Started)
            {
                throw new ProcessStartFailureException(
                    filename,
                    args
                );
            }

            return result;
        }

        private void LogProcessStartInfo(
            string logFile,
            string executable,
            string[] args
        )
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    using var f = new FileStream(logFile, FileMode.OpenOrCreate);
                    f.SetLength(0);
                    using var writer = new StreamWriter(f);
                    writer.WriteLine("MySql started with the following startup info:");
                    writer.WriteLine($"CLI: \"{executable}\" {args.Select(ProcessIO.QuoteIfNecessary).JoinWith(" ")}");
                    writer.WriteLine($"Working directory: {Directory.GetCurrentDirectory()}");
                    writer.WriteLine($"Current user: {Environment.UserName}");
                    writer.WriteLine("Environment:");
                    var envVars = Environment.GetEnvironmentVariables();
                    foreach (var key in envVars.Keys)
                    {
                        writer.WriteLine($"  {key} = {envVars[key]}");
                    }

                    return;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        private int DeterminePortToUse()
        {
            return Settings?.Options?.PortHint != null
                ? FindFirstPortFrom(Settings.Options.PortHint.Value)
                : FindRandomOpenPort();
        }

        private int FindFirstPortFrom(int portHint)
        {
            var minPort = Settings?.Options?.RandomPortMin
                ?? TempDbMySqlServerSettings.TempDbOptions.DEFAULT_RANDOM_PORT_MIN;
            var maxPort = Settings?.Options?.RandomPortMax
                ?? TempDbMySqlServerSettings.TempDbOptions.DEFAULT_RANDOM_PORT_MAX;
            return PortFinder.FindOpenPort(
                IPAddress.Loopback,
                minPort,
                maxPort,
                (min, max, last) => last < 1
                    ? portHint
                    : last + 1,
                LogPortDiscoveryInfo
            );
        }

        protected virtual int FindRandomOpenPort()
        {
            var minPort = Settings?.Options?.RandomPortMin
                ?? TempDbMySqlServerSettings.TempDbOptions.DEFAULT_RANDOM_PORT_MIN;
            var maxPort = Settings?.Options?.RandomPortMax
                ?? TempDbMySqlServerSettings.TempDbOptions.DEFAULT_RANDOM_PORT_MAX;
            return PortFinder.FindOpenPort(
                IPAddress.Loopback,
                minPort,
                maxPort,
                LogPortDiscoveryInfo
            );
        }

        private void LogPortDiscoveryInfo(string message)
        {
            if (Settings?.Options?.LogRandomPortDiscovery ?? false)
            {
                Log(message);
            }
        }
    }

    internal static class ProcessExtensions
    {
        public static bool HasStarted(
            this Process process
        )
        {
            try
            {
                return process.Id > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}