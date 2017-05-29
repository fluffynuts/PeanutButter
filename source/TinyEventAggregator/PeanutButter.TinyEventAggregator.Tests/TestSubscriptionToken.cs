using NUnit.Framework;

namespace PeanutButter.TinyEventAggregator.Tests
{
    [TestFixture]
    public class TestSubscriptionToken
    {
        [Test]
        public void Construct_SubsequentConstructionsShouldSetIncrementalIdForInspection()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var first = Create();
            var second = Create();
            var third = Create();

            //---------------Test Result -----------------------
            Assert.That(first.Id, Is.LessThan(second.Id));
            Assert.That(second.Id, Is.LessThan(third.Id));
        }

        private SubscriptionToken Create()
        {
            return new SubscriptionToken();
        }
    }
}
