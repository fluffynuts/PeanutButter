using System;
using EmailSpooler.Win32Service.DB.Entities;
using EmailSpooler.Win32Service.DB.Tests.Builders;
using NUnit.Framework;
using PeanutButter.TestUtils.Entity;
using PeanutButter.TestUtils.Generic;

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
            ShouldBeAbleToPersist<EmailBuilder, Email>(ctx => ctx.Emails,
                (ctx, email) =>
                {
                }, (before, after) =>
                {
                }, DefaultIgnoreFieldsFor<Email>());
        }

        [Test]
        public void EmailRecipient_ShouldBeAbleToPersistAndRecall()
        {
            ShouldBeAbleToPersist<EmailRecipientBuilder, EmailRecipient>(ctx => ctx.EmailRecipients,
                (ctx, email) =>
                {
                }, (before, after) =>
                {
                    PropertyAssert.AllPropertiesAreEqual(before.Email, after.Email, DefaultIgnoreFieldsFor<Email>());
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
                    PropertyAssert.AllPropertiesAreEqual(before.Email, after.Email, DefaultIgnoreFieldsFor<Email>());
                }, DefaultIgnoreFieldsFor<EmailAttachment>());
        }

    }

}
