using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
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
        private static readonly object _mysqldLock = new object();
        private static string _mysqld;

        private readonly FatalTempDbInitializationException _noMySqlInstalledException =
            new FatalTempDbInitializationException(
                "Unable to locate MySql service via Windows service registry. Please install an instance of MySql on this machine to use temporary MySql databases.");

        public TempDBMySql(params string[] creationScripts)
            : this(new TempDbMySqlServerSettings(), creationScripts)
        {
        }

        public TempDBMySql(
            TempDbMySqlServerSettings settings,
            params string[] creationScripts
        )
            : this(settings, o =>
            {
            }, creationScripts)
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
            self.Settings = settings;
            beforeInit?.Invoke(self);
        }

        private string FindInstalledMySqlD()
        {
            lock (_mysqldLock)
            {
                return _mysqld ?? (_mysqld = QueryForMySqld());
            }
        }

        private string QueryForMySqld()
        {
            var mysqlService = ServiceController.GetServices()
                .FirstOrDefault(s => s.ServiceName.ToLower().Contains("mysql"));
            if (mysqlService == null)
                throw _noMySqlInstalledException;
            var util = new WindowsServiceUtil(mysqlService.ServiceName);
            if (!util.IsInstalled)
                throw _noMySqlInstalledException;
            return FindServiceExecutablePartIn(util.ServiceExe);
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

            EnsureIsFolder(DatabasePath);
            InitializeWith(mysqld);
            DumpDefaultsFileAt(DatabasePath);
            StartServer(mysqld, _port);
        }

        private void EnsureIsFolder(string databasePath)
        {
            if (File.Exists(databasePath))
                File.Delete(databasePath);
            if (!Directory.Exists(databasePath))
                Directory.CreateDirectory(databasePath);
        }

        private void DumpDefaultsFileAt(string databasePath)
        {
            var generator = new MySqlConfigGenerator();
            File.WriteAllBytes(
                Path.Combine(databasePath, "my.cnf"),
                Encoding.UTF8.GetBytes(
                    generator.GenerateFor(Settings)
                )
            );
        }

        public override void Dispose()
        {
            try
            {
                AttemptShutdown();
                EndServerProcess();
            }
            catch (Exception ex)
            {
                Log($"Unable to kill MySql instance {_serverProcess?.Id}: {ex.Message}");
            }

            base.Dispose();
        }

        private void EndServerProcess()
        {
            lock (_mysqldLock)
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
                Server = "localhost"
            };
            return builder.ToString();
        }

        private void StartServer(string mysqld, int port)
        {
            _serverProcess = RunCommand(mysqld,
                $"--defaults-file={Path.Combine(DatabasePath, "my.cnf")}",
                $"--datadir={DatabasePath}",
                $"--port={port}");
            TestIsRunning();
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

            _serverProcess?.Kill();
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

        private void InitializeWith(string mysqld)
        {
            Directory.CreateDirectory(DatabasePath);
            Log($"Initializing MySql in {DatabasePath}");
            var process = RunCommand(mysqld,
                "--initialize-insecure",
                $"--datadir={DatabasePath}"
            );
            process.WaitForExit();
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