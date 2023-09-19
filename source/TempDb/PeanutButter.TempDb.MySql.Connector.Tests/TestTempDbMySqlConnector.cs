using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Dapper;
using NExpect;
using NUnit.Framework;
using PeanutButter.TempDb.MySql.Base;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static PeanutButter.Utils.PyLike;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AccessToDisposedClosure
namespace PeanutButter.TempDb.MySql.Connector.Tests
{
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
                var builder = new MySqlConnectionStringUtil(db.ConnectionString);
                Expect(builder.Database).Not.To.Be.Null.Or.Empty();
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
                Expect(
                    () =>
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
                using var db = Create(inactivityTimeout: TimeSpan.FromSeconds(2));
                var disposed = false;
                db.Disposed += (o, e) => disposed = true;
                // Act
                Expect(
                    () =>
                    {
                        using var conn = db.OpenConnection();
                    }
                ).Not.To.Throw();
                if (Debugger.IsAttached)
                {
                    Thread.Sleep(300000);
                }
                else
                {
                    Thread.Sleep(3000);
                }

                Expect(
                        () =>
                        {
                            using var conn = db.OpenConnection();
                        }
                    ).To.Throw()
                    .With.Property(
                        e => new
                        {
                            Type = e.GetType().Name,
                            IsConnectionError = e.Message.ToLowerInvariant().Contains("unable to connect")
                        }
                    ).Deep.Equal.To(
                        new
                        {
                            Type = "MySqlException",
                            IsConnectionError = true
                        }
                    );
                // Assert
                Expect(disposed)
                    .To.Be.True();
            }

            [Test]
            public void ConnectionUseShouldExtendLifetime()
            {
                // Arrange
                using var db = Create(inactivityTimeout: TimeSpan.FromSeconds(1));
                var disposed = false;
                db.Disposed += (o, e) => disposed = true;
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

                Thread.Sleep(2000);

                Expect(
                        () =>
                        {
                            using var conn = db.OpenConnection();
                        }
                    ).To.Throw()
                    .With.Property(
                        e => new
                        {
                            Type = e.GetType().Name,
                            IsConnectionError = e.Message.ToLowerInvariant().Contains("unable to connect")
                        }
                    ).Deep.Equal.To(
                        new
                        {
                            Type = "MySqlException",
                            IsConnectionError = true
                        }
                    );
                // Assert
                Expect(disposed)
                    .To.Be.True();
            }

            [Test]
            [Timeout(45000)]
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
                    ).To.Throw()
                    .With.Property(
                        e => new
                        {
                            Type = e.GetType().Name,
                            IsConnectionError = e.Message.ToLowerInvariant().Contains("unable to connect")
                        }
                    ).Deep.Equal.To(
                        new
                        {
                            Type = "MySqlException",
                            IsConnectionError = true
                        }
                    );
                // Assert
                Expect(connections)
                    .To.Be.Greater.Than(3);
            }
        }

        [Test]
        public void StatsFetcher()
        {
            // Arrange
            using var db = Create();
            // Act
            var count = MySqlPoolStatsFetcher.FetchSessionCountFor(db.ConnectionString);
            Expect(count)
                .To.Equal(0);
            using (var outer = db.OpenConnection())
            {
                using (var inner = db.OpenConnection())
                {
                    count = MySqlPoolStatsFetcher.FetchSessionCountFor(db.ConnectionString);
                    Expect(count)
                        .To.Equal(2);
                }
            }

            count = MySqlPoolStatsFetcher.FetchSessionCountFor(db.ConnectionString);
            Expect(count)
                .To.Equal(0);

            // Assert
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
            bool forcePathSearch = false,
            TimeSpan? inactivityTimeout = null,
            TimeSpan? absoluteLifespan = null
        )
        {
            return new TempDBMySql(
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
                ServiceController.GetServices().Where(s => s.DisplayName.ToLower().Contains("mysql"));
            if (!mysqlServices.Any())
            {
                Assert.Ignore(
                    "Test only works when there is at least one mysql service installed and that service has 'mysql' in the name (case-insensitive)"
                );
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
    }
}