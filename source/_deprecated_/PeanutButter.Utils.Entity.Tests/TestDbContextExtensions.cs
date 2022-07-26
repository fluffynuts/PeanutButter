using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Entity;
// ReSharper disable AccessToDisposedClosure

namespace PeanutButter.Utils.Entity.Tests
{
    [TestFixture]
    public class TestDbContextExtensions: EntityPersistenceTestFixtureBase<TestDbContextExtensions.ThingContext>
    {
        public class Thing
        {
            public int Id { get; set; }
            [MaxLength(50)]
            public string Name { get; set; }
            public string Notes { get; set; }
        }
        public class ThingContext: DbContext
        {
            public IDbSet<Thing> Things { get; set; }
            public ThingContext(DbConnection connection): base(connection, true)
            {
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Configure(false,
                s =>
                    new DbSchemaImporter(s,
                        "create table Things(id int primary key identity, Name nvarchar(50), Notes nvarchar(100));"));
        }

        [Test]
        public void SaveChangesWithErrorReporting_WhenNoError_ShouldSave()
        {
            //---------------Set up test pack-------------------
            var expectedName = RandomValueGen.GetRandomString(2, 10);
            var expectedNotes = RandomValueGen.GetRandomString(2, 20);
            int id;
            using (var ctx = GetContext())
            {
                ctx.Things.Clear();
                var newThing = new Thing()
                {
                    Name = expectedName,
                    Notes = expectedNotes
                };
                ctx.Things.Add(newThing);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                ctx.SaveChangesWithErrorReporting();
                id = newThing.Id;
            }
            using (var ctx = GetContext())
            {
                //---------------Test Result -----------------------
                var persisted = ctx.Things.FirstOrDefault(o => o.Id == id);
                Assert.IsNotNull(persisted);
                Assert.AreEqual(expectedName, persisted.Name);
                Assert.AreEqual(expectedNotes, persisted.Notes);
            }
        }

        [Test]
        public async Task SaveChangesWithErrorReportingAsync_WhenNoError_ShouldSave()
        {
            //---------------Set up test pack-------------------
            var expectedName = RandomValueGen.GetRandomString(2, 10);
            var expectedNotes = RandomValueGen.GetRandomString(2, 20);
            int id;
            using (var ctx = GetContext())
            {
                ctx.Things.Clear();
                var newThing = new Thing()
                {
                    Name = expectedName,
                    Notes = expectedNotes
                };
                ctx.Things.Add(newThing);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                await ctx.SaveChangesWithErrorReportingAsync();
                id = newThing.Id;
            }
            using (var ctx = GetContext())
            {
                //---------------Test Result -----------------------
                var persisted = ctx.Things.FirstOrDefault(o => o.Id == id);
                Assert.IsNotNull(persisted);
                Assert.AreEqual(expectedName, persisted.Name);
                Assert.AreEqual(expectedNotes, persisted.Notes);
            }
        }

        [Test]
        public void SaveChangesWithErrorReporting_WhenValidationError_ShouldThrowAndLogToOutput()
        {
            //---------------Set up test pack-------------------
            var expectedName = RandomValueGen.GetRandomString(60, 100);
            var expectedNotes = RandomValueGen.GetRandomString(2, 20);
            using (var ctx = GetContext())
            {
                ctx.Things.Clear();
                var newThing = new Thing()
                {
                    Name = expectedName,
                    Notes = expectedNotes
                };
                ctx.Things.Add(newThing);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<Exception>(() => ctx.SaveChangesWithErrorReporting());
                StringAssert.Contains("Error whilst trying to persist to the database:", ex.Message);
                StringAssert.Contains("maximum length", ex.Message);
            }
        }

        [Test]
        public void SaveChangesWithErrorReportingAsync_WhenValidationError_ShouldThrowAndLogToOutput()
        {
            //---------------Set up test pack-------------------
            var expectedName = RandomValueGen.GetRandomString(60, 100);
            var expectedNotes = RandomValueGen.GetRandomString(2, 20);
            using (var ctx = GetContext())
            {
                ctx.Things.Clear();
                var newThing = new Thing()
                {
                    Name = expectedName,
                    Notes = expectedNotes
                };
                ctx.Things.Add(newThing);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.ThrowsAsync<Exception>(() => ctx.SaveChangesWithErrorReportingAsync());
                StringAssert.Contains("Error whilst trying to persist to the database:", ex.Message);
                StringAssert.Contains("maximum length", ex.Message);
            }
        }

        [Test]
        public void SaveChangesWithErrorReporting_WhenSqlError_ShouldThrowAndLogToOutput()
        {
            //---------------Set up test pack-------------------
            var expectedName = RandomValueGen.GetRandomString(10, 15);
            var expectedNotes = RandomValueGen.GetRandomString(128, 150);
            using (var ctx = GetContext())
            {
                ctx.Things.Clear();
                var newThing = new Thing()
                {
                    Name = expectedName,
                    Notes = expectedNotes
                };
                ctx.Things.Add(newThing);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<Exception>(() => ctx.SaveChangesWithErrorReporting());
                StringAssert.Contains("DBUpdate Error:", ex.Message);
                StringAssert.Contains("truncated", ex.Message);
            }
        }

        [Test]
        public void SaveChangesWithErrorReportingAsync_WhenSqlError_ShouldThrowAndLogToOutput()
        {
            //---------------Set up test pack-------------------
            var expectedName = RandomValueGen.GetRandomString(10, 15);
            var expectedNotes = RandomValueGen.GetRandomString(128, 150);
            using (var ctx = GetContext())
            {
                ctx.Things.Clear();
                var newThing = new Thing()
                {
                    Name = expectedName,
                    Notes = expectedNotes
                };
                ctx.Things.Add(newThing);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.ThrowsAsync<Exception>(() => ctx.SaveChangesWithErrorReportingAsync());
                StringAssert.Contains("DBUpdate Error:", ex.Message);
                StringAssert.Contains("truncated", ex.Message);
            }
        }

    }
}