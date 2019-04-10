// ReSharper disable UnusedMember.Global

using System;
using System.Diagnostics;

namespace PeanutButter.TempDb.MySql
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
        public class TempDbOptions
        {
            public const int DEFAULT_RANDOM_PORT_MIN = 13306;
            public const int DEFAULT_RANDOM_PORT_MAX = 23306;
            public bool LogRandomPortDiscovery { get; set; }
            public int RandomPortMin { get; set; } = DEFAULT_RANDOM_PORT_MIN;
            public int RandomPortMax { get; set; } = DEFAULT_RANDOM_PORT_MAX;
            public Action<string> LogAction { get; set; } = s => Trace.WriteLine(s);
            public string PathToMySqlD { get; set; }
            public string DefaultSchema { get; set; } = "tempdb";
            /// <summary>
            /// Force finding mysqld in the path
            /// -> this is the default on !windows, but it can be forced there
            /// </summary>
            public bool ForceFindMySqlInPath { get; set; }
        }

        public TempDbOptions Options { get; } = new TempDbOptions();

        [Setting("sync_relay_log_info")]
        public int SyncRelayLogInfo { get; set; } = 10000;

        [Setting("sync_relay_log")]
        public int SyncRelayLog { get; set; } = 10000;

        [Setting("sync_master_info")]
        public int SyncMasterInfo { get; set; } = 10000;

        [Setting("binlog_row_event_max_size")]
        public string BinLogRowEventMaxSize { get; set; } = "8K";

        [Setting("table_definition_cache")]
        public int TableDefinitionCache { get; set; } = 1400;

        [Setting("sort_buffer_size")]
        public string SortBufferSize { get; set; } = "256K";

        [Setting("open_files_limit")]
        public int OpenFilesLimit { get; set; } = 4161;

        [Setting("max_connect_errors")]
        public int MaxConnectErrors { get; set; } = 100;

        [Setting("max_allowed_packet")]
        public string MaxAllowedPacket { get; set; } = "64M";

        [Setting("join_buffer_size")]
        public string JoinBufferize { get; set; } = "256K";

        [Setting("flush_time")]
        public int FlushTime { get; set; } = 0;

        [Setting("back_log")]
        public int BackLog { get; set; } = 80;

        [Setting("innodb_checksum_algorithm")]
        public int InnodbChecksumAlgorithm { get; set; } = 0;

        [Setting("innodb_file_per_table")]
        public int InnodbFilePerTable { get; set; } = 1;

        [Setting("innodb_stats_on_metadata")]
        public int InnodbStatsOnMetadata { get; set; } = 0;

        [Setting("innodb_old_blocks_time")]
        public int InnodbOldBlocksTime { get; set; } = 1000;

        [Setting("innodb_autoextend_increment")]
        public int InnodbAutoextendIncrement { get; set; } = 64;

        [Setting("innodb_concurrency_tickets")]
        public int InnodbConcurrencyTickets { get; set; } = 5000;

        [Setting("innodb_thread_concurrency")]
        public int InnodbThreadConcurrency { get; set; } = 8;

        [Setting("innodb_log_file_size")]
        public string InnodbLogFileSize { get; set; } = "48M";

        [Setting("innodb_buffer_pool_size")]
        public string InnodbBufferPoolSize { get; set; } = "384M";

        [Setting("innodb_flush_log_at_trx_commit")]
        public int InnodbFlushLogAtTrxCommit { get; set; } = 1;

        [Setting("myisam_max_sort_file_size")]
        public string MyIsamMaxSortFileSize { get; set; } = "100G";

        [Setting("thread_cache_size")]
        public int ThreadCacheSize { get; set; } = 10;

        [Setting("table_open_cache")]
        public int TableOpenCache { get; set; } = 2000;

        [Setting("max_connections")]
        public int MaxConnections { get; set; } = 150;

        [Setting("server-id")]
        public int ServerId { get; set; } = 1;

        [Setting("log-error")]
        public string LogError { get; set; } = "mysql-err.log";

        [Setting("long_query_time")]
        public int LongQueryTime { get; set; } = 10;

        [Setting("slow_query_log_file")]
        public string SlowQueryLogFile { get; set; } = "mysql-slow.log";

        [Setting("slow-query-log")]
        public int SlowQueryLog { get; set; } = 1;

        [Setting("general-log")]
        public int GeneralLog { get; set; } = 0;

        [Setting("general_log_file")]
        public string GeneralLogFile { get; set; } = "mysql-general.log";

        [Setting("log-output")]
        public string LogOutput { get; set; } = "FILE";

        [Setting("sql-mode")]
        public string SqlMode { get; set; } = "STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION";

        [Setting("default-storage-engine")]
        public string DefaultStorageEngine { get; set; } = "INNODB";

        [Setting("character-set-server")]
        public string CharacterSetServer { get; set; } = "utf8";

    }
}