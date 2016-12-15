using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.TinyEventAggregator.Tests
{
    [TestFixture]
    public class TestEventBase
    {
        public class SomeEvent : EventBase<object>
        {
            public bool Suspended => IsSuspended;
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

        [Test]
        public void SubscribeOnce_SubscribesToEventForOnePublication()
        {
            //---------------Set up test pack-------------------
            var callCount = 0;
            var ev = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            ev.SubscribeOnce(o => callCount++);
            ev.Publish(null);
            ev.Publish(null);

            //---------------Test Result -----------------------
            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void SubscriptionCount_ReflectsCountOfActiveSubscriptions()
        {
            //---------------Set up test pack-------------------
            var ev = Create();

            //---------------Assert Precondition----------------
            Assert.AreEqual(0, ev.SubscriptionCount);
            //---------------Execute Test ----------------------
            var t1 = ev.Subscribe(o => { });
            Assert.AreEqual(1, ev.SubscriptionCount);
            var t2 = ev.Subscribe(o => { });
            Assert.AreEqual(2, ev.SubscriptionCount);
            ev.Unsubscribe(t2);
            Assert.AreEqual(1, ev.SubscriptionCount);
            ev.Unsubscribe(new SubscriptionToken());
            Assert.AreEqual(1, ev.SubscriptionCount);
            ev.Unsubscribe(t1);
            Assert.AreEqual(0, ev.SubscriptionCount);
            //---------------Test Result -----------------------
        }

        [Test]
        public void Subscribing_FiresSubscriptionAddedEventHandler()
        {
            //---------------Set up test pack-------------------
            var ev = Create();
            object eventSender = null;
            SubscriptionToken eventToken = null;
            ev.OnSubscriptionAdded += (s, e) =>
            {
                eventSender = s;
                eventToken = e.Token;
            };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var token = ev.Subscribe(o => { });

            //---------------Test Result -----------------------
            Assert.IsNotNull(eventSender);
            Assert.AreEqual(ev, eventSender);
            Assert.AreEqual(token, eventToken);
        }

        [Test]
        public void Subscribing_FiresSubscriptionRemovedEventHandler()
        {
            //---------------Set up test pack-------------------
            var ev = Create();
            object eventSender = null;
            SubscriptionToken eventToken = null;
            ev.OnSubscriptionAdded += (s, e) =>
                {
                    eventSender = s;
                    eventToken = e.Token;
                };
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var token = ev.Subscribe(o => { });

            //---------------Test Result -----------------------
            Assert.IsNotNull(eventSender);
            Assert.AreEqual(ev, eventSender);
            Assert.AreEqual(token, eventToken);
        }

        [Test]
        public void Suspend_ShouldSetSuspendedToTrue()
        {
            //---------------Set up test pack-------------------
            var sut = Create();

            //---------------Assert Precondition----------------
            Assert.IsFalse(sut.Suspended);

            //---------------Execute Test ----------------------
            sut.Suspend();

            //---------------Test Result -----------------------
            Assert.IsTrue(sut.Suspended);
        }

        [Test]
        public void Unsuspend_ShouldSetSuspendedToFalse()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            sut.Suspend();
            //---------------Assert Precondition----------------
            Assert.IsTrue(sut.Suspended);
            //---------------Execute Test ----------------------
            sut.Unsuspend();

            //---------------Test Result -----------------------
            Assert.IsFalse(sut.Suspended);
        }

        [Test]
        public void WaitForSuspension_WhenNotSuspended_ShouldNotWait()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            //---------------Assert Precondition----------------
            Assert.IsFalse(sut.Suspended);

            //---------------Execute Test ----------------------
            var beforeTest = DateTime.Now;
            sut.WaitForSuspension();
            var afterTest = DateTime.Now;

            //---------------Test Result -----------------------
            var delta = afterTest - beforeTest;
            Assert.That(delta.TotalMilliseconds, Is.LessThanOrEqualTo(200));
        }

        [Test]
        public void WaitForSuspension_WhenSuspended_ShouldNotProceedUntilUnsuspended()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            sut.Suspend();
            //---------------Assert Precondition----------------
            Assert.IsTrue(sut.Suspended);

            //---------------Execute Test ----------------------
            var barrier = new Barrier(2);
            var called = false;
            var task1 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                sut.WaitForSuspension();
                called = true;
            });
            var task2 = Task.Run(() =>
            {
                barrier.SignalAndWait();
                Thread.Sleep(1000);
                Assert.IsFalse(called);
                sut.Unsuspend();
            });


            //---------------Test Result -----------------------
            Task.WaitAll(task1, task2);
            Assert.IsTrue(called);
        }





    }
}
