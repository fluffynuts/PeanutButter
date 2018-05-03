using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using static NExpect.Expectations;
using NExpect;

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

        private AutoDisposer Create()
        {
            return new AutoDisposer();
        }

        public class SomeDisposableWithCallback : IDisposable
        {
            private Action _toCall;

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
            Assert.AreEqual(2, calls.Count);
            Assert.AreEqual(2, calls[0]);
            Assert.AreEqual(1, calls[1]);
        }
    }
}