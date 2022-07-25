using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        public bool VerboseLoggingEnabled =>
            DetermineIfVerboseLoggingShouldBeEnabled();

        private string[] VerboseLoggingCommandLineArgs =>
            VerboseLoggingEnabled
                ? new[] { "--log-error-verbosity=3" }
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
                    new[] { toLog }
                );
            }
        }

        /// <summary>
        /// Set to true to see trace logging about discovery of a port to
        /// instruct mysqld to bind to
        /// </summary>
        public TempDbMySqlServerSettings Settings { get; private set; }

        public int? ServerProcessId => _serverProcess?.Id;

        private Process _serverProcess;
        public int Port { get; protected set; }
        public bool RootPasswordSet { get; private set; }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly SemaphoreSlim MysqldLock = new SemaphoreSlim(1);

        // ReSharper disable once StaticMemberInGenericType
        private static string _mysqld;

        public Guid InstanceId { get; } = Guid.NewGuid();
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
                creationScripts)
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
            TempDbMySqlServerSettings settings)
        {
            self.LogAction = WrapWithDebugLogger(self, settings?.Options?.LogAction);
            self._autoDeleter = new AutoDeleter();
            self.Settings = settings;
            beforeInit?.Invoke(self);
        }

        private static Action<string> WrapWithDebugLogger(
            TempDBMySqlBase<T> tempDb,
            Action<string> optionsLogAction)
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

            var servicePath = MySqlWindowsServiceFinder.FindPathToMySql();
            // prefer the pathed mysqld, but fall back on service path if available
            var resolved = mysqld ?? servicePath;
            Log($@"mysqld from path: {
                mysqld ?? "(not found)"
            }; mysqld from service finder: {
                servicePath ?? "(not found)"
            }");
            return resolved ?? throw _noMySqlFoundException;
        }

        private string MySqld =>
            _mysqld ??= Settings?.Options?.PathToMySqlD ?? FindInstalledMySqlD();

        /// <inheritdoc />
        protected override void CreateDatabase()
        {
            Log("create initial database");
            _startAttempts = 0;
            MySqlVersion = QueryVersionOf(MySqld);
            Log($"mysql is version: {MySqlVersion}");
            EnsureIsRemoved(DatabasePath);
            if (IsMySql8(MySqlVersion))
            {
                RemoveDeprecatedOptions();
            }

            // mysql 5.7 wants an empty data base dir, so we have to use
            // a temp defaults file for init only, which 8 seems to be ok with
            using var tempFolder = new AutoTempFolder();
            Log($"temp folder created at {tempFolder}");
            Log($"dumping defaults in temp folder {tempFolder.Path} for initialization");
            DumpDefaultsFileAt(tempFolder.Path);
            InitializeWith(MySqld, Path.Combine(tempFolder.Path, "my.cnf"));

            RedirectDebugLogging();

            // now we need the real config file, sitting in the db dir
            Log($"dumping run-time defaults file into {DatabasePath}");
            DumpDefaultsFileAt(DatabasePath);
            ConfigFilePath = Path.Combine(DatabasePath, MYSQL_CONFIG_FILE);
            Port = DeterminePortToUse();
            StartServer(MySqld);
            SetRootPassword();
            CreateInitialSchema();
            SetUpAutoDisposeIfRequired();
        }


        private void RedirectDebugLogging()
        {
            lock (_debugLogLock)
            {
                var existingFile = _debugLogFile;
                _debugLogFile = DataFilePath("tempdb-debug.log");
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
            using var conn = OpenConnection();
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
            using var conn = OpenConnection();
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
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    // suppress
                }
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
                throw new InvalidOperationException(
                    $"Unable to find mysqldump at {mysqldump}"
                );
            }

            using var io = ProcessIO.Start(
                mysqldump,
                "-u", "root",
                $"--password={Settings.Options.RootUserPassword}",
                "-h", "localhost",
                "-P", Port.ToString(),
                SchemaName);
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
            params string[] forSchemas)
        {
            Execute(
                $"create user {Quote(user)}@'%' identified with mysql_native_password by {Quote(password)}"
            );
            forSchemas.ForEach(schema =>
            {
                GrantAllPermissionsFor(user, schema, "%");
            });
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
            using var connection = OpenConnection();
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
                using (new AutoLocker(_disposalLock))
                {
                    if (_disposed)
                    {
                        return;
                    }

                    _disposed = true;
                }

                Stop();

                base.Dispose();
                _autoDeleter?.Dispose();
                _autoDeleter = null;
            }
            catch (Exception ex)
            {
                Log($"Unable to kill MySql instance {_serverProcess?.Id}: {ex.Message}");
            }
        }

        private void Stop()
        {
            StopWatcher();
            AttemptGracefulShutdown();
            EndServerProcess();
        }

        private void StopWatcher()
        {
            using (new AutoLocker(_disposalLock))
            {
                _running = false;
            }

            _processWatcherThread?.Join();
            _processWatcherThread = null;
        }

        private void EndServerProcess()
        {
            using (new AutoLocker(MysqldLock))
            {
                if (_serverProcess is null)
                {
                    return;
                }

                Trace.WriteLine($"Stopping mysqld with process id {_serverProcess.Id}");
                if (!_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                    _serverProcess.WaitForExit(3000);

                    if (!_serverProcess.HasExited)
                    {
                        throw new Exception($"MySql process {_serverProcess.Id} has not shut down!");
                    }
                }

                _serverProcess = null;
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
            var task = Task.Run(() =>
            {
                try
                {
                    SwitchToSchema("mysql");
                    Execute("SHUTDOWN");
                }
                catch (Exception ex)
                {
                    Log($"Unable to perform graceful shutdown: {ex.Message}");
                }
            });
            task.ConfigureAwait(false);
            task.Wait(TimeSpan.FromSeconds(2));
        }

        private int _startAttempts;

        private void StartServer(string mysqld)
        {
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

            _serverProcess = RunCommand(
                true,
                mysqld,
                args
            );
            try
            {
                try
                {
                    TestIsRunning();
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
                    return;
                }
            }
            catch (TryAnotherPortException)
            {
                Log($"Looks like a port conflict at {Port}. Will try another port.");
                Retry();
                return;
            }

            Log("MySql appears to be up an running! Setting up an auto-restarter in case it falls over.");
            StartProcessWatcher();

            void Retry()
            {
                if (Settings?.Options?.PortHint.HasValue ?? false)
                {
                    Log($"incrementing port hint to {Settings.Options.PortHint + 1}");
                    Settings.Options.PortHint++;
                }

                Port = DeterminePortToUse();
                StartServer(mysqld);
            }
        }

        public void Restart()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("This instance has already been disposed");
            }
            Stop();
            StartServer(MySqld);
        }

        public string DataDir =>
            MySqlVersion.Version.Major >= 8
                ? Path.Combine(DatabasePath, "data") // mysql 8 wants a clean dir to init in
                : DatabasePath;

        private Thread _processWatcherThread;
        private readonly SemaphoreSlim _disposalLock = new SemaphoreSlim(1);

        private void StartProcessWatcher()
        {
            _running = true;
            _processWatcherThread = new Thread(ObserveMySqlProcess);
            _processWatcherThread.Start();
        }

        private bool _disposed;
        private bool _running;

        private void ObserveMySqlProcess()
        {
            while (_running)
            {
                using (new AutoLocker(_disposalLock))
                {
                    if (!_running)
                    {
                        // we're outta here
                        break;
                    }
                }

                if (ServerProcessId == null)
                {
                    // undefined state -- possibly never was started
                    break;
                }

                bool shouldResurrect;
                try
                {
                    var process = Process.GetProcessById(
                        ServerProcessId.Value
                    );
                    shouldResurrect = process.HasExited;
                }
                catch
                {
                    // unable to query the process
                    shouldResurrect = true;
                }

                if (shouldResurrect)
                {
                    Log($"{MySqld} seems to have gone away; restarting on port {Port}");
                    StartServer(MySqld);
                }

                Thread.Sleep(PROCESS_POLL_INTERVAL);
            }
        }

        private string BaseDirOf(string mysqld)
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(mysqld));
        }

        protected virtual bool IsMyInstance()
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            // first, check if the id is in there already (restart)
            cmd.CommandText = $"select count(*) from sys.sys_config where `variable` = '__tempdb_id__' and `value` = '{InstanceId}';";
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read() && int.TryParse(reader[0]?.ToString(), out var rows) && rows == 1)
                {
                    return true;
                }
            }

            cmd.CommandText = @$"insert into sys.sys_config (variable, value, set_by) 
