using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Dapper;
using MySqlConnector;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static PeanutButter.Utils.PyLike;
using TimeoutException = System.TimeoutException;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AccessToDisposedClosure
namespace PeanutButter.TempDb.MySql.Connector.Tests;

[TestFixture]
[Parallelizable(ParallelScope.None)]
public class TestTempDbMySqlConnector
{
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
            var builder = new MySqlConnectionStringBuilder(db.ConnectionString);
            Expect(builder.Database)
                .Not.To.Be.Null.Or.Empty();
            Expect(builder.UserID)
                .To.Equal(TempDBMySql.DEFAULT_USER);
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
            Expect(inSchema1)
                .To.Contain.Exactly(1).Item();
            Expect(inSchema1[0]["id"])
                .To.Equal(1);
            Expect(inSchema1[0]["name"])
                .To.Equal("Daisy");

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
                    "create schema moocakes;", "use moocakes;",
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
                    new { id = 0 }
                );
                Expect(users).To.Contain.Only(1).Matched.By(u =>
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
                    "C:\\apps\\mysql-5.7\\bin\\mysqld.exe", "C:\\apps\\mysql-5.6\\bin\\mysqld.exe",
                    "C:\\apps\\MySQL Server 5.7\\bin\\mysqld.exe", "C:\\apps\\mysql-8.0.26-winx64\\bin\\mysqld.exe",
                }.Where(p =>
                    {
                        if (p == null)
                        {
                            return true;
                        }

                        var exists = Directory.Exists(p) || File.Exists(p);
                        if (!exists)
                        {
                            Console.WriteLine(
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
    public class WhenInstalledAsWindowsService
    {
        [Test]
        public void ShouldBeAbleToStartNewInstance()
        {
            // Arrange
            Expect(() =>
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

        [SetUp]
        public void Setup()
        {
            SkipIfNotOnWindows();
            SkipIfOnWindowsButNoMySqlInstalled();
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
            Expect(() =>
                {
                    using var db = Create(forcePathSearch: true);
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
    public class AutomaticDisposal
    {
        // perhaps the creator forgets to dispose
        // -> perhaps the creator is TempDb.Runner and the caller
        // of that dies before it can dispose!
        [Test]
        public void ShouldAutomaticallyDisposeAfterInactivityTimeoutHasExpired()
        {
            // Arrange
            ITempDB db;
            var disposed = new ConcurrentQueue<bool>();
            using (db = Create(inactivityTimeout: TimeSpan.FromSeconds(2)))
            {
                db.Disposed += (o, e) => disposed.Enqueue(true);
                // Act
                Expect(() =>
                    {
                        using var conn = db.OpenConnection();
                    }
                ).Not.To.Throw();
            }

            Expect(() =>
                    {
                        using var conn = db.OpenConnection();
                    }
                ).To.Throw<InvalidOperationException>()
                .With.Message.Containing("not running");
            // Assert
            Expect(disposed.ToArray())
                .To.Equal(
                    new[] { true }
                );
        }

        private void WaitFor(
            Func<bool> condition,
            int maxWaitMs
        )
        {
            WaitFor(
                condition,
                TimeSpan.FromMilliseconds(maxWaitMs)
            );
        }

        private void WaitFor(
            Func<bool> condition,
            TimeSpan maxWait
        )
        {
            var timeout = DateTime.Now + maxWait;
            while (DateTime.Now < timeout)
            {
                if (condition())
                {
                    break;
                }

                Thread.Sleep(100);
            }
        }

        [Test]
        public void ConnectionUseShouldExtendLifetime()
        {
            Retry.Max(3).Times(() =>
                {
                    // Arrange
                    var inactivitySeconds = 2;
                    var disposed = new ConcurrentQueue<bool>();
                    TempDBMySql db;
                    using (db = Create(inactivityTimeout: TimeSpan.FromSeconds(inactivitySeconds)))
                    {
                        db.Disposed += (
                            o,
                            e
                        ) =>
                        {
                            Console.Error.WriteLine(">>> dispose event handled <<<");
                            disposed.Enqueue(true);
                        };
                        // Act
                        for (var i = 0; i < 10; i++)
                        {
                            Expect(() =>
                                {
                                    using var conn = db.OpenConnection();
                                    Thread.Sleep(500);
                                }
                            ).Not.To.Throw();
                        }

                        var timeout = DateTime.Now.AddSeconds(10);
                        var stillConnected = true;
                        while (DateTime.Now < timeout && stillConnected && db.IsRunning)
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
                                : $"Received multiple dispose events: {disposed.ToArray().StringifyCollection()}"
                        );

                    Expect(() =>
                            {
                                using var conn = db.OpenConnection();
                            }
                        ).To.Throw<InvalidOperationException>()
                        .With.Message.Containing("not running");
                }
            );
        }

        [Test]
        public void AbsoluteLifespanShouldOverrideConnectionActivity()
        {
            // Arrange
            using var db = Create(
                inactivityTimeout: TimeSpan.FromSeconds(1),
                absoluteLifespan: TimeSpan.FromSeconds(3)
            );
            var connections = 0;
            var timeout = DateTime.Now.AddSeconds(45);
            // Act
            Expect(() =>
                    {
                        while (DateTime.Now <= timeout)
                        {
                            using var conn = db.OpenConnection();
                            connections++;
                            Thread.Sleep(300);
                        }

                        if (DateTime.Now > timeout)
                        {
                            throw new TimeoutException();
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

    [Test]
    public void ShouldBeAbleToRunSuperUserStuffWhenSelectingSuperUser()
    {
        // Arrange
        var sql = "set global unique_checks = 0;";
        using var sut = Create();
        var builder = new MySqlConnectionStringBuilder(sut.ConnectionString);
        Expect(builder.UserID)
            .Not.To.Equal("root");
        // Act
        Expect(() => sut.Execute(sql))
            .To.Throw();
        sut.UseSuperUser();
        Expect(() => sut.Execute(sql))
            .Not.To.Throw();

        // Assert
    }

    [Test]
    [Explicit("MySqlConnector stomps over the other side's init-connect cmd at connect time")]
    public void DefaultUserShouldExperienceConnectInit()
    {
        // Arrange
        var opts = new TempDbMySqlServerSettings()
        {
            PerformanceSchema = 0,
            MaxAllowedPacket = "128M",
            MaxConnections = 151,
            TableOpenCache = 2000,
            CharacterSetServer = "utf8mb4",
            DefaultClientCharacterSet = "utf8mb4",
            InnodbBufferPoolSize = "256M",
            SlowQueryLog = 0,
            InnodbFlushLogAtTrxCommit = 2, // reduce disk thrashing: only flush to disk once per second
            CustomConfiguration =
            {
                ["mysqld"] =
                {
                    ["init-connect"] = "SET NAMES utf8mb4 collate utf8mb4_unicode_ci",
                    ["collation-server"] = "utf8mb4_unicode_ci",

                    // https://www.percona.com/blog/how-to-deal-with-mysql-deadlocks/
                    ["innodb_print_all_deadlocks"] = "ON",

                    // https://stackoverflow.com/a/53528421
                    ["key-buffer-size"] = "16M",
                    ["tmp_table_size"] = "1M",
                    ["max_connections"] = "25",
                    ["sort_buffer_size"] = "512M",
                    ["read_buffer_size"] = "265K",
                    ["read_rnd_buffer_size"] = "512K",
                    ["join_buffer_size"] = "128K",
                    ["thread_stack"] = "196K"
                }
            }
        };

        // Act
        using var sut = Create(opts);
        var builder = new MySqlConnectionStringBuilder(sut.ConnectionString);
        Expect(builder.UserID)
            .To.Equal(TempDBMySql.DEFAULT_USER);

        builder.UserID = "root";
        using (var conn = new MySqlConnection(builder.ToString()))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT @@global.init_connect, HEX(@@global.init_connect), LENGTH(@@global.init_connect)";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"init_connect: '{reader[0]}' / hex: {reader[1]} / len: {reader[2]}");
            }
        }

        using (var logConn = new MySqlConnection(builder.ToString()))
        {
            logConn.Open();
            using var cmd = logConn.CreateCommand();
            cmd.CommandText =
                """
                set global general_log = 'ON';
                set global log_output = 'TABLE';
                """;
            cmd.ExecuteNonQuery();
        }

        builder = new MySqlConnectionStringBuilder(sut.ConnectionString);
        builder.CharacterSet = "";
        using (var conn = new MySqlConnection(builder.ToString()))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "select @@session.collation_connection;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(reader[0]);
            }
        }

        Console.WriteLine("--- start log dump ---");
        using (var conn = new MySqlConnection(builder.ToString()))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                """
                SELECT event_time, thread_id, command_type, argument
                FROM mysql.general_log
                WHERE thread_id = CONNECTION_ID()
                ORDER BY event_time;
                """;
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var eventTime = reader["event_time"];
                var threadId = reader["thread_id"];
                var commandType = reader["command_type"];
                var arg = Encoding.UTF8.GetString(reader["argument"] as byte[]);
                Console.WriteLine($"{eventTime} :: {threadId} :: {commandType} :: {arg}");
            }
        }

        Console.WriteLine("--- end log dump ---");

        var result = sut.ExecuteReader(
            "select @@session.collation_connection"
        ).First();

        // Assert
        // This _WILL_ fail because MySqlConnector is re-issuing SET NAMES
        // after connecting. Captured logs:
        /*
5/28/2026 4:38:21 PM :: 12 :: Connect :: tempdb_user@localhost on tempdb using TCP/IP
5/28/2026 4:38:21 PM :: 12 :: Query :: SET NAMES utf8mb4 collate utf8mb4_unicode_ci
5/28/2026 4:38:21 PM :: 12 :: Query :: SET NAMES utf8mb4
5/28/2026 4:38:21 PM :: 12 :: Query :: select @@session.collation_connection
5/28/2026 4:38:21 PM :: 12 :: Query :: SET NAMES utf8mb4
5/28/2026 4:38:21 PM :: 12 :: Query :: SELECT event_time, thread_id, command_type, argument
FROM mysql.general_log
WHERE thread_id = CONNECTION_ID()
ORDER BY event_time
         */
        Expect(result)
            .To.Contain.Key("@@session.collation_connection")
            .With.Value("utf8mb4_unicode_ci");
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
        TempDbMySqlServerSettings opts
    )
    {
        return new TempDBMySql(opts);
    }

    private static TempDBMySql Create(
        string pathToMySql = null,
        bool forcePathSearch = false,
        TimeSpan? inactivityTimeout = null,
        TimeSpan? absoluteLifespan = null
    )
    {
        return Create(
            new TempDbMySqlServerSettings()
            {
                Options =
                {
                    PathToMySqlD = pathToMySql,
                    ForceFindMySqlInPath = true,
                    InactivityTimeout = inactivityTimeout,
                    AbsoluteLifespan = absoluteLifespan
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
}