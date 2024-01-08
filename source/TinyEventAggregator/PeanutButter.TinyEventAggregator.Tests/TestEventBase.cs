using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TinyEventAggregator.Tests
{
    [TestFixture]
    public class TestEventBase
    {
        [TestFixture]
        public class Constructor
        {
            [Test]
            public void ShouldNotThrow()
            {
                // test setup

                // pre-conditions

                // execute test
                Expect(Create)
                    .Not.To.Throw();

                // test result
            }
        }

        [TestFixture]
        public class Behavior
        {
            [Test]
            public void Subscribe_GivenNullAction_ThrowsArgumentNullException()
            {
                // test setup
                var ev = Create();
                // pre-conditions

                // execute test
                Expect(() => ev.Subscribe(null))
                    .To.Throw<ArgumentNullException>();

                // test result
            }

            [Test]
            public void Publish_WhenNoSubscribers_DoesNotThrow()
            {
                // test setup
                var ev = Create();

                // pre-conditions

                // execute test
                Expect(() => ev.Publish(null))
                    .Not.To.Throw();

                // test result
            }

            [Test]
            public void SubscribeAndPublish_WhenOneSubscribedAndAnotherPublishes_CallsActionOnce()
            {
                // test setup
                var calls = 0;
                var ev = Create();

                // pre-conditions
                // execute test
                ev.Subscribe(_ => calls++);
                Expect(calls)
                    .To.Equal(0);
                ev.Publish(null);

                // test result
                Expect(calls)
                    .To.Equal(1);
            }
        }

        [Test]
        public void SubscribeAndPublish_PassesDataOnToAllSubscribers()
        {
            // test setup
            var ev = Create();
            object received1 = null;
            object received2 = null;

            // execute test
            ev.Subscribe(o => received1 = o);
            ev.Subscribe(o => received2 = o);

            Expect(received1)
                .To.Be.Null();
            Expect(received2)
                .To.Be.Null();
            var expected = new object();
            ev.Publish(expected);

            // test result
            Expect(received1)
                .Not.To.Be.Null();
            Expect(received2)
                .Not.To.Be.Null();
            Expect(received1)
                .To.Be(expected);
            Expect(received2)
                .To.Be(expected);
        }

        [Test]
        public void Unsubscribe_GivenNullToken_ThrowsArgumentNullException()
        {
            // test setup
            var ev = Create();

            // pre-conditions

            // execute test
            Expect(() => ev.Unsubscribe(null))
                .To.Throw<ArgumentNullException>();

            // test result
        }

        [Test]
        public void Unsubscribe_WhenPassedKnownToken_UnsubscribesReceiver()
        {
            // test setup
            var ev = Create();
            var callCount = 0;

            // pre-conditions

            // execute test
            var token = ev.Subscribe(_ => callCount++);
            Expect(callCount)
                .To.Equal(0);

            ev.Publish(null);
            Expect(callCount)
                .To.Equal(1);
            ev.Unsubscribe(token);
            ev.Publish(null);

            // test result
            Expect(callCount)
                .To.Equal(1);
        }

        [Test]
        public void SubscribeOnce_SubscribesToEventForOnePublication()
        {
            //---------------Set up test pack-------------------
            var callCount = 0;
            var ev = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            ev.SubscribeOnce(_ => callCount++);
            ev.Publish(null);
            ev.Publish(null);

            //---------------Test Result -----------------------
            Expect(callCount)
                .To.Equal(1);
        }

        [Test]
        public void SubscriptionCount_ReflectsCountOfActiveSubscriptions()
        {
            //---------------Set up test pack-------------------
            var ev = Create();

            //---------------Assert Precondition----------------
            Expect(ev.SubscriptionCount)
                .To.Equal(0);
            //---------------Execute Test ----------------------
            var t1 = ev.Subscribe(_ =>
            {
            });
            Expect(ev.SubscriptionCount)
                .To.Equal(1);
            var t2 = ev.Subscribe(_ =>
            {
            });
            Expect(ev.SubscriptionCount)
                .To.Equal(2);
            ev.Unsubscribe(t2);
            Expect(ev.SubscriptionCount)
                .To.Equal(1);
            // unknown token should not change subs
            ev.Unsubscribe(new SubscriptionToken());
            Expect(ev.SubscriptionCount)
                .To.Equal(1);
            ev.Unsubscribe(t1);
            Expect(ev.SubscriptionCount)
                .To.Equal(0);
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
            var token = ev.Subscribe(_ =>
            {
            });

            //---------------Test Result -----------------------
            Expect(eventSender)
                .Not.To.Be.Null();
            Expect(eventSender)
                .To.Be(ev);
            Expect(eventToken)
                .To.Be(token);
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
            var token = ev.Subscribe(_ =>
            {
            });

            //---------------Test Result -----------------------
            Expect(eventSender)
                .Not.To.Be.Null();
            Expect(eventSender)
                .To.Be(ev);
            Expect(eventToken)
                .To.Be(token);
        }

        [Test]
        public void Suspend_ShouldSetSuspendedToTrue()
        {
            //---------------Set up test pack-------------------
            var sut = Create();

            //---------------Assert Precondition----------------
            Expect(sut.IsSuspended)
                .To.Be.False();

            //---------------Execute Test ----------------------
            sut.Suspend();

            //---------------Test Result -----------------------
            Expect(sut.IsSuspended)
                .To.Be.True();
        }

        [Test]
        public void Unsuspend_ShouldSetSuspendedToFalse()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            sut.Suspend();
            //---------------Assert Precondition----------------
            Expect(sut.IsSuspended)
                .To.Be.True();
            //---------------Execute Test ----------------------
            sut.Unsuspend();

            //---------------Test Result -----------------------
            Expect(sut.IsSuspended)
                .To.Be.False();
        }

        [Test]
        public void WaitForSuspension_WhenNotSuspended_ShouldNotWait()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            //---------------Assert Precondition----------------
            Expect(sut.IsSuspended)
                .To.Be.False();

            //---------------Execute Test ----------------------
            var beforeTest = DateTime.Now;
            sut.WaitForSuspension();
            var afterTest = DateTime.Now;

            //---------------Test Result -----------------------
            var delta = afterTest - beforeTest;
            Expect(delta.TotalMilliseconds)
                .To.Be.Less.Than(200);
        }

        [Test]
        public void WaitForSuspension_WhenSuspended_ShouldNotProceedUntilUnsuspended()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            sut.Suspend();
            //---------------Assert Precondition----------------
            Expect(sut.IsSuspended)
                .To.Be.True();

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
                Expect(called)
                    .To.Be.False();
                sut.Unsuspend();
            });


            //---------------Test Result -----------------------
            Task.WaitAll(task1, task2);
            Expect(called)
                .To.Be.True();
        }

        public class SomeEvent : EventBase<object>
        {
        }

        private static SomeEvent Create()
        {
            return new SomeEvent();
        }
    }
}