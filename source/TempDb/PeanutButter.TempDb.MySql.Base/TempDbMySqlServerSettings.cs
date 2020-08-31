// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql.Base
{
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
        public static class EnvironmentVariables
        {
            public const string PORT_HINT = "TEMPDB_PORT_HINT";
        }

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
            /// no new connections have been made in that time via OpenConnection
            /// </summary>
            public TimeSpan? InactivityTimeout { get; set; }
        }

        /// <summary>
        /// Allows any other configuration/settings to be specified - any duplicates of first class
        /// configuration settings will override the first class setting
        /// </summary>
        public Dictionary<string, string> CustomConfiguration { get; set; } = new Dictionary<string, string>();


        /// <summary>
        /// Options for the instantiation of the temporary database
        /// </summary>
        public TempDbOptions Options { get; } = new TempDbOptions();

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
        public string JoinBufferize { get; set; } = "256K";

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
        public string InnodbLogFileSize { get; set; } = "48M";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_buffer_pool_size")]
        public string InnodbBufferPoolSize { get; set; } = "384M";

        /// <summary>
        /// mysql server setting
        /// </summary>
        [Setting("innodb_flush_log_at_trx_commit")]
        public int InnodbFlushLogAtTrxCommit { get; set; } = 1;

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
        public int InnoDbIoCapacity { get; set; } = 200;

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
        public string SqlMode { get; set; } = "STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION";

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
            InnoDbIoCapacity = isRunningOnSsdDisk
                ? 3000
                : InnoDbIoCapacity;
            return this;
        }
    }

    public class TempDbMySqlServerSettingsBuilder : Builder<TempDbMySqlServerSettingsBuilder, TempDbMySqlServerSettings>
    {
        public TempDbMySqlServerSettingsBuilder WithName(string name)
        {
            return WithProp(o => o.Options.Name = name);
        }
    }
}