using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
#if NETSTANDARD
#else
using System.ServiceProcess;
#endif
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace PeanutButter.TempDb.MySql
{
    // ReSharper disable once InconsistentNaming
    public class TempDBMySql : TempDB<MySqlConnection>
    {
        /// <summary>
        /// Set to true to see trace logging about discovery of a port to
        /// instruct mysqld to bind to
        /// </summary>
        public TempDbMySqlServerSettings Settings { get; private set; }

        private readonly Random _random = new Random();
        private Process _serverProcess;
        private int _port;
        private static readonly object MysqldLock = new object();
        private static string _mysqld;

        private readonly FatalTempDbInitializationException _noMySqlInstalledException =
            new FatalTempDbInitializationException(
                "Unable to locate MySql service via Windows service registry. Please install an instance of MySql on this machine to use temporary MySql databases.");
        private readonly FatalTempDbInitializationException _dotNetStandardRequiresMySqlPathException =
            new FatalTempDbInitializationException(
                "When running from .net standard / core, you must either provide the path to mysqld or ensure that mysqld is in your PATH, even on windows.");

        private string _schema;
        private AutoDeleter _autoDeleter;

        public TempDBMySql(params string[] creationScripts)
            : this(new TempDbMySqlServerSettings(), creationScripts)
        {
        }

        public TempDBMySql(
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

        public TempDBMySql(
            TempDbMySqlServerSettings settings,
            Action<object> beforeInit,
            params string[] creationScripts
        ) : base(
            o => BeforeInit(o as TempDBMySql, beforeInit, settings),
            creationScripts
        )
        {
        }

        private static void BeforeInit(
            TempDBMySql self,
            Action<object> beforeInit,
            TempDbMySqlServerSettings settings)
        {
            self._autoDeleter = new AutoDeleter();
            self.Settings = settings;
            beforeInit?.Invoke(self);
        }

        private string FindInstalledMySqlD()
        {
            lock (MysqldLock)
            {
                return _mysqld ?? (_mysqld = QueryForMySqld());
            }
        }

        private string QueryForMySqld()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT ||
                Settings.Options.ForceFindMySqlInPath)
            {
                var mysqlDaemonPath = Find.InPath("mysqld");
                if (mysqlDaemonPath != null)
                {
                    return mysqlDaemonPath;
                }
            }

            #if NETSTANDARD
            throw _dotNetStandardRequiresMySqlPathException;
            #else
            var mysqlService = ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName.ToLower().Contains("mysql"));
            if (mysqlService == null)
                throw _noMySqlInstalledException;
            var util = new WindowsServiceUtil(mysqlService.ServiceName);
            if (!util.IsInstalled)
                throw _noMySqlInstalledException;
            return FindServiceExecutablePartIn(util.ServiceExe);
            #endif
        }

        private static string FindServiceExecutablePartIn(string utilServiceExe)
        {
            var strings = utilServiceExe.Split(' ');
            var parts = strings;
            var offset = 0;
            var mysqldPath = new List<string>();

            do
            {
                var thisPart = parts.Skip(offset).FirstOrDefault();
                if (thisPart == null)
                    break;
                mysqldPath.Add(thisPart);
                offset++;
            } while (mysqldPath.JoinWith(" ").Count(c => c == '"') % 2 == 1);

            return mysqldPath.JoinWith(" ").Trim('"');
        }

        protected override void CreateDatabase()
        {
            var mysqld = Settings?.Options?.PathToMySqlD ?? FindInstalledMySqlD();
            _port = FindOpenRandomPort();

            var tempDefaultsPath = CreateTemporaryDefaultsFile();
            EnsureIsRemoved(DatabasePath);
            InitializeWith(mysqld, tempDefaultsPath);
            DumpDefaultsFileAt(DatabasePath);
            StartServer(mysqld, _port);
            CreateInitialSchema();
        }

        private string CreateTemporaryDefaultsFile()
        {
            var tempPath = Path.GetTempPath();
            var tempDefaultsPath = DumpDefaultsFileAt(tempPath, $"{Path.GetFileName(DatabasePath)}.cnf");
            _autoDeleter.Add(tempDefaultsPath);
            return tempDefaultsPath;
        }

        private void CreateInitialSchema()
        {
            var schema = Settings?.Options.DefaultSchema ?? "tempdb";
            SwitchToSchema(schema);
        }

        public void SwitchToSchema(string schema)
        {
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"create schema if not exists `{schema}`";
                command.ExecuteNonQuery();
                _schema = schema;
            }
        }

        private void EnsureIsRemoved(string databasePath)
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
            if (Directory.Exists(databasePath))
                Directory.Delete(databasePath);
        }

        private string DumpDefaultsFileAt(
            string databasePath,
            string configFileName = "my.cnf"
        )
        {
            var generator = new MySqlConfigGenerator();
            var outputFile = Path.Combine(databasePath, configFileName);
            File.WriteAllBytes(
                outputFile,
                Encoding.UTF8.GetBytes(
                    generator.GenerateFor(Settings)
                )
            );
            return outputFile;
        }

        public override void Dispose()
        {
            try
            {
                AttemptShutdown();
                EndServerProcess();
                _autoDeleter?.Dispose();
                _autoDeleter = null;
            }
            catch (Exception ex)
            {
                Log($"Unable to kill MySql instance {_serverProcess?.Id}: {ex.Message}");
            }

            base.Dispose();
        }

        private void EndServerProcess()
        {
            lock (MysqldLock)
            {
                if (_serverProcess == null)
                    return;
                Trace.WriteLine($"Stopping mysqld with process id {_serverProcess.Id}");
                _serverProcess.Kill();
                _serverProcess.WaitForExit(3000);
                if (!_serverProcess.HasExited)
                    throw new Exception($"MySql process {_serverProcess.Id} has not shut down!");
                _serverProcess = null;
            }
        }

        private void AttemptShutdown()
        {
            if (_serverProcess == null)
                return;
            using (var connection = CreateConnection())
            using (var command = connection.CreateCommand())
            {
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
        }

        protected override string GenerateConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Port = (uint) _port,
                UserID = "root",
                Password = "",
                Server = "localhost",
                AllowUserVariables = true,
                SslMode = MySqlSslMode.None,
                Database = _schema,
                ConnectionTimeout = DefaultTimeout,
                DefaultCommandTimeout = DefaultTimeout
            };
            return builder.ToString();
        }

        private void StartServer(string mysqld, int port)
        {
            _serverProcess = RunCommand(
                mysqld,
                $"\"--defaults-file={Path.Combine(DatabasePath, "my.cnf")}\"",
                $"\"--basedir={BaseDirOf(mysqld)}\"",
                $"\"--datadir={DatabasePath}\"",
                $"--port={port}");
            TestIsRunning();
        }

        private string BaseDirOf(string mysqld)
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(mysqld));
        }

        private void TestIsRunning()
        {
            var start = DateTime.Now;
            var max = TimeSpan.FromSeconds(5);
            do
            {
                if (CanConnect())
                    return;
                Thread.Sleep(1);
            } while (DateTime.Now - start < max);

            _keepTemporaryDatabaseArtifactsForDiagnostics = true;

            try
            {
                _serverProcess?.Kill();
                _serverProcess = null;
            }
            catch
            {
                /* ignore */
            }

            throw new FatalTempDbInitializationException(
                $"MySql doesn't want to start up. Please check logging in {DatabasePath}"
            );
        }

        private bool CanConnect()
        {
            try
            {
                CreateConnection()?.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void InitializeWith(string mysqld, string tempDefaultsFile)
        {
            Directory.CreateDirectory(DatabasePath);
            var mysqlVersion = GetVersionOf(mysqld);
            if (IsWindowsAndMySql56OrLower(mysqlVersion))
            {
                AttemptManualInitialization(mysqld);
                return;
            }

            Log($"Initializing MySql in {DatabasePath}");
            var process = RunCommand(
                mysqld,
                $"\"--defaults-file={tempDefaultsFile}\"",
                "--initialize-insecure",
                $"\"--basedir={BaseDirOf(mysqld)}\"",
                $"\"--datadir={DatabasePath}\""
            );
            process.WaitForExit();
        }

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

        private class MySqlVersionInfo
        {
            public string Platform { get; set; }
            public Version Version { get; set; }
        }

        private MySqlVersionInfo GetVersionOf(string mysqld)
        {
            var process = RunCommand(mysqld, "--version");
            process.WaitForExit();
            var result = process.StandardOutput.ReadToEnd();
            var parts = result.ToLower().Split(' ');
            var versionInfo = new MySqlVersionInfo();
            var last = "";
            parts.ForEach(
                p =>
                {
                    if (last == "ver")
                        versionInfo.Version = new Version(p);
                    else if (last == "for" && versionInfo.Platform == null)
                        versionInfo.Platform = p;
                    last = p;
                });
            return versionInfo;
        }

        private Process RunCommand(string filename, params string[] args)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = filename,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = args.JoinWith(" ")
                }
            };
            Log($"Running command:\n\"{filename}\" {args.JoinWith(" ")}");
            if (!process.Start())
                throw new Exception($"Unable to start process:\n\"{filename}\" {args.JoinWith(" ")}");
            return process;
        }

        private int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = NextRandomPort();
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
                Thread.Sleep(rnd.Next(1, 50));
                tryThis = NextRandomPort();
            }

            return tryThis;
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
                var listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                listener.Stop();
                return false;
            }
            catch
            {
                return true;
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

            return _random.Next(minPort, maxPort);
        }

        /// <summary>
        /// Provides a convenience logging mechanism which outputs via
        /// the established LogAction
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        protected void Log(string message, params object[] parameters)
        {
            var logAction = Settings?.Options?.LogAction;
            if (logAction == null)
                return;
            try
            {
                logAction(string.Format(message, parameters));
            }
            catch
            {
                /* intentionally left blank */
            }
        }
    }
}