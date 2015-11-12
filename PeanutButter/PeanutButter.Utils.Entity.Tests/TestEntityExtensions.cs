using System;
using System.Collections.Generic;
using System.Linq;
using EmailSpooler.Win32Service.DB.Entities;
using EmailSpooler.Win32Service.DB.Tests;
using EmailSpooler.Win32Service.DB.Tests.Builders;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Entity;
using PeanutButter.Utils;

namespace PeanutButter.Utils.Entity.Tests
{
    [TestFixture]
    public class TestEntityExtensions: EntityPersistenceTestFixtureBase<EmailContext>
    {
        public TestEntityExtensions()
        {
            Configure(false, cs => new DbMigrationsRunnerSqlServer(cs));
        }

        [Test]
        public void AddRange_OperatingOnDbSet_ShouldAddAllItemsFromIEnumerable()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var toAdd = new[]
                {
                    EmailBuilder.BuildRandom(),
                    EmailBuilder.BuildRandom(),
                    EmailBuilder.BuildRandom()
                };

                //---------------Assert Precondition----------------
                CollectionAssert.IsEmpty(ctx.Emails);

                //---------------Execute Test ----------------------
                ctx.Emails.AddRange(toAdd);
                ctx.SaveChanges();

                //---------------Test Result -----------------------
                Assert.AreEqual(toAdd.Length, ctx.Emails.Count());
            }
        }

        [Test]
        public void AddRange_OperatingOnDbSet_ShouldAddItemsFromParams()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var email1 = EmailBuilder.BuildRandom();
                var email2 = EmailBuilder.BuildRandom();
                var email3 = EmailBuilder.BuildRandom();

                //---------------Assert Precondition----------------
                CollectionAssert.IsEmpty(ctx.Emails);

                //---------------Execute Test ----------------------
                ctx.Emails.AddRange(email1, email2, email3);
                ctx.SaveChanges();

                //---------------Test Result -----------------------
                Assert.AreEqual(3, ctx.Emails.Count());
            }
        }

        [Test]
        public void AddRange_ActingOnICollection_ShouldAddAllItems()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var email = EmailBuilder.BuildRandom();
                var items = RandomValueGen.GetRandomCollection(EmailAttachmentBuilder.BuildRandom, 2);

                //---------------Assert Precondition----------------
                CollectionAssert.IsEmpty(email.EmailAttachments);

                //---------------Execute Test ----------------------
                email.EmailAttachments.AddRange(items);
                ctx.SaveChangesWithErrorReporting();    // verify the save works

                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(items, email.EmailAttachments);
            }
        }


        [Test]
        public void AddRange_ActingOnICollection_ShouldAddAllItemsFromParams()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var email = EmailBuilder.BuildRandom();
                var item1 = EmailAttachmentBuilder.BuildRandom();
                var item2 = EmailAttachmentBuilder.BuildRandom();
                var item3 = EmailAttachmentBuilder.BuildRandom();

                //---------------Assert Precondition----------------
                CollectionAssert.IsEmpty(email.EmailAttachments);

                //---------------Execute Test ----------------------
                email.EmailAttachments.AddRange(item1, item2, item3);
                ctx.SaveChangesWithErrorReporting();    // verify the save works

                //---------------Test Result -----------------------
                new[] {item1, item2, item3}.ToList().ForEach(i =>
                {
                    CollectionAssert.Contains(email.EmailAttachments, i);
                });
            }
        }

        [Test]
        public void RemoveRange_ActingOnIColleciton_ShouldRemoveCollectionOfItems()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var email = EmailBuilder.BuildRandom();
                Func<EmailAttachment> createAttachment = () => EmailAttachmentBuilder.Create().WithRandomProps().WithProp(o => o.Email = email).Build();
                var item1 = createAttachment();
                var item2 = createAttachment();
                var item3 = createAttachment();
                email.EmailAttachments.AddRange(item1, item2, item3);
                ctx.Emails.Add(email);
                ctx.SaveChangesWithErrorReporting();

                //---------------Assert Precondition----------------
                CollectionAssert.IsNotEmpty(email.EmailAttachments);

                //---------------Execute Test ----------------------
                var emailAttachments = EnumerableFor(item1, item3);
                email.EmailAttachments.RemoveRange(emailAttachments);
                ctx.EmailAttachments.RemoveRange(emailAttachments);
                ctx.SaveChangesWithErrorReporting();
                //---------------Test Result -----------------------
                CollectionAssert.DoesNotContain(email.EmailAttachments, item1);
                CollectionAssert.Contains(email.EmailAttachments, item2);
                CollectionAssert.DoesNotContain(email.EmailAttachments, item3);
            }
        }

        [Test]
        public void RemoveRange_ActingOnICollection_ShouldRemoveParamsItems()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var email = EmailBuilder.BuildRandom();
                var anotherEmail = EmailBuilder.BuildRandom();
                Func<EmailAttachment> createAttachment = () => EmailAttachmentBuilder.Create().WithRandomProps().WithProp(o => o.Email = email).Build();
                var item1 = createAttachment();
                var item2 = createAttachment();
                var item3 = createAttachment();
                email.EmailAttachments.AddRange(item1, item2, item3);
                ctx.Emails.AddRange(email, anotherEmail);
                ctx.SaveChangesWithErrorReporting();

                //---------------Assert Precondition----------------
                CollectionAssert.IsNotEmpty(email.EmailAttachments);

                //---------------Execute Test ----------------------
                email.EmailAttachments.RemoveRange(item1, item3);
                anotherEmail.EmailAttachments.AddRange(item1, item3);
                ctx.SaveChangesWithErrorReporting();
                //---------------Test Result -----------------------
                CollectionAssert.DoesNotContain(email.EmailAttachments, item1);
                CollectionAssert.Contains(email.EmailAttachments, item2);
                CollectionAssert.DoesNotContain(email.EmailAttachments, item3);
            }
        }

        [Test]
        public void RemoveRange_ActingOnDbSet_ShouldRemoveItemsInProvidedCollection()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var emails = RandomValueGen.GetRandomCollection(EmailBuilder.BuildRandom, 3);
                ctx.Emails.AddRange(emails);
                ctx.SaveChangesWithErrorReporting();
                var remove1 = RandomValueGen.GetRandomFrom(emails);
                var remove2 = RandomValueGen.GetRandomFrom(emails, remove1);

                //---------------Assert Precondition----------------
                Assert.AreEqual(emails.Count(), ctx.Emails.Count());

                //---------------Execute Test ----------------------
                ctx.Emails.RemoveRange(EnumerableFor(remove1, remove2));
                ctx.SaveChangesWithErrorReporting();

                //---------------Test Result -----------------------
                var shouldHave = emails.Except(new[] {remove1, remove2});
                CollectionAssert.AreEquivalent(shouldHave, ctx.Emails);
            }
        }

        [Test]
        public void Clear_ShouldDeleteAllEntitiesFromTheDbSet()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                var emails = RandomValueGen.GetRandomCollection(EmailBuilder.BuildRandom, 2);
                ctx.Emails.AddRange(emails);
                ctx.SaveChanges();

                //---------------Assert Precondition----------------
                CollectionAssert.IsNotEmpty(ctx.Emails);

                //---------------Execute Test ----------------------
                ctx.Emails.Clear();
                ctx.SaveChangesWithErrorReporting();

                //---------------Test Result -----------------------
                CollectionAssert.IsEmpty(ctx.Emails);
            }
        }

        [Test]
        public void AddNew_ActingOnDbSet_ShouldAddNewItemAndReturnIt()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                CollectionAssert.IsEmpty(ctx.Emails);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var email = ctx.Emails.AddNew();
                email.Subject = RandomValueGen.GetRandomString(2);
                email.Body = RandomValueGen.GetRandomString(2);
                email.SendAt = DateTime.Now;
                ctx.SaveChangesWithErrorReporting();

                //---------------Test Result -----------------------
                CollectionAssert.Contains(ctx.Emails, email);
            }
        }

        [Test]
        public void AddNew_ActingOnDbSet_ShouldAddNewItemAndReturnIt_AfterExecutingInitializerAction()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                CollectionAssert.IsEmpty(ctx.Emails);
                var called = false;

                //---------------Assert Precondition----------------
                CollectionAssert.IsEmpty(ctx.Emails);

                //---------------Execute Test ----------------------
                var email = ctx.Emails.AddNew(e =>
                {
                    e.Subject = RandomValueGen.GetRandomString(2);
                    e.Body = RandomValueGen.GetRandomString(2);
                    e.SendAt = DateTime.Now;
                    called = true;
                });
                ctx.SaveChangesWithErrorReporting();    // would fail if the entity wasn't set up properly

                //---------------Test Result -----------------------
                Assert.IsTrue(called);  // just-in-casies
                CollectionAssert.IsNotEmpty(ctx.Emails);
                CollectionAssert.Contains(ctx.Emails, email);
            }
        }

        [Test]
        public void AddNew_ActingOnDbSet_ShouldAddNewItemAndReturnIt_AfterExecutingInitializerActionAndAfterAddACtion()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                CollectionAssert.IsEmpty(ctx.Emails);
                var called = false;
                Action saveChanges = () =>
                {
                    ctx.SaveChangesWithErrorReporting();
                    called = true;
                };

                //---------------Assert Precondition----------------
                CollectionAssert.IsEmpty(ctx.Emails);

                //---------------Execute Test ----------------------
                var email = ctx.Emails.AddNew(e =>
                {
                    e.Subject = RandomValueGen.GetRandomString(2);
                    e.Body = RandomValueGen.GetRandomString(2);
                    e.SendAt = DateTime.Now;
                    called = true;
                }, o =>
                {
                    saveChanges();
                });

                //---------------Test Result -----------------------
                Assert.IsTrue(called);  // just-in-casies
                CollectionAssert.IsNotEmpty(ctx.Emails);
                CollectionAssert.Contains(ctx.Emails, email);
            }
        }


        [Test]
        public void AddNew_ActingOnCollection_ShouldAddNewItemAndReturnIt_AfterExecutingInitializerAction()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                CollectionAssert.IsEmpty(ctx.Emails);
                var called = false;
                var email = EmailBuilder.BuildRandom();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var attachment = email.EmailAttachments.AddNew(a =>
                {
                    a.Data = RandomValueGen.GetRandomBytes(100);
                    a.MIMEType = RandomValueGen.GetRandomString(2);
                    called = true;
                });
                ctx.SaveChangesWithErrorReporting();    // would fail if the entity wasn't set up properly

                //---------------Test Result -----------------------
                Assert.IsTrue(called);  // just-in-casies
                CollectionAssert.IsNotEmpty(email.EmailAttachments);
                CollectionAssert.Contains(email.EmailAttachments, attachment);
            }
        }

        [Test]
        public void AddNew_ActingOnCollection_ShouldAddNewItemAndReturnIt_AfterExecutingInitializerActionAndAfterAddACtion()
        {
            using (var ctx = GetContext())
            {
                //---------------Set up test pack-------------------
                CollectionAssert.IsEmpty(ctx.Emails);
                var called = false;
                Action saveChanges = () =>
                {
                    ctx.SaveChangesWithErrorReporting();
                    called = true;
                };
                var email = EmailBuilder.BuildRandom();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var attachment = email.EmailAttachments.AddNew(a =>
                {
                    a.Data = RandomValueGen.GetRandomBytes(100);
                    a.MIMEType = RandomValueGen.GetRandomString(2);
                    called = true;
                }, o =>
                {
                    saveChanges();
                });

                //---------------Test Result -----------------------
                Assert.IsTrue(called);  // just-in-casies
                CollectionAssert.IsNotEmpty(email.EmailAttachments);
                CollectionAssert.Contains(email.EmailAttachments, attachment);
            }
        }




        public IEnumerable<T> EnumerableFor<T>(params T[] items)
        {
            return items;
        }
    }

}
