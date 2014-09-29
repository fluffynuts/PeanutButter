using System;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
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
                this.Disposed = true;
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
            Assert.IsInstanceOf<SomeDisposable>(thing);
            Assert.IsFalse(thing.Disposed);
            disposer.Dispose();
            Assert.IsTrue(thing.Disposed);
        }
    }
}