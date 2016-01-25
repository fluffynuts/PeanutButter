using EmailSpooler.Win32Service.DB.Tests;
using NUnit.Framework;
using PeanutButter.TestUtils.Entity;
using PeanutButter.TestUtils.Generic;

namespace EmailSpooler.Win32Service.Entity.Tests
{
    [TestFixture]
    public class TestEmailContext: EntityPersistenceTestFixtureBase<EmailContext>
    {
        public TestEmailContext()
        {
            Configure(false, connectionString => new DbMigrationsRunnerSqlServer(connectionString));
        }

        [Test]
        public void Type_ShouldImplement_IEmailContext()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (EmailContext);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IEmailContext>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void StaticConstructor_ShouldSetInitializerToNull()
        {
            using (GetContext())
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var connection = _tempDb.CreateConnection())
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '__MigrationHistory';";
                        using (var reader = cmd.ExecuteReader())
                        {
                            Assert.IsFalse(reader.Read());
                        }
                    }
                }

                //---------------Test Result -----------------------
            }
        }


    }
}
