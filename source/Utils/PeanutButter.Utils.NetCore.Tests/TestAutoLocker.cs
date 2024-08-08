// ReSharper disable ObjectCreationAsStatement
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable MemberHidesStaticFromOuterClass

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestAutoLocker
{
    [TestFixture]
    public class OperatingOnSemaphore
    {
        [Test]
        public void GivenNullSemaphore_ShouldThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new AutoLocker(null as Semaphore))
                .To.Throw<ArgumentNullException>()
                .For("semaphore");

            //---------------Test Result -----------------------
        }

        [Test]
        public void GivenSemaphore_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new AutoLocker(new Semaphore(1, 1)))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void GivenSemaphore_ShouldLockSemaphore()
        {
            //---------------Set up test pack-------------------
            var theLock = new Semaphore(1, 1);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new AutoLocker(theLock))
            {
                var gotLockAgain = theLock.WaitOne(1);
                Expect(gotLockAgain)
                    .To.Be.False();
            }

            var gotLockAfter = theLock.WaitOne(1);
            //---------------Test Result -----------------------
            Expect(gotLockAfter)
                .To.Be.True();
            theLock.Release();
        }
    }

    [TestFixture]
    public class OperatingOnMutex
    {
        [Test]
        public void GivenNullMutex_ShouldThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new AutoLocker(null as Mutex))
                .To.Throw<ArgumentNullException>()
                .For("mutex");

            //---------------Test Result -----------------------
        }

        [Test]
        public void GivenMutex_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new AutoLocker(new Mutex()))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_WhenGivenMutex_ShouldLock()
        {
            //---------------Set up test pack-------------------
            var theLock = new Mutex();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new AutoLocker(theLock))
            {
                var test = new MutexLockTest(theLock);
                var t = new Thread(test.RunInThread);
                t.Start();
                t.Join();
                Expect(test.GotLock)
                    .To.Be.False();
            }

            var gotLockAfter = theLock.WaitOne(1);
            //---------------Test Result -----------------------
            Expect(gotLockAfter)
                .To.Be.True();
            theLock.ReleaseMutex();
        }
    }

    [TestFixture]
    public class OperatingOnSemaphoreSlim
    {
        [Test]
        public void GivenNullSemaphore_ShouldThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new AutoLocker(null as SemaphoreSlim))
                .To.Throw<ArgumentNullException>()
                .For("semaphore");

            //---------------Test Result -----------------------
        }

        [Test]
        public void GivenSemaphore_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new AutoLocker(new SemaphoreSlim(1, 1)))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void GivenSemaphoreSlim_ShouldLock()
        {
            //---------------Set up test pack-------------------
            var theLock = new SemaphoreSlim(1, 1);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new AutoLocker(theLock))
            {
                var gotLockAgain = theLock.Wait(1);
                Expect(gotLockAgain)
                    .To.Be.False();
            }

            var gotLockAfter = theLock.Wait(1);
            //---------------Test Result -----------------------
            Expect(gotLockAfter)
                .To.Be.True();
            theLock.Release();
        }
    }

    [TestFixture]
    public class Events
    {
        [TestFixture]
        public class WhenDisposing
        {
            [TestFixture]
            public class OperatingOnSemaphore
            {
                [Test]
                public void ShouldRunOnDisposedEvent()
                {
                    // Arrange
                    var lck = new Semaphore(1, 1);
                    var calls = 0;
                    // Act
                    using (var _ = new AutoLocker(lck, () => calls++))
                    {
                        Expect(lck.WaitOne(0))
                            .To.Be.False();
                    }

                    // Assert
                    Expect(lck.WaitOne(0))
                        .To.Be.True();
                    Expect(calls)
                        .To.Equal(1);
                }
            }

            [TestFixture]
            public class OperatingOnSemaphoreSlim
            {
                [Test]
                public void ShouldRunOnDisposedEvent()
                {
                    // Arrange
                    var lck = new SemaphoreSlim(1, 1);
                    var calls = 0;
                    // Act
                    using (var _ = new AutoLocker(lck, () => calls++))
                    {
                        Expect(lck.Wait(0))
                            .To.Be.False();
                    }

                    // Assert
                    Expect(lck.Wait(0))
                        .To.Be.True();
                    Expect(calls)
                        .To.Equal(1);
                }
            }

            [TestFixture]
            public class OperatingOnMutex
            {
                [Test]
                public void ShouldRunOnDisposedEvent()
                {
                    // Arrange
                    var lck = new Mutex();
                    var calls = 0;
                    // Act
                    using (var _ = new AutoLocker(lck, () => calls++))
                    {
                        // windows mutexes only lock _between_ threads
                        // -> which, trust me, has lead to some
                        //    interesting bugs in the past, and autolocker
                        //    only supports Mutex types to be more useful - 
                        //    I'd always recommend using a SemaphoreSlim(1,1)
                        //    instead so you can catch deadlocks before they're
                        //    a mystery; anyhoo, that's why this test runs in a
                        //    separate thread, in the most lazy way I could think
                        //    of 😅
                        var results = Run.InParallel(
                            () =>
                                Expect(lck.WaitOne(0))
                                    .To.Be.False(),
                            () => { }
                        );
                        Expect(results)
                            .To.Contain.All.Matched.By(
                                o => o is null
                            );
                    }

                    // Assert
                    Expect(lck.WaitOne(0))
                        .To.Be.True();
                    Expect(calls)
                        .To.Equal(1);
                }
            }
        }
    }

    public class MutexLockTest
    {
        public bool GotLock { get; private set; }
        private readonly Mutex _mutex;

        public MutexLockTest(Mutex mutex)
        {
            _mutex = mutex;
        }

        public void RunInThread()
        {
            GotLock = _mutex.WaitOne(1);
        }
    }
}