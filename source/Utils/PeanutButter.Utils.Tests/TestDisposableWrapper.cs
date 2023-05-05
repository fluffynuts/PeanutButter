using System;
using NUnit.Framework;
using NExpect;
using NSubstitute;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestDisposableWrapper
    {
        [Test]
        public void ShouldDisposeTheWrappedObject()
        {
            // Arrange
            var disposable = Substitute.For<IDisposable>();
            var sut = Create(disposable);
            // Act
            sut.Dispose();
            // Assert
            Expect(disposable)
                .To.Have.Received(1)
                .Dispose();
        }

        [Test]
        public void ShouldDisposeTheWrappedObjectOnceOnly()
        {
            // Arrange
            var disposable = Substitute.For<IDisposable>();
            var sut = Create(disposable);
            // Act
            sut.Dispose();
            sut.Dispose();
            // Assert
            Expect(disposable)
                .To.Have.Received(1)
                .Dispose();
        }

        [Test]
        public void ShouldCallTheProvidedHandlerBeforeDisposing()
        {
            // Arrange
            var disposable = Substitute.For<IDisposable>();
            var isDisposed = false;
            disposable.When(o => o.Dispose())
                .Do(_ => isDisposed = true);
            var sut = Create(disposable);
            var called = false;
            var wasDisposed = null as bool?;
            var capturedSender = null as object;
            var capturedArgs = null as DisposableWrapperEventArgs;
            sut.BeforeDisposing += (o, e) =>
            {
                called = true;
                capturedSender = o;
                capturedArgs = e;
                wasDisposed = isDisposed;
            };
            // Act
            sut.Dispose();
            // Assert
            Expect(called)
                .To.Be.True();
            Expect(wasDisposed)
                .To.Be.False();
            Expect(capturedSender)
                .To.Be(sut);
            Expect(capturedArgs.EventName)
                .To.Equal("BeforeDisposing");
            Expect(capturedArgs.Disposable)
                .To.Be(disposable);
        }

        [Test]
        public void ShouldCallTheProvidedHandlerAfterDisposing()
        {
            // Arrange
            var disposable = Substitute.For<IDisposable>();
            var isDisposed = false;
            var wasDisposed = null as bool?;
            disposable.When(o => o.Dispose())
                .Do(_ => isDisposed = true);
            var sut = Create(disposable);
            var called = false;
            var capturedSender = null as object;
            var capturedArgs = null as DisposableWrapperEventArgs;
            sut.AfterDisposing += (o, e) =>
            {
                called = true;
                capturedSender = o;
                capturedArgs = e;
                wasDisposed = isDisposed;
            };
            // Act
            sut.Dispose();
            // Assert
            Expect(called)
                .To.Be.True();
            Expect(wasDisposed)
                .To.Be.True();
            Expect(capturedSender)
                .To.Be(sut);
            Expect(capturedArgs.EventName)
                .To.Equal("AfterDisposing");
            Expect(capturedArgs.Disposable)
                .To.Be(disposable);
        }

        [Test]
        public void ShouldCallTheProvidedWrapperOnDisposalError()
        {
            // Arrange
            var disposable = Substitute.For<IDisposable>();
            var isDisposed = false;
            var wasDisposed = null as bool?;
            var message = GetRandomWords();
            disposable.When(o => o.Dispose())
                .Do(_ =>
                {
                    isDisposed = true;
                    throw new Exception(message);
                });
            var sut = Create(disposable);
            var called = false;
            var capturedSender = null as object;
            var capturedArgs = null as DisposableWrapperErrorEventArgs;
            var capturedGenericArgs = null as DisposableWrapperEventArgs;
            // just to ensure that a single handler could be registered for all
            sut.OnDisposingError += GenericHandler;
            sut.OnDisposingError += (o, e) =>
            {
                called = true;
                capturedSender = o;
                capturedArgs = e;
                wasDisposed = isDisposed;
            };
            // Act
            Expect(() => sut.Dispose())
                .To.Throw<Exception>()
                .With.Message.Equal.To(message);
            // Assert
            Expect(called)
                .To.Be.True();
            Expect(wasDisposed)
                .To.Be.True();
            Expect(capturedSender)
                .To.Be(sut);
            Expect(capturedArgs.EventName)
                .To.Equal("OnDisposingError");
            Expect(capturedArgs.Disposable)
                .To.Be(disposable);
            Expect(capturedArgs.Exception.Message)
                .To.Equal(message);
            Expect(capturedGenericArgs)
                .To.Be(capturedArgs);

            void GenericHandler(object sender, DisposableWrapperEventArgs args)
            {
                capturedGenericArgs = args;
            }
        }


        private static DisposableWrapper Create(
            IDisposable toWrap
        )
        {
            return new(toWrap);
        }
    }
}