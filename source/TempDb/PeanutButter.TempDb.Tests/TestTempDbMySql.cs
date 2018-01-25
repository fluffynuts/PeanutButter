using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using static NExpect.Expectations;
using NExpect;
using PeanutButter.TempDb.MySql;
using PeanutButter.Utils;
using IniFile = PeanutButter.INIFile.INIFile;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestTempDbMySql
    {
        [Test]
        public void ShouldImplement_ITempDB()
        {
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            Expect(sut).To.Be.An.Instance.Of<ITempDB>();
            // Assert
        }

        [Test]
        public void ShouldBeDisposable()
        {
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            Expect(sut).To.Be.An.Instance.Of<IDisposable>();
            // Assert
        }

        [TestFixture]
        public class Functionality
        {
            [Test]
            public void ShouldBeAbleToCreateATable_InsertData_QueryData()
            {
                using (var db = new TempDBMySql())
                {
                    try
                    {
                        // Arrange
                        // Act
                        using (var connection = db.CreateConnection())
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = new[]
                            {
                                "create schema moocakes;",
                                "use moocakes;",
                                "create table `users` (id int, name varchar(100));",
                                "insert into `users` (id, name) values (1, 'Daisy the cow');"
                            }.JoinWith("\n");
                            command.ExecuteNonQuery();
                        }

                        using (var connection = db.CreateConnection())
                        using (var command = connection.CreateCommand())
                        {
                            // Assert
                            command.CommandText = "select * from `users`;";
                            using (var reader = command.ExecuteReader())
                            {
                                Expect(reader.HasRows).To.Be.True();
                                while (reader.Read())
                                {
                                    Expect(reader["id"]).To.Equal(1);
                                    Expect(reader["name"]).To.Equal("Daisy the cow");
                                }
                            }
                        }
                    }
                    catch (MySqlException)
                    {
                        
                    }
                }
            }
        }

        private TempDBMySql Create()
        {
            return new TempDBMySql();
        }
    }

    [TestFixture]
    public class TestMySqlConfigGenerator
    {
        [Test]
        public void WhenCreatingOnDefaultSettings_ShouldCreateDefaultConfig()
        {
            // Arrange
            var defaultIni = new IniFile();
            defaultIni.Parse(_defaults);
            var sut = Create();
            // Pre-Assert
            // Act
            var rawResult = sut.GenerateFor(new MySqlSettings());
            var resultIni = new IniFile();
            resultIni.Parse(rawResult);
            // Assert
            Expect(defaultIni.Sections)
                .To.Be.Equivalent.To(resultIni.Sections);
            defaultIni.Sections.ForEach(section =>
            {
                var expectedSettings = defaultIni[section];
                var resultSettings = resultIni[section];
                expectedSettings.ForEach(kvp =>
                {
                    Expect(resultSettings.TryGetValue(kvp.Key, out var value))
                        .To.Be.True($"result is missing setting '{kvp.Key}'");
                    Expect(value).To.Equal(kvp.Value,
                        $"Mismatched values for setting '{kvp.Key}'");
                });
                Expect(resultSettings.Count)
                    .To.Equal(expectedSettings.Count);
            });
        }

        private MySqlConfigGenerator Create()
        {
            return new MySqlConfigGenerator();
        }

        private const string _defaults =
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
