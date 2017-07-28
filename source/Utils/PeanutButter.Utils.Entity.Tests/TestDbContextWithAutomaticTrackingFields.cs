using System;
using System.Linq;
using System.Threading.Tasks;
using EmailSpooler.Win32Service.DB.Tests;
using EmailSpooler.Win32Service.Entity;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Entity;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.Utils.Entity.Tests
{
    // takes advantage of the fact that the EmailSpooler DB uses a context which derives from DbContextWithAutomaticTrackingFields
    [TestFixture]
    public class TestDbContextWithAutomaticTrackingFields: EntityPersistenceTestFixtureBase<EmailContext>
    {
        public TestDbContextWithAutomaticTrackingFields()
        {
            Configure(false, connectionString => new DbMigrationsRunnerSqlServer(connectionString, s => { }));
        }

        [Test]
        public void ContextForTesting_ShouldDeriveFrom_DbContextWithAutomaticTrackingFields()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(EmailContext);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<DbContextWithAutomaticTrackingFields>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void EntityPersistence_ShouldSetCreatedToCurrentTimeStamp()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var beforeTest = DateTime.Now;
                var email = CreateRandomValidEmail();

                //---------------Assert Precondition----------------
                Assert.AreEqual(DateTime.MinValue, email.Created);

                //---------------Execute Test ----------------------
                ctx.Emails.Add(email);
                ctx.SaveChangesWithErrorReporting();

                //---------------Test Result -----------------------
                var afterTest = DateTime.Now;
                Assert.That(email.Created, Is.GreaterThanOrEqualTo(beforeTest));
                Assert.That(email.Created, Is.LessThanOrEqualTo(afterTest));
            }
        }

        [Test]
        public void EntityPersistence_ShouldUpdateLastModified()
        {
            int id;
            //---------------Set up test pack-------------------
            using (var ctx = GetContext())
            {
                var email = CreateRandomValidEmail();
                ctx.Emails.Add(email);
                ctx.SaveChangesWithErrorReporting();
                id = email.EmailId;
            }

            using (var ctx = GetContext())
            {
                //---------------Assert Precondition----------------
                var beforeTest = DateTime.Now;
                var email = ctx.Emails.First(e => e.EmailId == id);
                Assert.IsNull(email.LastModified);

                //---------------Execute Test ----------------------
                email.Subject += RandomValueGen.GetRandomString(2);
                ctx.SaveChangesWithErrorReporting();

                //---------------Test Result -----------------------
                var afterTest = DateTime.Now;
                Assert.That(email.LastModified, Is.GreaterThanOrEqualTo(beforeTest));
                Assert.That(email.LastModified, Is.LessThanOrEqualTo(afterTest));
            }
        }

//        [Test]
//        public async Task AsyncEntityPersistence_ShouldSetCreatedToCurrentTimeStamp()
//        {
//            using (var ctx = GetContext())
//            {
//                //---------------Set up test pack-------------------
//                var beforeTest = DateTime.Now;
//                var email = CreateRandomValidEmail();
//
//                //---------------Assert Precondition----------------
//                Assert.AreEqual(DateTime.MinValue, email.Created);
//
//                //---------------Execute Test ----------------------
//                ctx.Emails.Add(email);
//                // TESTME: no GetAwaiter error
//                ctx.SaveChangesWithErrorReportingAsync().Wait();
//
//                //---------------Test Result -----------------------
//                var afterTest = DateTime.Now;
//                Assert.That(email.Created, Is.GreaterThanOrEqualTo(beforeTest));
//                Assert.That(email.Created, Is.LessThanOrEqualTo(afterTest));
//            }
//        }

//        [Test]
//        public async Task AsyncEntityPersistence_ShouldUpdateLastModified()
//        {
//            int id;
//            //---------------Set up test pack-------------------
//            using (var ctx = GetContext())
//            {
//                var email = CreateRandomValidEmail();
//                ctx.Emails.Add(email);
//                ctx.SaveChangesWithErrorReporting();
//                id = email.EmailId;
//            }
//
//            using (var ctx = GetContext())
//            {
//                //---------------Assert Precondition----------------
//                var beforeTest = DateTime.Now;
//                var email = ctx.Emails.First(e => e.EmailId == id);
//                Assert.IsNull(email.LastModified);
//
//                //---------------Execute Test ----------------------
//                email.Subject += RandomValueGen.GetRandomString(2);
//                ctx.SaveChangesWithErrorReportingAsync().Wait();
//
//                //---------------Test Result -----------------------
//                var afterTest = DateTime.Now;
//                Assert.That(email.LastModified, Is.GreaterThanOrEqualTo(beforeTest));
//                Assert.That(email.LastModified, Is.LessThanOrEqualTo(afterTest));
//            }
//        }


        private static Email CreateRandomValidEmail()
        {
            var email = new Email()
            {
                Subject = RandomValueGen.GetRandomString(2),
                Body = RandomValueGen.GetRandomString(2),
                SendAt = DateTime.Now
            };
            return email;
        }
    }
}
