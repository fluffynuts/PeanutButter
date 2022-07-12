using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NExpect;
using NUnit.Framework;
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
            public class ConcurrentDictionary
            {
                [TestFixture]
                public class GivenUnknownKey
                {
                    [Test]
                    public void ShouldAddNewItem()
                    {
                        // Arrange
                        var key = GetRandomString(1);
                        var value = GetRandomInt(0, 10);
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
                        var key = GetRandomString(1);
                        var value1 = GetRandomInt();
                        var value2 = GetAnother(value1);
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
            public class RegularDictionary
            {
                [TestFixture]
                public class GivenUnknownKey
                {
                    [Test]
                    public void ShouldAddNewItem()
                    {
                        // Arrange
                        var key = GetRandomString(1);
                        var value = GetRandomInt(0, 10);
                        var collection = new Dictionary<string, int>();

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
                        var key = GetRandomString(1);
                        var value1 = GetRandomInt();
                        var value2 = GetAnother(value1);
                        var collection = new Dictionary<string, int>();

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
                        [GetRandomString(10)] = GetRandomString(),
                        [GetRandomString(10)] = GetRandomString()
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
                        [GetRandomInt(1000, 2000).ToString()] = GetRandomInt().ToString(),
                        [GetRandomInt(2001, 3000).ToString()] = GetRandomInt().ToString()
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

        [TestFixture]
        public class MergeWith
        {
            [TestFixture]
            public class WhenMergePrecedenceIsLast
            {
                [Test]
                public void ShouldMergeSecondIntoFirstWithSecondOverriding()
                {
                    // Arrange
                    var d1 = new Dictionary<string, string>()
                    {
                        ["one"] = "1",
                        ["last"] = "---"
                    };
                    var d2 = new Dictionary<string, string>()
                    {
                        ["two"] = "2",
                        ["last"] = "end"
                    };
                    var expected = new Dictionary<string, string>()
                    {
                        ["one"] = "1",
                        ["two"] = "2",
                        ["last"] = "end",
                    };

                    // Act
                    var result = d1
                        .MergedWith(d2, MergePrecedence.Last);
                    // Assert
                    // key order isn't guaranteed by Dictionary<TKey, TValue>
                    // but it turns out that in a small collection, they will
                    // be in first-seen order; however, this test is sufficient
                    Expect(result)
                        .To.Be.Equivalent.To(expected);
                }
            }

            [TestFixture]
            public class DefaultPrecedence
            {
                [Test]
                public void ShouldMergeSecondIntoFirstWithSecondOverriding()
                {
                    // Arrange
                    var d1 = new Dictionary<string, string>()
                    {
                        ["one"] = "1",
                        ["last"] = "---"
                    };
                    var d2 = new Dictionary<string, string>()
                    {
                        ["two"] = "2",
                        ["last"] = "end"
                    };
                    var expected = new Dictionary<string, string>()
                    {
                        ["one"] = "1",
                        ["two"] = "2",
                        ["last"] = "end",
                    };

                    // Act
                    var result = d1
                        .MergedWith(d2);
                    // Assert
                    // key order isn't guaranteed by Dictionary<TKey, TValue>
                    // but it turns out that in a small collection, they will
                    // be in first-seen order; however, this test is sufficient
                    Expect(result)
                        .To.Be.Equivalent.To(expected);
                }
            }

            [TestFixture]
            public class WhenMergePrecedenceIsFirst
            {
                [Test]
                public void ShouldMergeSecondIntoFirstWithFirstOverriding()
                {
                    // Arrange
                    var d1 = new Dictionary<string, string>()
                    {
                        ["one"] = "1",
                        ["last"] = "---"
                    };
                    var d2 = new Dictionary<string, string>()
                    {
                        ["two"] = "2",
                        ["last"] = "end"
                    };
                    var expected = new Dictionary<string, string>()
                    {
                        ["one"] = "1",
                        ["two"] = "2",
                        ["last"] = "---",
                    };

                    // Act
                    var result = d1
                        .MergedWith(d2, MergePrecedence.First);
                    // Assert
                    // key order isn't guaranteed by Dictionary<TKey, TValue>
                    // but it turns out that in a small collection, they will
                    // be in first-seen order; however, this test is sufficient
                    Expect(result)
                        .To.Be.Equivalent.To(expected);
                }
            }
        }
    }
}