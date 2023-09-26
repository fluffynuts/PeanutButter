using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Dapper;
using MySql.Data.MySqlClient;
using NExpect;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using NUnit.Framework;
using PeanutButter.INI;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.Utils.PyLike;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AccessToDisposedClosure

namespace PeanutButter.TempDb.MySql.Data.Tests
{
    [TestFixture]
    [Timeout(DEFAULT_TIMEOUT)]
    public class TestTempDbMySqlData
    {
        public const int DEFAULT_TIMEOUT = 90000;
        public const int DEFAULT_RETRIES = 3;

        [Test]
        public void ShouldImplement_ITempDB()
        {
            // Arrange
            var sut = typeof(TempDBMySql);

            // Pre-Assert
            // Act
            Expect(sut).To.Implement<ITempDB>();
            // Assert
        }

        [Test]
        public void ShouldBeDisposable()
        {
            // Arrange
            var sut = typeof(TempDBMySql);

            // Pre-Assert
            // Act
            Expect(sut).To.Implement<IDisposable>();
            // Assert
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class SnapshottingAndReusing
        {
            [Test]
            [Parallelizable]
            [Retry(DEFAULT_RETRIES)]
            public void ShouldBeAbleToSnapshotAndReuseDatabaseFilesAuto()
            {
                // Arrange
                string snapshotPath;
                using (var db1 = Create())
                {
                    using var conn1 = db1.OpenConnection();
                    conn1.Execute("create table people (id int primary key not null auto_increment, name text);");
                    conn1.Execute("insert into people(name) values('bob');");
                    snapshotPath = db1.Snapshot();
                }

                Expect(snapshotPath)
                    .To.Be.A.Folder();
                Console.Error.WriteLine(
                    new
                    {
                        snapshotPath
                    }.Stringify()
                );

                // Act
                using var sut = Create(templatePath: snapshotPath);
                using var conn2 = sut.OpenConnection();
                var result = conn2.Query<Person>("select * from people");
                // Assert
                Expect(result)
                    .To.Contain.Only(1)
                    .Matched.By(o => o.Id > 0 && o.Name == "bob");
            }

            [Test]
            [Parallelizable]
            [Retry(DEFAULT_RETRIES)]
            public void ShouldBeAbleToSnapshotAndReuseDatabaseFilesSpecified()
            {
                // Arrange
                using var tempFolder = new AutoTempFolder();
                string snapshotPath;
                using (var db1 = Create())
                {
                    using var conn1 = db1.OpenConnection();
                    conn1.Execute("create table people (id int primary key not null auto_increment, name text);");
                    conn1.Execute("insert into people(name) values('bob');");
                    snapshotPath = db1.Snapshot(tempFolder.Path);
                }

                Expect(snapshotPath)
                    .To.Be.A.Folder();
                Expect(snapshotPath)
                    .To.Equal(tempFolder.Path);
                Console.Error.WriteLine(
                    new
                    {
                        snapshotPath
                    }.Stringify()
                );

                // Act
                using var sut = Create(templatePath: snapshotPath);
                using var conn2 = sut.OpenConnection();
                var result = conn2.Query<Person>("select * from people");
                // Assert
                Expect(result)
                    .To.Contain.Only(1)
                    .Matched.By(o => o.Id > 0 && o.Name == "bob");
            }

            public class Person
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }
        }


        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class WhenProvidedPathToMySqlD
        {
            [TestCaseSource(nameof(MySqlPathFinders))]
            public void Construction_ShouldCreateSchemaAndSwitchToIt(
                string mysqld
            )
            {
                // Arrange
                var expectedId = GetRandomInt();
                var expectedName = GetRandomAlphaNumericString(5);
                // Pre-Assert
                // Act
                using var db = Create(mysqld);
                var util = new MySqlConnectionStringUtil(db.ConnectionString);
                Expect(util.Database).Not.To.Be.Null.Or.Empty();
                using (var connection = db.OpenConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = new[]
                    {
                        "create table cows (id int, name varchar(128));",
                        $"insert into cows (id, name) values ({expectedId}, '{expectedName}');"
                    }.JoinWith("\n");
                    command.ExecuteNonQuery();
                }

                using (var connection = db.OpenConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select id, name from cows;";
                    using (var reader = command.ExecuteReader())
                    {
                        Expect(reader.HasRows).To.Be.True();
                        Expect(reader.Read()).To.Be.True();
                        Expect(reader["id"]).To.Equal(expectedId);
                        Expect(reader["name"]).To.Equal(expectedName);
                        Expect(reader.Read()).To.Be.False();
                        // Assert
                    }
                }
            }

            [Test]
            [TestCaseSource(nameof(MySqlPathFinders))]
            public void ShouldBeAbleToSwitch(
                string mysqld
            )
            {
                using var sut = Create(mysqld);
                // Arrange
                var expected = GetRandomAlphaString(5, 10);
                // Pre-assert
                var builder = new MySqlConnectionStringUtil(sut.ConnectionString);
                Expect(builder.Database).To.Equal("tempdb");
                // Act
                sut.SwitchToSchema(expected);
                // Assert
                builder = new MySqlConnectionStringUtil(sut.ConnectionString);
                Expect(builder.Database).To.Equal(expected);
            }

            [Test]
            [TestCaseSource(nameof(MySqlPathFinders))]
            public void ShouldBeAbleToSwitchBackAndForthWithoutLoss(
                string mysqld
            )
            {
                using var sut = Create(mysqld);
                // Arrange
                var schema1 =
                    "create table cows (id int, name varchar(100)); insert into cows (id, name) values (1, 'Daisy');";
                var schema2 =
                    "create table bovines (id int, name varchar(100)); insert into bovines (id, name) values (42, 'Douglas');";
                var schema2Name = GetRandomAlphaString(4);
                Execute(sut, schema1);

                // Pre-assert
                var inSchema1 = Query(sut, "select * from cows;");
                Expect(inSchema1).To.Contain.Exactly(1).Item();
                Expect(inSchema1[0]["id"]).To.Equal(1);
                Expect(inSchema1[0]["name"]).To.Equal("Daisy");

                // Act
                sut.SwitchToSchema(schema2Name);
                Expect(() => Query(sut, "select * from cows;"))
                    .To.Throw()
                    .With.Property(o => o.GetType().Name)
                    .Containing("MySqlException");
                Execute(sut, schema2);
                var results = Query(sut, "select * from bovines;");

                // Assert
                Expect(results).To.Contain.Exactly(1).Item();
                Expect(results[0]["id"]).To.Equal(42);
                Expect(results[0]["name"]).To.Equal("Douglas");

                sut.SwitchToSchema("tempdb");
                var testAgain = Query(sut, "select * from cows;");
                Expect(testAgain).To.Contain.Exactly(1).Item();
                Expect(testAgain[0]).To.Deep.Equal(inSchema1[0]);
            }

            [TestCaseSource(nameof(MySqlPathFinders))]
            public void ShouldBeAbleToCreateATable_InsertData_QueryData(
                string mysqld
            )
            {
                using var sut = Create(mysqld);
                // Arrange
                // Act
                using (var connection = sut.OpenConnection())
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

                using (var connection = sut.OpenConnection())
                {
                    // Assert
                    var users = connection.Query<User>(
                        "use moocakes; select * from users where id > @id; ",
                        new
                        {
                            id = 0
                        }
                    );
                    Expect(users).To.Contain.Only(1).Matched.By(
                        u =>
                            u.Id == 1 && u.Name == "Daisy the cow"
                    );
                }
            }

            public static string[] MySqlPathFinders()
            {
                // add mysql installs at the following folders
                // to test, eg 5.6 vs 5.7 & effect of spaces in the path
                return new[]
                    {
                        null, // will try to seek out the mysql installation
                        "C:\\apps\\mysql-5.7\\bin\\mysqld.exe",
                        "C:\\apps\\mysql-5.6\\bin\\mysqld.exe",
                        "C:\\apps\\MySQL Server 5.7\\bin\\mysqld.exe",
                        "C:\\apps\\mysql-server-8\\bin\\mysqld.exe",
                        "C:\\apps\\mysql-8.0.26-winx64\\bin\\mysqld.exe",
                    }.Where(
                        p =>
                        {
                            if (p == null)
                            {
                                return true;
                            }

                            var exists = Directory.Exists(p) || File.Exists(p);
                            if (!exists)
                            {
                                Console.Error.WriteLine(
                                    $"WARN: specific test path for mysql not found: {p}"
                                );
                            }

                            return exists;
                        }
                    )
                    .ToArray();
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class WhenInstalledAsWindowsService
        {
            [Test]
            public void ShouldBeAbleToStartNewInstance()
            {
                // Arrange
                Expect(
                    () =>
                    {
                        using var db = Create();
                        Expect(db.ConfigFilePath)
                            .To.Exist();
                        var config = File.ReadAllText(db.ConfigFilePath);
                        var ini = INIFile.FromString(config);
                        Expect(ini)
                            .To.Have.Section("mysqld");
                        using (db.OpenConnection())
                        {
                            // Act
                            // Assert
                        }
                    }
                ).Not.To.Throw();
            }

            [Test]
            public void ShouldBeAbleToRestart()
            {
                // Arrange
                Expect(
                    () =>
                    {
                        using var db = Create();
                        Expect(db.ConfigFilePath)
                            .To.Exist();
                        var config = File.ReadAllText(db.ConfigFilePath);
                        var ini = INIFile.FromString(config);
                        Expect(ini)
                            .To.Have.Section("mysqld");
                        using (db.OpenConnection())
                        {
                            // Act
                            // Assert
                        }

                        var originalPid = db.ServerProcessId;
                        db.Restart();

                        using (db.OpenConnection())
                        {
                        }

                        Expect(db.ServerProcessId)
                            .Not.To.Equal(originalPid);
                    }
                ).Not.To.Throw();
            }

            [Test]
            public void ShouldReportErrorsFromErrorFile()
            {
                // Arrange
                // Act
                Expect(
                        () =>
                        {
                            using var db = new TempDBMySql(
                                new TempDbMySqlServerSettings()
                                {
                                    Options =
                                    {
                                        LogAction = Console.Error.WriteLine,
                                        EnableVerboseLogging = true,
                                    },
                                    CustomConfiguration =
                                    {
                                        ["mysqld"] =
                                        {
                                            ["default-character-set"] = "utf8mb4"
                                        }
                                    }
                                }
                            );
                        }
                    ).To.Throw<UnableToInitializeMySqlException>()
                    .With.Message.Containing("unknown variable");
                // Assert
            }

            [SetUp]
            public void Setup()
            {
                SkipIfNotOnWindows();
                SkipIfOnWindowsButNoMySqlInstalled();
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class Cleanup
        {
            [Test]
            public void ShouldCleanUpResourcesWhenDisposed()
            {
                // Arrange
                using var tempFolder = new AutoTempFolder();
                using (new AutoResetter<string>(
                           () =>
                           {
                               var original = TempDbHints.PreferredBasePath;
                               TempDbHints.PreferredBasePath = tempFolder.Path;
                               return original;
                           },
                           original =>
                           {
                               TempDbHints.PreferredBasePath = original;
                           }
                       ))
                {
                    // Act
                    DbConnection conn;
                    using (var db = new TempDBMySql())
                    {
                        conn = db.OpenConnection();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = "select * from information_schema.tables limit 1";
                        using var reader = cmd.ExecuteReader();
                    }

                    Expect(() => conn.ExecuteReader("select * from information_schema.tables limit 1;"))
                        .To.Throw();

                    // Assert
                    var entries = Directory.EnumerateDirectories(
                        tempFolder.Path
                    );
                    Expect(entries).To.Be.Empty();
                }
            }
        }

        [TestFixture]
        [Explicit("relies on machine-specific setup")]
        public class FindingInPath
        {
            [Test]
            public void ShouldBeAbleToFindInPath_WhenIsInPath()
            {
                // Arrange
                Expect(
                    () =>
                    {
                        using var db = Create();
                        using (db.OpenConnection())
                        {
                            // Act
                            // Assert
                        }
                    }
                ).Not.To.Throw();
            }

            private string _envPath;

            [SetUp]
            public void Setup()
            {
                if (Platform.IsUnixy)
                {
                    // allow this test to be run on a unixy platform where
                    //  mysqld is actually in the path
                    return;
                }

                _envPath = Environment.GetEnvironmentVariable("PATH");
                if (_envPath == null)
                {
                    throw new InvalidOperationException("How can you have no PATH variable?");
                }

                var modified = $"C:\\Program Files\\MySQL\\MySQL Server 5.7\\bin;{_envPath}";
                Environment.SetEnvironmentVariable("PATH", modified);
            }

            [TearDown]
            public void TearDown()
            {
                Environment.SetEnvironmentVariable("PATH", _envPath);
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class LoggingProcessStartup
        {
            [Test]
            public void ShouldLogProcessStartupInfoToFileInDataDir()
            {
                // Arrange
                var expected = GetRandomString(5);
                Environment.SetEnvironmentVariable("LOG_PROCESS_STARTUP_TEST", expected);
                // Act
                using var db = new TempDBMySql();
                var logFile = Path.Combine(db.DataDir, "startup-info.log");
                // Assert
                Expect(logFile)
                    .To.Exist();
                var contents = File.ReadAllLines(logFile);
                Expect(contents)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        s => s.StartsWith("CLI:") &&
                            s.Contains("mysqld.exe")
                    );
                Expect(contents)
                    .To.Contain.Exactly(1)
                    .Matched.By(s => s.StartsWith("Environment"));
                Expect(contents)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        s =>
                        {
                            var trimmed = s.Trim();
                            return trimmed.StartsWith("LOG_PROCESS_STARTUP_TEST") &&
                                trimmed.EndsWith(expected);
                        }
                    );
            }

            [SetUp]
            public void Setup()
            {
                SkipIfNotOnWindows();
                SkipIfOnWindowsButNoMySqlInstalled();
            }
        }

        [Test]
        public void ConnectionStringStrangeness()
        {
            // Arrange
            var sut = new TempDBMySql(
                new TempDbMySqlServerSettings()
                {
                    Options =
                    {
                        LogAction = s => Console.Error.WriteLine($"debug: {s}"),
                    }
                }
            );
            // Act
            var connectionString = sut.ConnectionString;
            var builder = new MySqlConnectionStringBuilder(connectionString);
            // Assert
            Expect(builder.SslMode)
                .To.Equal(MySqlSslMode.Disabled);
            Expect(builder.AllowPublicKeyRetrieval)
                .To.Be.False();
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class Reset
        {
            [Test]
            public void ShouldResetConnections()
            {
                // Arrange
                using var db = new TempDBMySql();
                using var conn1 = db.OpenConnection();
                using var cmd1 = conn1.CreateCommand();
                cmd1.CommandText = "select * from information_schema.TABLES limit 1";
                using var reader1 = cmd1.ExecuteReader();
                Expect(reader1.HasRows)
                    .To.Be.True();
                // Act
                Expect(() => db.CloseAllConnections())
                    .Not.To.Throw();
                using var conn2 = db.OpenConnection();
                using var cmd2 = conn2.CreateCommand();
                cmd2.CommandText = "select * from information_schema.TABLES limit 1";
                using var reader2 = cmd2.ExecuteReader();

                // Assert
                Expect(reader2.HasRows)
                    .To.Be.True();
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class StayingAlive
        {
            [Test]
            [Explicit("flaky since allowing longer to test connect at startup")]
            public void ShouldResurrectADerpedServerWhilstNotDisposed()
            {
                // Arrange
                using var db = new TempDBMySql(
                    new TempDbMySqlServerSettings()
                    {
                        Options =
                        {
                            MaxTimeToConnectAtStartInSeconds = 0
                        }
                    }
                );
                // Act
                using (var conn = db.OpenConnection())
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"create schema moo_cakes;";
                        cmd.ExecuteNonQuery();
                        db.SwitchToSchema("moo_cakes");
                    }
                }


                using (var conn = db.OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "create table cows (id int, name text);";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "insert into cows (id, name) values (1, 'daisy');";
                    cmd.ExecuteNonQuery();
                }


                var originalId = db.ServerProcessId.Value;
                var process = Process.GetProcessById(db.ServerProcessId.Value);
                process.Kill();

                var reconnected = false;
                var maxWait = DateTime.Now.AddMilliseconds(
                    TempDBMySql.PROCESS_POLL_INTERVAL * 50
                );
                while (DateTime.Now < maxWait)
                {
                    try
                    {
                        using var conn2 = db.OpenConnection();
                        using var cmd2 = conn2.CreateCommand();
                        cmd2.CommandText = "select * from moo_cakes.cows;";
                        using (var reader = cmd2.ExecuteReader())
                        {
                            Expect(reader.Read())
                                .To.Be.True();
                        }

                        reconnected = true;
                        break;
                    }
                    catch
                    {
                        Console.Error.WriteLine("-- mysql process not yet resurrected --");
                        /* suppressed */
                    }

                    Thread.Sleep(50);
                }

                // Assert
                Expect(reconnected)
                    .To.Be.True(
                        "Should have been able to reconnect to mysql server"
                    );
                var resurrectedPid = db.ServerProcessId.Value;

                Expect(resurrectedPid)
                    .To.Be.Greater.Than(0);
                Expect(() => Process.GetProcessById(originalId))
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing(
                        "not running",
                        "Server should be dead after disposal"
                    );
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class PortHint
        {
            [TestFixture]
            [Timeout(DEFAULT_TIMEOUT)]
            public class ConfiguredFromApi
            {
                [Test]
                [Retry(3)]
                public void ShouldListenOnHintedPortWhenAvailable()
                {
                    // Arrange
                    PortFinder.ResetUsedHistory();
                    var port = PortFinder.FindOpenPort();
                    using var db = new TempDBMySql(CreateForPort(port));
                    // Act
                    var configuredPort = GrokPortFrom(db.ConnectionString);
                    // Assert
                    Expect(configuredPort - port)
                        .To.Be.Greater.Than.Or.Equal.To(0)
                        .And
                        .To.Be.Less.Than.Or.Equal.To(10);
                }

                [Test]
                [Retry(3)]
                public void ShouldIncrementPortWhenHintedPortIsNotAvailable()
                {
                    // Arrange
                    if (Environment.GetEnvironmentVariable("TEMPDB_PORT_HINT") is null)
                    {
                        Assert.Ignore("Requires TEMPDB_PORT_HINT env var to be set");
                    }

                    PortFinder.ResetUsedHistory();
                    var port = PortFinder.FindOpenPort();
                    while (PortFinder.PortIsActivelyInUse(port + 1))
                    {
                        port = PortFinder.FindOpenPort();
                    }

                    using var outer = new TempDBMySql(CreateForPort(port));
                    using var inner = new TempDBMySql(CreateForPort(port));
                    // Act
                    var outerPort = GrokPortFrom(outer.ConnectionString);
                    var innerPort = GrokPortFrom(inner.ConnectionString);
                    // Assert
                    Expect(outerPort - port)
                        .To.Be.Greater.Than.Or.Equal.To(1)
                        .And
                        .To.Be.Less.Than.Or.Equal.To(10);
                    Expect(innerPort - outerPort)
                        .To.Be.Greater.Than.Or.Equal.To(1)
                        .And
                        .To.Be.Less.Than.Or.Equal.To(10);
                }
            }

            [TestFixture]
            [Timeout(DEFAULT_TIMEOUT)]
            public class ConfiguredFromEnvironment
            {
                [Test]
                [Retry(3)]
                public void ShouldListenOnHintedPortWhenAvailable()
                {
                    // Arrange
                    PortFinder.ResetUsedHistory();
                    var port = PortFinder.FindOpenPort();
                    using (new AutoResetter<string>(
                               () => SetPortHintEnvVar(port),
                               RestorePortHintEnvVar
                           ))
                    {
                        var settings = new TempDbMySqlServerSettings()
                        {
                            Options =
                            {
                                EnableVerboseLogging = true
                            }
                        };
                        using (var db = new TempDBMySql(settings))
                        {
                            // Act
                            var configuredPort = GrokPortFrom(db.ConnectionString);
                            // Assert
                            Expect(configuredPort - port)
                                .To.Be.Greater.Than.Or.Equal.To(0)
                                .And
                                .To.Be.Less.Than.Or.Equal.To(10);
                        }
                    }
                }

                private void RestorePortHintEnvVar(string prior)
                {
                    Environment.SetEnvironmentVariable(
                        TempDbMySqlServerSettings.EnvironmentVariables.PORT_HINT,
                        prior
                    );
                }

                private string SetPortHintEnvVar(int port)
                {
                    var existing = Environment.GetEnvironmentVariable(
                        TempDbMySqlServerSettings.EnvironmentVariables.PORT_HINT
                    );
                    Environment.SetEnvironmentVariable(
                        TempDbMySqlServerSettings.EnvironmentVariables.PORT_HINT,
                        port.ToString()
                    );
                    return existing;
                }
            }

            private static TempDbMySqlServerSettings CreateForPort(int port)
            {
                return new TempDbMySqlServerSettings()
                {
                    Options =
                    {
                        PortHint = port
                    }
                };
            }

            private static int GrokPortFrom(string connectionString)
            {
                // can't use MySqlConnectionStringBuilder because
                //  of a conflict between Connector and .Data
                return connectionString
                    .Split(
                        new[]
                        {
                            ";"
                        },
                        StringSplitOptions.RemoveEmptyEntries
                    )
                    .First(part => part.StartsWith("port", StringComparison.OrdinalIgnoreCase))
                    .Split('=')
                    .Skip(1)
                    .Select(int.Parse)
                    .First();
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class SharingSchemaBetweenNamedInstances
        {
            [Test]
            public void ShouldBeAbleToQueryDumpedSchema()
            {
                // Arrange
                using var outer = new TempDBMySql(SCHEMA);
                // Act
                var dumped = outer.DumpSchema();
                using var inner = new TempDBMySql(dumped);
                var result = InsertAnimal(inner, "moo-cow");
                // Assert
                Expect(result).To.Be.Greater.Than(0);
            }

            [Test]
            [Explicit("WIP: requires a cross-platform, reliable method of IPC ...")]
            public void SimpleSchemaSharing()
            {
                // Arrange
                var name = GetRandomString(10, 20);
                var settings = TempDbMySqlServerSettingsBuilder.Create()
                    .WithName(name)
                    .Build();
                using (new TempDBMySql(
                           settings,
                           SCHEMA
                       ))
                {
                    using (var inner = new TempDBMySql(settings))
                    {
                        // Act
                        var result = InsertAnimal(inner, "cow");
                        Expect(result).To.Be.Greater.Than(0);
                    }
                }
            }

            private const string SCHEMA = "create table animals (id int primary key auto_increment, name text);";

            private int InsertAnimal(
                ITempDB db,
                string name
            )
            {
                using var conn = db.OpenConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"insert into animals (name) values ('{name}'); select LAST_INSERT_ID() as id;";
                return int.Parse(cmd.ExecuteScalar()?.ToString() ?? "0");
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class HandlingPortConflicts
        {
            [Test]
            [Retry(3)]
            public void ShouldReconfigurePortOnConflict_QuickStart()
            {
                // because sometimes a port is taken after it was tested for
                // viability :/
                // Arrange
                // Act
                using (var db1 = new TempDbMySqlWithDeterministicPort())
                {
                    using var db2 = new TempDbMySqlWithDeterministicPort();
                    using var conn1 = db1.OpenConnection();
                    using var conn2 = db2.OpenConnection();
                    // Assert
                    Expect(conn1.State)
                        .To.Equal(ConnectionState.Open);
                    Expect(conn2.State)
                        .To.Equal(ConnectionState.Open);
                    Expect(db1.Port)
                        .Not.To.Equal(db2.Port);
                }
            }

            [Test]
            [Retry(3)]
            public void ShouldReconfigurePortOnConflict_SlowerStart()
            {
                // because sometimes a port is taken after it was tested for
                // viability :/
                // Arrange
                // Act
                using (var db1 = new TempDbMySqlWithDeterministicPort())
                {
                    // with a slower start, the conflict may be with an existing
                    // tempdb (or other) mysql instance, so the connect test
                    // will appear to work -- hence the `IsMyInstance` check too
                    // -> so we ensure that we can connect to the first instance
                    using var conn1 = db1.OpenConnection();
                    using var db2 = new TempDbMySqlWithDeterministicPort();
                    using var conn2 = db2.OpenConnection();
                    // Assert
                    Expect(conn1.State)
                        .To.Equal(ConnectionState.Open);
                    Expect(conn2.State)
                        .To.Equal(ConnectionState.Open);
                    Expect(db1.Port)
                        .Not.To.Equal(db2.Port);
                }
            }

            public class TempDbMySqlWithDeterministicPort : TempDBMySql
            {
                public const int STARTING_PORT = 21000;
                private int _lastAttempt = 0;

                protected override int FindRandomOpenPort()
                {
                    return _lastAttempt > 0
                        ? (++_lastAttempt)
                        : (_lastAttempt = STARTING_PORT);
                }
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class AutomaticDisposal
        {
            // perhaps the creator forgets to dispose
            // -> perhaps the creator is TempDb.Runner and the caller
            // of that dies before it can dispose!
            [Test]
            [Parallelizable]
            [Retry(3)]
            public void ShouldAutomaticallyDisposeAfterMaxLifetimeHasExpired()
            {
                // Arrange
                TempDBMySql db;
                using (db = Create(inactivityTimeout: TimeSpan.FromSeconds(2)))
                {
                    // Act
                    Expect(
                        () =>
                        {
                            using var conn = db.OpenConnection();
                        }
                    ).Not.To.Throw();
                    while (db.IsRunning)
                    {
                        Thread.Sleep(100);
                    }
                }

                Expect(
                        () =>
                        {
                            using var conn = db.OpenConnection();
                        }
                    ).To.Throw<InvalidOperationException>()
                    .With.Message.Containing("not running");
                // Assert
            }

            [Test]
            [Retry(3)]
            [Parallelizable]
            public void ConnectionUseShouldExtendLifetime()
            {
                // Arrange
                var inactivitySeconds = 1;
                var disposed = new ConcurrentQueue<bool>();
                TempDBMySql db;
                using (db = Create(inactivityTimeout: TimeSpan.FromSeconds(inactivitySeconds)))
                {
                    db.Disposed += (o, e) =>
                    {
                        Console.Error.WriteLine(">>> dispose event handled <<<");
                        disposed.Enqueue(true);
                    };
                    // Act
                    for (var i = 0; i < 5; i++)
                    {
                        Expect(
                            () =>
                            {
                                using var conn = db.OpenConnection();
                                Thread.Sleep(500);
                            }
                        ).Not.To.Throw();
                    }

                    var timeout = DateTime.Now.AddSeconds(10);
                    var stillConnected = true;
                    while (DateTime.Now < timeout && stillConnected)
                    {
                        stillConnected = db.TryFetchCurrentConnectionCount() > 0;
                        if (stillConnected)
                        {
                            Thread.Sleep(100);
                        }
                    }

                    if (stillConnected)
                    {
                        Assert.Fail("Still appear to have connections?!");
                    }

                    while (DateTime.Now < timeout && disposed.Count == 0)
                    {
                        Thread.Sleep(100);
                    }
                }

                // Assert
                Expect(db.IsRunning)
                    .To.Be.False(() => $"db still running against {db.DatabasePath}");
                Expect(disposed.ToArray())
                    .To.Equal(
                        new[] { true },
                        () => disposed.Count == 0
                            ? $"dispose event not triggered"
                            : $"Received multiple dispose events: {disposed.ToArray().Stringify()}");

                Expect(
                        () =>
                        {
                            using var conn = db.OpenConnection();
                        }
                    ).To.Throw<InvalidOperationException>()
                    .With.Message.Containing("not running");
            }

            [Test]
            [Parallelizable]
            [Retry(3)]
            public void AbsoluteLifespanShouldOverrideConnectionActivity()
            {
                // Arrange
                using var db = Create(
                    inactivityTimeout: TimeSpan.FromSeconds(1),
                    absoluteLifespan: TimeSpan.FromSeconds(3)
                );
                var connections = 0;
                // Act
                Expect(
                        () =>
                        {
                            while (true)
                            {
                                using var conn = db.OpenConnection();
                                connections++;
                                Thread.Sleep(300);
                            }

                            // ReSharper disable once FunctionNeverReturns
                        }
                    ).To.Throw<InvalidOperationException>()
                    .With.Message.Containing("not running");
                // Assert
                Expect(connections)
                    .To.Be.Greater.Than(3);
            }
        }

        [TestFixture]
        [Timeout(DEFAULT_TIMEOUT)]
        public class CreatingUsers
        {
            [Test]
            public void ShouldBeAbleToCreateTheUserAndConnectWithThoseCredentials()
            {
                // Arrange
                using var db = Create();
                var user = "testuser";
                var password = "testuser";
                var schema = "guest_schema";
                db.CreateSchemaIfNotExists(schema);
                // Act
                db.CreateUser(user, password, schema);
                // Assert
                var builder =
                    new MySqlConnectionStringBuilder(db.ConnectionString)
                    {
                        UserID = user,
                        Password = password,
                        Database = schema
                    };
                var connectionString = builder.ToString();
                using var connection = new MySqlConnection(connectionString);
                Expect(() => connection.Open())
                    .Not.To.Throw();
            }
        }

        private static void Execute(
            ITempDB tempDb,
            string sql
        )
        {
            using var conn = tempDb.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static Dictionary<string, object>[] Query(
            ITempDB tempDb,
            string sql
        )
        {
            using var conn = tempDb.OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var result = new List<Dictionary<string, object>>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                Range(reader.FieldCount)
                    .ForEach(i => row[reader.GetName(i)] = reader[i]);
                result.Add(row);
            }

            return result.ToArray();
        }

        private static TempDBMySql Create(
            string pathToMySql = null,
            TimeSpan? inactivityTimeout = null,
            TimeSpan? absoluteLifespan = null,
            string templatePath = null
        )
        {
            return new TempDBMySql(
                new TempDbMySqlServerSettings()
                {
                    Options =
                    {
                        PathToMySqlD = pathToMySql,
                        ForceFindMySqlInPath = true,
                        LogAction = Console.Error.WriteLine,
                        InactivityTimeout = inactivityTimeout,
                        AbsoluteLifespan = absoluteLifespan,
                        EnableVerboseLogging = true,
                        TemplateDatabasePath = templatePath
                    }
                }
            );
        }


        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static void SkipIfNotOnWindows()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore("This test is designed for a windows environment");
            }
        }

        private static void SkipIfOnWindowsButNoMySqlInstalled()
        {
            var mysqlServices =
#pragma warning disable CA1416
                ServiceController.GetServices().Where(s => s.DisplayName.ToLower().Contains("mysql"));
            if (!mysqlServices.Any())
            {
                Assert.Ignore(
                    "Test only works when there is at least one mysql service installed and that service has 'mysql' in the name (case-insensitive)"
                );
            }
#pragma warning restore CA1416
        }
    }

    public class MySqlConnectionStringUtil
    {
        public string Database { get; }

        public MySqlConnectionStringUtil(
            string connectionString
        )
        {
            Database = connectionString
                .Split(';')
                .Select(p => p.Trim())
                .FirstOrDefault(p => p.StartsWith("DATABASE", StringComparison.OrdinalIgnoreCase))
                ?.Split('=')
                ?.Last();
        }
    }

    public static class IniMatchers
    {
        public static IMore<INIFile> Section(
            this IHave<INIFile> have,
            string expected
        )
        {
            return have.Compose(
                actual =>
                {
                    Expect(actual.HasSection(expected))
                        .To.Be.True(() => $"Expected to find section '{expected}' in ini file");
                }
            );
        }
    }
}