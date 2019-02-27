using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using static NExpect.Expectations;
using NExpect;
using PeanutButter.TempDb.MySql;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static PeanutButter.Utils.PyLike;


// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable AccessToDisposedClosure

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class TestTempDbMySql
    {
        private static TempDBMySql Create(
            string pathToMySql = null)
        {
            return new TempDBMySql(
                new TempDbMySqlServerSettings()
                {
                    Options =
                    {
                        PathToMySqlD = pathToMySql
                    }
                });
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
                    "C:\\apps\\MySQL Server 5.7\\bin\\mysqld.exe"
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
                })
                .ToArray();
        }


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


        [TestCaseSource(nameof(MySqlPathFinders))]
        public void ShouldBeAbleToCreateATable_InsertData_QueryData(
            string mysqld)
        {
            using (var sut = Create(mysqld))
            {
                // Arrange
                // Act
                using (var connection = sut.CreateConnection())
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

                using (var connection = sut.CreateConnection())
                {
                    // Assert
                    var users = connection.Query<User>(
                        "use moocakes; select * from users where id > @id; ",
                        new {id = 0});
                    Expect(users).To.Contain.Only(1).Matched.By(u =>
                        u.Id == 1 && u.Name == "Daisy the cow");
                }
            }
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [TestCaseSource(nameof(MySqlPathFinders))]
        public void Construction_ShouldCreateSchemaAndSwitchToIt(
            string mysqld)
        {
            // Arrange
            var expectedId = GetRandomInt();
            var expectedName = GetRandomAlphaNumericString(5);
            // Pre-Assert
            // Act
            using (var db = Create(mysqld))
            {
                var builder = new MySqlConnectionStringBuilder(db.ConnectionString);
                Expect(builder.Database).Not.To.Be.Null.Or.Empty();
                using (var connection = db.CreateConnection())
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = new[]
                    {
                        "create table cows (id int, name varchar(128));",
                        $"insert into cows (id, name) values ({expectedId}, '{expectedName}');"
                    }.JoinWith("\n");
                    command.ExecuteNonQuery();
                }

                using (var connection = db.CreateConnection())
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
        }

        [Test]
        [TestCaseSource(nameof(MySqlPathFinders))]
        public void ShouldBeAbleToSwitch(
            string mysqld)
        {
            using (var sut = Create(mysqld))
            {
                // Arrange
                var expected = GetRandomAlphaString(5, 10);
                // Pre-assert
                var builder = new MySqlConnectionStringBuilder(sut.ConnectionString);
                Expect(builder.Database).To.Equal("tempdb");
                // Act
                sut.SwitchToSchema(expected);
                // Assert
                builder = new MySqlConnectionStringBuilder(sut.ConnectionString);
                Expect(builder.Database).To.Equal(expected);
            }
        }

        [Test]
        [TestCaseSource(nameof(MySqlPathFinders))]
        public void ShouldBeAbleToSwitchBackAndForthWithoutLoss(
            string mysqld)
        {
            using (var sut = Create(mysqld))
            {
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
                    .To.Throw<MySqlException>();
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
        }

        private void Execute(
            ITempDB tempDb,
            string sql)
        {
            using (var conn = tempDb.CreateConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }

        private Dictionary<string, object>[] Query(
            ITempDB tempDb,
            string sql
        )
        {
            using (var conn = tempDb.CreateConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                var result = new List<Dictionary<string, object>>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        Range(reader.FieldCount)
                            .ForEach(i => row[reader.GetName(i)] = reader[i]);
                        result.Add(row);
                    }
                }

                return result.ToArray();
            }
        }
    }
}