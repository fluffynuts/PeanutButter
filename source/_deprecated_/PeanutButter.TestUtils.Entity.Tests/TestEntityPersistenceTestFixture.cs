using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils.Entity;
using NExpect;
using static NExpect.Expectations;

// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestComBlockListReason : EntityPersistenceTestFixtureBase<CommunicatorContext>
    {
        private int _clearCalled;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Configure(true, connectionString => new DbSchemaImporter(connectionString, TestResources.dbscript));
            RunBeforeFirstGettingContext(Clear);
        }

        [SetUp]
        public void SetUp()
        {
            _clearCalled = 0;
        }

        [TearDown]
        public void TearDown()
        {
            Assert.AreEqual(1, _clearCalled);
        }

        private void Clear(CommunicatorContext ctx)
        {
            ctx.BlockLists.Clear();
            ctx.SaveChangesWithErrorReporting();
            _clearCalled++;
        }

        [Test]
        public void RunBeforeFirstGettingContext_ActionShouldOnlyBeRunOnce()
        {
            //---------------Set up test pack-------------------
            Assert.AreEqual(0, _clearCalled);
            using (GetContext())
            {
                Assert.AreEqual(1, _clearCalled);
            }

            //---------------Assert Precondition----------------
            Assert.AreEqual(1, _clearCalled);

            //---------------Execute Test ----------------------
            using (GetContext())
            {
                Assert.AreEqual(1, _clearCalled);
            }

            //---------------Test Result -----------------------
            Assert.AreEqual(1, _clearCalled);
        }


        [Test]
        public void RunBeforeFirstGettingContext_ActionShouldOnlyBeRunOncePerTest()
        {
            //---------------Set up test pack-------------------
            Assert.AreEqual(0, _clearCalled);
            using (GetContext())
            {
                Assert.AreEqual(1, _clearCalled);
            }

            //---------------Assert Precondition----------------
            Assert.AreEqual(1, _clearCalled);

            //---------------Execute Test ----------------------
            using (GetContext())
            {
                Assert.AreEqual(1, _clearCalled);
            }

            //---------------Test Result -----------------------
            Assert.AreEqual(1, _clearCalled);
        }

        public class SomeEntity : EntityBase
        {
        }

        [Test]
        public void Type_ShouldInheritFromEntityBase_GivenTypeInheritingFromEntityBase_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            _clearCalled = 1; // work around kludgey test and lazy me

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() =>
                Type_ShouldInheritFromEntityBase<SomeEntity>()
            );

            //--------------- Assert -----------------------
        }

        public class SomeNonEntity
        {
        }

        [Test]
        public void Type_ShouldInheritFromEntityBase_GivenTypeNotInheritingFromEntityBase_ShouldThrow()
        {
            //--------------- Arrange -------------------
            _clearCalled = 1; // work around kludgey test and lazy me

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.Throws<AssertionException>(() =>
                Type_ShouldInheritFromEntityBase<SomeNonEntity>()
            );

            //--------------- Assert -----------------------
        }


        [Test]
        public void COMCategory_ShouldBeAbleToPersistAndRecall()
        {
            ShouldBeAbleToPersist<ComBlockListReasonBuilder, COMBlockListReason>(ctx => ctx.BlockListReasons,
                (ctx, entity) =>
                {
                }, (before, after) =>
                {
                });
        }
    }

    public class ComBlockListReasonBuilder : GenericBuilder<ComBlockListReasonBuilder, COMBlockListReason>
    {
    }


    [TestFixture]
    public class OptionsTest : EntityPersistenceTestFixtureBase<APIFeedContext>
    {
        private int _clearCalled;

        [SetUp]
        public void SetUp()
        {
            _clearCalled = 0;
        }

        [TearDown]
        public void TearDown()
        {
            Assert.AreEqual(1, _clearCalled);
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Configure(true, connectionString => new DbSchemaImporter(connectionString, ""));
            RunBeforeFirstGettingContext(Clear);
        }

        private void Clear(APIFeedContext db)
        {
            _clearCalled++;
        }

        [Test]
        public void RunBeforeFirstGettingContext_ActionShouldOnlyBeRunOnce()
        {
            Expect(_clearCalled).To.Equal(0, "Clear Called not 0");
            using (GetContext())
            {
                Expect(_clearCalled).To.Equal(1, "Clear Called not 1");
            }

            Expect(_clearCalled).To.Equal(1, "Clear Called not 1");

            using (GetContext())
            {
                Expect(_clearCalled).To.Equal(1, "Clear Called not 1");
            }

            Expect(_clearCalled).To.Equal(1, "Clear Called not 1");
        }

        [Test]
        public void GetDealers()
        {
            using (var db = GetContext())
            {
                Expect(db).Not.To.Be.Null("Wasn&#39;t able to get a context");
                Expect(() => db.AutoFeedDealers.ToList())
                    .Not.To.Throw();
            }
        }
    }

    public class APIFeedContext : DbContext
    {
        public APIFeedContext()
        {
        }

        public APIFeedContext(string connection) : base(connection)
        {
        }

        public APIFeedContext(SqlConnection connection) : base(connection, true)
        {
        }

        public IDbSet<CORESubscription> CoreSubscriptions { get; set; }
        public IDbSet<AUTOFeedDealer> AutoFeedDealers { get; set; }
        public IDbSet<AUTOFeedSettings> AutoFeedSettings { get; set; }
        public IDbSet<AUTOFeedDealerSettings> AutoFeedDealerSettings { get; set; }
    }

    public class AUTOFeedDealerSettings
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class AUTOFeedSettings
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class AUTOFeedDealer
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class CORESubscription
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}