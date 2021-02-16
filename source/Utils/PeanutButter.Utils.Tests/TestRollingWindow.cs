using System;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    
    [TestFixture]
    public class TestRollingWindow
    {
        [Test]
        public void ShouldStoreUpToWindowSizeAndProvideSnapshot()
        {
            // Arrange
            var sut = Create<int>(5);
            // Act
            sut.Add(1);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1 } );
            sut.Add(2);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2 } );
            sut.Add(3);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2, 3 } );
            sut.Add(4);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2, 3, 4 } );
            sut.Add(5);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2, 3, 4, 5 } );
            sut.Add(6);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 2, 3, 4, 5, 6 } );
            // Assert
        }

        [Test]
        public void ShouldBeAbleToResize()
        {
            // Arrange
            var sut = Create<int>(5);
            // Act
            sut.Add(1);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1 } );
            sut.Add(2);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2 } );
            sut.Add(3);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2, 3 } );
            sut.Add(4);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2, 3, 4 } );
            sut.Add(5);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 1, 2, 3, 4, 5 } );
            sut.MaxSize = 3;
            Expect(sut.Snapshot())
                .To.Equal(new[] { 3, 4, 5 });
            sut.Add(6);
            Expect(sut.Snapshot())
                .To.Equal(new[] { 4, 5, 6 } );
            // Assert
        }

        [Test]
        public void ShouldDispose()
        {
            // Arrange
            var sut = Create<int>(5);
            // Act
            sut.Dispose();
            Expect(() => sut.Dispose())
                .To.Throw<ObjectDisposedException>();
            // Assert
        }

        private static RollingWindow<T> Create<T>(long size)
        {
            return new RollingWindow<T>(size);
        }
    }
}