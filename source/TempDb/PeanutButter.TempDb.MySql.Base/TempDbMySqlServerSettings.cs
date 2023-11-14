// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using PeanutButter.Utils;
using PeanutButter.Utils.Dictionaries;

namespace PeanutButter.TempDb.MySql.Base
{
    public static class EnvironmentVariables
    {
        public const string PORT_HINT = "TEMPDB_PORT_HINT";
        public const string SQL_MODE = "TEMPDB_MYSQL_SQL_MODE";
        public const string MYSQL_MAX_STARTUP_TIME_IN_SECONDS = 
            "TEMPDB_MYSQL_MAX_STARTUP_TIME_IN_SECONDS";
        public const string VERBOSE = "TEMPDB_VERBOSE";
        public const string GRACEFUL_SHUTDOWN = "TEMPDB_GRACEFUL_SHUTDOWN";
        public const string BASE_PATH = "TEMPDB_BASE_PATH";
        public const string PREFERRED_SERVICE = "TEMPDB_MYSQL_PREFERRED_SERVICE";
        
        // timings
        public const string STARTUP_COMMAND_TIMEOUT = "TEMPDB_MYSQL_STARTUP_COMMAND_TIMEOUT";
        public const string SHUTDOWN_TIMEOUT = "TEMPDB_MYSQL_SHUTDOWN_TIMEOUT";
        public const string INIT_TIMEOUT = "TEMPDB_MYSQL_INIT_TIMEOUT";
        public const string POLL_INTERVAL = "TEMPDB_MYSQL_POLL_INTERVAL";
        public const string DISABLE_TEMPLATING = "TEMPDB_DISABLE_TEMPLATING";
    }

    public static class Defaults
    {
        public const int MIN_STARTUP_COMMAND_TIMEOUT = 5000;
        public const int DEFAULT_STARTUP_COMMAND_TIMEOUT = 30000;
        public const int MAX_STARTUP_COMMAND_TIMEOUT = 90000;
        
        public const int MIN_SHUTDOWN_TIMEOUT = 1000;
        public const int DEFAULT_SHUTDOWN_TIMEOUT = 3000;
        public const int MAX_SHUTDOWN_TIMEOUT = 10000;
        
        public const int MIN_POLL_INTERVAL = 100;
        public const int MAX_POLL_INTERVAL = 2000;
        public const int DEFAULT_POLL_INTERVAL = 500;
    }

    /// <summary>
    /// Settings for starting up a TempDbMySql instance
    /// - Settings decorated with a [Setting("setting_name")] attribute
    ///     will be included in the defaults file provided to MySql at startup
    ///     in the [mysqld] section
    /// - you may inherit from this to add settings not already provide or override
    ///     default settings values with your own instance of TempDbMySqlServerSettings
    /// </summary>
    public class TempDbMySqlServerSettings
    {
        /// <summary>
        /// Options which define how to start up mysqld
        /// </summary>
        public class TempDbOptions
        {
            /// <summary>
            /// Default minimum port to use when selecting a random port
            /// </summary>
            public const int DEFAULT_RANDOM_PORT_MIN = 13306;

            /// <summary>
            /// Default maximum port to use when selecting a random port
            /// </summary>
            public const int DEFAULT_RANDOM_PORT_MAX = 53306;

            /// <summary>
            /// The default max time to wait for a successful connection at
            /// startup, in seconds
            /// </summary>
            public const int DEFAULT_MAX_TIME_TO_CONNECT_AT_START_IN_SECONDS = 15;

            /// <summary>
            /// Flag: log attempts to locate a random, usable port to listen on
            /// </summary>
            public bool LogRandomPortDiscovery { get; set; }

            /// <summary>
            /// Minimum port to use when selecting a random port
            /// </summary>
            public int RandomPortMin { get; set; } = DEFAULT_RANDOM_PORT_MIN;

            /// <summary>
            /// Maximum port to use when selecting a random port
            /// </summary>
            public int RandomPortMax { get; set; } = DEFAULT_RANDOM_PORT_MAX;

            /// <summary>
            /// If a port hint is set (or implied via the environment
            /// variable TEMPDB_PORT_HINT), then that port is used as the
            /// first attempt for an open port and failures to bind on that
            /// port are dealt with by incrementing the attempted port number
            /// instead of randomizing. This can be useful if debugging against
            /// a tempdb instance.
            /// </summary>
            public int? PortHint { get; set; }

