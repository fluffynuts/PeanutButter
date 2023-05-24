using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using PeanutButter.Utils;
using StackExchange.Redis;

namespace PeanutButter.TempRedis
{
    /// <summary>
    /// Strategies for resolving a temporary redis server executable
    /// </summary>
    [Flags]
    public enum RedisLocatorStrategies
    {
        /// <summary>
        /// Look for redis-server.exe or redis-server (linux/osx) in the path
        /// </summary>
        FindInPath = 1,

        /// <summary>
        /// Look for an installed Redis service on windows (does not apply to linux/osx)
        /// The service must be called 'redis'
        /// </summary>
        FindAsWindowsService = 2,

        /// <summary>
        /// Attempt to download win32 binaries from Microsoft archives at github if
        /// no redis-server found in the path and the service is not installed or not
        /// found
        /// </summary>
        DownloadForWindowsIfNecessary = 4
    }

    /// <summary>
    /// Provides a temporary, disposable Redis instance
    /// levering off of a locally installed redis-server
    /// installation or binary within the path
    /// </summary>
    public interface ITempRedis : IDisposable
    {
        /// <summary>
        /// The port this instance is listening on
        /// </summary>
        int Port { get; }

        /// <summary>
        /// The resolved path to redis-server
        /// (may be passed in as a constructor parameter if necessary)
        /// </summary>
        string RedisExecutable { get; }

        /// <summary>
        /// The currently-used redis service locator strategies
        /// </summary>
        RedisLocatorStrategies LocatorStrategies { get; set; }

        /// <summary>
        /// Provides a simple mechanism to open a connection to the
        /// redis instance
        /// </summary>
        /// <returns></returns>
        ConnectionMultiplexer Connect();

        /// <summary>
        /// Starts up the server, if not auto-started at construction time
        /// (default is to auto-start)
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the redis server. You probably don't need to
        /// call this as the server will automatically be stopped
        /// when disposed
        /// </summary>
        void Stop();

        /// <summary>
        /// Restarts the server process (shortcut for Stop/Start)
        /// </summary>
        void Restart();
    }

    /// <summary>
    /// Options for construction of a TempRedis instance
    /// </summary>
    public class TempRedisOptions
    {
        /// <summary>
        /// Auto-start the service on construction? (default true)
        /// </summary>
        public bool AutoStart { get; set; } = true;

        /// <summary>
        /// Provide a custom path to the redis-server executable
        /// (default not set)
        /// </summary>
        public string PathToRedisService { get; set; }

        /// <summary>
        /// Strategies which may be employed to locate redis
        /// This is a flag set (default: all strategies)
        /// </summary>
        public RedisLocatorStrategies LocatorStrategies { get; set; } =
            RedisLocatorStrategies.FindInPath |
            RedisLocatorStrategies.FindAsWindowsService |
            RedisLocatorStrategies.DownloadForWindowsIfNecessary;

        /// <summary>
        /// Enables save-to-disk, which means redis-server should
        /// (mostly) survive a crash.
        /// </summary>
        public bool EnableSaveToDisk { get; set; } = true;

        /// <summary>
        /// When enabled (default), only bind to 127.0.0.1
        /// - default redis binding is to all interfaces,
        /// this option is defaulted to true such that the
        /// default TempRedis behavior is to bind to localhost
        /// _only_
        /// </summary>
        public bool BindToLocalhostOnly { get; set; } = true;

        /// <summary>
        /// How many databases to provide for (default is 1)
        /// </summary>
        public int DatabaseCount { get; set; } = 1;

        /// <summary>
        /// Environment variable observed for a preference of port
        /// to run on (may make debugging easier). This port is tried
        /// first, if set, and ports are sequentially tried after that
        /// in the event that it is not available.
        /// </summary>
        public const string TEMPREDIS_PORT_HINT = nameof(TEMPREDIS_PORT_HINT);

        /// <summary>
        /// The preferred port to run on. If set, TempRedis will attempt
        /// to start a server on this port. If not available, ports will
        /// be sequentially tested from this value upward to find an
        /// available one. May make debugging easier.
        /// </summary>
        public int? PortHint { get; set; } =
            int.TryParse(Environment.GetEnvironmentVariable(TEMPREDIS_PORT_HINT), out var env)
                ? env
                : null;

        /// <summary>
        /// Set this to get debug logs, eg around startup and restart
        /// </summary>
        public Action<string> DebugLogger { get; set; }
    }

    /// <inheritdoc />
    public class TempRedis : ITempRedis
    {
        /// <summary>
        /// For diagnostic purposes: monitor the actual server process
        /// </summary>
        public Process ServerProcess => _serverProcess;

