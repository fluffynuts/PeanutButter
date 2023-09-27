using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestMostRecentlyUsedList
    {
        [TestFixture]
        public class Add
        {
            [Test]
            public void ShouldAddSingleItemToSingleCapacityList()
            {
                // Arrange
                var sut = Create<string>(1);
                // Act
                sut.Add("abc");
                // Assert
                Expect(sut.Contains("abc"))
                    .To.Be.True();
            }

            [Test]
            public void ShouldEvictTheOlderItem()
            {
                // Arrange
                var sut = Create<string>(1);
                // Act
                sut.Add("abc");
                sut.Add("123");
                // Assert
                Expect(sut.Contains("abc"))
                    .To.Be.False();
                Expect(sut.Contains("123"))
                    .To.Be.True();
            }

            [Test]
            public void ShouldNotAddTheSameItemTwice()
            {
                // Arrange
                var sut = Create<string>(5);
                // Act
                sut.Add("abc");
                sut.Add("abc");
                // Assert
                Expect(sut.Items.Keys)
                    .To.Equal(new[] { "abc" } );
            }
        }

        [TestFixture]
        public class Enumerating
        {
            [Test]
            public void ShouldEnumerateInMostRecentOrder()
            {
                // Arrange
                var sut = Create<string>(5);
                // Act
                sut.Add("c");
                Thread.Sleep(1);
                sut.Add("a");
                Thread.Sleep(1);
                sut.Add("def");
                Thread.Sleep(1);
                sut.Add("123");
                var result = sut.ToArray();
                // Assert
                Expect(result)
                    .To.Equal(new[]
                    {
                        "123",
                        "def",
                        "a",
                        "c"
                    });
            }

            [Test]
            public void ShouldEnumerateInMostRecentOrder2()
            {
                // Arrange
                var sut = Create<string>(5);
                // Act
                sut.Add("c");
                Thread.Sleep(1);
                sut.Add("a");
                Thread.Sleep(1);
                sut.Add("def");
                Thread.Sleep(1);
                sut.Add("123");
                Thread.Sleep(1);
                sut.Add("c");
                var result = sut.ToArray();
                // Assert
                Expect(result)
                    .To.Equal(new[]
                    {
                        "c",
                        "123",
                        "def",
                        "a"
                    });
            }

            [Test]
            public void ShouldEnumerateInMostRecentOrder3()
            {
                // Arrange
                var sut = Create<string>(3);
                // Act
                sut.Add("c");
                Thread.Sleep(1);
                sut.Add("a");
                Thread.Sleep(1);
                sut.Add("def");
                Thread.Sleep(1);
                sut.Add("123");
                Thread.Sleep(1);
                sut.Add("c");
                var result = sut.ToArray();
                // Assert
                Expect(result)
                    .To.Equal(new[]
                    {
                        "c",
                        "123",
                        "def",
                    });
            }
        }

        private static MostRecentlyUsedList<T> Create<T>(
            int capacity
        )
        {
            return new MostRecentlyUsedList<T>(capacity);
        }
    }
}