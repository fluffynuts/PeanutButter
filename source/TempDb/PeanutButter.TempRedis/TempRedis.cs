using System;
using System.Diagnostics;
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
    }

    /// <inheritdoc />
    public class TempRedis : ITempRedis
    {
        private Process _serverProcess;

        /// <inheritdoc />
        public int Port { get; }

        /// <inheritdoc />
        public string RedisExecutable =>
            _executable ??= FindRedisExecutable();

        private string _executable;

        /// <summary>
        /// Possible strategies for locating a redis server executable
        /// </summary>
        public RedisLocatorStrategies LocatorStrategies { get; set; }

        /// <inheritdoc />
        public ConnectionMultiplexer Connect() =>
            ConnectionMultiplexer.Connect(
                $"localhost:{this.Port}"
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
            _executable = options.PathToRedisService;
            LocatorStrategies = options.LocatorStrategies;
            Port = PortFinder.FindOpenPort();
            if (options.AutoStart)
            {
                Start();
            }
        }

        /// <inheritdoc />
        public void Start()
        {
            var canStart = _serverProcess?.HasExited ?? true;
            if (!canStart)
            {
                Console.WriteLine(
                    $"-- temp redis already running with pid: {_serverProcess.Id}"
                );
                return;
            }

            _serverProcess = new Process()
            {
                StartInfo =
                {
                    FileName = RedisExecutable,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = $"--port {Port}",
                    UseShellExecute = false
                }
            };
            if (!_serverProcess.Start())
            {
                throw new Exception($"Unable to start {RedisExecutable} on port {Port}");
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            try
            {
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
            Stop();
            Start();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
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
                        ? $"{nameof(TempRedis)} requires redis-server to be in your path for this platform. Is redis installed on this system?"
                        : $"{nameof(TempRedis)} only supports finding redis-server in your path for this platform. Please enable the flag or pass in a path to redis-server"
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
            var result = Async.RunSync(() =>
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
            });
            
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