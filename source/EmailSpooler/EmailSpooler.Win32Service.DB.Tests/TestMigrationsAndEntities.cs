using System;
using EmailSpooler.Win32Service.Entity;
using NUnit.Framework;
using PeanutButter.FluentMigrator;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.TestUtils.Entity;
using PeanutButter.Utils.Entity;

namespace EmailSpooler.Win32Service.DB.Tests
{
    [TestFixture]
    public class TestMigrationsAndEntities: EntityPersistenceTestFixtureBase<EmailContext>
    {
        private TimeSpan _oneSecond = new TimeSpan(0, 0, 0, 1);
        private TempDBLocalDb _sharedTempDb;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _sharedTempDb = new TempDBLocalDb();
        }

        [TearDown]
        public void TearDown()
        {
            using (var ctx = GetContext())
            {
                ctx.EmailAttachments.Clear();
                ctx.EmailRecipients.Clear();
                ctx.Emails.Clear();
                ctx.SaveChangesWithErrorReporting();
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _sharedTempDb.Dispose();
        }

        public TestMigrationsAndEntities()
        {
            Configure(false,
                connectionString => new DbMigrationsRunnerSqlServer(connectionString, s => { }));
        }

        private IDBMigrationsRunner MigratorFactory(string connectionString)
        {
            return new DbMigrationsRunnerSqlServer(connectionString, s => { });
        }

        [Test]
        public void Email_ShouldBeAbleToPersistAndRecall()
        {

            EntityPersistenceTester.CreateFor<Email>()
                .WithContext<EmailContext>()
                .WithDbMigrator(MigratorFactory)
                .WithSharedDatabase(_sharedTempDb)
                .WithAllowedDateTimePropertyDelta(_oneSecond)
                .ShouldPersistAndRecall();
        }

        [Test]
        public void EmailRecipient_ShouldBeAbleToPersistAndRecall()
        {
            EntityPersistenceTester.CreateFor<EmailRecipient>()
                .WithContext<EmailContext>()
                .WithDbMigrator(MigratorFactory)
                .WithSharedDatabase(_sharedTempDb)
                .WithAllowedDateTimePropertyDelta(_oneSecond)
                .ShouldPersistAndRecall();
        }

        [Test]
        public void EmailAttachment_ShouldBeAbleToPersistAndRecall()
        {
            EntityPersistenceTester.CreateFor<EmailAttachment>()
                .WithContext<EmailContext>()
                .WithDbMigrator(MigratorFactory)
                .WithSharedDatabase(_sharedTempDb)
                .WithAllowedDateTimePropertyDelta(_oneSecond)
                .ShouldPersistAndRecall();
        }

    }

}
