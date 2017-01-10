using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils.Entity;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestTestFixtureWithTempDb
        : TestFixtureWithTempDb<NotesDbContext>
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Configure(false, cstr => new DbSchemaImporter(cstr, NotesDbContext.SCHEMA));
            EnableTestIsolationInTransactions();
        }

        [Test]
        [Order(0)]
        public void WhenTestIsolationIsEnabled_ShouldIsolateTestsFromEachOtherUsingSameDatabase_pt1()
        {
            //--------------- Arrange -------------------
            var expected = GetRandomString(5, 10);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            using (var ctx = GetContext())
            {
                Expect(ctx.Notes.ToArray(), Is.Empty);
                var note = ctx.Notes.Create();
                note.NoteText = expected;
                ctx.Notes.Add(note);
                ctx.SaveChangesWithErrorReporting();
            }

            //--------------- Assert -----------------------
            using (var ctx = GetContext())
            {
                var existing = ctx.Notes.FirstOrDefault(n => n.NoteText == expected);
                Expect(existing, Is.Not.Null);
            }
        }

        [Test]
        [Order(1)]
        public void WhenTestIsolationIsEnabled_ShouldIsolateTestsFromEachOtherUsingSameDatabase_pt2()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            using (var ctx = GetContext())
            {
                Expect(ctx.Notes.ToArray(), Is.Empty);
            }

            //--------------- Assert -----------------------
        }


    }
}