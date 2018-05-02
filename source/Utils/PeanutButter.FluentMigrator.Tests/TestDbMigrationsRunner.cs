using System.Linq;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Tests.Shared;
using PeanutButter.TestUtils.Entity;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.FluentMigrator.Tests
{
    [TestFixture]
    public class TestDbMigrationsRunner : TestFixtureWithTempDb<MooContext>
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Configure(
                false, 
                str => new RunningDbMigrationsRunnerForSqlServer(
                GetType().Assembly, str));
        }

        [Test]
        [Explicit("Fails when running through coverage, works when running in IDE")]
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
                Expect(allCows).To.Contain.Exactly(1).Item();
                var stored = allCows[0];
                Expect(
                    stored.DeepEquals(cow, ObjectComparisons.PropertiesOnly, "Id")
                ).To.Be.True();
            }
        }
    }
}
