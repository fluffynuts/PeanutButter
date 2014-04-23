using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.TinyEventAggregator;

namespace PeanutButter.TinyEventAggregator.Tests
{
    [TestFixture]
    public class TestEventBase
    {
        public class SomeEvent : EventBase<object>
        {
        }
        [Test]
        public void Construct_DoesNotThrow()
        {
            // test setup
            
            // pre-conditions

            // execute test
            Assert.DoesNotThrow(() => Create());

            // test result
        }

        private static SomeEvent Create()
        {
            return new SomeEvent();
        }

        [Test]
        public void Subscribe_GivenNullAction_ThrowsArgumentNullException()
        {
            // test setup
            var ev = Create();    
            // pre-conditions

            // execute test
            Assert.Throws<ArgumentNullException>(() => ev.Subscribe(null));

            // test result
        }

        [Test]
        public void Publish_WhenNoSubscribers_DoesNotThrow()
        {
            // test setup
            var ev = Create();

            // pre-conditions

            // execute test
            Assert.DoesNotThrow(() => ev.Publish(null));

            // test result
        }

        [Test]
        public void SubscribeAndPublish_WhenOneSubscribedAndAnotherPublishes_CallsAction()
        {
            // test setup
            var called = false;
            var ev = Create();
            
            // pre-conditions
            Assert.IsFalse(called);

            // execute test
            ev.Subscribe(o => called = true);
            Assert.IsFalse(called);
            ev.Publish(null);

            // test result
            Assert.IsTrue(called);
        }

        [Test]
        public void SubscribeAndPublish_PassesDataOnToAllSubscribers()
        {
            // test setup
            var ev = Create();
            object received1 = null;
            object received2 = null;

            // pre-conditions
            Assert.IsNull(received1);
            Assert.IsNull(received2);
            // execute test

            ev.Subscribe(o => received1 = o);
            ev.Subscribe(o => received2 = o);
            Assert.IsNull(received1);
            Assert.IsNull(received2);
            var published = new object();
            ev.Publish(published);

            // test result
            Assert.IsNotNull(received1);
            Assert.IsNotNull(received2);
            Assert.AreEqual(published, received1);
            Assert.AreEqual(published, received2);
        }

        [Test]
        public void Unsubscribe_GivenNullToken_ThrowsArgumentNullException()
        {
            // test setup
            var ev = Create();
            
            // pre-conditions

            // execute test
            Assert.Throws<ArgumentNullException>(() => ev.Unsubscribe(null));

            // test result
        }

        [Test]
        public void Unsubscribe_WhenPassedKnownToken_UnsubscribesReciever()
        {
            // test setup
            var ev = Create();
            var callCount = 0;
            
            // pre-conditions

            // execute test
            var token = ev.Subscribe(o => callCount++);
            Assert.AreEqual(0, callCount);
            ev.Publish(null);
            Assert.AreEqual(1, callCount);
            ev.Unsubscribe(token);
            ev.Publish(null);

            // test result
            Assert.AreEqual(1, callCount);
        }
    }
}
