﻿using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils.Entity;
// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable InconsistentNaming

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestCOMBlockListReason: EntityPersistenceTestFixtureBase<CommunicatorContext>
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
            using (var ctx = GetContext())
            {
                Assert.AreEqual(1, _clearCalled);
            }

            //---------------Assert Precondition----------------
            Assert.AreEqual(1, _clearCalled);

            //---------------Execute Test ----------------------
            using (var ctx = GetContext())
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
            using (var ctx = GetContext())
            {
                Assert.AreEqual(1, _clearCalled);
            }

            //---------------Assert Precondition----------------
            Assert.AreEqual(1, _clearCalled);

            //---------------Execute Test ----------------------
            using (var ctx = GetContext())
            {
                Assert.AreEqual(1, _clearCalled);
            }

            //---------------Test Result -----------------------
            Assert.AreEqual(1, _clearCalled);
        }

        public class SomeEntity: EntityBase
        {
        }

        [Test]
        public void Type_ShouldInheritFromEntityBase_GivenTypeInheritingFromEntityBase_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            _clearCalled = 1;   // work around kludgey test and lazy me

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
            _clearCalled = 1;   // work around kludgey test and lazy me

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
            ShouldBeAbleToPersist<COMBlockListReasonBuilder, COMBlockListReason>(ctx => ctx.BlockListReasons, (ctx, entity) =>
            {
            }, (before, after) =>
            {
            });
        }

    }

    public class COMBlockListReasonBuilder: GenericBuilder<COMBlockListReasonBuilder, COMBlockListReason>
    {
    }

    public class COMBlockListBuilder: GenericBuilder<COMBlockListBuilder, COMBlockList>
    {
    }
}
