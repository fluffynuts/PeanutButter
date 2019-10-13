using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

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
            Expect(defaultIni.Sections)
                .To.Be.Equivalent.To(resultIni.Sections);
            defaultIni.Sections.ForEach(section =>
            {
                var expectedSettings = defaultIni[section];
                // socket is unix-specific and randomly generated
                var resultSettings = resultIni[section].Where(kvp => kvp.Key != "socket")
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                var socketSetting = resultIni[section].Where(kvp => kvp.Key == "socket");
                if (socketSetting.Any())
                {
                    var kvp = socketSetting.First();
                    var parts = kvp.Value.Split('/');
                    Expect(parts).To.Contain.Only(3).Items();
                    Expect(parts[0]).To.Be.Empty();
                    Expect(parts[1]).To.Equal("tmp");
                    Expect(parts[2]).To.End.With(".socket");
                    var noExt = Path.GetFileNameWithoutExtension(parts[2]);
                    var subParts = noExt.Split('-');
                    Expect(subParts[0]).To.Equal("mysql");
                    Expect(subParts[1]).To.Equal("temp");
                    var guidPart = subParts.Skip(2).JoinWith("-");
                    Expect(Guid.TryParse(guidPart, out var _)).To.Be.True();
                }

                expectedSettings.ForEach(kvp =>
                {
                    Expect(resultSettings.TryGetValue(kvp.Key, out var value))
                        .To.Be.True($"result is missing setting '{kvp.Key}'");
                    Expect(value).To.Equal(kvp.Value,
                        $"Mismatched values for setting '{kvp.Key}'");
                });
                Expect(resultSettings.Count)
                    .To.Equal(expectedSettings.Count, () =>
                    {
                            var extraKeys = resultSettings.Keys.Except(
                                expectedSettings.Keys);
                            var extraSettings = resultSettings.Where(
                                kvp => extraKeys.Contains(kvp.Key)
                                ).Select(KvpLine);
                            var missingKeys = expectedSettings.Keys.Except(
                                resultSettings.Keys);
                            var missingSettings = expectedSettings.Where(
                                kvp => missingKeys.Contains(kvp.Key)
                                ).Select(KvpLine);
                            var final = new List<string>();
                            if (missingSettings.Any())
                            {
                                final.Add("Missing settings;");
                                final.AddRange(missingSettings);
                            }

                            if (extraSettings.Any())
                            {
                                final.Add("Extra settings:");
                                final.AddRange(extraSettings);
                            }
                            return final.JoinWith("\n");
                            string KvpLine(KeyValuePair<string, string> kvp)
                            {
                                return $"{kvp.Key} = {kvp.Value}"; 
                            }
                    });
            });
        }

        [Test]
        public void WhenCustomSettingsSetToNull_ShouldStillGenerate()
        {
            // Arrange
            var defaultIni = new INIFile.INIFile();
            defaultIni.Parse(DEFAULTS);
            var sut = Create();
            var settings = new TempDbMySqlServerSettings {CustomConfiguration = null};

            // Act
            var rawResult = sut.GenerateFor(settings);
            var resultIni = new INIFile.INIFile();
            resultIni.Parse(rawResult);
            
            // Assert
            Expect(defaultIni.Sections)
                .To.Be.Equivalent.To(resultIni.Sections);

        }

        [Test]
        public void WhenHasCustomSettings_ShouldEmitCustomSettings()
        {
            // Arrange
            var defaultIni = new INIFile.INIFile();
            defaultIni.Parse(DEFAULTS);
            var sut = Create();
            var settings = new TempDbMySqlServerSettings();
            var key = GetRandomString(32);
            var value = GetRandomString(32);
            settings.CustomConfiguration[key] = value;
            
            // Act
            var rawResult = sut.GenerateFor(settings);
            var resultIni = new INIFile.INIFile();
            resultIni.Parse(rawResult); 
            
            // Assert
            var resultValue = resultIni.GetValue(MySqlConfigGenerator.SECTION, key);
            Expect(resultValue).To.Equal(value);
        }

        [Test]
        public void WhenCustomSettingIsDuplicateOfFirstClassSetting_ShouldOverrideFirstClassSetting()
        {
            // Arrange
            var defaultIni = new INIFile.INIFile();
            defaultIni.Parse(DEFAULTS);
            var sut = Create();
            var settings = new TempDbMySqlServerSettings
            {
                MaxConnections = -GetRandomInt(),
                CustomConfiguration = {["max_connections"] = GetRandomInt(1000, 2000).ToString()}
            };

            // Act
            var rawResult = sut.GenerateFor(settings);
            var resultIni = new INIFile.INIFile();
            resultIni.Parse(rawResult); 
            
            // Assert
            var resultValue = resultIni.GetValue(MySqlConfigGenerator.SECTION, "max_connections");
            Expect(resultValue).To.Equal(settings.CustomConfiguration["max_connections"]); 
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