values ('__tempdb_id__', '{InstanceId}', 'root');";
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void TestIsRunning()
        {
            Log("testing to see if mysqld is running");
            var maxTime = DateTime.Now.AddSeconds(MaxSecondsToWaitForMySqlToStart);
            do
            {
                if (_serverProcess.HasExited)
                {
                    Log("Server process has exited");
                    break;
                }

                Log($"Server process is running as {_serverProcess.Id}");

                if (CanConnect())
                {
                    if (!IsMyInstance())
                    {
                        throw new TryAnotherPortException($"Encountered existing instance on port {Port}");
                    }

                    return;
                }

                Thread.Sleep(250);
            } while (DateTime.Now < maxTime);

            KeepTemporaryDatabaseArtifactsForDiagnostics = true;

            var stderr = _serverProcess?.StandardError.ReadToEnd() ?? "(no stderr)";
            var stdout = _serverProcess?.StandardOutput.ReadToEnd() ?? "(no stdout)";

            try
            {
                if (_serverProcess is not null)
                {
                    Log($"killing server process {_serverProcess?.Id} - seems to be unresponsive to connect?");
                }

                _serverProcess?.Kill();
                _serverProcess?.Dispose();
                _serverProcess = null;
            }
            catch
            {
                /* ignore */
            }

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
            var logFile = Path.Combine(DatabasePath, "mysql-err.log");
            if (!File.Exists(logFile))
            {
                Log($"no logfile found at {logFile}");
                return false;
            }

            try
            {
                var lines = File.ReadLines(logFile);
                var re = new Regex("bind on tcp/ip port: no such file or directory", RegexOptions.IgnoreCase);
                return lines.Any(l => re.IsMatch(l));
            }
            catch
            {
                Log($"Can't read from {logFile}");
                return false; // can't read the file, so can't say it might just be a port conflict
            }
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
                try
                {
                    Log($"Attempt to connect on {Port}...");
                    OpenConnection()?.Dispose();
                    Log("Connection established!");
                    return true;
                }
                catch
                {
                    Log($"Unable to connect. Will continue to try until {cutoff}");
                    Thread.Sleep(500);
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
            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                return;
            }

            throw new UnableToInitializeMySqlException(
                process.StandardError.ReadToEnd()
            );
        }

        private bool IsMySql8(MySqlVersionInfo mySqlVersion)
        {
            return mySqlVersion.Version.Major >= 8;
        }

        public MySqlVersionInfo MySqlVersion { get; private set; }

        private void AttemptManualInitialization(string mysqld)
        {
            Log("Attempting manual initialization for older mysql");
            var installFolder = Path.GetDirectoryName(
                Path.GetDirectoryName(
                    mysqld
                ));
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

            var result = process.StandardOutput.ReadToEnd();
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
                });
            return versionInfo;
        }

        private string DataFilePath(string relativePath)
        {
            return Path.Combine(DatabasePath, relativePath);
        }

        private Process RunCommand(
            bool logStartupInfo,
            string filename,
            params string[] args
        )
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = filename,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Arguments = args.JoinWith(" ")
            };
            if (logStartupInfo)
            {
                LogProcessStartInfo(
                    startInfo,
                    DataFilePath("startup-info.log")
                );
            }

            var process = new Process()
            {
                StartInfo = startInfo
            };
            Log($"Running command:\n\"{filename}\" {args.JoinWith(" ")}");
            if (!process.Start())
            {
                throw new ProcessStartFailureException(startInfo);
            }

            return process;
        }

        private void LogProcessStartInfo(
            ProcessStartInfo startInfo,
            string logFile
        )
        {
            using var f = new FileStream(logFile, FileMode.OpenOrCreate);
            f.SetLength(0);
            using var writer = new StreamWriter(f);
            writer.WriteLine("MySql started with the following startup info:");
            writer.WriteLine($"CLI: \"{startInfo.FileName}\" {startInfo.Arguments}");
            writer.WriteLine($"Working directory: {Directory.GetCurrentDirectory()}");
            writer.WriteLine($"Current user: {Environment.UserName}");
            writer.WriteLine("Environment:");
            var envVars = Environment.GetEnvironmentVariables();
            foreach (var key in envVars.Keys)
            {
                writer.WriteLine($"  {key} = {envVars[key]}");
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
}