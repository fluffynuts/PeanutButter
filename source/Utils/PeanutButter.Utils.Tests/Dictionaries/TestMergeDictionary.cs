using System;
using System.Collections.Generic;
using System.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils.Dictionaries;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;

// ReSharper disable ExpressionIsAlwaysNull

// ReSharper disable PossibleNullReferenceException
// ReSharper disable TryCastAlwaysSucceeds

namespace PeanutButter.Utils.Tests.Dictionaries
{
    [TestFixture]
    public class TestMergeDictionary
    {
        [Test]
        public void Type_ShouldImplement_IDictionaryOfSameGeneric()
        {
            // Arrange
            var sut1 = typeof(MergeDictionary<string, bool>);
            var sut2 = typeof(MergeDictionary<string, int>);

            // Pre-assert

            // Act
            sut1.ShouldImplement<IDictionary<string, bool>>();
            sut2.ShouldImplement<IDictionary<string, int>>();

            // Assert
        }

        [Test]
        public void GivenTwoDictionariesWhichDontIntersect_ShouldReturnValueFromEach()
        {
            // Arrange
            var key1 = GetRandomString();
            var value1 = GetRandomString();
            var key2 = GetAnother(key1);
            var value2 = GetAnother(value1);
            var dict1 = new Dictionary<string, string>() {[key1] = value1};
            var dict2 = new Dictionary<string, string>() {[key2] = value2};
            var sut = Create(dict1, dict2);

            // Pre-assert

            // Act
            var result1 = sut[key1];
            var result2 = sut[key2];

            // Assert
            Expect(result1).To.Equal(value1);
            Expect(result2).To.Equal(value2);
        }

        [Test]
        public void GivenTwoDictionarisWhicCollide_ShouldReturnValueFromFirst()
        {
            // Arrange
            var key = GetRandomString();
            var value1 = GetRandomString();
            var value2 = GetAnother(value1);
            var dict1 = new Dictionary<string, string>() {[key] = value1};
            var dict2 = new Dictionary<string, string>() {[key] = value2};
            var sut = Create(dict1, dict2);

            // Pre-assert

            // Act
            var result = sut[key];

            // Assert
            Expect(result).To.Equal(value1);
        }

        [Test]
        public void ReadOnly_ShouldBeTrue()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());

            // Pre-assert

            // Act
            Expect(sut.IsReadOnly).To.Be.True();

