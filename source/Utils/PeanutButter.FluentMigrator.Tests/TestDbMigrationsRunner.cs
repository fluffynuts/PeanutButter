using System.Linq;
using NUnit.Framework;
using PeanutButter.FluentMigrator.Tests.Shared;
using PeanutButter.TestUtils.Entity;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FluentMigrator.Tests
{
    [TestFixture]
    public class TestDbMigrationsRunner : TestFixtureWithTempDb<MooContext>
    {
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
}
