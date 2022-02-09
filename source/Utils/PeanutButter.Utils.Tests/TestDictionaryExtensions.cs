using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestDictionaryExtensions
    {
        [TestFixture]
        public class FindOrAdd
        {
            [TestFixture]
            public class GivenUnknownKey
            {
                [Test]
                public void ShouldAddNewItem()
                {
                    // Arrange
                    var key = RandomValueGen.GetRandomString(1);
                    var value = RandomValueGen.GetRandomInt(0, 10);
                    var collection = new ConcurrentDictionary<string, int>();

                    // Act
                    var result = collection.FindOrAdd(key, () => value);
                    // Assert
                    Expect(result)
                        .To.Equal(value);
                }
            }

            [TestFixture]
            public class GivenKnownKey
            {
                [Test]
                public void ShouldReturnExistingItemForKey()
                {
                    // Arrange
                    var key = RandomValueGen.GetRandomString(1);
                    var value1 = RandomValueGen.GetRandomInt();
                    var value2 = RandomValueGen.GetAnother(value1);
                    var collection = new ConcurrentDictionary<string, int>();

                    // Act
                    var result1 = collection.FindOrAdd(key, () => value1);
                    var result2 = collection.FindOrAdd(key, () => value2);
                    // Assert
                    Expect(result1)
                        .To.Equal(value1)
                        .And.To.Equal(result2);
                }
            }
        }

        [TestFixture]
        public class ToDictionary
        {
            [TestFixture]
            public class DefaultInvocation
            {
                [Test]
                public void ShouldProduceTypedDictionary()
                {
                    // Arrange
                    var dict = new Dictionary<string, string>()
                    {
                        [RandomValueGen.GetRandomString(10)] = RandomValueGen.GetRandomString(),
                        [RandomValueGen.GetRandomString(10)] = RandomValueGen.GetRandomString()
                    };
                    var cast = dict as IDictionary;
                    // Act
                    var result = cast.ToDictionary<string, string>();
                    // Assert
                    Expect(result)
                        .To.Equal(dict);
                }
            }

            [TestFixture]
            public class InvokedWithTransforms
            {
                [Test]
                public void ShouldProduceTypedDictionary()
                {
                    // Arrange
                    var dict = new Dictionary<string, string>()
                    {
                        [RandomValueGen.GetRandomInt(1000, 2000).ToString()] = RandomValueGen.GetRandomInt().ToString(),
                        [RandomValueGen.GetRandomInt(2001, 3000).ToString()] = RandomValueGen.GetRandomInt().ToString()
                    };
                    var cast = dict as IDictionary;
                    // Act
                    var result = cast.ToDictionary(
                        kvp => int.Parse(kvp.Key as string),
                        kvp => int.Parse(kvp.Value as string)
                    );
                    // Assert
                    var keys = dict.Keys.Select(int.Parse)
                        .ToArray();
                    foreach (var key in keys)
                    {
                        Expect(result[key])
                            .To.Equal(int.Parse(dict[key.ToString()]));
                    }
                }
            }
        }

        [TestFixture]
        public class Clone
        {
            [Test]
            public void ShouldProduceANewDictionaryWithTheSameElements()
            {
                // Arrange
                var input = new Dictionary<string, int>()
                {
                    [GetRandomString(10)] = GetRandomInt(),
                    [GetRandomString(10)] = GetRandomInt(),
                    [GetRandomString(10)] = GetRandomInt()
                };
                // Act
                var result = input.Clone();
                // Assert
                Expect(result)
                    .Not.To.Be(input);
                Expect(result)
                    .To.Deep.Equal(input);
            }
        }
    }
}