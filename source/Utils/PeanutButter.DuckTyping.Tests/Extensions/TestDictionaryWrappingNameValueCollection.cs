using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestDictionaryWrappingNameValueCollection : AssertionHelper
    {
        [Test]
        public void ExplicitIEnumerator_GetEnumerator_ShouldReturnSameAs_GenericMethod()
        {
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            var reference = sut.GetEnumerator();
            var result = (sut as IEnumerable).GetEnumerator();
            // Assert
            Expect(reference, Is.InstanceOf<DictionaryWrappingNameValueCollectionEnumerator>());
            Expect(result, Is.InstanceOf<DictionaryWrappingNameValueCollectionEnumerator>());
            Expect(reference.Get<DictionaryWrappingNameValueCollection>("Data"), Is.EqualTo(sut));
            Expect(result.Get<DictionaryWrappingNameValueCollection>("Data"), Is.EqualTo(sut));
        }

        [Test]
        public void Add_GivenKeyValuePair_ShouldAddItToTheUnderlyingCollection()
        {
            // Arrange
            var collection = new NameValueCollection();
            var sut = Create(collection);
            var kvp = new KeyValuePair<string, object>(GetRandomString(), GetRandomString());
            // Pre-Assert
            // Act
            sut.Add(kvp);
            // Assert
            Expect(collection[kvp.Key], Is.EqualTo(kvp.Value));
        }

        [Test]
        public void Clear_ShouldClearTheUnderlyingNameValueCollection()
        {
            // Arrange
            var collection = new NameValueCollection();
            collection.Add(GetRandomString(), GetRandomString());
            var sut = Create(collection);
            // Pre-Assert
            // Act
            sut.Clear();
            // Assert
            Expect(collection, Is.Empty);
        }

        [Test]
        public void Contains_GivenKeyValuePair_OperatingOnEmptyCollection_ShouldReturnFalse()
        {
            // Arrange
            var arena = CreateArena();
            var kvp = GetRandom<KeyValuePair<string, object>>();
            // Pre-Assert
            Expect(arena.Collection, Is.Empty);
            // Act
            var result = arena.Sut.Contains(kvp);
            // Assert
            Expect(result, Is.False);
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenNoSuchKeyInCollection_ShouldReturnFalse()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            var notInCollection = GetRandom<KeyValuePair<string, object>>();
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Contains(notInCollection);
            // Assert
            Expect(result, Is.False);
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenHaveKeyInCollectionButMismatchesValue_ShouldReturnFalse()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            var notInCollection = new KeyValuePair<string, object>(inCollection.Key, GetRandomString());
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Contains(notInCollection);
            // Assert
            Expect(result, Is.False);
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenHaveKeyInCollectionAndMatchingValue_ShouldReturnTrue()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            var notInCollection = new KeyValuePair<string, object>(inCollection.Key, inCollection.Value);
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Contains(notInCollection);
            // Assert
            Expect(result, Is.True);
        }

        [Test]
        public void CopyTo_ShouldThrowNotImplementedException()
        {
            // DictionaryWrappingNameValueCollection is internal and I don't need this functionality (yet)
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            Expect(() => sut.CopyTo(new KeyValuePair<string, object>[5], 0),
                Throws.Exception.InstanceOf<NotImplementedException>());
            // Assert
        }

        [Test]
        public void Remove_GivenItemNotInCollection_ShouldDoNothing()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            var notInCollection = GetRandom<KeyValuePair<string, string>>(kvp => kvp.Key != inCollection.Key);
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            Expect(arena.Sut.Contains(
                notInCollection.AsKeyValuePairOfStringObject()
            ), Is.False);
            // Act
            var result = arena.Sut.Remove(notInCollection.AsKeyValuePairOfStringObject());
            // Assert
            Expect(result, Is.False);
            Expect(arena.Collection[inCollection.Key], Is.EqualTo(inCollection.Value));
        }

        [Test]
        public void Remove_GivenItemInCollection_ShouldRemoveIt()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Remove(inCollection.AsKeyValuePairOfStringObject());
            // Assert
            Expect(result, Is.True);
            Expect(arena.Collection, Is.Empty);
        }

        [Test]
        public void IsReadOnly_ShouldBeFalse()
        {
            // Arrange
            var sut = Create();
            // Pre-Assert
            // Act
            var result = sut.IsReadOnly;
            // Assert
            Expect(result, Is.False);
        }

        [Test]
        public void Remove_GivenKeyNotInCollection_ShouldReturnFalse()
        {
            // Arrange
            var arena = CreateArena();
            var key = GetRandomString(4);
            var value = GetRandomString(4);
            arena.Collection.Add(key, value);
            var search = GetAnother(key);
            // Pre-Assert
            // Act
            var result = arena.Sut.Remove(search);
            // Assert
            Expect(result, Is.False);
            Expect(arena.Collection.Count, Is.EqualTo(1));
            Expect(arena.Collection[key], Is.EqualTo(value));
        }

        [Test]
        public void Remove_GivenKeyInCollection_ShouldRemove()
        {
            // Arrange
            var arena = CreateArena();
            var key = GetRandomString(4);
            var value = GetRandomString(4);
            arena.Collection.Add(key, value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Remove(key);
            // Assert
            Expect(result, Is.True);
            Expect(arena.Collection.Count, Is.EqualTo(0));
        }

        [Test]
        public void Add_GivenKeyAndValue_ShouldAdd()
        {
            // Arrange
            var arena = CreateArena();
            var key = GetRandomString(4);
            var value = GetRandomString(4);
            // Pre-Assert
            Expect(arena.Collection, Is.Empty);
            // Act
            arena.Sut.Add(key, value);
            // Assert
            Expect(arena.Collection[key], Is.EqualTo(value));
        }

        [Test]
        public void Values_ShouldReturnAllValues()
        {
            // Arrange
            var arena = CreateArena();
            var data = GetRandomCollection<KeyValuePair<string, string>>(3);
            data.ForEach(d => arena.Collection.Add(d.Key, d.Value));
            var expected = data.Select(d => d.Value).ToArray();
            // Pre-Assert
            // Act
            var result = arena.Sut.Values;
            // Assert
            Expect(result, Is.EquivalentTo(expected));
        }

        private TestArena CreateArena()
        {
            return new TestArena();
        }

        private class TestArena
        {
            public NameValueCollection Collection { get; }
            public DictionaryWrappingNameValueCollection Sut { get; }
            public bool CaseInsensitive { get; }

            public TestArena(
            )
            {
                Collection = new NameValueCollection();
                Sut = new DictionaryWrappingNameValueCollection(Collection, CaseInsensitive);
            }
        }

        private DictionaryWrappingNameValueCollection Create(
            NameValueCollection collection = null,
            bool caseInsensitive = false
        )
        {
            return new DictionaryWrappingNameValueCollection(
                collection ?? new NameValueCollection(),
                caseInsensitive
            );
        }
    }
}