using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestDictionaryWrappingNameValueCollection: AssertionHelper
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

        private TestArena CreateArena() {
            return new TestArena();
        }

        private class TestArena {
            public NameValueCollection Collection { get; }
            public DictionaryWrappingNameValueCollection Sut { get; }
            public bool CaseInsensitive { get; }
            public TestArena(

            ) {
                Collection = new NameValueCollection();
                Sut = new DictionaryWrappingNameValueCollection(Collection, CaseInsensitive);
            }
        }

        private DictionaryWrappingNameValueCollection Create(
            NameValueCollection collection = null,
            bool caseInsensitive = false
        ) {
            return new DictionaryWrappingNameValueCollection(
                collection ?? new NameValueCollection(),
                caseInsensitive
            );
        }
    }
}
