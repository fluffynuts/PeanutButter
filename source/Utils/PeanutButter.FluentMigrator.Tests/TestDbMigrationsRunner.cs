using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PeanutButter.FluentMigrator.Fakes;
using PeanutButter.FluentMigrator.MigrationDumping;
using PeanutButter.FluentMigrator.MigrationDumping.Examples;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Entity;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests
{
    [TestFixture]
    public class TestDbMigrationsRunner : TestFixtureWithTempDb<MooContext>
    {
        private static void Expect<TActual>(TActual actual, IResolveConstraint constraint)
        {
            AssertionHelper.Expect(actual, constraint);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Configure(false, str => new RunningDbMigrationsRunnerForSqlServer(
                GetType().Assembly, str));
        }
        [Test]
        public void MigrateToLatest_ShouldMigrateDbToLatest()
        {
            //--------------- Arrange -------------------
            var cow = GetRandom<Cow>();
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            using (var ctx = GetContext())
            {
                ctx.Cows.Add(cow);
                ctx.SaveChanges();
            }

            //--------------- Assert -----------------------
            using (var ctx = GetContext())
            {
                var allCows = ctx.Cows.ToArray();
                Expect(allCows, Has.Length.EqualTo(1));
                var stored = allCows[0];
                Expect(stored.DeepEquals(cow, "Id"), Is.True);
            }
        }
    }


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
                var cow = GetRandom<Cow>();
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
                    Expect(stored.DeepEquals(cow, "Id"), Is.True);
                }
            }
        }
    }

    public class RunningDbMigrationsRunnerForSqlServer :
        DBMigrationsRunner<SqlServer2000ProcessorFactory>
    {
        public RunningDbMigrationsRunnerForSqlServer(
            Assembly assemblyToLoadMigrationsFrom,
            string connectionString,
            Action<string> textWriterAction = null)
            : base(assemblyToLoadMigrationsFrom,
                  connectionString,
                  textWriterAction)
        {
        }
    }

    public class Cow
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual string MooDialect { get; set; }
        public virtual string Name { get; set; }
        public virtual bool IsDairy { get; set; }
        public virtual bool IsLactoseIntolerant { get; set; }
    }

    public class MooContext : DbContext
    {
        public virtual IDbSet<Cow> Cows { get; set; }
        public MooContext(
            DbConnection connection
        ) : base(connection, true)
        {
        }

        static MooContext()
        {
            Database.SetInitializer<MooContext>(null);
        }
    }

    [Migration(1)]
    public class Migration1 : MigrationBase
    {
        public override void Up()
        {
            Create.Table("Cows")
                .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("MooDialect").AsString().NotNullable()
                .WithColumn("IsDairy").AsBoolean().NotNullable();
        }

        public override void Down()
        {
            /* do nothing */
        }
    }

    [Migration(2)]
    public class Migration2 : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("Cows")
                    .AddColumn("IsLactoseIntolerant").AsBoolean()
                        .Nullable().WithDefaultValue(false);
        }

        public override void Down()
        {
            /* do nothing */
        }
    }
}