            // Assert
        }

        [Test]
        public void ContainsKey_WhenOneLayerContainsTheKey_ShouldReturnTrue()
        {
            // Arrange
            var key = GetRandomString();
            var sut = Create(
                new Dictionary<string, string>(),
                new Dictionary<string, string>() {[key] = GetRandomString()}
            );

            // Pre-assert

            // Act
            var result = sut.ContainsKey(key);

            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void Count_ShouldReturnCombinedCountOfUniqueKeys()
        {
            // Arrange
            var key1 = GetRandomString();
            var key2 = GetRandomString();
            var sut = Create(
                new Dictionary<string, string>()
                {
                    [key1] = GetRandomString(),
                    [key2] = GetRandomString()
                },
                new Dictionary<string, string>()
                {
                    [key1] = GetRandomString()
                });

            // Pre-assert

            // Act
            var result = sut.Count;

            // Assert
            Expect(result).To.Equal(2);
        }

        [Test]
        public void GetEnumerator_ShouldEnumerateOverDistinctKeyValuePairsWithPriority()
        {
            // Arrange
            var key1 = GetRandomString();
            var key2 = GetAnother(key1);
            var key3 = GetAnother<string>(new[] {key1, key2});
            var expected1 = GetRandomString();
            var expected2 = GetAnother(expected1);
            var expected3 = GetAnother<string>(new[] {expected1, expected2});
            var unexpected = GetAnother<string>(new[] {expected1, expected2, expected3});
            var sut = Create(
                new Dictionary<string, string>()
                {
                    [key1] = expected1,
                    [key2] = expected2
                },
                new Dictionary<string, string>()
                {
                    [key1] = unexpected,
                    [key3] = expected3
                });
            var collector = new List<KeyValuePair<string, string>>();
            // Pre-assert

            // Act
            foreach (var kvp in sut)
            {
                collector.Add(kvp);
            }

            // Assert
            Expect(collector.Count).To.Equal(3);
            Expect(collector.Any(kvp => kvp.Key == key1 && kvp.Value == expected1)).To.Be.True();
            Expect(collector.Any(kvp => kvp.Key == key2 && kvp.Value == expected2)).To.Be.True();
            Expect(collector.Any(kvp => kvp.Key == key3 && kvp.Value == expected3)).To.Be.True();
        }

        [Test]
        public void GetEnumerator_ShouldEnumerateOverDistinctKeyValuePairsWithPrioritySecondTime()
        {
            // Arrange
            var key1 = GetRandomString();
            var key2 = GetAnother(key1);
            var key3 = GetAnother<string>(new[] {key1, key2});
            var expected1 = GetRandomString();
            var expected2 = GetAnother(expected1);
            var expected3 = GetAnother<string>(new[] {expected1, expected2});
            var unexpected = GetAnother<string>(new[] {expected1, expected2, expected3});
            var sut = Create(
                new Dictionary<string, string>()
                {
                    [key1] = expected1,
                    [key2] = expected2
                },
                new Dictionary<string, string>()
                {
                    [key1] = unexpected,
                    [key3] = expected3
                });
            var collector = new List<KeyValuePair<string, string>>();
            // Pre-assert

            // Act
            using (var enumerator = sut.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    /* do nothing -- just getting the enumerator to expire */
                }

                enumerator.Reset();
                while (enumerator.MoveNext())
                {
                    collector.Add(enumerator.Current);
                }
            }


            // Assert
            Expect(collector.Count).To.Equal(3);
            Expect(collector.Any(kvp => kvp.Key == key1 && kvp.Value == expected1)).To.Be.True();
            Expect(collector.Any(kvp => kvp.Key == key2 && kvp.Value == expected2)).To.Be.True();
            Expect(collector.Any(kvp => kvp.Key == key3 && kvp.Value == expected3)).To.Be.True();
        }

        [Test]
        public void Add_GivenKeyValuePair_ShouldThrow_InvalidOperationException()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());

            // Pre-assert

            // Act
            Expect(
                    () => sut.Add(new KeyValuePair<string, string>(GetRandomString(), GetRandomString()))
                ).To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");

            // Assert
        }

        [Test]
        public void Add_GivenKeyAndValue_ShouldThrow_InvalidOperationException()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());

            // Pre-assert

            // Act
            Expect(() => sut.Add(GetRandomString(), GetRandomString()))
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");

            // Assert
        }

        [Test]
        public void Clear_ShouldThrow_InvalidOperationException()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());

            // Pre-assert

            // Act
            Expect(() => sut.Clear())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");
            // Assert
        }

        [Test]
        public void Contains_WhenOneDictionaryAndItContainsAMatchingPair_ShouldReturnTrue()
        {
            // Arrange
            var kvp = new KeyValuePair<string, string>(GetRandomString(), GetRandomString());
            var inner = new Dictionary<string, string>()
            {
                [kvp.Key] = kvp.Value
            };
            var sut = Create(inner);

            // Pre-assert

            // Act
            var result = sut.Contains(kvp);

            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void Contains_WhenOneDictionaryAndItDoesNotContainAMatchingPair_ShouldReturnFalse()
        {
            // Arrange
            var kvp = new KeyValuePair<string, string>(GetRandomString(), GetRandomString());
            var inner = new Dictionary<string, string>();
            var sut = Create(inner);

            // Pre-assert

            // Act
            var result = sut.Contains(kvp);

            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Contains_WhenTwoDictionariesAndSecondContainsAMatchingPair_ShouldReturnTrue()
        {
            // Arrange
            var kvp = new KeyValuePair<string, string>(GetRandomString(), GetRandomString());
            var inner1 = new Dictionary<string, string>();
            var inner2 = new Dictionary<string, string>
            {
                [kvp.Key] = kvp.Value
            };
            var sut = Create(inner1, inner2);

            // Pre-assert

            // Act
            var result = sut.Contains(kvp);

            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void Contains_WhenTwoDictionariesAndNeitherContainAMatchingPair_ShouldReturnFalse()
        {
            // Arrange
            var kvp = new KeyValuePair<string, string>(GetRandomString(), GetRandomString());
            var inner1 = new Dictionary<string, string>();
            var inner2 = new Dictionary<string, string>();
            var sut = Create(inner1, inner2);

            // Pre-assert

            // Act
            var result = sut.Contains(kvp);

            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Remove_GivenKeyValuePair_ShouldThrow_InvalidOperationException()
        {
            // Arrange
            var sut = Create(new Dictionary<string, int>());

            // Pre-assert

            // Act
            Expect(() => sut.Remove(new KeyValuePair<string, int>("moo", 1)))
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");

            // Assert
        }

        [Test]
        public void Remove_GivenKey_ShouldThrow_InvalidOperationException()
        {
            // Arrange
            var sut = Create(new Dictionary<string, int>());

            // Pre-assert

            // Act
            Expect(() => sut.Remove("moo"))
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");

            // Assert
        }

        [Test]
        public void CopyTo_ShouldCopy_AllUniqueByKey_ToTheGivenIndexOfTheArray()
        {
            // Arrange
            var array = new KeyValuePair<string, string>[10];
            const int index = 1;
            var kvp1 = new KeyValuePair<string, string>("key1", "value1");
            var kvp2 = new KeyValuePair<string, string>("key2", "value2");
            var kvp3 = new KeyValuePair<string, string>("key3", "value3");
            var inner1 = new Dictionary<string, string>
            {
                [kvp1.Key] = kvp1.Value,
                [kvp2.Key] = kvp2.Value
            };

            var inner2 = new Dictionary<string, string>()
            {
                ["key1"] = "moocakes",
                [kvp3.Key] = kvp3.Value
            };

            var sut = Create(inner1, inner2);

            // Pre-assert

            // Act
            sut.CopyTo(array, index);

            // Assert
            var defaultVal = default(KeyValuePair<string, string>);
            Expect(array[0]).To.Equal(defaultVal);
            Expect(array.Any(e => e.Equals(kvp1))).To.Be.True();
            Expect(array.Any(e => e.Equals(kvp2))).To.Be.True();
            Expect(array.Any(e => e.Equals(kvp3))).To.Be.True();
            PyLike.Range(4, 10).ForEach(i => Expect(array[i]).To.Equal(defaultVal));
        }

        [Test]
        public void Keys_ShouldReturnAllDistinctKeys()
        {
            // Arrange
            var k1 = GetRandomString();
            var k2 = GetAnother(k1);
            var sut = Create(
                new Dictionary<string, string>()
                {
                    [k1] = GetRandomString(),
                    [k2] = GetRandomString()
                },
                new Dictionary<string, string>()
                {
                    [k1] = GetRandomString()
                });

            // Pre-assert

            // Act
            var result = sut.Keys;

            // Assert
            Expect(result.Count).To.Equal(2);
            Expect(result).To.Contain.Exactly(1).Equal.To(k1);
            Expect(result).To.Contain.Exactly(1).Equal.To(k2);
        }

        [Test]
        public void Values_ShouldReturnAllPriorityValues()
        {
            // Arrange
            var k1 = GetRandomString();
            var k2 = GetAnother(k1);
            var v1 = GetRandomString();
            var v2 = GetAnother(v1);
            var v3 = GetAnother<string>(new[] {v1, v2});
            var sut = Create(
                new Dictionary<string, string>()
                {
                    [k1] = v1,
                    [k2] = v2
                },
                new Dictionary<string, string>()
                {
                    [k2] = v3
                });

            // Pre-assert

            // Act
            var result = sut.Values;

            // Assert
            Expect(result.Count).To.Equal(2);
            Expect(result).To.Contain.Exactly(1).Equal.To(v1);
            Expect(result).To.Contain.Exactly(1).Equal.To(v2);
        }

        [Test]
        public void Comparer_WhenKeyTypeIsString_ShouldReturnLeastRestrictive()
        {
            // Arrange
            var sensitive = new Dictionary<string, string>(StringComparer.Ordinal);
            var insensitive = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sut = Create(sensitive, insensitive) as MergeDictionary<string, string>;
            // Pre-Assert
            // Act
            var result = sut.Comparer;
            // Assert
            Expect(result as object).To.Equal(StringComparer.OrdinalIgnoreCase);
        }

        [Test]
        public void Comparer_WhenKeyTypeIsString_ShouldReturnLeastRestrictive2()
        {
            // Arrange
            var sensitive = new Dictionary<string, string>(StringComparer.Ordinal);
            var insensitive = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sut = Create(insensitive, sensitive) as MergeDictionary<string, string>;
            // Pre-Assert
            // Act
            var result = sut.Comparer;
            // Assert
            Expect(result as object).To.Equal(StringComparer.OrdinalIgnoreCase);
        }

        [Test]
        public void Comparer_WhenKeyTypeIsNotString_ShouldReturnFirstFound()
        {
            // Arrange
            var c1 = new SomeComparer();
            var c2 = new SomeComparer();
            var d1 = new Dictionary<int, string>(c1);
            var d2 = new Dictionary<int, string>(c2);
            var sut = Create(d1, d2) as MergeDictionary<int, string>;
            // Pre-Assert
            // Act
            var result = sut.Comparer;
            // Assert
            Expect(result).To.Equal(c1);
        }

        [Test]
        public void ShouldIgnoreUnderlyingNullDictionaries()
        {
            // Arrange
            var dict = null as IDictionary<string, string>;
            var other = new DefaultDictionary<string, string>(k => "moo");
            var sut = Create(dict, other);
            // Pre-assert
            // Act
            var result = sut["cow says"];
            // Assert
            Expect(result).To.Equal("moo");
        }

        [Test]
        public void ShouldThrowIfNoLayers()
        {
            // Arrange
            // Pre-assert
            // Act
            Expect(() => new MergeDictionary<string, string>())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("No non-null layers provided");
            // Assert
        }

        [Test]
        public void Clear_ShouldThrow()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());
            // Pre-assert
            // Act
            Expect(() => sut.Clear())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");
            // Assert
        }

        [Test]
        public void Add_ShouldThrow()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());
            // Pre-assert
            // Act
            Expect(() => sut.Add(new KeyValuePair<string, string>(GetRandomString(1), GetRandomString())))
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");
            // Assert
        }

        [Test]
        public void WhenKeyNotFound_ShouldThrow()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());
            // Pre-assert
            // Act
            Expect(() => sut[GetRandomString(10)])
                .To.Throw<KeyNotFoundException>();
            // Assert
        }

        [Test]
        public void AttemptToSet_ShouldThrow()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());
            // Pre-assert
            // Act
            Expect(() => sut[GetRandomString(1)] = GetRandomString())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("read-only");
            // Assert
        }

        [Test]
        public void TryGetValue_WhenNoMatchingKey_ShouldReturnFalse()
        {
            // Arrange
            var sut = Create(new Dictionary<string, string>());
            // Pre-assert
            // Act
            var result = sut.TryGetValue(GetRandomString(10), out var value);
            // Assert
            Expect(result).To.Be.False();
            Expect(value).To.Be.Null();
        }

        [Test]
        public void Enumerating_ShouldEnumerateAllUnderlyingDictionaries()
        {
            // Arrange
            var k1 = GetRandomString(10);
            var v1 = GetRandomString(10);
            var k2 = GetRandomString(10);
            var v2 = GetRandomString(10);
            var first = new Dictionary<string, string>()
            {
                [k1] = v1
            };
            var second = new Dictionary<string, string>()
            {
                [k2] = v2
            };
            var sut = Create(first, second);
            // Pre-assert
            // Act
            var result = sut.ToArray();
            // Assert
            Expect(result).To.Contain.Exactly(1).Deep.Equal.To(
                new KeyValuePair<string, string>(k1, v1));
            Expect(result).To.Contain.Exactly(1).Deep.Equal.To(
                new KeyValuePair<string, string>(k2, v2));
        }

        public class SomeComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                throw new NotImplementedException();
            }

            public int GetHashCode(int obj)
            {
                throw new NotImplementedException();
            }
        }


        private IDictionary<TKey, TValue> Create<TKey, TValue>(
            params IDictionary<TKey, TValue>[] dictionaries
        )
        {
            return new MergeDictionary<TKey, TValue>(dictionaries);
        }
    }
}