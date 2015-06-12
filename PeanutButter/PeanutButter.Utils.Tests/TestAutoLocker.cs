using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoLockerFat
    {
        [Test]
        public void Construct_WhenGivenNullSemaphore_ShouldThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => new AutoLocker((Semaphore)null));

            //---------------Test Result -----------------------
            Assert.AreEqual("semaphore", ex.ParamName);
        }

        [Test]
        public void Construct_WhenGivenNullMutex_ShouldThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => new AutoLocker((Mutex)null));

            //---------------Test Result -----------------------
            Assert.AreEqual("mutex", ex.ParamName);
        }

        [Test]
        public void Construct_WhenGivenSemaphore_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new AutoLocker(new Semaphore(1, 1)));

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_WhenGivenMutex_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new AutoLocker(new Mutex()));

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_WhenGivenSemaphore_ShouldLockSemaphore()
        {
            //---------------Set up test pack-------------------
            var theLock = new Semaphore(1, 1);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new AutoLocker(theLock))
            {
                var gotLockAgain = theLock.WaitOne(1);
                Assert.IsFalse(gotLockAgain);
            }
            var gotLockAfter = theLock.WaitOne(1);
            //---------------Test Result -----------------------
            Assert.IsTrue(gotLockAfter);
            theLock.Release();
        }

        public class MutexLockTest
        {
            public bool GotLock { get; private set; }
            private Mutex _mutex;

            public MutexLockTest(Mutex mutex)
            {
                _mutex = mutex;
            }

            public void RunInThread()
            {
                GotLock = _mutex.WaitOne(1);
            }

        }

        [Test]
        public void Construct_WhenGivenMutex_ShouldLockSemaphore()
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
                Assert.IsFalse(test.GotLock);
            }
            var gotLockAfter = theLock.WaitOne(1);
            //---------------Test Result -----------------------
            Assert.IsTrue(gotLockAfter);
            theLock.ReleaseMutex();
        }

    }
    [TestFixture]
    public class TestAutoLockerSlim
    {
        [Test]
        public void Construct_WhenGivenNullSemaphore_ShouldThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => new AutoLocker((SemaphoreSlim)null));

            //---------------Test Result -----------------------
            Assert.AreEqual("semaphore", ex.ParamName);
        }

        [Test]
        public void Construct_WhenGivenNullMutex_ShouldThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<ArgumentNullException>(() => new AutoLocker((Mutex)null));

            //---------------Test Result -----------------------
            Assert.AreEqual("mutex", ex.ParamName);
        }

        [Test]
        public void Construct_WhenGivenSemaphore_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new AutoLocker(new SemaphoreSlim(1, 1)));

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_WhenGivenMutex_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new AutoLocker(new Mutex()));

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_WhenGivenSemaphore_ShouldLockSemaphore()
        {
            //---------------Set up test pack-------------------
            var theLock = new SemaphoreSlim(1, 1);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new AutoLocker(theLock))
            {
                var gotLockAgain = theLock.Wait(1);
                Assert.IsFalse(gotLockAgain);
            }
            var gotLockAfter = theLock.Wait(1);
            //---------------Test Result -----------------------
            Assert.IsTrue(gotLockAfter);
            theLock.Release();
        }

        public class MutexLockTest
        {
            public bool GotLock { get; private set; }
            private Mutex _mutex;

            public MutexLockTest(Mutex mutex)
            {
                _mutex = mutex;
            }

            public void RunInThread()
            {
                GotLock = _mutex.WaitOne(1);
            }

        }

        [Test]
        public void Construct_WhenGivenMutex_ShouldLockSemaphore()
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
                Assert.IsFalse(test.GotLock);
            }
            var gotLockAfter = theLock.WaitOne(1);
            //---------------Test Result -----------------------
            Assert.IsTrue(gotLockAfter);
            theLock.ReleaseMutex();
        }

    }
}
