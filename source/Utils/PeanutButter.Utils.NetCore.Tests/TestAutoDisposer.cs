using NSubstitute;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoDisposer
    {
        [Test]
        public void Construct_WhenGivenFilePathForExistingFile_ShouldDeleteFileWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var disposable = Substitute.For<IDisposable>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new AutoDisposer(disposable))
            {
            }

            //---------------Test Result -----------------------
            disposable.Received(1).Dispose();
        }

        [Test]
        public void Add_WhenGivenFilePathForExistingFile_ShouldDeleteFileWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var disposable = Substitute.For<IDisposable>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var ad = new AutoDisposer())
            {
                ad.Add(disposable);
            }

            //---------------Test Result -----------------------
            disposable.Received(1).Dispose();
        }

        [Test]
        public void AutodisposablesAreOnlyDisposedOnce()
        {
            //---------------Set up test pack-------------------
            var disposable = Substitute.For<IDisposable>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ad = new AutoDisposer();
            ad.Add(disposable);
            ad.Dispose();
            ad.Dispose();

            //---------------Test Result -----------------------
            disposable.Received(1).Dispose();
        }

        public class SomeDisposable : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        [Test]
        public void Add_GenericVersion_ShouldReturnThing()
        {
            //---------------Set up test pack-------------------
            var disposer = new AutoDisposer();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var thing = disposer.Add(new SomeDisposable());

            //---------------Test Result -----------------------
            Expect(thing).To.Be.An.Instance.Of<SomeDisposable>();
            Expect(thing.Disposed).To.Be.False();
            disposer.Dispose();
            Expect(thing.Disposed).To.Be.True();
        }

        [Test]
        public void ShouldNotThrowIfSubThrowsOnDisposal()
        {
            // Arrange
            var called = false;
            var sub = new SomeDisposableWithCallback(
                () =>
                {
                    called = true;
                    throw new Exception("moo");
                });
            var sut = Create();
            // Pre-assert
            // Act
            sut.Add(sub);
            Expect(() => sut.Dispose())
                .Not.To.Throw();
            // Assert
            Expect(called).To.Be.True();
        }

        [Test]
        public void ShouldAllowSingleManualDisposal()
        {
            // Arrange
            var sut = Create();
            // Act
            var disposable = sut.Add<IDisposable>(Substitute.For<IDisposable>());
            sut.DisposeNow(disposable);
            // Assert
            Expect(disposable)
                .To.Have.Received(1)
                .Dispose();
            sut.Dispose();
            Expect(disposable)
                .To.Have.Received(1)
                .Dispose();
        }

        [Test]
        public void ShouldAllowParallelDisposalOnRequest()
        {
            // Arrange
            var howMany = GetRandomInt(20, 30);
            var sut = Create();
            sut.ThreadedDisposal = true;
            sut.MaxDegreeOfParallelism = howMany;
            var start = new Barrier(2);
            var done = new Barrier(howMany + 1);
            // Act
            for (var i = 0; i < howMany; i++)
            {
                sut.Add(new SomeDisposableWithCallback(() => done.SignalAndWait()));
            }
            Task.Run(
                () =>
                {
                    start.SignalAndWait();
                    sut.Dispose();
                    done.SignalAndWait();
                }
            );
            start.SignalAndWait();
            var completed = done.SignalAndWait(5000);
            // Assert
            Expect(completed)
                .To.Be.True();
        }

        private AutoDisposer Create()
        {
            return new AutoDisposer();
        }

        public class SomeDisposableWithCallback : IDisposable
        {
            private readonly Action _toCall;

            public SomeDisposableWithCallback(Action toCall)
            {
                _toCall = toCall;
            }
            
            public void Dispose()
            {
                _toCall();
            }
        }

        [Test]
        public void Dispose_ShouldDisposeOfRegisteredArticlesInReverseOrder()
        {
            //---------------Set up test pack-------------------
            var calls = new List<int>();
            using (var disposer = new AutoDisposer())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                disposer.Add(new SomeDisposableWithCallback(() => calls.Add(1)));
                disposer.Add(new SomeDisposableWithCallback(() => calls.Add(2)));

                //---------------Test Result -----------------------
            }
            Expect(calls)
                .To.Equal(new[] { 2, 1 });
        }
    }
}