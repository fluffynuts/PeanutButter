using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        /// listening for connections after starting up. Defaults to 30 seconds,
        /// but left static so consumers can tweak the value as required.
        /// </summary>
        public static int DefaultStartupMaxWaitSeconds = 30;
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
            self.LogAction = settings?.Options?.LogAction;
            self._autoDeleter = new AutoDeleter();
            self.Settings = settings;
            beforeInit?.Invoke(self);
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
            var mysqld = Find.InPath("mysqld");
            var shouldFindInPath = Platform.IsUnixy || Settings.Options.ForceFindMySqlInPath;
            if (shouldFindInPath && !(mysqld is null))
            {
                return mysqld;
            }

            if (Platform.IsWindows)
            {
                var servicePath = MySqlWindowsServiceFinder.FindPathToMySql();
                // prefer the pathed mysqld, but fall back on service path if available
                var resolved = mysqld ?? servicePath;
                if (!(resolved is null))
                {
                    return resolved;
                }
            }

            throw _noMySqlFoundException;
        }

        private string MySqld =>
            _mysqld ??= Settings?.Options?.PathToMySqlD ?? FindInstalledMySqlD();

        /// <inheritdoc />
        protected override void CreateDatabase()
        {
            _startAttempts = 0;
            MySqlVersion = QueryVersionOf(MySqld);
            EnsureIsRemoved(DatabasePath);
            if (IsMySql8(MySqlVersion))
            {
                RemoveDeprecatedOptions();
            }

            // mysql 5.7 wants an empty data base dir, so we have to use
            // a temp defaults file for init only, which 8 seems to be ok with
            using var tempFolder = new AutoTempFolder();
            DumpDefaultsFileAt(tempFolder.Path);
            InitializeWith(MySqld, Path.Combine(tempFolder.Path, "my.cnf"));

            // now we need the real config file, sitting in the db dir
            DumpDefaultsFileAt(DatabasePath);
            Port = DeterminePortToUse();
            StartServer(MySqld);
            SetRootPassword();
            CreateInitialSchema();
            SetUpAutoDisposeIfRequired();
        }

        private void SetUpAutoDisposeIfRequired()
        {
            var timeout = Settings?.Options?.InactivityTimeout;
            var absoluteLifespan = Settings?.Options?.AbsoluteLifespan;
            SetupAutoDispose(absoluteLifespan, timeout);
        }


        private void RemoveDeprecatedOptions()
        {
            Settings.SqlMode = (Settings.SqlMode ?? "").Split(',')
                .Where(p => p != "NO_AUTO_CREATE_USER")
                .JoinWith(",");
        }

        private void SetRootPassword()
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $@"alter user 'root'@'localhost' identified with mysql_native_password by '{
                        Settings.Options.RootUserPassword.Replace("'", "''")
                    }';";
            cmd.ExecuteNonQuery();
            RootPasswordSet = true;
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
                return;
            }

            Log($"Attempting to switch to schema {schema} with connection string: {ConnectionString}");
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $"create schema if not exists `{schema}`";
            command.ExecuteNonQuery();

            command.CommandText = $"use `{schema}`";
            command.ExecuteNonQuery();

            SchemaName = schema;
        }

        private void EnsureIsRemoved(string databasePath)
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }

            if (Directory.Exists(databasePath))
            {
                Directory.Delete(databasePath, true);
            }
        }

        private string DumpDefaultsFileAt(
            string databasePath,
            string configFileName = "my.cnf"
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

                _processWatcherThread?.Join();
                _processWatcherThread = null;

                AttemptGracefulShutdown();
                EndServerProcess();
                base.Dispose();
                _autoDeleter?.Dispose();
                _autoDeleter = null;
            }
            catch (Exception ex)
            {
                Log($"Unable to kill MySql instance {_serverProcess?.Id}: {ex.Message}");
            }
        }

        private void EndServerProcess()
        {
            using (new AutoLocker(MysqldLock))
            {
                if (_serverProcess == null)
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
            if (_serverProcess == null)
            {
                return;
            }

            var task = Task.Run(() =>
            {
                try
                {
                    SwitchToSchema("mysql");
                    using var connection = OpenConnection();
                    using var command = connection.CreateCommand();
                    // this is only available from mysql 5.7.9 onward (https://dev.mysql.com/doc/refman/5.7/en/shutdown.html)
                    command.CommandText = "SHUTDOWN";
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch
                    {
                        /* ignore */
                    }
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
            _serverProcess = RunCommand(
                true,
                mysqld,
                $"\"--defaults-file={Path.Combine(DatabasePath, "my.cnf")}\"",
                $"\"--basedir={BaseDirOf(mysqld)}\"",
                $"\"--datadir={DataDir}\"",
                $"--port={Port}");
            try
            {
                try
                {
                    TestIsRunning();
                }
                catch (FatalTempDbInitializationException)
                {
                    // I've seen the mysqld process simply exit early
                    // (5.7, win32) without anything interesting in the
                    // error log; so let's just give this a few attempts
                    // before giving up completely
                    if (++_startAttempts >= MaxStartupAttempts)
                    {
                        throw;
                    }

                    Log("MySql appears to not start up properly; retrying...");
                    Retry();
                    return;
                }
            }
            catch (TryAnotherPortException)
            {
                Retry();
                return;
            }

            StartProcessWatcher();

            void Retry()
            {
                if (Settings?.Options?.PortHint.HasValue ?? false)
                {
                    Settings.Options.PortHint++;
                }

                Port = DeterminePortToUse();
                StartServer(mysqld);
            }
        }

        public string DataDir =>
            MySqlVersion.Version.Major >= 8
                ? Path.Combine(DatabasePath, "data") // mysql 8 wants a clean dir to init in
                : DatabasePath;

        private Thread _processWatcherThread;
        private readonly SemaphoreSlim _disposalLock = new SemaphoreSlim(1);

        private void StartProcessWatcher()
        {
            _processWatcherThread = new Thread(ObserveMySqlProcess);
            _processWatcherThread.Start();
        }

        private bool _disposed;

        private void ObserveMySqlProcess()
        {
            while (!_disposed)
            {
                using (new AutoLocker(_disposalLock))
                {
                    if (_disposed)
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
            var start = DateTime.Now;
            var max = TimeSpan.FromSeconds(MaxSecondsToWaitForMySqlToStart);
            do
            {
                if (_serverProcess.HasExited)
                {
                    break;
                }

                if (CanConnect())
                {
                    if (!IsMyInstance())
                    {
                        throw new TryAnotherPortException($"Encountered existing instance on port {Port}");
                    }

                    return;
                }

                Thread.Sleep(10);
            } while (DateTime.Now - start < max);

            KeepTemporaryDatabaseArtifactsForDiagnostics = true;

            var stderr = _serverProcess?.StandardError.ReadToEnd() ?? "(no stderr)";
            var stdout = _serverProcess?.StandardOutput.ReadToEnd() ?? "(no stdout)";

            try
            {
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
                return false; // can't read the file, so can't say it might just be a port conflict
            }
        }

        protected bool CanConnect()
        {
            try
            {
                OpenConnection()?.Dispose();

                return true;
            }
            catch
            {
                return false;
            }
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
            using var process = RunCommand(
                mysqld,
                $"\"--defaults-file={tempDefaultsFile}\"",
                "--initialize-insecure",
                $"\"--basedir={BaseDirOf(mysqld)}\"",
                $"\"--datadir={DataDir}\""
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
            var installFolder = Path.GetDirectoryName(
                Path.GetDirectoryName(
                    mysqld
                ));
            var dataDir = Path.Combine(
                installFolder ??
                throw new InvalidOperationException($"Unable to determine hosting folder for {mysqld}"),
                "data");
            if (!Directory.Exists(dataDir))
                throw new FatalTempDbInitializationException(
                    $"Unable to manually initialize: folder not found: {dataDir}"
                );
            Directory.EnumerateDirectories(dataDir)
                .ForEach(d => CopyFolder(d, CombinePaths(DatabasePath, Path.GetFileName(d))));
            Directory.EnumerateFiles(dataDir)
                .ForEach(f => File.Copy(Path.Combine(f), CombinePaths(DatabasePath, Path.GetFileName(f))));
        }

        private static string CombinePaths(params string[] parts)
        {
            var nonNull = parts.Where(p => p != null).ToArray();
            if (nonNull.IsEmpty())
                throw new InvalidOperationException($"no paths provided for {nameof(CombinePaths)}");
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
                return;
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
        }

        private MySqlVersionInfo QueryVersionOf(string mysqld)
        {
            using var process = RunCommand(mysqld, "--version");
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
                    Path.Combine(DataDir, "startup-info.log")
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

        private Process RunCommand(
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

        private int DeterminePortToUse()
        {
            return Settings?.Options?.PortHint != null
                ? FindFirstPortFrom(Settings.Options.PortHint.Value)
                : FindRandomOpenPort();
        }

        private int FindFirstPortFrom(int portHint)
        {
            return FindPort(last => last == 0
                ? portHint
                : last + 1);
        }

        private int FindPort(
            Func<int, int> portGenerator
        )
        {
            var tryThis = portGenerator(0);
            var attempts = 0;
            while (attempts++ < 1000)
            {
                LogPortDiscoveryInfo($"Testing if port can be bound {tryThis} on any available IP address");
                if (!PortIsInUse(tryThis))
                {
                    LogPortDiscoveryInfo($"Port looks to be available: {tryThis}");
                    break;
                }

                if (attempts == 1000)
                {
                    throw new FatalTempDbInitializationException(
                        "Unable to find high random port to bind on."
                    );
                }

                LogPortDiscoveryInfo($"Port {tryThis} looks to be unavailable. Seeking another...");
                Thread.Sleep(RandomNumber.Next(1, 50));
                tryThis = portGenerator(tryThis);
            }

            return tryThis;
        }

        protected virtual int FindRandomOpenPort()
        {
            return FindPort(last => NextRandomPort());
        }

        private void LogPortDiscoveryInfo(string message)
        {
            if (Settings?.Options?.LogRandomPortDiscovery ?? false)
                Log(message);
        }

        private bool PortIsInUse(int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(new IPEndPoint(IPAddress.Loopback, port));
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private int NextRandomPort()
        {
            var minPort = Settings?.Options?.RandomPortMin
                ?? TempDbMySqlServerSettings.TempDbOptions.DEFAULT_RANDOM_PORT_MIN;
            var maxPort = Settings?.Options?.RandomPortMax
                ?? TempDbMySqlServerSettings.TempDbOptions.DEFAULT_RANDOM_PORT_MAX;
            if (minPort > maxPort)
            {
                var swap = minPort;
                minPort = maxPort;
                maxPort = swap;
            }

            return RandomNumber.Next(minPort, maxPort);
        }
    }
}