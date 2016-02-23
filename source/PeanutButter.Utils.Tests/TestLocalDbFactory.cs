using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Data.SqlClient;
using System.IO;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.Utils.Tests
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
                var builder = new SqlConnectionStringBuilder();
                builder.InitialCatalog = dbName;
                builder.DataSource = "(localdb)\\v11.0";
                builder.IntegratedSecurity = true;
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
            public string InstanceName { get; set; } = "v11.0";
            private const string MasterConnectionString = @"Data Source=(localdb)\{0};Initial Catalog=master;Integrated Security=True";

            public LocalDbDestroyer()
            {
            }

            public string GetMasterConnectionString()
            {
                return string.Format(MasterConnectionString, InstanceName);
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
                    
                }
            }
        }

    }
}
