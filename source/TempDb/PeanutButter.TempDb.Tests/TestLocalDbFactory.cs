using System;
using System.Data.SqlClient;
using System.IO;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Generic;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestLocalDbFactory
    {
        [Test]
        public void Construct_GivenNoParameters_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new LocalDbFactory());

            //---------------Test Result -----------------------
        }

        [Test]
        public void Type_ShouldImplement_ILocalDbFactory()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(LocalDbFactory);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<ILocalDbFactory>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenInstanceName_ShouldUseThatName()
        {
            //---------------Set up test pack-------------------
            var instanceName = RandomValueGen.GetRandomString();
            var sut = new LocalDbFactory(instanceName);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.AreEqual(instanceName, sut.InstanceName);
            var connectionString = sut.GetMasterConnectionString();


            //---------------Test Result -----------------------
            StringAssert.Contains(instanceName, connectionString);
        }



        [Test]
        public void CreateDatabase_GivenFileNameAndDatabaseName_ShouldCreateDatabase()
        {
            //---------------Set up test pack-------------------
            using (var destroyer = new LocalDbDestroyer())
            {
                var sut = Create();
                var dbName = RandomValueGen.GetRandomAlphaNumericString(5, 10);
                var dbFile = Path.GetTempFileName() + ".sdb";
                destroyer.DatabaseFile = dbFile;
                destroyer.DatabaseName = dbName;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.CreateDatabase(dbName, dbFile);

                //---------------Test Result -----------------------
                var builder = new SqlConnectionStringBuilder
                {
                    InitialCatalog = dbName,
                    DataSource = $"(localdb)\\{new LocalDbInstanceEnumerator().FindFirstAvailableInstance()}",
                    IntegratedSecurity = true
                };
                using (var conn = new SqlConnection(builder.ToString()))
                {
                    Assert.DoesNotThrow(() => conn.Open());
                }
            }
        }

        private ILocalDbFactory Create()
        {
            return new LocalDbFactory();
        }


        public class LocalDbDestroyer: IDisposable
        {
            public string DatabaseFile { get; set; }
            public string DatabaseName { get; set; }
            public string InstanceName { get; set; } = new LocalDbInstanceEnumerator().FindFirstAvailableInstance();
            private const string MASTER_CONNECTION_STRING = @"Data Source=(localdb)\{0};Initial Catalog=master;Integrated Security=True";

            public string GetMasterConnectionString()
            {
                return string.Format(MASTER_CONNECTION_STRING, InstanceName);
            }

            private void DeleteTemporaryDatabase()
            {
                if (string.IsNullOrWhiteSpace(DatabaseFile) ||
                    string.IsNullOrWhiteSpace(DatabaseName))
                    return;
                using (var connection = new SqlConnection(GetMasterConnectionString()))
                {
                    connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = string.Format("alter database [{0}] set SINGLE_USER WITH ROLLBACK IMMEDIATE;", DatabaseName);
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = string.Format("drop database [{0}]", DatabaseName);
                        cmd.ExecuteNonQuery();
                    }
                }
                File.Delete(DatabaseFile);
            }

            public void Dispose()
            {
                try
                {
                    DeleteTemporaryDatabase();
                }
                catch
                {
                    // ignored
                }
            }
        }

    }
}
