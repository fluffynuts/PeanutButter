using System;
using System.Linq;

#pragma warning disable CS0618 // Type or member is obsolete

namespace PeanutButter.Utils.Tests
{
    
    [TestFixture]
    public class TestRollingWindow
    {
        [Test]
        public void ShouldStoreUpToWindowSize()
        {
            // Arrange
            var sut = Create<int>(5);
            // Act
            sut.Add(1);
            Expect(sut.ToArray())
                .To.Equal([1]);
            sut.Add(2);
            Expect(sut.ToArray())
                .To.Equal([1, 2]);
            sut.Add(3);
            Expect(sut.ToArray())
                .To.Equal([1, 2, 3]);
            sut.Add(4);
            Expect(sut.ToArray())
                .To.Equal([1, 2, 3, 4]);
            sut.Add(5);
            Expect(sut.ToArray())
                .To.Equal([1, 2, 3, 4, 5]);
            sut.Add(6);
            Expect(sut.ToArray())
                .To.Equal([2, 3, 4, 5, 6]);
            // Assert
        }

        [Test]
        public void ShouldBeAbleToResize()
        {
            // Arrange
            var sut = Create<int>(5);
            // Act
            sut.Add(1);
            Expect(sut.ToArray())
                .To.Equal([1]);
            sut.Add(2);
            Expect(sut.ToArray())
                .To.Equal([1, 2]);
            sut.Add(3);
            Expect(sut.ToArray())
                .To.Equal([1, 2, 3]);
            sut.Add(4);
            Expect(sut.ToArray())
                .To.Equal([1, 2, 3, 4]);
            sut.Add(5);
            Expect(sut.ToArray())
                .To.Equal([1, 2, 3, 4, 5]);
            sut.MaxSize = 3;
            Expect(sut.ToArray())
                .To.Equal([3, 4, 5]);
            sut.Add(6);
            Expect(sut.ToArray())
                .To.Equal([4, 5, 6]);
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

        [Test]
        public void ShouldBeAbleToEnumerate()
        {
            // Arrange
            var sut = Create<int>(5);
            // Act
            sut.Add(1);
            sut.Add(2);
            sut.Add(3);
            sut.Add(4);
            sut.Add(5);
            
            var all = sut.ToArray();
            Expect(all)
                .To.Equal([1, 2, 3, 4, 5]);
            var avg = sut.Average();
            var count = sut.Count();
            // Assert
            Expect(avg)
                .To.Equal(3);
            Expect(count)
                .To.Equal(5);
        }

        private static RollingWindow<T> Create<T>(long size)
        {
            return new RollingWindow<T>(size);
        }
    }
}