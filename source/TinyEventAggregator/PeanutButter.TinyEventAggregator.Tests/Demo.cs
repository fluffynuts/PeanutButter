using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NExpect;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TinyEventAggregator.Tests
{
    [TestFixture]
    public class Demo
    {
        [Test]
        public async Task DemonstrateUsage()
        {
            // Arrange
            // you could use the singleton EventAggregator.Instance if you like
            // or have an instance provided by DI, or even use multiple instances
            // in your application if that makes sense for the usage
            var eventAggregator = new EventAggregator();
            var loggedInUsers = new List<User>();
            // Act

            // somewhere in the system, we'd like to know when a user logs in...
            eventAggregator.GetEvent<LoginEvent>()
                .Subscribe(user => loggedInUsers.Add(user));
            eventAggregator.GetEvent<LogoutEvent>()
                .Subscribe(user => loggedInUsers.Remove(user));

            // elsewhere, logins are happening
            var karen = new User()
            {
                Id = 1,
                Name = "Karen"
            };
            var bob = new User()
            {
                Id = 2,
                Name = "Bob"
            };

            // GetEvent<T> always returns the same event instance,
            // so if you're going to use it a lot, you can var it
            // off
            var loginEvent = eventAggregator.GetEvent<LoginEvent>();
            loginEvent.Publish(karen);
            Expectations.Expect(loggedInUsers)
                .To.Contain(karen);
            loginEvent.Publish(bob);
            Expectations.Expect(loggedInUsers)
                .To.Contain(bob);

            // we can suspend messaging:
            eventAggregator.Suspend();
            // now, messages are not processed...
            // note that publisher threads which use the `Publish` method will suspend too!
            // -> so for the test, we put this in a task
            var beforeBarrier = new Barrier(2);
            var afterBarrier = new Barrier(2);
            var sam = new User()
            {
                Id = 3,
                Name = "Sam"
            };

            Task.Run(() =>
            {
                beforeBarrier.SignalAndWait();
                loginEvent.Publish(sam);
                afterBarrier.SignalAndWait();
            });

            beforeBarrier.SignalAndWait();
            Thread.Sleep(100); // wait a little to let the Publish call kick off
            Expectations.Expect(loggedInUsers)
                .Not.To.Contain(sam);

            // we can unsuspend
            eventAggregator.Unsuspend();
            afterBarrier.SignalAndWait();
            Expectations.Expect(loggedInUsers)
                .To.Contain(sam);

            // there's an async publish which won't block up the publisher:
            eventAggregator.Suspend();
            var task = eventAggregator.GetEvent<LogoutEvent>()
                .PublishAsync(sam);
            var waited = task.Wait(100);
            Expectations.Expect(waited)
                .To.Be.False();
            Expectations.Expect(loggedInUsers)
                .To.Contain(sam);
            eventAggregator.Unsuspend();
            await task;
            Expectations.Expect(loggedInUsers)
                .Not.To.Contain(sam);

            // when to use Publish vs PublishAsync
            // - Publish is useful when you'd like to use your event aggregator
            //   from, eg, a WPF application, since handlers will get called
            //   on the same thread (most likely your UI thread, if Publish was
            //   called, eg, from a button click event handler), so you won't have to 
            //   marshall to the UI thread. However, you'll still have to consider
            //   what your consumer code is doing to ensure that the UI doesn't
            //   lock up, of course!
            //   Since Publish will BLOCK when the event aggregator (or single event
            //   type) is suspended, it's a bad idea to use Publish from your UI handlers
            //   if you ever plan on suspending pub/sub as suspension will likely cause
            //   your app to hang (UI thread will go unresponsive and you'll get a white
            //   app window
            //   On the other hand, the blocking nature of Publish means that you are
            //   guaranteed of message delivery order
            // - PublishAsync can be used as "fire and forget" for background
            //   handling, but of course this means that you're not guaranteed of
            //   message delivery order (chances are good they'll come in order, but
            //   this is really up to the TPL and how the task is scheduled) and you'll
            //   have to marshal back to the UI thread if that makes sense for your
            //   application
            // Naturally, receivers are invoked on the final thread that the publish
            //   happened on -- if you're using PublishAsync, that means it's quite
            //   likely to _not_ be the same thread as the caller

            // we can also suspend / resume for specific events
            var logoutEvent = eventAggregator.GetEvent<LogoutEvent>();
            logoutEvent.Suspend();
            var logoutTask = logoutEvent.PublishAsync(bob);
            logoutTask.Wait(100);
            Expectations.Expect(loggedInUsers)
                .To.Contain(bob); // logout messages were suspended!
            await loginEvent.PublishAsync(sam);
            Expectations.Expect(loggedInUsers)
                .To.Contain(sam);
            logoutEvent.Unsuspend();
            waited = logoutTask.Wait(100);
            Expectations.Expect(waited)
                .To.Be.True();
            Expectations.Expect(loggedInUsers)
                .Not.To.Contain(bob);

            // we can also have limited subscriptions
            var onceOffUser = null as User;
            var limitedUsers = new List<User>();
            var resubscribe = false;
            eventAggregator.GetEvent<OnceOffEvent>()
                .SubscribeOnce(OnceOffSubscription);
                
            var limit = RandomValueGen.GetRandomInt(3, 5);
            eventAggregator.GetEvent<LimitedEvent>()
                .LimitedSubscription(user => limitedUsers.Add(user), limit);
            for (var i = 0; i < 10; i++)
            {
                eventAggregator.GetEvent<OnceOffEvent>()
                    .Publish(RandomValueGen.GetRandom<User>());
                eventAggregator.GetEvent<LimitedEvent>()
                    .Publish(RandomValueGen.GetRandom<User>());
            }

            Expectations.Expect(onceOffUser)
                .Not.To.Be.Null();
            Expectations.Expect(limitedUsers)
                .To.Contain.Only(limit).Items();
            
            onceOffUser = null;
            resubscribe = true;
            eventAggregator.GetEvent<OnceOffEvent>()
                .SubscribeOnce(OnceOffSubscription);
            eventAggregator.GetEvent<OnceOffEvent>()
                .Publish(sam);
            Expectations.Expect(onceOffUser)
                .To.Be(sam);
            // since we should have resubscribed, this publish
            // will now throw
            Expectations.Expect(() => eventAggregator.GetEvent<OnceOffEvent>()
                .Publish(sam)
            ).To.Throw<InvalidOperationException>();

            // Assert

            void OnceOffSubscription(User user)
            {
                if (onceOffUser != null)
                {
                    throw new InvalidOperationException(
                        $"Once-off subscription should limit to one callback!"
                    );
                }

                onceOffUser = user;
                // sometimes it's useful to re-subscribe here, depending
                // on your app requirements (:
                if (resubscribe)
                {
                    eventAggregator.GetEvent<OnceOffEvent>()
                        .SubscribeOnce(OnceOffSubscription);
                }
            }
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class LoginEvent : EventBase<User>
        {
        }

        public class LogoutEvent : EventBase<User>
        {
        }

        public class OnceOffEvent : EventBase<User>
        {
        }

        public class LimitedEvent : EventBase<User>
        {
        }
    }
}