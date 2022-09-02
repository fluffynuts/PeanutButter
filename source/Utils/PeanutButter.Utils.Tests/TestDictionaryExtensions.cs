using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                        .MergedWith(d2, MergeWithPrecedence.PreferLastSeen);
                    // Assert
                    // key order isn't guaranteed by Dictionary<TKey, TValue>
                    // but it turns out that in a small collection, they will
                    // be in first-seen order; however, this test is sufficient
                    var noMutate = "should not mutate either of the original dictionaries";
                    Expect(d2)
                        .Not.To.Contain.Key("one", noMutate);
                    Expect(d1)
                        .Not.To.Contain.Key("two", noMutate);
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
                        .MergedWith(d2, MergeWithPrecedence.PreferFirstSeen);
                    // Assert
                    // key order isn't guaranteed by Dictionary<TKey, TValue>
                    // but it turns out that in a small collection, they will
                    // be in first-seen order; however, this test is sufficient
                    Expect(result)
                        .To.Be.Equivalent.To(expected);
                }
            }
        }

        [TestFixture]
        public class MergeInto
        {
            [Test]
            public void ShouldNotThrowIfNewDataIsNull()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var newData = null as IDictionary<string, string>;
                var target = new Dictionary<string, string>()
                {
                    [key] = value
                };
                // Act
                Expect(() => newData.MergeInto(target))
                    .Not.To.Throw();
                // Assert
            }

            [TestFixture]
            public class DefaultBehavior
            {
                [Test]
                public void ShouldNotOverwriteTargetData()
                {
                    // Arrange
                    var newKey = GetRandomString(10);
                    var sharedKey = GetRandomString(10);
                    var existingKey = GetRandomString(10);
                    var newValue = GetRandomString(10);
                    var expected = GetRandomString(10);
                    var existingValue = GetRandomString(10);

                    var newData = new Dictionary<string, string>()
                    {
                        [newKey] = newValue,
                        [sharedKey] = GetRandomString(10)
                    };
                    var target = new Dictionary<string, string>()
                    {
                        [existingKey] = existingValue,
                        [sharedKey] = expected
                    };
                    // Act
                    newData.MergeInto(target);
                    // Assert
                    Expect(target)
                        .To.Contain.Only(3).Items();
                    Expect(target[newKey])
                        .To.Equal(newValue);
                    Expect(target[existingKey])
                        .To.Equal(existingValue);
                    Expect(target[sharedKey])
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class WhenExplicitlyChoosingPreferTargetData
            {
                [Test]
                public void ShouldNotOverwriteTargetData()
                {
                    // Arrange
                    var newKey = GetRandomString(10);
                    var sharedKey = GetRandomString(10);
                    var existingKey = GetRandomString(10);
                    var newValue = GetRandomString(10);
                    var expected = GetRandomString(10);
                    var existingValue = GetRandomString(10);

                    var newData = new Dictionary<string, string>()
                    {
                        [newKey] = newValue,
                        [sharedKey] = GetRandomString(10)
                    };
                    var target = new Dictionary<string, string>()
                    {
                        [existingKey] = existingValue,
                        [sharedKey] = expected
                    };
                    // Act
                    newData.MergeInto(target, MergeIntoPrecedence.PreferTargetData);
                    // Assert
                    Expect(target)
                        .To.Contain.Only(3).Items();
                    Expect(target[newKey])
                        .To.Equal(newValue);
                    Expect(target[existingKey])
                        .To.Equal(existingValue);
                    Expect(target[sharedKey])
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class WhenExplicitlyChoosingPreferNewData
            {
                [Test]
                public void ShouldOverwriteTargetData()
                {
                    // Arrange
                    var newKey = GetRandomString(10);
                    var sharedKey = GetRandomString(10);
                    var existingKey = GetRandomString(10);
                    var newValue = GetRandomString(10);
                    var expected = GetRandomString(10);
                    var existingValue = GetRandomString(10);

                    var newData = new Dictionary<string, string>()
                    {
                        [newKey] = newValue,
                        [sharedKey] = expected
                    };
                    var target = new Dictionary<string, string>()
                    {
                        [existingKey] = existingValue,
                        [sharedKey] = GetRandomString(10)
                    };
                    // Act
                    newData.MergeInto(target, MergeIntoPrecedence.PreferNewData);
                    // Assert
                    Expect(target)
                        .To.Contain.Only(3).Items();
                    Expect(target[newKey])
                        .To.Equal(newValue);
                    Expect(target[existingKey])
                        .To.Equal(existingValue);
                    Expect(target[sharedKey])
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class FluentMultipleMergeInto
            {
                [Test]
                public void ShouldMergeAll()
                {
                    // Arrange
                    var target = new Dictionary<string, string>()
                    {
                        ["one"] = "1"
                    };
                    var expected = new Dictionary<string, string>()
                    {
                        ["one"] = "1",
                        ["two"] = "2",
                        ["three"] = "3"
                    };
                    // Act
                    var result = new Dictionary<string, string>()
                    {
                        ["two"] = "2"
                    }.MergeInto(new Dictionary<string, string>()
                    {
                        ["three"] = "3"
                    }).MergeInto(target);
                    // Assert
                    Expect(result)
                        .To.Be(target);
                    Expect(result)
                        .To.Be.Equivalent.To(expected);
                }
            }
        }

        [TestFixture]
        public class ConvertingBetweenNameValueCollectionAndDictOfStringString
        {
            [Test]
            public void ShouldBeAbleToConvertNameValueCollectionToDict()
            {
                // Arrange
                var key1 = GetRandomString(10);
                var key2 = GetRandomString(10);
                var value1 = GetRandomString();
                var value2 = GetRandomString();
                var input = new NameValueCollection()
                {
                    { key1, value1 },
                    { key2, value2 }
                };

                // Act
                var result = input.ToDictionary();
                // Assert
                Expect(result)
                    .To.Contain.Only(2)
                    .Items();
                Expect(result)
                    .To.Contain.Key(key1)
                    .With.Value(value1);
                Expect(result)
                    .To.Contain.Key(key2)
                    .With.Value(value2);
            }

            [Test]
            public void ShouldBeAbleToSetTheKeyComparer()
            {
                // Arrange
                var key = "AAA";
                var value = "BBB";
                var input = new NameValueCollection()
                {
                    { key, value }
                };

                // Act
                var result = input.ToDictionary(StringComparer.OrdinalIgnoreCase);
                // Assert
                Expect(result["aaa"])
                    .To.Equal(value);
            }

            [Test]
            public void ShouldBeAbleToConvertDictionaryOfStringStringToNameValueCollection()
            {
                // Arrange
                var key1 = GetRandomString(10);
                var key2 = GetRandomString(10);
                var value1 = GetRandomString();
                var value2 = GetRandomString();
                var input = new Dictionary<string, string>()
                {
                    { key1, value1 },
                    { key2, value2 }
                };

                // Act
                var result = input.ToNameValueCollection();
                // Assert
                Expect(result)
                    .To.Contain.Only(2)
                    .Items();
                Expect(result)
                    .To.Contain.Key(key1)
                    .With.Value(value1);
                Expect(result)
                    .To.Contain.Key(key2)
                    .With.Value(value2);
            }
        }

        [TestFixture]
        public class AsDictionary
        {
            [Test]
            public void ShouldProduceOriginalRefForDictionary()
            {
                // Arrange
                var dict = GetRandom<Dictionary<string, string>>();
                Expect(dict as IDictionary<string, string>)
                    .Not.To.Be.Null();

                // Act
                var result = dict.AsDictionary<string, string>();
                // Assert
                Expect(result)
                    .To.Be(dict);
            }

            [Test]
            public void ShouldProduceOriginalRefForIDictionary()
            {
                // Arrange
                var dict = GetRandom<Dictionary<string, string>>();

                // Act
                var result = (dict as IDictionary<string, string>).AsDictionary<string, string>();
                // Assert
                Expect(result)
                    .To.Be(dict);
            }

            [Test]
            public void ShouldProduceOriginalRefForDictionaryUntypedFromStringObject()
            {
                // Arrange
                var dict = GetRandom<Dictionary<string, object>>();

                // Act
                var result = dict.AsDictionary();
                // Assert
                Expect(result)
                    .To.Be(dict);
            }

            [Test]
            public void ShouldProduceOriginalRefForIDictionaryUntypedFromStringObject()
            {
                // Arrange
                var dict = GetRandom<Dictionary<string, object>>();

                // Act
                var result = (dict as IDictionary<string, object>).AsDictionary();
                // Assert
                Expect(result)
                    .To.Be(dict);
            }
        }
    }
}