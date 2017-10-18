using NUnit.Framework;
using PeanutButter.FluentMigrator.MigrationDumping;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.FluentMigrator.Tests
{
    [TestFixture]
    public class TestAnnouncerMessageRecorder
    {
        public class Opts : IMigrationDumperOptions
        {
            public bool IncludeComments { get; set; }
            public bool IncludeFluentMigratorStructures { get; set; }
        }

        [Test]
        public void Record_WhenCommentsDisabled_GivenComment_ShouldNotRecordIt()
        {
            //--------------- Arrange -------------------
            var opts = CreateOpts(false, false);
            var sut = Create(opts);

            //--------------- Assume ----------------
            Expect(sut.Statements).To.Be.Empty();

            //--------------- Act ----------------------
            sut.Record("/* some comment */");

            //--------------- Assert -----------------------
            Expect(sut.Statements).To.Be.Empty();
        }

        [Test]
        public void Record_WhenCommentsEnabled_GivenComment_ShouldRecordIt()
        {
            //--------------- Arrange -------------------
            var opts = CreateOpts(true, false);
            var sut = Create(opts);
            var expected = "/* some comment */";

            //--------------- Assume ----------------
            Expect(sut.Statements).To.Be.Empty();

            //--------------- Act ----------------------
            sut.Record(expected);

            //--------------- Assert -----------------------
            Expect(sut.Statements).To.Contain.Only(1).Equal.To(expected);
        }

        [TestCase("Create table VersionInfo")]
        [TestCase("Alter tabLe versionInfo")]
        [TestCase("insert Into VersionInfo")]
        [TestCase("create index on VersionInfo")]
        public void Recode_WhenFluentMigratorStructuresDisabled_ShouldNotRecordThem(string statement)
        {
            //--------------- Arrange -------------------
            var opts = CreateOpts(false, false);
            var sut = Create(opts);

            //--------------- Assume ----------------
            Expect(sut.Statements).To.Be.Empty();

            //--------------- Act ----------------------
            sut.Record(statement);

            //--------------- Assert -----------------------
            Expect(sut.Statements).To.Be.Empty();
        }

        [TestCase("Create table VersionInfo")]
        [TestCase("Alter tabLe versionInfo")]
        [TestCase("insert Into VersionInfo")]
        [TestCase("create index on VersionInfo")]
        public void Recode_WhenFluentMigratorStructuresEnabled_ShouldRecordThem(string statement)
        {
            //--------------- Arrange -------------------
            var opts = CreateOpts(false, true);
            var sut = Create(opts);

            //--------------- Assume ----------------
            Expect(sut.Statements).To.Be.Empty();

            //--------------- Act ----------------------
            sut.Record(statement);

            //--------------- Assert -----------------------
            Expect(sut.Statements).To.Contain.Only(1).Equal.To(statement);
        }


        private AnnouncerMessageRecorder Create(IMigrationDumperOptions opts)
        {
            return new AnnouncerMessageRecorder(opts);
        }

        private IMigrationDumperOptions CreateOpts(bool comments, bool structures)
        {
            return new Opts()
            {
                IncludeComments = comments,
                IncludeFluentMigratorStructures = structures
            };
        }
    }
}
