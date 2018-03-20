using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestMySqlConfigGenerator
    {
        [Test]
        public void WhenCreatingOnDefaultSettings_ShouldCreateDefaultConfig()
        {
            // Arrange
            var defaultIni = new INIFile.INIFile();
            defaultIni.Parse(DEFAULTS);
            var sut = Create();
            // Pre-Assert
            // Act
            var rawResult = sut.GenerateFor(new TempDbMySqlServerSettings());
            var resultIni = new INIFile.INIFile();
            resultIni.Parse(rawResult);
            // Assert
            Expectations.Expect(defaultIni.Sections)
                .To.Be.Equivalent.To(resultIni.Sections);
            defaultIni.Sections.ForEach(section =>
            {
                var expectedSettings = defaultIni[section];
                var resultSettings = resultIni[section];
                expectedSettings.ForEach(kvp =>
                {
                    Expectations.Expect(resultSettings.TryGetValue(kvp.Key, out var value))
                        .To.Be.True($"result is missing setting '{kvp.Key}'");
                    Expectations.Expect(value).To.Equal(kvp.Value,
                        $"Mismatched values for setting '{kvp.Key}'");
                });
                Expectations.Expect(resultSettings.Count)
                    .To.Equal(expectedSettings.Count);
            });
        }

        private MySqlConfigGenerator Create()
        {
            return new MySqlConfigGenerator();
        }

        private const string DEFAULTS =
            @"[mysqld]
character-set-server=utf8
default-storage-engine=INNODB
sql-mode=""STRICT_TRANS_TABLES,NO_AUTO_CREATE_USER,NO_ENGINE_SUBSTITUTION""
log-output=FILE
general-log=0
general_log_file=""mysql-general.log""
slow-query-log=1
slow_query_log_file=""mysql-slow.log""
long_query_time=10
log-error=""mysql-err.log""
server-id=1
max_connections=150
table_open_cache=2000
thread_cache_size=10
myisam_max_sort_file_size=100G
innodb_flush_log_at_trx_commit=1
innodb_buffer_pool_size=384M
innodb_log_file_size=48M
innodb_thread_concurrency=8
innodb_autoextend_increment=64
innodb_concurrency_tickets=5000
innodb_old_blocks_time=1000
innodb_stats_on_metadata=0
innodb_file_per_table=1
innodb_checksum_algorithm=0
back_log=80
flush_time=0
join_buffer_size=256K
max_allowed_packet=64M
max_connect_errors=100
open_files_limit=4161
sort_buffer_size=256K
table_definition_cache=1400
binlog_row_event_max_size=8K
sync_master_info=10000
sync_relay_log=10000
sync_relay_log_info=10000
";
    }
}