using System;
using System.Collections.Generic;
using System.Linq;
using NExpect;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using PeanutButter.TestUtils.Generic;
using static NExpect.Expectations;
// ReSharper disable CollectionNeverUpdated.Local

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestDefaultDictionary {
        [Test]
        public void Type_ShouldImplement_IDictionary() {
            // Arrange
            var sut = typeof(DefaultDictionary<string, bool>);

            // Pre-assert

            // Act
            sut.ShouldImplement<IDictionary<string, bool>>();

            // Assert
        }

        [Test]
        public void IndexedValue_GivenNoDefaultResolverAtConstruction_WhenAskedForMissingValue_ShouldReturnDefaultOfT() {
            // Arrange
            var sut = Create<string, bool>();
            var key = GetRandomString();

            // Pre-assert

            // Act
            var result = sut[key];

            // Assert
            Expect(result).To.Equal(default(bool));
        }

        [Test]
        public void IndexedValue_GivenDefaultResolverAtConstruction_ShouldReturnThatValue() {
            // Arrange
            var expected = GetRandomString();
            var key = GetRandomString();
            var sut = Create<string, string>(() => expected);

            // Pre-assert

            // Act
            var result = sut[key];

            // Assert
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void IndexedValue_WhenKeyExists_ShouldReturnThatValue() {
            // Arrange
            var expected = GetRandomString();
            var key = GetRandomString();
            var unexpected = GetAnother(expected);
            var sut = Create<string, string>(() => unexpected);

            // Pre-assert

            // Act
            sut[key] = expected;
            var result = sut[key];

            // Assert
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void ContainsKey_ShouldReturnTrue() {
            // Arrange
            var haveKey = GetRandomString();
            var missingKey = GetAnother(haveKey);
            var sut = Create<string, string>();
            sut[haveKey] = GetRandomString();

            // Pre-assert

            // Act
            var haveResult = sut.ContainsKey(haveKey);
            var missingResult = sut.ContainsKey(missingKey);

            // Assert
            Expect(haveResult).To.Be.True();
            Expect(missingResult).To.Be.True();
        }

        [Test]
        public void Enumeration_ShouldPassThrough() {
            // Arrange
            var k1 = GetRandomString();
            var k2 = GetAnother(k1);
            var v1 = GetRandomString();
            var v2 = GetAnother(v1);
            var sut = Create<string, string>();
            sut[k1] = v1;
            sut[k2] = v2;
            var collector = new List<KeyValuePair<string, string>>();

            // Pre-assert

            // Act
            foreach (var kvp in sut) {
                collector.Add(kvp);
            }

            // Assert
            Expect(collector.Count).To.Equal(2);
            Expect(collector).To.Contain.Exactly(1).Equal.To(new KeyValuePair<string, string>(k1, v1));
            Expect(collector).To.Contain.Exactly(1).Equal.To(new KeyValuePair<string, string>(k2, v2));
        }

        [Test]
        public void Add_GivenKeyValuePair_ShouldPassThrough() {
            // Arrange
            var sut = Create<string, string>();
            var kvp = GetRandom<KeyValuePair<string, string>>();


            // Pre-assert

            // Act
            sut.Add(kvp);

            // Assert
            Expect(sut[kvp.Key]).To.Equal(kvp.Value);
        }

        [Test]
        public void Clear_ShouldPassThrough() {
            // Arrange
            var sut = Create<string, string>();
            var item = GetRandom<KeyValuePair<string, string>>();
            sut.Add(item);

            // Pre-assert
            Expect(sut[item.Key]).To.Equal(item.Value);

            // Act
            sut.Clear();

            // Assert
            Expect(sut[item.Key]).To.Be.Null();
        }

        [Test]
        public void Contains_ShouldReturnTrue() {
            // Arrange
            var have = GetRandom<KeyValuePair<string, string>>();
            var missing = GetAnother(have);
            var sut = Create<string, string>();
            sut.Add(have);

            // Pre-assert

            // Act
            var haveResult = sut.Contains(have);
            var missingResult = sut.Contains(missing);

            // Assert
            Expect(haveResult).To.Be.True();
            Expect(missingResult).To.Be.True();
        }

        [Test]
        public void Remove_GivenKeyValuePair_ShouldPassThrough_SortOf() {
            // Arrange
            var have = GetRandom<KeyValuePair<string, string>>();
            var missing = GetAnother(have);
            var sut = Create<string, string>();
            sut.Add(have);

            // Pre-assert
            Expect(sut[have.Key]).Not.To.Be.Null();

            // Act
            var haveResult = sut.Remove(have);
            var missingResult = sut.Remove(missing);

            // Assert
            Expect(haveResult).To.Be.True();
            Expect(missingResult).To.Be.False();
            Expect(sut[have.Key]).To.Be.Null();
        }

        [Test]
        public void Remove_GivenKey_ShouldPassThrough() {
            // Arrange
            var have = GetRandom<KeyValuePair<string, string>>();
            var missing = GetAnother(have);
            var sut = Create<string, string>();
            sut.Add(have);
            Expect(sut[have.Key]).Not.To.Be.Null();

            // Pre-assert

            // Act
            var haveResult = sut.Remove(have.Key);
            var missingResult = sut.Remove(missing.Key);

            // Assert
            Expect(haveResult).To.Be.True();
            Expect(missingResult).To.Be.False();
            Expect(sut[have.Key]).To.Be.Null();
        }

        [Test]
        public void Add_GivenKeyAndValue_ShouldPassThrough() {
            // Arrange
            var sut = Create<string, string>();
            var kvp = GetRandom<KeyValuePair<string, string>>();

            // Pre-assert

            // Act
            sut.Add(kvp.Key, kvp.Value);

            // Assert
            Expect(sut[kvp.Key]).To.Equal(kvp.Value);
        }

        [Test]
        public void TryGetValue_WhenKeyIsKnown_ShouldReturnThatValue() {
            // Arrange
            var have = GetRandom<KeyValuePair<string, string>>();
            var sut = Create<string, string>();
            sut.Add(have);

            // Pre-assert

            // Act
            var result = sut.TryGetValue(have.Key, out var found);

            // Assert
            Expect(result).To.Be.True();
            Expect(found).To.Equal(have.Value);
        }

        [Test]
        public void TryGetValue_WhenKeyIsUnknown_ShouldReturnDefault() {
            // Arrange
            var expected = GetRandomString();
            var sut = Create<string, string>(() => expected);

            // Pre-assert

            // Act
            var result = sut.TryGetValue(GetRandomString(), out var found);

            // Assert
            Expect(result).To.Be.True();
            Expect(found).To.Equal(expected);
        }

        [Test]
        public void CopyTo_ShouldCopyKnownKeyValuePairs() {
            // Arrange
            var start = GetRandomInt(2, 4);
            var arraySize = GetRandomInt(10, 15);
            var items = GetRandomArray<KeyValuePair<string, string>>(2, 4);
            var target = new KeyValuePair<string, string>[arraySize];
            var sut = Create<string, string>();
            items.ForEach(sut.Add);

            // Pre-assert

            // Act
            sut.CopyTo(target, start);

            // Assert
            var defaultValue = default(KeyValuePair<string, string>);
            PyLike.Range(start).ForEach(i => Expect(target[i]).To.Equal(defaultValue));
            PyLike.Range(start + items.Length, arraySize).ForEach(
                                i => Expect(target[i]).To.Equal(defaultValue));
            items.ForEach(i => Expect(target).To.Contain.Exactly(1).Equal.To(i));
        }

        [Test]
        public void Count_ShouldReturnActualCount() {
            // Arrange
            var sut = Create<string, bool>();
            var items = GetRandomArray<KeyValuePair<string, bool>>();
            items.ForEach(sut.Add);

            // Pre-assert

            // Act
            var result = sut.Count;

            // Assert
            Expect(result).To.Equal(items.Length);
        }

        [Test]
        public void IsReadonly_ShouldReturnFalse() {
            // Arrange
            var sut = Create<int, bool>();

            // Pre-assert

            // Act
            var result = sut.IsReadOnly;

            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Keys_ShouldReturnKnownKeys() {
            // Arrange
            var items = GetRandomArray<KeyValuePair<string, string>>();
            var sut = Create<string, string>();
            items.ForEach(sut.Add);

            // Pre-assert

            // Act
            var result = sut.Keys;

            // Assert
            // TODO: swap out with To.Be.Equivalent.To() when the syntax is available in NExpect
            Assert.That(result, Is.EquivalentTo(items.Select(i => i.Key)));
        }

        [Test]
        public void Values_ShouldReturnKnownValues() {
            // Arrange
            var items = GetRandomArray<KeyValuePair<string, string>>();
            var sut = Create<string, string>();
            items.ForEach(sut.Add);

            // Pre-assert

            // Act
            var result = sut.Values;

            // Assert
            // TODO: swap out with To.Be.Equivalent.To() when the syntax is available in NExpect
            Assert.That(result, Is.EquivalentTo(items.Select(i => i.Value)));
        }

        [Test]
        public void IndexFetch_WithSmartResolver_ShouldReturnExpectedValue()
        {
            // Arrange
            var sut = new DefaultDictionary<string, string>(s => s + "moo");
            var index = GetRandomString();
            var expected = index + "moo";
            // Pre-Assert
            // Act
            var result = sut[index];
            // Assert
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void Instance_ShouldAppearToBeCaseInsensitive()
        {
            // Arrange
            var dict = new DefaultDictionary<string, object>();
            // Pre-Assert
            // Act
            var result = dict.GetPropertyValue("Comparer");
            // Assert
            Expect(result).To.Equal(StringComparer.OrdinalIgnoreCase);
        }

        private IDictionary<TKey, TValue> Create<TKey, TValue>(
            Func<TValue> defaultResolver = null
        ) {
            return defaultResolver == null
                ? new DefaultDictionary<TKey, TValue>()
                : new DefaultDictionary<TKey, TValue>(
                    defaultResolver
                );
        }
    }
}