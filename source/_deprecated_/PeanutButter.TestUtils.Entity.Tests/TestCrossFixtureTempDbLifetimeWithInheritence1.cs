using System;
using NUnit.Framework;
using PeanutButter.Utils.Entity;
using System.Linq;
using PeanutButter.TestUtils.Entity.Attributes;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException

namespace PeanutButter.TestUtils.Entity.Tests
{

    [UseSharedTempDb("NotesDatabase")]
    public abstract class TestCrossFixtureTempDbLifetimeWithInheritence_BaseWithAttribute
        : TestFixtureWithTempDb<NotesDbContext>
    {

        [OneTimeSetUp]
        public void OneTimeSetupBase()
        {
            Configure(false, cstr => new DbSchemaImporter(cstr, NotesDbContext.SCHEMA));
            DisableDatabaseRegeneration();
            DisableTestIsolationInTransactions();
        }

        private static string _connectionString;
        private static int _createdNoteId;
        private static string _createdNoteText;

        private static void CreateNoteOn(NotesDbContext ctx)
        {
            var note = ctx.Notes.Create();
            note.NoteText = GetRandomString(5, 15);
            ctx.Notes.Add(note);
            ctx.SaveChangesWithErrorReporting();
            _createdNoteId = note.Id;
            _createdNoteText = note.NoteText;

        }

        private static void TestCreatedNoteOn(NotesDbContext ctx)
        {
            var note = ctx.Notes.FirstOrDefault(n => n.Id == _createdNoteId);
            Expect(note).Not.To.Be.Null();
            Expect(note.NoteText).To.Equal(_createdNoteText);
        }

        [Test]
        public void TestDbIsShared()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            using (var ctx = GetContext())
            {
                var currentConnectionString = ctx.Database.Connection.ConnectionString;
                if (_connectionString == null)
                {
#pragma warning disable S2696
                    _connectionString = currentConnectionString;
#pragma warning restore S2696
                    Console.WriteLine($@"Database is at: {_connectionString}");
                    CreateNoteOn(ctx);
                }
                else
                {
                    Expect(_connectionString).To.Equal(currentConnectionString);
                    TestCreatedNoteOn(ctx);
                }
            }

            //--------------- Assert -----------------------
        }

    }

    [TestFixture]
    public class TestCrossFixtureTempDbLifetimeWithInheritence_UsingBaseWithAttribute_Part1
        : TestCrossFixtureTempDbLifetimeWithInheritence_BaseWithAttribute
    {
    }

    [TestFixture]
    public class TestCrossFixtureTempDbLifetimeWithInheritence_UsingBaseWithAttribute_Part2
        : TestCrossFixtureTempDbLifetimeWithInheritence_BaseWithAttribute
    {
    }
}