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

namespace PeanutButter.TempDb.MySql
{
    public class TempDBMySql : TempDB<MySqlConnection>
    {
        public bool LogRandomPortDiscovery { get; set; }
        public int RandomPortMin { get; set; } = 13306;
        public int RandomPortMax { get; set; } = 23306;
        public Action<string> LogAction { get; set; } = Console.WriteLine;
        private readonly Random _random = new Random();
        private Process _serverProcess;
        private int _port;
        private readonly MySqlSettings _settings;
        private static object _mysqldLock = new object();
        private static string _mysqld;

        public TempDBMySql(params string[] creationScripts) 
            : this(new MySqlSettings(), creationScripts)
        {
        }

        public TempDBMySql(MySqlSettings settings, params string[] creationScripts) 
            : this(settings, o => { }, creationScripts)
        {
        }

        public TempDBMySql(
            MySqlSettings settings, 
            Action<object> beforeInit, 
            params string[] creationScripts)
        {
            _settings = settings;
        }

        private string FindMySqlD()
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
                throw new NotImplementedException("Only know how to query existing mysql service for mysqld");
            var util = new WindowsServiceUtil(mysqlService.ServiceName);
            if (!util.IsInstalled)
                throw new InvalidOperationException($"{mysqlService.ServiceName} is apparently not installed?");
            var parts = util.ServiceExe.Split(' ');
            var offset = 0;
            var mysqldPath = new List<string>();
            ;
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
            var mysqld = FindMySqlD();
            _port = FindOpenRandomPort();

            EnsureIsFolder(DatabasePath);
            InitializeWith(mysqld, _port);
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
                    generator.GenerateFor(_settings ?? new MySqlSettings()) // FIXME: base ctor is called first, which calls into init, so this is null
                )
            );
        }

        public override void Dispose()
        {
            try
            {
                // FIXME: process is not being killed!
                _serverProcess.Kill();
            }
            catch
            {
                /* ignore */
            }

            base.Dispose();
        }

        protected override string GenerateConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Port = (uint)_port;
            builder.UserID = "root";
            builder.Password = "";
            builder.Server = "localhost";
            return builder.ToString();
        }

        private void StartServer(string mysqld, int port)
        {
            _serverProcess = RunCommand(mysqld,
                $"--defaults-file={Path.Combine(DatabasePath, "my.cnf")}",
                $"--datadir={DatabasePath}",
                $"--port={port}");
            Thread.Sleep(1000);
            if (_serverProcess.HasExited)
            {
            }
        }

        private void InitializeWith(string mysqld, int port)
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


        protected int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = NextRandomPort();
            var seekingPort = true;
            Action<string> log = s =>
            {
                if (LogRandomPortDiscovery) Log(s);
            };
            while (seekingPort)
            {
                try
                {
                    log($"Attempting to bind to random port {tryThis} on any available IP address");
                    var listener = new TcpListener(IPAddress.Any, (int)tryThis);
                    log("Attempt to listen...");
                    listener.Start();
                    log("Attempt to stop listening...");
                    listener.Stop();
                    log($"HUZZAH! We have a port, squire! ({tryThis})");
                    seekingPort = false;
                }
                catch
                {
                    Thread.Sleep(rnd.Next(1, 50));
                    tryThis = NextRandomPort();
                }
            }

            return tryThis;
        }

        /// <summary>
        /// Guesses the next random port to attempt to bind to
        /// </summary>
        /// <returns></returns>
        protected int NextRandomPort()
        {
            var minPort = RandomPortMin;
            var maxPort = RandomPortMax;
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
            var logAction = LogAction;
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