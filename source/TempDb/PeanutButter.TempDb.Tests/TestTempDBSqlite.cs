using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.TempDb.Sqlite;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

// ReSharper disable InconsistentNaming

namespace PeanutButter.TempDb.Tests;

[TestFixture]
public class TestTempDBSqlite
{
    [Test]
    public void ShouldImplementIDisposable()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        typeof(TempDBSqlite).ShouldImplement<IDisposable>();

        //---------------Test Result -----------------------
    }

    [Test]
    public void Construct_ShouldCreateTemporarySqliteDatabase()
    {
        //---------------Set up test pack-------------------
        using (var db = new TempDBSqlite())
        {
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsTrue(File.Exists(db.DatabasePath));
            using (var conn = new SQLiteConnection(db.ConnectionString))
            {
                Assert.DoesNotThrow(conn.Open);
            }
        }
    }

    [Test]
    public void Dispose_ShouldRemoveTheTempDatabase()
    {
        //---------------Set up test pack-------------------
        string file;
        using (var db = new TempDBSqlite())
        {
            //---------------Assert Precondition----------------
            file = db.DatabasePath;
            Assert.IsTrue(File.Exists(file));

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
        }

        Assert.IsFalse(File.Exists(file));
    }

    [Test]
    public void Construct_ShouldRunGivenScriptsOnDatabase()
    {
        var createTable = "create table TheTable(id int primary key, name nvarchar(128));";
        var insertData = "insert into TheTable(id, name) values (1, 'one');";
        var selectData = "select name from TheTable where id = 1;";
        using (var db = new TempDBSqlite(createTable, insertData))
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var conn = db.OpenConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = selectData;
                    using (var rdr = cmd.ExecuteReader())
                    {
                        Assert.IsTrue(rdr.Read());
                        Assert.AreEqual("one", rdr["name"].ToString());
                    }
                }
            }

            //---------------Test Result -----------------------
        }
    }

    [Test]
    public void ShouldPlayNicelyInParallel()
    {
        //---------------Set up test pack-------------------
        using (var disposer = new AutoDisposer())
        {
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Parallel.For(
                0,
                100,
                i =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    disposer.Add(new TempDBSqlite());
                }
            );

            //---------------Test Result -----------------------
        }
    }
}
