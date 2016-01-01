using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TempDb.LocalDb;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestDbSchemaImporter
    {
        private TempDBLocalDb _migratedDb;
        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            _migratedDb = CreateMigratedDb();
        }

        [OneTimeTearDown]
        public void TestFixtureTearDown()
        {
            _migratedDb.Dispose();
        }

        private TempDBLocalDb CreateMigratedDb()
        {
            var db = CreateTempDb();
            var migrator = Create(db.ConnectionString);
            migrator.MigrateToLatest();
            return db;
        }


        [Test]
        public void CleanCommentsFrom_GivenStringWithoutComments_ShouldReturnIt()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var input = "create table foo (id int primary key identity);";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.CleanCommentsFrom(input);

            //---------------Test Result -----------------------
            Assert.AreEqual(input, result);
        }

        [Test]
        public void CleanCommentsFrom_GivenStringWithSingleLineComment_ShouldRemoveIt()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected = "create table foo (id int primary key identity);";
            var input = string.Join("\r\n", "-- this is a single line comment", expected);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.CleanCommentsFrom(input);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CleanCommentsFrom_GivenStringWithMultilineComment_ShouldRemoveIt()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var expected = "create table foo (id int primary key identity);";
            var input = string.Join("\r\n", "/* this is the start of a multiline comment", "and here is some more comment */", expected);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.CleanCommentsFrom(input);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MigrateToLatest_ShouldNotThrow()
        {
            using (var db = CreateTempDb())
            {
                //---------------Set up test pack-------------------
                var migrator = new DbSchemaImporter(db.ConnectionString, TestResources.dbscript);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                migrator.MigrateToLatest();

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void MakeATempDb()
        {
            //---------------Set up test pack-------------------
            var db = CreateTempDb();
            var migrator = new DbSchemaImporter(db.ConnectionString, TestResources.dbscript);
            migrator.MigrateToLatest();
            Console.WriteLine(db.DatabaseFile);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
        }


        [TestCase("COMBlockList")]
        [TestCase("COMBlockListReason")]
        [TestCase("COMMessagePlatformOption")]
        [TestCase("COMMessageRequestLog")]
        [TestCase("COMNotificationCustomer")]
        [TestCase("COMNotificationCustomerHistory")]
        [TestCase("COMNotificationMember")]
        [TestCase("COMNotificationMemberHistory")]
        [TestCase("COMNotificationRestriction")]
        [TestCase("COMPromotionCustomer")]
        [TestCase("COMPromotionCustomerHistory")]
        [TestCase("COMPromotionMember")]
        [TestCase("COMPromotionMemberHistory")]
        [TestCase("COMProtocol")]
        [TestCase("COMProtocolOption")]
        [TestCase("COMSubscriptionOption")]
        public void ShouldHaveTableAfterMigration_(string tableName)
        {
            //---------------Set up test pack-------------------
            using (var connection = _migratedDb.CreateConnection())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = '" + tableName + "';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        Assert.IsTrue(reader.Read());
                    }
                }

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void SplitPartsOutOf_GivenStringWithNo_GO_ShouldReturnIt()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var input = RandomValueGen.GetRandomString();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.SplitPartsOutOf(input);

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(input, result[0]);
        }

        [Test]
        public void SplitPartsOutOf_GivenStringWithA_GO_ShouldReturnTheParts()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var first = RandomValueGen.GetRandomString();
            var second = RandomValueGen.GetRandomString();
            var input = first + "\r\nGO\r\n" + second;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.SplitPartsOutOf(input);

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(first, result[0]);
            Assert.AreEqual(second, result[1]);
        }


        private DbSchemaImporter Create(string connectionString = null,
                                        string schema = null)
        {
            return new DbSchemaImporter(connectionString ?? RandomValueGen.GetRandomString(1),
                                        schema ?? TestResources.dbscript);
        }


        private static TempDBLocalDb CreateTempDb()
        {
            return new TempDBLocalDb();
        }
    }
}