        private Process _serverProcess;
        
        /// <summary>
        /// Test if the server process is running. If we're in the
        /// middle of a restart, it's possible to get the exception
        /// "No process is associated with this object" when attempting
        /// to read HasExited off of the exposed ServerProcess
        /// </summary>
        public bool ServerProcessIsRunning => ReadProcessIsRunning();

        private bool ReadProcessIsRunning()
        {
            try
            {
                return ServerProcess is not null &&
                    !ServerProcess.HasExited;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public int Port { get; }

        /// <inheritdoc />
        public string RedisExecutable =>
            _executable ??= FindRedisExecutable();

        private string _executable;

        private bool _stopped = false;
        private Thread _watcherThread;
        private AutoTempFile _configFile;
        private AutoTempFile _saveFile;
        private readonly Action<string> _logger;

        /// <summary>
        /// Possible strategies for locating a redis server executable
        /// </summary>
        public RedisLocatorStrategies LocatorStrategies { get; set; }

        /// <inheritdoc />
        public ConnectionMultiplexer Connect() =>
            ConnectionMultiplexer.Connect(
                new ConfigurationOptions()
                {
                    EndPoints =
                    {
                        {
                            "127.0.0.1",
                            Port
                        },
                    },
                    AbortOnConnectFail = false,
                    // don't want infinite - rather fail a test than stall
                    ConnectRetry = 10,
                    ConnectTimeout = 500,
                    AsyncTimeout = 1000,
                    SyncTimeout = 1000,
                }
            );


        /// <summary>
        /// Constructs an instance of the temporary server,
        /// automatically started up and using redis-server
        /// as found in your PATH or via Windows service registry,
        /// where applicable; will attempt to download redis server
        /// on a windows host if not found in your path or as a service
        /// </summary>
        public TempRedis() : this(new TempRedisOptions())
        {
        }

        /// <summary>
        /// Construct an instance of the temporary server
        /// </summary>
        /// <param name="options"></param>
        public TempRedis(TempRedisOptions options)
        {
            _logger = options.DebugLogger ?? NullLogger;
            _executable = options.PathToRedisService;
            LocatorStrategies = options.LocatorStrategies;
            Port = options.PortHint.HasValue
                ? PortFinder.FindOpenPortFrom(options.PortHint.Value)
                : PortFinder.FindOpenPort();
            GenerateConfig(options);
            if (options.AutoStart)
            {
                Start();
            }
        }

        private static void NullLogger(string s)
        {
            // intentionally left blank
        }

        private void GenerateConfig(TempRedisOptions options)
        {
            _saveFile = new AutoTempFile();
            _configFile = new AutoTempFile();
            _logger.Invoke($"writing config at {_configFile.Path} with append storage at {_saveFile.Path}");
            File.WriteAllText(
                _configFile.Path,
                @$"
# putting in the port for completeness, though it will be
# specified on the commandline to make it easier to find
# this instance via process monitoring
port {Port}
{(options.BindToLocalhostOnly ? "bind 127.0.0.1" : "")}
databases {options.DatabaseCount}
aof-load-truncated yes
appendfsync {(options.EnableSaveToDisk ? "always" : "no")}
appendonly yes
appendfilename {Path.GetFileName(_saveFile.Path)}
".Trim()
            );
        }

        /// <inheritdoc />
        public void Start()
        {
            StartInternal(startWatcher: true);
        }

        private void StartInternal(bool startWatcher)
        {
            var canStart = _serverProcess?.HasExited ?? true;
            if (!canStart)
            {
                _logger(
                    $"{nameof(TempRedis)} already running with pid: {_serverProcess.Id}"
                );
                return;
            }

            _logger($"attempting to start up on port: {Port}");

            _serverProcess = new Process()
            {
                StartInfo =
                {
                    WorkingDirectory = Path.GetDirectoryName(_configFile.Path)!,
                    FileName = RedisExecutable,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = $"\"{_configFile.Path}\" --port {Port}",
                    UseShellExecute = false
                }
            };
            if (!_serverProcess.Start())
            {
                throw new Exception($"Unable to start {RedisExecutable} on port {Port}");
            }

            _logger($"redis-server process started: {_serverProcess.Id}");
            TestServerIsUp();

            if (startWatcher)
            {
                WatchServerProcess();
            }
        }

        private void TestServerIsUp()
        {
            try
            {
                _logger("testing connection to server...");
                using var db = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions()
                    {
                        EndPoints =
                        {
                            {
                                "127.0.0.1",
                                Port
                            }
                        },
                        ConnectTimeout = 50,
                        ConnectRetry = 10,
                        AbortOnConnectFail = false
                    }
                );
                _logger("server is up!");
            }
            catch (RedisConnectionException)
            {
                if (!_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                }

                var stdout = _serverProcess.StandardOutput.ReadToEnd();
                var stderr = _serverProcess.StandardOutput.ReadToEnd();
                throw new Exception(
                    $@"Unable to start {RedisExecutable} on port {Port}:
stdout:
{stdout}
stderr:
{stderr}
"
                );
            }
        }

        private void WatchServerProcess()
        {
            var t = new Thread(
                () =>
                {
                    while (!_stopped)
                    {
                        RestartServerIfRequired();
                        Thread.Sleep(100);
                    }
                }
            );
            t.Start();
            var existingWatcher = Interlocked.Exchange(ref _watcherThread, t);
            if (existingWatcher is not null)
            {
                existingWatcher.Join();
            }
        }

        private void RestartServerIfRequired()
        {
            if (_stopped)
            {
                return;
            }

            var serverProcess = _serverProcess;
            if (serverProcess is null)
            {
                return;
            }

            if (!serverProcess.HasExited)
            {
                return;
            }

            _logger("redis-server appears to have exited... restarting...");
            _serverProcess = null;
            StartInternal(startWatcher: false);
        }

        /// <inheritdoc />
        public void Stop()
        {
            try
            {
                var watcher = Interlocked.Exchange(ref _watcherThread, null);
                _stopped = true;
                _logger("stopping redis-server watcher thread");
                watcher?.Join();
                _logger("killing redis-server");
                _serverProcess?.Kill();
                _serverProcess?.WaitForExit();
                _serverProcess = null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error stopping temp redis: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public void Restart()
        {
            _logger("manual restart invoked");
            Stop();
            Start();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
            _configFile?.Dispose();
            _configFile = null;
            _saveFile?.Dispose();
            _saveFile = null;
        }

        private string FindRedisExecutable()
        {
            var lookInPath = LocatorStrategies.HasFlag(RedisLocatorStrategies.FindInPath);
            if (lookInPath)
            {
                // prefer redis-server in the path
                var inPath = Find.InPath("redis-server");
                if (inPath is not null)
                {
                    return inPath;
                }
            }

            if (Platform.IsUnixy)
            {
                throw new NotSupportedException(
                    lookInPath
                        ? $"{nameof(TempRedis)} requires redis-server to be in your path for this platform. Is redis installed on this system? Searched folders:\n{string.Join("\n", Find.FoldersInPath)}"
                        : $"{nameof(TempRedis)} only supports finding redis-server in your path for this platform. Please enable the flag or pass in a path to redis-server."
                );
            }

            var lookForService = LocatorStrategies.HasFlag(RedisLocatorStrategies.FindAsWindowsService);
            if (lookForService)
            {
                var serviceExePath = RedisWindowsServiceFinder.FindPathToRedis();
                if (serviceExePath is not null)
                {
                    return serviceExePath;
                }
            }

            var attemptDownload = LocatorStrategies.HasFlag(RedisLocatorStrategies.DownloadForWindowsIfNecessary);
            if (!attemptDownload)
            {
                throw new NotSupportedException(
                    GenerateFailMessageFor(lookInPath, lookForService, false, null)
                );
            }

            var downloadError = "unknown";
            var result = Async.RunSync(
                () =>
                {
                    var downloader = new MicrosoftRedisDownloader();
                    try
                    {
                        return downloader.Fetch();
                    }
                    catch (Exception ex)
                    {
                        downloadError = ex.Message;
                        return null;
                    }
                }
            );

            if (result is not null)
            {
                return result;
            }

            throw new NotSupportedException(
                GenerateFailMessageFor(lookInPath, lookForService, true, downloadError)
            );
        }

        private static string GenerateFailMessageFor(
            bool lookInPath,
            bool lookForService,
            bool attemptDownload,
            string downloadError
        )
        {
            return new[]
            {
                "Unable to start up a temporary redis instance: no redis-server.exe could be found",
                lookInPath
                    ? "* Try adding the folder containing redis-server.exe to your path"
                    : "* Try enabling the FindInPath location strategy",
                lookForService
                    ? "* Try installing the Redis windows service (must be called 'redis' to be found)"
                    : "* Try enabling a search for a locally-installed Redis service",
                attemptDownload
                    ? $"* Unable to download Redis from github/Microsoft/archive: {downloadError}"
                    : "* Try enabling auto-download of Redis from github/Microsoft/archive"
            }.JoinWith("\n");
        }
    }
}