using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PeanutButter.FluentMigrator.MigrationDumping;
using PeanutButter.FluentMigrator.MigrationDumping.Examples;
using PeanutButter.FluentMigrator.Tests.Shared;
using PeanutButter.RandomGenerators;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Entity;
using PeanutButter.Utils;

namespace PeanutButter.FluentMigrator.Tests
{
    [TestFixture]
    public class TestDbMigrationsDumper : AssertionHelper
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
            Expect(result, Is.Not.Null);
            Expect(result, Is.Not.Empty);
            Expect(result.Any(r => r.ToLower().Contains("create table [cows]")),
                Is.True);
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
            Expect(result, Is.Not.Null);
            Expect(result, Is.Not.Empty);
            Expect(result.Any(r => r.ToLower().Contains("create table [cows]")),
                Is.True);
            Expect(result.Any(r => r.Contains("/*")), Is.False);
            Console.WriteLine(string.Join("\n", result));
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
            Expect(result, Is.Not.Null);
            Expect(result, Is.Not.Empty);
            Expect(result.Any(r => r.ToLower().Contains("create table [cows]")),
                Is.True);
            Console.Write(string.Join("\n", result.Where(r => r.Contains("VersionInfo"))));
            Expect(result.Any(r => r.Contains("VersionInfo")), Is.False);
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
                using (var ctx = new MooContext(db.CreateConnection()))
                {
                    ctx.Cows.Add(cow);
                    ctx.SaveChanges();
                }
                using (var ctx = new MooContext(db.CreateConnection()))
                {
                    var allCows = ctx.Cows.ToArray();
                    Expect(allCows, Has.Length.EqualTo(1));
                    var stored = allCows[0];
                    Expect(stored.DeepEquals(cow, ObjectComparisons.PropertiesOnly, "Id"), Is.True);
                }
            }
        }
    }
}