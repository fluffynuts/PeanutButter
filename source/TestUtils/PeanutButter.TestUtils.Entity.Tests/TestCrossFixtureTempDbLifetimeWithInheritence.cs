using System;
using System.Data.Common;
using System.Data.Entity;
using NUnit.Framework;
using PeanutButter.Utils.Entity;
using System.Linq;
using PeanutButter.TestUtils.Entity.Attributes;
using static PeanutButter.RandomGenerators.RandomValueGen;

[assembly: AllowSharedTempDbInstances]

namespace PeanutButter.TestUtils.Entity.Tests
{
    public class Note
    {
        public int Id { get; set; }
        public string NoteText { get; set; }
    }

    public class NotesContext : DbContext
    {
        public DbSet<Note> Notes { get; set; }
        public NotesContext(DbConnection connection): base(connection, true)
        {
        }
    }

    public abstract class TestCrossFixtureTempDbLifetimeWithInheritence_Base
        : TestFixtureWithTempDb<NotesContext>
    {
        private const string SCHEMA = "create table Notes(Id int primary key identity, NoteText varchar(128));";

        [OneTimeSetUp]
        public void OneTimeSetupBase()
        {
            Configure(false, cstr => new DbSchemaImporter(cstr, SCHEMA));
            DisableDatabaseRegeneration();
        }

        private static string _connectionString;
        private static int _createdNoteId;
        private static string _createdNoteText;

        private static void CreateNoteOn(NotesContext ctx)
        {
            var note = ctx.Notes.Create();
            note.NoteText = GetRandomString(5, 15);
            ctx.Notes.Add(note);
            ctx.SaveChangesWithErrorReporting();
            _createdNoteId = note.Id;
            _createdNoteText = note.NoteText;

        }

        private static void TestCreatedNoteOn(NotesContext ctx)
        {
            var note = ctx.Notes.FirstOrDefault(n => n.Id == _createdNoteId);
            Expect(note, Is.Not.Null);
            Expect(note.NoteText, Is.EqualTo(_createdNoteText));
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
                    Expect(_connectionString, Is.EqualTo(currentConnectionString));
                    TestCreatedNoteOn(ctx);
                }
            }

            //--------------- Assert -----------------------
        }

    }

    [TestFixture]
    [UseSharedTempDb("NotesDatabase")]
    public class TestCrossFixtureTempDbLifetimeWithInheritence_Part1
        : TestCrossFixtureTempDbLifetimeWithInheritence_Base
    {
    }
    [TestFixture]
    [UseSharedTempDb("NotesDatabase")]
    public class TestCrossFixtureTempDbLifetimeWithInheritence_Part2
        : TestCrossFixtureTempDbLifetimeWithInheritence_Base
    {
    }
}