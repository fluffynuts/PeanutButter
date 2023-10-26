using System.Collections.Concurrent;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;
using NExpect;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestConcurrentCollectionExtensions
{
    [TestFixture]
    public class Clear
    {
        [Test]
        public void ShouldBeAbleToClearConcurrentQueue()
        {
            // Arrange
            var queue = new ConcurrentQueue<string>();
            var itemCount = GetRandomInt(5, 15);
            for (var i = 0; i < itemCount; i++)
            {
                queue.Enqueue(GetRandomString());
            }

            // Act
            queue.Clear();
            // Assert
            Expect(queue)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldBeAbleToClearAConcurrentBag()
        {
            // Arrange
            var bag = new ConcurrentBag<string>();
            var itemCount = GetRandomInt(5, 15);
            for (var i = 0; i < itemCount; i++)
            {
                bag.Add(GetRandomString());
            }

            // Act
            bag.Clear();
            // Assert
            Expect(bag)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldBeAbleToClearAConcurrentStack()
        {
            // this is natively supported - just including the test for completeness
            // Arrange
            var bag = new ConcurrentStack<string>();
            var itemCount = GetRandomInt(5, 15);
            for (var i = 0; i < itemCount; i++)
            {
                bag.Push(GetRandomString());
            }

            // Act
            bag.Clear();
            // Assert
            Expect(bag)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldBeAbleToClearAConcurrentDictionary()
        {
            // this is natively supported - just including the test for completeness
            // Arrange
            var dict = new ConcurrentDictionary<string, string>();
            var itemCount = GetRandomInt(5, 15);
            for (var i = 0; i < itemCount; i++)
            {
                dict.TryAdd(GetRandomString(), GetRandomString());
            }

            // Act
            dict.Clear();
            // Assert
            Expect(dict)
                .To.Be.Empty();
        }
    }
}