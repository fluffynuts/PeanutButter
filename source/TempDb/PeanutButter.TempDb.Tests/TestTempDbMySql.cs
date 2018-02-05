using System;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using static NExpect.Expectations;
using NExpect;
using PeanutButter.TempDb.MySql;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestTempDbMySql
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

            [Test]
            public void Construction_ShouldCreateSchemaAndSwitchToIt()
            {
                // Arrange
                var expectedId = GetRandomInt();
                var expectedName = GetRandomAlphaNumericString(5);
                // Pre-Assert
                // Act
                using (var db = new TempDBMySql())
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
        }

        private TempDBMySql Create()
        {
            return new TempDBMySql();
        }
    }
}
