using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PeanutButter.Utils;
using StackExchange.Redis;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.TempRedis
#else
    PeanutButter.TempRedis
#endif
{
    /// <summary>
    /// Strategies for resolving a temporary redis server executable
    /// </summary>
    [Flags]
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        enum RedisLocatorStrategies
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
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface ITempRedis : IDisposable
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
        /// Flag: true when the redis server is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Exposes the internal disposed flag for reading
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Database 0 - for simple store/retrieve operations
        /// </summary>
        IDatabase DefaultDatabase { get; }

        /// <summary>
        /// When the server is running, this is an easy way
        /// to get at the PID; if you get null, it's not running.
        /// </summary>
        int? ServerProcessId { get; }

        /// <summary>
        /// Provides a simple mechanism to open a connection to the
        /// redis instance
        /// </summary>
        /// <returns></returns>
        ConnectionMultiplexer Connect();

        /// <summary>
        /// Connect to the redis instance with your own options
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        ConnectionMultiplexer Connect(ConfigurationOptions config);

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

        /// <summary>
        /// Stores the value at the redis server
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        void Store<T>(string key, T value);

        /// <summary>
        /// Attempts to fetch the value of the provided key
        /// - will return default(T) if missing
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        T Fetch<T>(string key);

        /// <summary>
        /// Attempts to fetch the value of the provided key
        /// - will null if missing
        /// - shorthand for Fetch&lt;string&gt;(key)
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        string Fetch(string key);

        /// <summary>
        /// Attempts to fetch the value of the provided key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryFetch<T>(string key, out T result);

        /// <summary>
        /// Creates a connection to the provided database
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        IDatabase ConnectAndSelectDatabase(int db);

        /// <summary>
        /// Fetch all keys at the default database
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> FetchKeys();

        /// <summary>
        /// Fetch all keys matching
        /// </summary>
        /// <param name="matching"></param>
        /// <returns></returns>
        IEnumerable<string> FetchKeys(string matching);

        /// <summary>
        /// Flush all keys from the default database (0)
        /// </summary>
        void FlushAll();
    }

    /// <summary>
    /// Options for construction of a TempRedis instance
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class TempRedisOptions
    {
        /// <summary>
        /// name of the environment variable which is observed to disable
        /// the automatically-enabled feature to store redis data on-disc
        /// </summary>
        public const string ENV_VAR_DISABLE_DISK = "TEMPREDIS_DISABLE_DISK";

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
        public bool EnableSaveToDisk { get; set; } =
            (
                Environment.GetEnvironmentVariable(ENV_VAR_DISABLE_DISK)
                ?? "1"
            )
            .AsBoolean();

        /// <summary>
        /// When enabled (default), only bind to localhost
        /// - preferring 127.0.0.1 over ::1
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
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class TempRedis : ITempRedis
    {
        /// <summary>
        /// For diagnostic purposes: monitor the actual server process
        /// </summary>
        public Process ServerProcess => _serverProcess;

        /// <inheritdoc />
        public int? ServerProcessId => _serverProcess?.Id;

        private Process _serverProcess;
        private AutoDisposer _autoDisposer = new();

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

        /// <inheritdoc />
        public bool IsRunning => _running;

        private string _executable;

        private Thread _watcherThread;
        private AutoTempFile _configFile;
        private AutoTempFile _saveFile;
        private readonly Action<string> _logger;
        private bool _running;
        private readonly SemaphoreSlim _runningLock = new(1, 1);

        /// <summary>
        /// Exposes the internal disposed flag for reading
        /// </summary>
        public bool IsDisposed => _disposed;

        private bool _disposed;

        /// <summary>
        /// Sets the default connect timeout for new instances of TempRedis::Connect() (2000)
        /// </summary>
        public static int DefaultConnectTimeoutMilliseconds { get; set; } = 2000;
        /// <summary>
        /// Sets the default connect retry count for new instances of TempRedis::Connect() (15)
        /// </summary>
        public static int DefaultConnectRetry { get; set; } = 15;
        /// <summary>
        /// Sets the default async timeout for new instances of TempRedis::Connect() (2000)
        /// </summary>
        public static int DefaultAsyncTimeoutMilliseconds { get; set; } = 2000;
        /// <summary>
        /// Sets the default sync timeout for new instances of TempRedis::Connect() (2000)
        /// </summary>
        public static int DefaultSyncTimeoutMilliseconds { get; set; } = 2000;

        /// <summary>
        /// Possible strategies for locating a redis server executable
        /// </summary>
        public RedisLocatorStrategies LocatorStrategies { get; set; }

        /// <inheritdoc />
        public ConnectionMultiplexer Connect()
        {
            var result = Connect(
                new ConfigurationOptions()
                {
                    AbortOnConnectFail = false,
                    // don't want infinite - rather fail a test than stall
                    ConnectRetry = DefaultConnectRetry,
                    ConnectTimeout = DefaultConnectTimeoutMilliseconds,
                    AsyncTimeout = DefaultAsyncTimeoutMilliseconds,
                    SyncTimeout = DefaultSyncTimeoutMilliseconds,
                    AllowAdmin = true
                }
            );
            return result;
        }

        /// <inheritdoc />
        public ConnectionMultiplexer Connect(
            ConfigurationOptions options
        )
        {
            return IfNotDisposed(
                () => ConnectInternal(options)
            );
        }

        private ConnectionMultiplexer ConnectInternal(
            ConfigurationOptions options
        )
        {
            options.EndPoints.Clear();
            options.EndPoints.Add("127.0.0.1", Port);
            return _autoDisposer.Add(
                ConnectionMultiplexer.Connect(options)
            );
        }

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
            if (options.DebugLogger is null && Debugger.IsAttached)
            {
                _logger = s => Console.Error.WriteLine($"TempRedis: {s}");
            }

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
{(options.BindToLocalhostOnly ? $"bind {ListLocalHostAddresses()}" : "")}
databases {options.DatabaseCount}
aof-load-truncated yes
appendfsync {(options.EnableSaveToDisk ? "always" : "no")}
appendonly yes
appendfilename {Path.GetFileName(_saveFile.Path)}
".Trim()
            );
        }

        private string ListLocalHostAddresses()
        {
            var entry = Dns.GetHostEntry("localhost");

            var addresses = entry.AddressList
                // prefer 127.0.0.1 over ::1
                .OrderBy(a => a.AddressFamily == AddressFamily.InterNetworkV6)
                .Select(a => $"{a}")
                .ToArray();
            _localHostIp = addresses.First();
            return string.Join(" ", addresses);
        }

        /// <inheritdoc />
        public void Start()
        {
            IfNotDisposed(
                () => StartInternal(startWatcher: true)
            );
        }

        private void Log(Func<string> messageGenerator)
        {
            try
            {
                Log(messageGenerator());
            }
            catch
            {
                // suppress
            }
        }

        private void Log(string message)
        {
            try
            {
                _logger(message);
            }
            catch
            {
                // suppress
            }
        }

        private void StartInternal(bool startWatcher, int attempt = 0)
        {
            using (var _ = new AutoLocker(_runningLock))
            {
                if (ServerProcessIsRunning)
                {
                    Log(
                        () =>
                            $"{nameof(TempRedis)} already running with pid: {_serverProcess.Id}"
                    );
                    return;
                }

                if (_running)
                {
                    if (!CanConnect())
                    {
                        _running = false;
                    }
                    else
                    {
                        // server process may have bean dead and restarted in time
                        Log(
                            () =>
                                $"{nameof(TempRedis)} already running with pid: {ServerProcessId}"
                        );
                        return;
                    }
                }
            }

            var serverProcess = Interlocked.Exchange(ref _serverProcess, null);
            var canStart = serverProcess?.HasExited ?? true;
            if (!canStart)
            {
                Log(
                    $"{nameof(TempRedis)} already running with pid: {serverProcess.Id}"
                );
                return;
            }

            Log($"attempting to start up on port: {Port}");

            serverProcess = new Process()
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
            if (!serverProcess.Start())
            {
                throw new Exception($"Unable to start {RedisExecutable} on port {Port}");
            }

            Log($"redis-server process started: {serverProcess.Id}");
            Interlocked.Exchange(ref _serverProcess, serverProcess);
            TestServerIsUp();
            _running = true;

            if (startWatcher)
            {
                WatchServerProcess();
            }
        }

        private void TestServerIsUp()
        {
            try
            {
                Log("testing connection to server...");
                using var db = ConnectionMultiplexer.Connect(
                    new ConfigurationOptions()
                    {
                        EndPoints =
                        {
                            {
                                _localHostIp, Port
                            }
                        },
                        ConnectTimeout = 50,
                        ConnectRetry = 10,
                        AbortOnConnectFail = false
                    }
                );
                Log("server is up!");
            }
            catch (RedisConnectionException)
            {
                var serverProcess = Interlocked.Exchange(ref _serverProcess, null);
                if (!serverProcess.HasExited)
                {
                    serverProcess.Kill();
                }

                var stdout = serverProcess.StandardOutput.ReadToEnd();
                var stderr = serverProcess.StandardOutput.ReadToEnd();
                throw new Exception(
                    $@"Unable to start {RedisExecutable} on port {Port}:
stdout:
{stdout}
stderr:
{stderr}
"
                );
            }
            catch (ObjectDisposedException)
            {
                // suppress - perhaps a bug in StackExchange.Redis?
            }
        }

        private void WatchServerProcess()
        {
            if (_watcherThread is not null)
            {
                // already watching
                return;
            }

            _watcherCancellationTokenSource = new CancellationTokenSource();
            _watcherThread = new Thread(
                () =>
                {
                    try
                    {
                        while (!_watcherCancellationTokenSource.IsCancellationRequested)
                        {
                            RestartServerIfRequired();
                            Thread.Sleep(100);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        // suppress, just exit out
                    }
                    catch (Exception ex)
                    {
                        Log($"{nameof(WatchServerProcess)} died: {ex}");
                    }
                }
            );
            _watcherThread.Start();
        }

        private void RestartServerIfRequired()
        {
            if (!_runningLock.Wait(50))
            {
                return;
            }

            try
            {
                if (_disposed || !_running)
                {
                    return;
                }

                if (ServerProcessIsRunning && CanConnect())
                {
                    return;
                }

                _running = false;
            }
            finally
            {
                _runningLock.Release();
            }

            try
            {
                Log("redis-server appears to have exited... restarting...");
                var serverProcess = Interlocked.Exchange(ref _serverProcess, null);
                try
                {
                    if (!serverProcess.HasExited)
                    {
                        serverProcess.Kill();
                    }
                }
                catch
                {
                    // suppress
                }

                StartInternal(startWatcher: false);
            }
            catch (Exception ex)
            {
                // don't want to leave an unhandled exception in a background thread!
                Log($"an attempt to start redis-server failed: {ex}; next round of server-checks will try again");
            }
        }

        private bool CanConnect()
        {
            try
            {
                ConnectInternal(
                    new ConfigurationOptions()
                    {
                        ConnectTimeout = 2000
                    }
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            IfNotDisposed(() => StopInternal(runnerLocked: false));
        }

        /// <inheritdoc />
        public void Restart()
        {
            Log("manual restart invoked");
            IfNotDisposed(
                () =>
                {
                    {
                        StopInternal(runnerLocked: false);
                        StartInternal(startWatcher: true);
                    }
                }
            );
        }

        /// <inheritdoc />
        public IDatabase DefaultDatabase
            => _defaultDatabase ??= ConnectAndSelectDatabase(0);

        private ConnectionMultiplexer DefaultConnection
            => _defaultConnection ??= Connect();

        // FIXME: should be disposed
        private ConnectionMultiplexer _defaultConnection;

        private IDatabase _defaultDatabase;

        /// <inheritdoc />
        public IDatabase ConnectAndSelectDatabase(int db)
        {
            return DefaultConnection.GetDatabase(db);
        }

        /// <inheritdoc />
        public IEnumerable<string> FetchKeys()
        {
            return FetchKeys("*");
        }

        /// <inheritdoc />
        public IEnumerable<string> FetchKeys(string matching)
        {
            var server = FetchDefaultServer();
            foreach (var k in server.Keys(pattern: matching))
            {
                yield return $"{k}";
            }
        }

        private IServer FetchDefaultServer()
        {
            // this overly convoluted code brought to you by
            // https://stackexchange.github.io/StackExchange.Redis/KeysScan.html
            // -> because it's so much more convenient than exposing at the DB
            //    level, right? Even though I don't "have to remember which server
            //    I connected to", this is not a great DX.
            var conn = DefaultConnection;
            var endpoints = conn.GetEndPoints();
            var target = endpoints.FirstOrDefault(
                o => o.AddressFamily != AddressFamily.InterNetworkV6
            ) ?? endpoints.FirstOrDefault();
            var server = DefaultConnection.GetServer(target);
            return server;
        }

        /// <inheritdoc />
        public void FlushAll()
        {
            var server = FetchDefaultServer();
            Retry.Max(10).Times(() => server.FlushAllDatabases());
        }

        /// <inheritdoc />
        public void Store<T>(string key, T value)
        {
            if (TypeIsSimpleEnough<T>())
            {
                DefaultDatabase.StringSet(key, $"{value}");
                return;
            }

            throw new NotImplementedException(
                $"{nameof(TempRedis)}.{nameof(Store)} is for convenience - no complex types are handled: rather serialise to string in your own code."
            );
        }

        private bool TypeIsSimpleEnough<T>()
        {
            // used to decide what we will attempt to store
            var type = typeof(T);
            return TypeIsSimpleEnough(type);
        }

        private bool TypeIsSimpleEnough(
            Type type
        )
        {
            return type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(DateTime) ||
                type == typeof(TimeSpan);
        }

        /// <inheritdoc />
        public T Fetch<T>(string key)
        {
            TryFetch<T>(key, out var result);
            return result;
        }

        /// <inheritdoc />
        public string Fetch(string key)
        {
            TryFetch<string>(key, out var result);
            return result;
        }

        /// <inheritdoc />
        public bool TryFetch<T>(string key, out T result)
        {
            result = default;
            var type = typeof(T);
            if (TypeIsSimpleEnough(type))
            {
                var stringValue = (string)DefaultDatabase.StringGet(key);
                if (stringValue is null)
                {
                    return false;
                }

                try
                {
                    result = (T)Convert.ChangeType(
                        stringValue,
                        type
                    );
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            throw new NotImplementedException(
                $"{nameof(TempRedis)}.{nameof(TryFetch)} is for convenience - no complex types are handled: rather serialise to string in your own code."
            );
        }

        private void StopInternal(bool runnerLocked)
        {
            if (!runnerLocked)
            {
                _runningLock.Wait();
            }

            try
            {
                _running = false;
                Log("stopping redis-server watcher thread");
                Log("killing redis-server");
                ClearDefaultConnectionAndDatabase();
                var serverProcess = Interlocked.Exchange(ref _serverProcess, null);
                serverProcess?.Kill();
                serverProcess?.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error stopping temp redis: {ex.Message}");
            }
            finally
            {
                if (!runnerLocked)
                {
                    _runningLock.Release();
                }
            }
        }

        private void ClearDefaultConnectionAndDatabase()
        {
            _defaultConnection?.Close();
            _defaultConnection = null;
            _defaultDatabase = null;
        }

        private void IfNotDisposed(Action toRun)
        {
            using var _ = new AutoLocker(_disposedLock);
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(TempRedis),
                    "This instance has already been disposed"
                );
            }

            toRun();
        }

        private T IfNotDisposed<T>(Func<T> toRun)
        {
            using var _ = new AutoLocker(_disposedLock);
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(TempRedis),
                    "This instance has already been disposed"
                );
            }

            return toRun();
        }

        private readonly SemaphoreSlim _disposedLock = new(1, 1);
        private string _localHostIp;
        private CancellationTokenSource _watcherCancellationTokenSource;

        /// <inheritdoc />
        public void Dispose()
        {
            using var _2 = new AutoLocker(_disposedLock);
            if (_disposed)
            {
                return;
            }

            using var _1 = new AutoLocker(_runningLock);
            StopInternal(runnerLocked: true);
            // _disposed flag _must_ be set after stopping
            // otherwise StopInternal will throw as it
            // will think the instance is disposed
            _disposed = true;
            _autoDisposer?.Dispose();
            _autoDisposer = null;
            _watcherCancellationTokenSource.Cancel();
            _watcherThread?.Join();
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