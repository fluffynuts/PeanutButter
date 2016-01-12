using NUnit.Framework;

namespace PeanutButter.TinyEventAggregator.Tests
{
    [TestFixture]
    public class TestTinyEventAggregator
    {
        [Test]
        public void Construct_DoesNotThrow()
        {
            // test setup
            
            // pre-conditions

            // execute test
            Assert.DoesNotThrow(() => new EventAggregator());

            // test result
        }

        [Test]
        public void Instance_ReturnsSameInstanceEveryTime()
        {
            // test setup
            
            // pre-conditions

            // execute test
            var i1 = EventAggregator.Instance;
            var i2 = EventAggregator.Instance;

            // test result
            Assert.IsNotNull(i1);
            Assert.IsNotNull(i2);
            Assert.AreEqual(i1, i2);
        }

        public class SomeEvent : EventBase<object> { }
        [Test]
        public void GetEvent_AlwaysReturnsSameInstanceOfEvent()
        {
            // test setup
            
            // pre-conditions

            // execute test
            var ev1 = EventAggregator.Instance.GetEvent<SomeEvent>();
            var ev2 = EventAggregator.Instance.GetEvent<SomeEvent>();

            // test result
            Assert.IsNotNull(ev1);
            Assert.IsNotNull(ev2);
            Assert.AreEqual(ev1, ev2);
        }

    }
}
