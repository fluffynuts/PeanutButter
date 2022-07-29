using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.Utils.PyLike;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestCircularList
    {
        [Test]
        public void ShouldStartEmpty()
        {
            // Arrange
            var sut = Create<int>();
            // Act
            Expect(sut.Count)
                .To.Equal(0);
            Expect(sut as IEnumerable<int>)
                .To.Be.Empty();
            // Assert
        }

        [TestFixture]
        public class WhenContainsOnlyOneElement
        {
            [Test]
            public void ShouldRepeatThatElement()
            {
                // Arrange
                var expected = GetRandomInt();
                var sut = Create(expected);
                var collected = new List<int>();
                // Act
                foreach (var item in sut)
                {
                    collected.Add(item);
                    if (collected.Count > 2)
                    {
                        break;
                    }
                }

                // Assert
                Expect(collected)
                    .To.Contain.Only(3).Equal.To(expected);
            }
        }

        [TestFixture]
        public class WhenContainsSequence
        {
            [Test]
            public void ShouldRepeatThatSequence()
            {
                // Arrange
                var expected = new[] { 1, 2, 1, 2, 1, 2 };
                var sut = Create(1, 2);
                var collected = new List<int>();
                // Act
                foreach (var item in sut)
                {
                    collected.Add(item);
                    if (collected.Count == expected.Length)
                    {
                        break;
                    }
                }

                // Assert
                Expect(collected)
                    .To.Equal(expected);
            }
        }

        [Test]
        public void ShouldBeAbleToAddItems()
        {
            // Arrange
            var sut = Create<int>();
            var expected = GetRandomInt();
            // Act
            sut.Add(expected);
            // Assert
            Expect(sut.Count)
                .To.Equal(1);
            Expect(sut.Take(3))
                .To.Contain.All.Equal.To(expected);
        }

        [Test]
        public void ShouldBeAbleToClear()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            sut.Clear();
            // Assert
            Expect(sut)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldBeAbleToTestForItem()
        {
            // Arrange
            var expected = GetRandomInt(11, 20);
            var sut = Create(1, 2, expected, 3, 4);
            // Act
            var result = sut.Contains(expected);
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        public void ShouldRemoveItem()
        {
            // Arrange
            var special = GetRandomInt(11, 20);
            var sut = Create(1, special, 4, 2);
            // Act
            sut.Remove(special);
            // Assert
            Expect(sut.Contains(special))
                .To.Be.False();
        }

        [Test]
        public void ShouldOnlyRemoveTheFirstOccurrenceOfTheItem()
        {
            // Arrange
            var special = GetRandomInt(11, 20);
            var sut = Create(1, special, 4, 2, special);
            // Act
            sut.Remove(special);
            // Assert
            Expect(sut.Contains(special))
                .To.Be.True();
            Expect(sut.Last())
                .To.Equal(special);
        }

        [Test]
        public void ShouldOnlyCopyOriginalData()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            var target = new int[3];
            // Act
            sut.CopyTo(target, 0);
            // Assert
            Expect(target)
                .To.Equal(new[] { 1, 2, 3 });
        }

        [Test]
        public void ShouldReturnIndexOfFromOriginalCollection()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            var result = sut.IndexOf(2);
            // Assert
            Expect(result)
                .To.Equal(1);
        }

        [Test]
        public void ShouldInsertIntoTheOriginalCollection()
        {
            // Arrange
            var sut = Create(1, 2);
            // Act
            sut.Insert(1, 3);
            // Assert
            Expect(sut.Count)
                .To.Equal(3);
            Expect(sut[1])
                .To.Equal(3);
        }

        [Test]
        public void ShouldRemoveAtOriginalIndex()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            sut.RemoveAt(1);
            // Assert
            Expect(sut.Take(3))
                .To.Equal(new[] { 1, 3, 1 });
        }

        [Test]
        public void ShouldRemoveAtModuloIndex()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            sut.RemoveAt(4);
            // Assert
            Expect(sut.Take(3))
                .To.Equal(new[] { 1, 3, 1 });
        }

        [Test]
        public void ShouldSetAtProvidedIndex()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            sut[1] = 4;
            // Assert
            Expect(sut.Take(4))
                .To.Equal(new[] { 1, 4, 3, 1 });
        }

        [Test]
        public void ShouldSetAtModuloIndex()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            sut[4] = 4;
            // Assert
            Expect(sut.Take(4))
                .To.Equal(new[] { 1, 4, 3, 1 });
        }

        [Test]
        public void ShouldGetAtProvidedIndex()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            var result = sut[1];
            // Assert
            Expect(result)
                .To.Equal(2);
        }

        [Test]
        public void ShouldGetAtModuloIndex()
        {
            // Arrange
            var sut = Create(1, 2, 3);
            // Act
            var result = sut[4];
            // Assert
            Expect(result)
                .To.Equal(2);
        }

        [Test]
        public void ShouldThrowCorrectExceptionWhenNoItemsAndTryingToSetItem()
        {
            // Arrange
            var sut = Create<int>();
            // Act
            Expect(() => sut[1] = 1)
                .To.Throw<ArgumentOutOfRangeException>();
            // Assert
        }

        [Test]
        public void ShouldThrowCorrectExceptionWhenNoItemsAndTryingToRemoveItem()
        {
            // Arrange
            var sut = Create<int>();
            // Act
            Expect(() => sut.RemoveAt(0))
                .To.Throw<ArgumentOutOfRangeException>();
            // Assert
        }

        [Test]
        public void ShouldZipNicelyWithFixedCollection()
        {
            // Arrange
            var sut = Create("red", "green", "blue");
            var numbers = new[] { 1, 2, 3, 4, 5 };
            var expected = new[]
            {
                Tuple.Create("red", 1),
                Tuple.Create("green", 2),
                Tuple.Create("blue", 3),
                Tuple.Create("red", 4),
                Tuple.Create("green", 5)
            };

            // Act
            var result = Zip(sut, numbers);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        private static IList<T> Create<T>(
            params T[] items
        )
        {
            return new CircularList<T>(items);
        }
    }
}