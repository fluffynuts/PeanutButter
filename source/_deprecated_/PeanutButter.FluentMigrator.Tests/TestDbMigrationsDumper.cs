using System.Collections.Generic;
using System.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.FluentMigrator.MigrationDumping;
using PeanutButter.FluentMigrator.MigrationDumping.Examples;
using PeanutButter.FluentMigrator.Tests.Shared;
using PeanutButter.RandomGenerators;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Entity;
using PeanutButter.Utils;
using static NExpect.Expectations;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.FluentMigrator.Tests
{
    [TestFixture]
    public class TestDbMigrationsDumper
    {
        [Test]
        public void DumpMigrationScript_ShouldReturnMigrationScript()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.DumpMigrationScript(GetType().Assembly);

            //--------------- Assert -----------------------
            Expect(result).Not.To.Be.Null();
            Expect(result).Not.To.Be.Empty();
            Expect(result.ToLower().JoinWith(" ")).To.Contain("create table [cows]");
            ExpectScriptsCanSupportDatabase(result);
        }

        [Test]
        public void DumpMigrationsScript_WhenIncludeCommentsIsFalse_ShouldOmitComments()
        {
            //--------------- Arrange -------------------
            var options = new MigrationDumperOptions()
            {
                IncludeComments = false,
                IncludeFluentMigratorStructures = true
            };
            var sut = Create(options);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.DumpMigrationScript(GetType().Assembly);

            //--------------- Assert -----------------------
            Expect(result).Not.To.Be.Null();
            Expect(result).Not.To.Be.Empty();
            Expect(result.ToLower().JoinWith(" ")).To.Contain("create table [cows]");
            Expect(result).Not.To.Contain("/*");
            ExpectScriptsCanSupportDatabase(result);
        }

        [Test]
        public void DumpMigrationsScript_WhenIncludeFluentMigratorStructuresIsFalse_ShouldLeaveOutVersionInfoTable()
        {
            //--------------- Arrange -------------------
            var options = new MigrationDumperOptions()
            {
                IncludeComments = false,
                IncludeFluentMigratorStructures = false
            };
            var sut = Create(options);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.DumpMigrationScript(GetType().Assembly);

            //--------------- Assert -----------------------
            Expect(result).Not.To.Be.Null();
            Expect(result).Not.To.Be.Empty();
            Expect(result.ToUpper().JoinWith(" ")).To.Contain("CREATE TABLE [COWS]");
            Expect(result).Not.To.Contain("VersionInfo");
            ExpectScriptsCanSupportDatabase(result);
        }

        private static DBMigrationsDumper<MigrationsDumpingFactoryForSqlServer> Create(IMigrationDumperOptions options = null)
        {
            return options == null
                ? new DBMigrationsDumper<MigrationsDumpingFactoryForSqlServer>()
                : new DBMigrationsDumper<MigrationsDumpingFactoryForSqlServer>(options);
        }

        public class MigrationDumperOptions: IMigrationDumperOptions
        {
            public bool IncludeComments { get; set; }
            public bool IncludeFluentMigratorStructures { get; set; }
        }


        private void ExpectScriptsCanSupportDatabase(IEnumerable<string> result)
        {
            using (var db = new TempDBLocalDb())
            {
                var migrator = new DbSchemaImporter(
                    db.ConnectionString,
                    string.Join("\r\n", result)
                );
                migrator.MigrateToLatest();
                var cow = RandomValueGen.GetRandom<Cow>();
                using (var ctx = new MooContext(db.OpenConnection()))
                {
                    ctx.Cows.Add(cow);
                    ctx.SaveChanges();
                }
                using (var ctx = new MooContext(db.OpenConnection()))
                {
                    var allCows = ctx.Cows.ToArray();
                    Expect(allCows).To.Contain.Exactly(1).Item();
                    var stored = allCows[0].DuckAs<ICow>();
                    Expect(cow.DuckAs<ICow>()).To.Deep.Equal(stored);
                }
            }
        }
    }
}