using System;
using EmailSpooler.Win32Service.DB.Tests.Builders;
using EmailSpooler.Win32Service.Entity;
using NUnit.Framework;
using PeanutButter.TestUtils.Entity;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace EmailSpooler.Win32Service.DB.Tests
{
    [TestFixture]
    public class TestMigrationsAndEntities: EntityPersistenceTestFixtureBase<EmailContext>
    {
        public TestMigrationsAndEntities()
        {
            Configure(false,
                connectionString => new DbMigrationsRunnerSqlServer(connectionString, s => { }));
        }

        [Test]
        public void Email_ShouldBeAbleToPersistAndRecall()
        {

            EntityPersistenceTester.CreateFor<Email>()
                    .WithContext<EmailContext>()
                    .WithDbMigrator(cs => new DbMigrationsRunnerSqlServer(cs, s => { }))
                    .ShouldPersistAndRecall();
        }

        [Test]
        public void EmailRecipient_ShouldBeAbleToPersistAndRecall()
        {
            ShouldBeAbleToPersist<EmailRecipientBuilder, EmailRecipient>(ctx => ctx.EmailRecipients,
                (ctx, email) =>
                {
                }, (before, after) =>
                {
                    PropertyAssert.AllPropertiesAreEqual(before.Email, after.Email, 
                        DefaultIgnoreFieldsFor<Email>().And("SendAt"));
                }, DefaultIgnoreFieldsFor<EmailRecipient>());
        }

        [Test]
        public void EmailAttachment_ShouldBeAbleToPersistAndRecall()
        {
            ShouldBeAbleToPersist<EmailAttachmentBuilder, EmailAttachment>(ctx => ctx.EmailAttachments,
                (ctx, email) =>
                {
                }, (before, after) =>
                {
                    PropertyAssert.AllPropertiesAreEqual(before.Email, after.Email, 
                            DefaultIgnoreFieldsFor<Email>().And("SendAt"));

                }, DefaultIgnoreFieldsFor<EmailAttachment>());
        }

    }

}
