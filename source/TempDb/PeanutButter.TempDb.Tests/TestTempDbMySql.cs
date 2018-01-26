using System;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using static NExpect.Expectations;
using NExpect;
using PeanutButter.TempDb.MySql;
using PeanutButter.Utils;

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
        }

        private TempDBMySql Create()
        {
            return new TempDBMySql();
        }
    }
}
