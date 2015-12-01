using System;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils.Entity;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestCOMBlockListReason: EntityPersistenceTestFixtureBase<CommunicatorContext>
    {
        private int _clearCalled;

        public TestCOMBlockListReason()
        {
            Configure(true, connectionString => new DbSchemaImporter(connectionString));
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