            /// <summary>
            /// Action to invoke when attempting to log
            /// </summary>
            public Action<string> LogAction { get; set; } = s => Trace.WriteLine(s);

            /// <summary>
            /// Full path to mysqld.exe, if you wish to specify a specific instance
            /// </summary>
            public string PathToMySqlD { get; set; }

            /// <summary>
            /// Default schema name to use
            /// </summary>
            public string DefaultSchema { get; set; } = "tempdb";

            /// <summary>
            /// Force finding mysqld in the path
            /// -> this is the default on !windows, but it can be forced there
            /// </summary>
            public bool ForceFindMySqlInPath { get; set; }

            /// <summary>
            /// Sets a name for this instance. Named instances will attempt to
            /// share schema.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Set the root password; used to default to blank
            /// but there are stupid connectors which infer that
            /// a blank password means _no_ password :/
            /// </summary>
            /// <summary>
            /// Use this as the root password; doesn't have to be secure for
            /// testing purposes, but does have to pass any default mysql
            /// password constraints for the service to start up
            /// </summary>
            public string RootUserPassword { get; set; } = DEFAULT_ROOT_PASSWORD;

            public const string DEFAULT_ROOT_PASSWORD = "root";

            /// <summary>
            /// When set, will automatically shut down the service process if
            /// still alive this long from after first properly started up and
            /// no new connections have been made in that time.
            /// </summary>
            public TimeSpan? InactivityTimeout { get; set; }

            /// <summary>
            /// When set, sets an absolute lifespan for this temp db process
            /// </summary>
            public TimeSpan? AbsoluteLifespan { get; set; }

            /// <summary>
            /// Enable verbose logging if you want to track down issues with more
            /// information at hand. This may also be enabled at run-time with the
            /// environment variable TEMPDB_VERBOSE, which overrides any setting
            /// that was provided
            /// - turn on with "1", "yes", "true"
            /// - turn off with "0", "no", "false"
            /// 
            /// </summary>
            public bool EnableVerboseLogging { get; set; }

            /// <summary>
            /// When attempting to spin up MySql, how much grace period (in seconds)
            /// to give before giving up on connecting
            /// </summary>
            public int MaxTimeToConnectAtStartInSeconds { get; set; } =
                DEFAULT_MAX_TIME_TO_CONNECT_AT_START_IN_SECONDS;

            /// <summary>
            /// When shutting down the mysql server, attempt to do so gracefully
            /// with the SHUTDOWN command, before terminating the process
            /// </summary>
            public bool? AttemptGracefulShutdown { get; set; }

            /// <summary>
            /// When set, instead of winding up a mysql instance from scratch,
            /// copy the contents of this folder to the working dir and start
            /// with that
            /// </summary>
            public string TemplateDatabasePath { get; set; }

            /// <summary>
            /// When set (default true), then:
            /// 1. if there is no template stored for the current mysql version, one is created
            /// 2. if there is a template found for the current mysql version, it's used
            /// </summary>
            public bool AutoTemplate { get; set; } = true;

            /// <summary>
            /// When set (default true), then:
            /// set the my.cnf option skip-name-resolve to (hopefully) speed things
            /// up a little
            /// </summary>
            public bool DisableHostnameLookups { get; set; } = true;
        }

        /// <summary>
        /// Allows any other configuration/settings to be specified - any duplicates of first class
        /// configuration settings will override the first class setting
        /// This configuration is of the form of nested string dictionaries which
        /// are automatically created for you - you should be able to, for instance, simply do:
        /// settings.CustomConfiguration["mysqld"]["max_connection"] = "128";
        /// </summary>
        public DefaultDictionary<string, Dictionary<string, string>> CustomConfiguration { get; }
            = new(() => new Dictionary<string, string>(), DefaultDictionaryFlags.CacheResolvedDefaults);


        /// <summary>
        /// Options for the instantiation of the temporary database
        /// </summary>
        public TempDbOptions Options { get; } = new();

        [Setting("innodb_doublewrite")]
        public int InnoDbDoubleWrite { get; set; } = 0;

        [Setting("innodb_use_native_aio")]
        public int InnoDbUseNativeAIO { get; set; } = 0;

