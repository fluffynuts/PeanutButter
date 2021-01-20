using System;
using System.Diagnostics;
using PeanutButter.Utils;
using StackExchange.Redis;

namespace PeanutButter.TempRedis
{
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


        /// <inheritdoc />
        public ConnectionMultiplexer Connect() =>
            ConnectionMultiplexer.Connect(
                $"localhost:{this.Port}"
            );


        /// <summary>
        /// Constructs an instance of the temporary server,
        /// automatically started up and using redis-server
        /// as found in your PATH or via Windows service registry,
        /// where applicable.
        /// </summary>
        public TempRedis() : this(true)
        {
        }

        /// <summary>
        /// Construct an instance of the temporary server with the
        /// default mechanisms for finding the redis-server executable
        /// and only auto-starts if autoStart is true
        /// </summary>
        /// <param name="autoStart">flag: whether the server should be automatically started</param>
        public TempRedis(bool autoStart) : this(autoStart, null)
        {
        }

        /// <summary>
        /// Construct an instance of the temporary server
        /// </summary>
        /// <param name="autoStart">flag: whether the server should be automatically started</param>
        /// <param name="pathToRedisServer">override path to redis-server executable</param>
        public TempRedis(bool autoStart, string pathToRedisServer)
        {
            _executable = pathToRedisServer;
            Port = FindOpenRandomPort();
            if (autoStart)
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
        public void Dispose()
        {
            Stop();
        }

        private int FindOpenRandomPort()
        {
            using var finder = new PortFinder();
            return finder.Port;
        }

        private string FindRedisExecutable()
        {
            // prefer redis-server in the path
            var inPath = Find.InPath("redis-server");
            if (!(inPath is null))
            {
                return inPath;
            }

            if (!Platform.IsUnixy)
            {
                throw new NotSupportedException(
                    $"{nameof(TempRedis)} requires redis-server to be in your path. Is redis installed on this system?"
                );
            }

            var serviceExePath = RedisWindowsServiceFinder.FindPathToRedis();
            if (serviceExePath is null)
            {
                throw new NotSupportedException(
                    $"{nameof(TempRedis)} requires either redis-server.exe in your path, or the Redis windows service to be installed."
                );
            }
            return serviceExePath;
        }
    }
}