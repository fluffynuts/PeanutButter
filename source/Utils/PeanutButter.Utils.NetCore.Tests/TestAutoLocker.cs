// ReSharper disable ObjectCreationAsStatement
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestAutoLockerFat
{
    [Test]
    public void Construct_WhenGivenNullSemaphore_ShouldThrow()
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
    public void Construct_WhenGivenNullMutex_ShouldThrow()
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
    public void Construct_WhenGivenSemaphore_ShouldNotThrow()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(() => new AutoLocker(new Semaphore(1, 1)))
            .Not.To.Throw();

        //---------------Test Result -----------------------
    }

    [Test]
    public void Construct_WhenGivenMutex_ShouldNotThrow()
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(() => new AutoLocker(new Mutex()))
            .Not.To.Throw();

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
            Expect(gotLockAgain)
                .To.Be.False();
        }

        var gotLockAfter = theLock.WaitOne(1);
        //---------------Test Result -----------------------
        Expect(gotLockAfter)
            .To.Be.True();
        theLock.Release();
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
            Expect(test.GotLock)
                .To.Be.False();
        }

        var gotLockAfter = theLock.WaitOne(1);
        //---------------Test Result -----------------------
        Expect(gotLockAfter)
            .To.Be.True();
        theLock.ReleaseMutex();
    }

    [TestFixture]
    public class OperatingOnSemaphoreSlim
    {
        [Test]
        public void Construct_WhenGivenNullSemaphore_ShouldThrow()
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
        public void Construct_WhenGivenSemaphore_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new AutoLocker(new SemaphoreSlim(1, 1)))
                .Not.To.Throw();

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
                Expect(gotLockAgain)
                    .To.Be.False();
            }

            var gotLockAfter = theLock.Wait(1);
            //---------------Test Result -----------------------
            Expect(gotLockAfter)
                .To.Be.True();
            theLock.Release();
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
}