        /// <summary>
        /// mysql setting
        /// </summary>
        [Setting("sync_relay_log_info")]
        public int SyncRelayLogInfo { get; set; } = 10000;

        /// <summary>
        /// mysql setting
        /// </summary>
        [Setting("sync_relay_log")]
        public int SyncRelayLog { get; set; } = 10000;

        /// <summary>
        /// mysql setting
        /// </summary>
        [Setting("sync_master_info")]
        public int SyncMasterInfo { get; set; } = 10000;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("binlog_row_event_max_size")]
        public string BinLogRowEventMaxSize { get; set; } = "8K";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("table_definition_cache")]
        public int TableDefinitionCache { get; set; } = 1400;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("sort_buffer_size")]
        public string SortBufferSize { get; set; } = "256K";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("open_files_limit")]
        public int OpenFilesLimit { get; set; } = 4161;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("max_connect_errors")]
        public int MaxConnectErrors { get; set; } = 100;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("max_allowed_packet")]
        public string MaxAllowedPacket { get; set; } = "64M";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("join_buffer_size")]
        public string JoinBufferSize { get; set; } = "256K";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("flush_time")]
        public int FlushTime { get; set; } = 0;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("back_log")]
        public int BackLog { get; set; } = 80;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_checksum_algorithm")]
        public int InnodbChecksumAlgorithm { get; set; } = 0;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_file_per_table")]
        public int InnodbFilePerTable { get; set; } = 1;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_stats_on_metadata")]
        public int InnodbStatsOnMetadata { get; set; } = 0;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_old_blocks_time")]
        public int InnodbOldBlocksTime { get; set; } = 1000;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_autoextend_increment")]
        public int InnodbAutoextendIncrement { get; set; } = 64;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_concurrency_tickets")]
        public int InnodbConcurrencyTickets { get; set; } = 5000;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_thread_concurrency")]
        public int InnodbThreadConcurrency { get; set; } = 0;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_log_file_size")]
        public string InnodbLogFileSize { get; set; } = "256M";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_buffer_pool_size")]
        public string InnodbBufferPoolSize { get; set; } = "512M";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_flush_log_at_trx_commit")]
        public int InnodbFlushLogAtTrxCommit { get; set; } = 2;

        /// <summary>
        /// mysql server setting
        /// </summary> 
        [Setting("innodb_flush_log_at_timeout")]
        public int InnodbFlushLogAtTimeout { get; set; } = 1;

        /// <summary>
        /// mysql server setting
        /// </summary> 
        [Setting("sync_binlog")]
        public int SyncBinLog { get; set; } = 1;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_io_capacity")]
        public int InnodbIoCapacity { get; set; } = 200;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("myisam_max_sort_file_size")]
        public string MyIsamMaxSortFileSize { get; set; } = "100G";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("thread_cache_size")]
        public int ThreadCacheSize { get; set; } = 10;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("table_open_cache")]
        public int TableOpenCache { get; set; } = 2000;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("max_connections")]
        public int MaxConnections { get; set; } = 150;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("server-id")]
        public int ServerId { get; set; } = 1;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("log-error")]
        public string LogError { get; set; } = "mysql-err.log";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("long_query_time")]
        public int LongQueryTime { get; set; } = 10;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("slow_query_log_file")]
        public string SlowQueryLogFile { get; set; } = "mysql-slow.log";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("slow-query-log")]
        public int SlowQueryLog { get; set; } = 0;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("general-log")]
        public int GeneralLog { get; set; } = 0;

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("general_log_file")]
        public string GeneralLogFile { get; set; } = "mysql-general.log";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("log-output")]
        public string LogOutput { get; set; } = "FILE";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("sql-mode")]
        public string SqlMode { get; set; } = 
            Environment.GetEnvironmentVariable(EnvironmentVariables.SQL_MODE)
                ?? "STRICT_TRANS_TABLES,STRICT_ALL_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("default-storage-engine")]
        public string DefaultStorageEngine { get; set; } = "INNODB";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("character-set-server")]
        public string CharacterSetServer { get; set; } = "utf8";

        /// <summary>
        /// mysql server setting: UNIX socket path
        /// </summary>
        [Setting("socket")]
        public string Socket { get; set; } = $"/tmp/mysql-temp-{Guid.NewGuid()}.socket";

        [Setting("default-character-set", "client", isBare: false, ignoreIfNull: true)]
        public string DefaultClientCharacterSet { get; set; }

        [Setting("performance-schema")]
        public int PerformanceSchema { get; set; } = 0;

        public TempDbMySqlServerSettings()
        {
            SetPortHintFromEnvironment();
        }

        private void SetPortHintFromEnvironment()
        {
            if (Options.PortHint != null)
            {
                // port hint was set programatically
                return;
            }

            var env = Environment.GetEnvironmentVariable(
                EnvironmentVariables.PORT_HINT
            );

            if (int.TryParse(env, out var envPort))
            {
                Options.PortHint = envPort;
            }
        }

        /// <summary>
        /// Optimises configuration for performance. Warning, this has an effect on durability in the event
        /// of a server crash. If you care about your data in the event of a system/process crash, do not
        /// use this.
        /// This overload optimises to run on a non-SSD disk
        /// </summary>
        public TempDbMySqlServerSettings OptimizeForPerformance()
        {
            return OptimizeForPerformance(false);
        }

        /// <summary>
        /// Optimises configuration for performance. Warning, this has an effect on durability in the event
        /// of a server crash. If you care about your data in the event of a system/process crash, do not
        /// use this.
        /// </summary>
        /// <param name="isRunningOnSsdDisk">Set this to true to cap off InnoDbIoCapacity to 3000 (for 
        /// spinning rust disks)</param>
        public TempDbMySqlServerSettings OptimizeForPerformance(bool isRunningOnSsdDisk)
        {
            SlowQueryLog = 0;
            GeneralLog = 0;
            InnodbThreadConcurrency = 0;
            InnodbFlushLogAtTrxCommit = 2;
            InnodbFlushLogAtTimeout = 10;
            SyncBinLog = 0;
            InnodbIoCapacity = isRunningOnSsdDisk
                ? 3000
                : InnodbIoCapacity;
            return this;
        }
    }

    public class TempDbMySqlServerSettingsBuilder : Builder<TempDbMySqlServerSettingsBuilder, TempDbMySqlServerSettings>
    {
        public TempDbMySqlServerSettingsBuilder WithName(string name)
        {
            return WithProp(o => o.Options.Name = name);
        }

        public TempDbMySqlServerSettingsBuilder SyncRelayLogInfo(int value)
        {
            return WithProp(o => o.SyncRelayLogInfo = value);
        }

        public TempDbMySqlServerSettingsBuilder WithSyncRelayLog(int value)
        {
            return WithProp(o => o.SyncRelayLog = value);
        }

        public TempDbMySqlServerSettingsBuilder WithSyncMasterInfo(int value)
        {
            return WithProp(o => o.SyncMasterInfo = value);
        }

        public TempDbMySqlServerSettingsBuilder WithBinLogRowEventMaxSize(string size)
        {
            return WithProp(o => o.BinLogRowEventMaxSize = size);
        }

        public TempDbMySqlServerSettingsBuilder WithTableDefinitionCache(int value)
        {
            return WithProp(o => o.TableDefinitionCache = value);
        }

        public TempDbMySqlServerSettingsBuilder WithSortBufferSize(string size)
        {
            return WithProp(o => o.SortBufferSize = size);
        }

        public TempDbMySqlServerSettingsBuilder WithOpenFilesLimit(int limit)
        {
            return WithProp(o => o.OpenFilesLimit = limit);
        }

        public TempDbMySqlServerSettingsBuilder WithMaxConnectionErrors(int value)
        {
            return WithProp(o => o.MaxConnectErrors = value);
        }

        public TempDbMySqlServerSettingsBuilder WithMaxAllowedPacket(string size)
        {
            return WithProp(o => o.MaxAllowedPacket = size);
        }

        public TempDbMySqlServerSettingsBuilder WithJoinBufferSize(string size)
        {
            return WithProp(o => o.JoinBufferSize = size);
        }

        public TempDbMySqlServerSettingsBuilder WithFlushTime(int seconds)
        {
            return WithProp(o => o.FlushTime = seconds);
        }

        public TempDbMySqlServerSettingsBuilder WithBackLog(int value)
        {
            return WithProp(o => o.BackLog = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbChecksumAlgorithm(int value)
        {
            return WithProp(o => o.InnodbChecksumAlgorithm = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbFilePerTable(int value)
        {
            return WithProp(o => o.InnodbFilePerTable = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbStatsOnMetadata(int value)
        {
            return WithProp(o => o.InnodbStatsOnMetadata = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbOldBlocksTime(int value)
        {
            return WithProp(o => o.InnodbOldBlocksTime = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbAutoextendIncrement(int value)
        {
            return WithProp(o => o.InnodbAutoextendIncrement = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbConcurrencyTickets(int value)
        {
            return WithProp(o => o.InnodbConcurrencyTickets = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbThreadConcurrency(int value)
        {
            return WithProp(o => o.InnodbThreadConcurrency = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbLogFileSize(string size)
        {
            return WithProp(o => o.InnodbLogFileSize = size);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbBufferPoolSize(string size)
        {
            return WithProp(o => o.InnodbBufferPoolSize = size);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbFlushLogAtTrxCommit(int value)
        {
            return WithProp(o => o.InnodbFlushLogAtTrxCommit = value);
        }


        public TempDbMySqlServerSettingsBuilder WithInnodbFlushLogAtTimeout(int value)
        {
            return WithProp(o => o.InnodbFlushLogAtTimeout = value);
        }

        public TempDbMySqlServerSettingsBuilder WithSyncBinLog(int value)
        {
            return WithProp(o => o.SyncBinLog = value);
        }

        public TempDbMySqlServerSettingsBuilder WithInnodbIoCapacity(int value)
        {
            return WithProp(o => o.InnodbIoCapacity = value);
        }

        public TempDbMySqlServerSettingsBuilder WithMyIsamMaxSortFileSize(string size)
        {
            return WithProp(o => o.MyIsamMaxSortFileSize = size);
        }

        public TempDbMySqlServerSettingsBuilder WithThreadCacheSize(int value)
        {
            return WithProp(o => o.ThreadCacheSize = value);
        }

        public TempDbMySqlServerSettingsBuilder WithTableOpenCache(int value)
        {
            return WithProp(o => o.TableOpenCache = value);
        }

        public TempDbMySqlServerSettingsBuilder WithMaxConnections(int value)
        {
            return WithProp(o => o.MaxConnections = value);
        }

        public TempDbMySqlServerSettingsBuilder WithServerId(int value)
        {
            return WithProp(o => o.ServerId = value);
        }

        public TempDbMySqlServerSettingsBuilder WithLogError(string filename)
        {
            return WithProp(o => o.LogError = filename);
        }

        public TempDbMySqlServerSettingsBuilder WithLongQueryTime(int value)
        {
            return WithProp(o => o.LongQueryTime = value);
        }

        public TempDbMySqlServerSettingsBuilder WithSlowQueryLogFile(string fileName)
        {
            return WithProp(o => o.SlowQueryLogFile = fileName);
        }

        public TempDbMySqlServerSettingsBuilder WithSlowQueryLog(int value)
        {
            return WithProp(o => o.SlowQueryLog = value);
        }

        public TempDbMySqlServerSettingsBuilder WithGeneralLog(int value)
        {
            return WithProp(o => o.GeneralLog = value);
        }

        public TempDbMySqlServerSettingsBuilder WithGeneralLogFile(string filename)
        {
            return WithProp(o => o.GeneralLogFile = filename);
        }

        public TempDbMySqlServerSettingsBuilder WithLogOutput(string value)
        {
            return WithProp(o => o.LogOutput = value);
        }

        public TempDbMySqlServerSettingsBuilder WithSqlMode(string value)
        {
            return WithProp(o => o.SqlMode = value);
        }

        public TempDbMySqlServerSettingsBuilder WithDefaultStorageEngine(string value)
        {
            return WithProp(o => o.DefaultStorageEngine = value);
        }

        public TempDbMySqlServerSettingsBuilder WithCharacterSetServer(string charset)
        {
            return WithProp(o => o.CharacterSetServer = charset);
        }

        public TempDbMySqlServerSettingsBuilder WithSocket(string socketPath)
        {
            return WithProp(o => o.Socket = socketPath);
        }

        [Setting("default-character-set", "client", isBare: false, ignoreIfNull: true)]
        public string DefaultClientCharacterSet { get; set; }

        public TempDbMySqlServerSettingsBuilder WithDefaultClientCharacterSet(string charset)
        {
            return WithProp(o => o.DefaultClientCharacterSet = charset);
        }
    }
}