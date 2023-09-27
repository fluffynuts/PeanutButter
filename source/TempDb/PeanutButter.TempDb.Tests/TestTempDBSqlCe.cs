using System;
using System.Data.SqlServerCe;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.TempDb.SqlCe;
using PeanutButter.Utils;
using static NExpect.Expectations;
using NExpect;
// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming

namespace PeanutButter.TempDb.Tests
{
    [TestFixture]
    public class TestTempDBSqlCe
    {
        [Test]
        public void ShouldImplementIDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(TempDBSqlCe);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut)
                .To.Implement<IDisposable>();

            //---------------Test Result -----------------------
        }

        public abstract class BehaviorTests
        {
            [OneTimeSetUp]
            public void OneTimeSetup()
            {
                if (!Platform.IsWindows)
                {
                    Assert.Ignore(
                        "SQLCE tests will only work on windows"
                    );
                }
            }
        }

        [TestFixture]
        public class Construction : BehaviorTests
        {
            [Test]
            public void ShouldCreateTemporarySqlCeDatabase()
            {
                //---------------Set up test pack-------------------
                using var db = new TempDBSqlCe();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------

                //---------------Test Result -----------------------
                Expect(db.DatabasePath)
                    .To.Exist();
                
                using var conn = new SqlCeConnection(db.ConnectionString);
                Expect(() => conn.Open())
                    .Not.To.Throw();
            }

            [Test]
            public void ShouldRunGivenScriptsOnDatabase()
            {
                //---------------Set up test pack-------------------
                var createTable = "create table TheTable(id int primary key, name nvarchar(128));";
                var insertData = "insert into TheTable(id, name) values (1, 'one');";
                var selectData = "select name from TheTable where id = 1;";
                using var db = new TempDBSqlCe(createTable, insertData);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var conn = new SqlCeConnection(db.ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = selectData;
                using var rdr = cmd.ExecuteReader();

                //---------------Test Result -----------------------
                Expect(rdr.Read())
                    .To.Be.True();
                Expect(rdr["name"].ToString())
                    .To.Equal("one");
            }
        }

        [TestFixture]
        public class Disposal : BehaviorTests
        {
            [Test]
            public void ShouldRemoveTheTempDatabase()
            {
                //---------------Set up test pack-------------------
                string file;
                using (var db = new TempDBSqlCe())
                {
                    //---------------Assert Precondition----------------
                    var conn = db.OpenConnection();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "create table [test] ([id] int primary key identity, [name] nvarchar(128));";
                    cmd.ExecuteNonQuery();
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "insert into [test] ([name]) values ('the name');";
                    cmd.ExecuteNonQuery();
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "select * from [test];";
                    cmd.ExecuteReader();
                    file = db.DatabasePath;
                    Assert.IsTrue(File.Exists(file));

                    //---------------Execute Test ----------------------

                    //---------------Test Result -----------------------
                }

                Expect(file)
                    .Not.To.Exist();
            }

            [Test]
            public void ShouldCloseManagedConnectionsBeforeAttemptingToDeleteTheFile()
            {
                var createTable = "create table TheTable(id int primary key, name nvarchar(128));";
                var insertData = "insert into TheTable(id, name) values (1, 'one');";
                var selectData = "select name from TheTable where id = 1;";
                string theFile;
                using (var db = new TempDBSqlCe(createTable, insertData))
                {
                    theFile = db.DatabasePath;
                    Assert.IsTrue(File.Exists(theFile));
                    //---------------Set up test pack-------------------

                    //---------------Assert Precondition----------------

                    var conn = db.OpenConnection();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = selectData;
                    using var rdr = cmd.ExecuteReader();

                    //---------------Execute Test ----------------------
                    Expect(rdr.Read())
                        .To.Be.True();
                    Expect(rdr["name"].ToString())
                        .To.Equal("one");
                }

                //---------------Test Result -----------------------
                Expect(theFile)
                    .Not.To.Exist();
            }
        }

        [TestFixture]
        public class GetConnection : BehaviorTests
        {
            [Test]
            public void ShouldReturnValidConnection()
            {
                var createTable = "create table TheTable(id int primary key, name nvarchar(128));";
                var insertData = "insert into TheTable(id, name) values (1, 'one');";
                var selectData = "select name from TheTable where id = 1;";
                using var db = new TempDBSqlCe(createTable, insertData);
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var conn = db.OpenConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = selectData;
                using var rdr = cmd.ExecuteReader();


                //---------------Test Result -----------------------
                
                Expect(rdr.Read())
                    .To.Be.True();
                Expect(rdr["name"].ToString())
                    .To.Equal("one");
            }
        }

        [TestFixture]
        public class General : BehaviorTests
        {
            [Test]
            public void ShouldPlayNicelyInParallel()
            {
                //---------------Set up test pack-------------------
                using var disposer = new AutoDisposer();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Parallel.For(
                    0,
                    100,
                    i =>
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        disposer.Add(new TempDBSqlCe());
                    }
                );

                //---------------Test Result -----------------------
            }
        }
    }
}