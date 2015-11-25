using System;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.Entity.Tests
{
    [TestFixture]
    public class TestCOMBlockListReason: EntityPersistenceTestFixtureBase<CommunicatorContext>
    {
        public TestCOMBlockListReason()
        {
            Configure(true, connectionString => new DbSchemaImporter(connectionString));
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
