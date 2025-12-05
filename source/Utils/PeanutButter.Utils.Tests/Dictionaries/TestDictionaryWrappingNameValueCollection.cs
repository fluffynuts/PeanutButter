using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using PeanutButter.Utils.Dictionaries;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable TryCastAlwaysSucceeds

namespace PeanutButter.Utils.Tests.Dictionaries
{
    [TestFixture]
    public class TestDictionaryWrappingNameValueCollection
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
            Expect(reference)
                .To.Be.An.Instance.Of<DictionaryWrappingNameValueCollectionEnumerator<string>>();
            Expect(result)
                .To.Be.An.Instance.Of<DictionaryWrappingNameValueCollectionEnumerator<string>>();
            Expect(reference.Get<DictionaryWrappingNameValueCollection>("Data"))
                .To.Equal(sut);
            Expect(result.Get<DictionaryWrappingNameValueCollection>("Data"))
                .To.Equal(sut);
        }

        [Test]
        public void Add_GivenKeyValuePair_ShouldAddItToTheUnderlyingCollection()
        {
            // Arrange
            var collection = new NameValueCollection();
            var sut = Create(collection);
            var kvp = new KeyValuePair<string, string>(GetRandomString(), GetRandomString());
            // Pre-Assert
            // Act
            sut.Add(kvp);
            // Assert
            Expect(collection).To.Contain.Key(kvp.Key).With.Value(kvp.Value as string);
        }

        [Test]
        public void Clear_ShouldClearTheUnderlyingNameValueCollection()
        {
            // Arrange
            var collection = new NameValueCollection
            {
                {GetRandomString(), GetRandomString()}
            };
            var sut = Create(collection);
            // Pre-Assert
            // Act
            sut.Clear();
            // Assert
            Expect(collection).To.Be.Empty();
        }

        [Test]
        public void Contains_GivenKeyValuePair_OperatingOnEmptyCollection_ShouldReturnFalse()
        {
            // Arrange
            var arena = CreateArena();
            var kvp = GetRandom<KeyValuePair<string, string>>();
            // Pre-Assert
            Expect(arena.Collection).To.Be.Empty();
            // Act
            var result = arena.Sut.Contains(kvp);
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenNoSuchKeyInCollection_ShouldReturnFalse()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            var notInCollection = GetRandom<KeyValuePair<string, string>>();
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Contains(notInCollection);
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenHaveKeyInCollectionButMismatchesValue_ShouldReturnFalse()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            var notInCollection = new KeyValuePair<string, string>(inCollection.Key, GetRandomString());
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Contains(notInCollection);
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenHaveKeyInCollectionAndMatchingValue_ShouldReturnTrue()
        {
            // Arrange
            var arena = CreateArena();
            var inCollection = GetRandom<KeyValuePair<string, string>>();
            var notInCollection = new KeyValuePair<string, string>(inCollection.Key, inCollection.Value);
            arena.Collection.Add(inCollection.Key, inCollection.Value);
            // Pre-Assert
            // Act
            var result = arena.Sut.Contains(notInCollection);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void CopyTo_ShouldCopyItemsToArray()
        {
            // Arrange
            var sut = Create();
            var kvp = new KeyValuePair<string, string>(GetRandomString(2), GetRandomString(2));
            sut.Add(kvp);
            var target = new KeyValuePair<string, string>[2];
            // Pre-Assert
            // Act
            sut.CopyTo(target, 1);
            // Assert
            Expect(target[1]).To.Equal(kvp);
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
            Expect(arena.Sut)
                .Not.To.Contain(notInCollection);
            // Act
            var result = arena.Sut.Remove(notInCollection);
            // Assert
            Expect(result).To.Be.False();
            Expect(arena.Collection)
                .To.Contain.Key(inCollection.Key)
                .With.Value(inCollection.Value);
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
            var result = arena.Sut.Remove(inCollection);
            // Assert
            Expect(result).To.Be.True();
            Expect(arena.Collection).To.Be.Empty();
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
            Expect(result).To.Be.False();
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
            Expect(result).To.Be.False();
            Expect(arena.Collection).To.Contain.Only(1).Item();
            Expect(arena.Collection).To.Contain.Key(key).With.Value(value);
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
            Expect(result).To.Be.True();
            Expect(arena.Collection).To.Be.Empty();
        }

        [Test]
        public void Add_GivenKeyAndValue_ShouldAdd()
        {
            // Arrange
            var arena = CreateArena();
            var key = GetRandomString(4);
            var value = GetRandomString(4);
            // Pre-Assert
            Expect(arena.Collection).To.Be.Empty();
            // Act
            arena.Sut.Add(key, value);
            // Assert
            Expect(arena.Collection).To.Contain.Key(key).With.Value(value);
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
            Expect(result).To.Be.Equivalent.To(expected);
        }

        [Test]
        public void ShouldReturnNullForUnknownKey()
        {
            // Arrange
            var nameValueCollection = new NameValueCollection();
            var expected = nameValueCollection["foo"];
            var sut = Create(nameValueCollection);
            Expect(expected)
                .To.Be.Null();
            // Act
            var result = sut["foo"];
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        private TestArena CreateArena()
        {
            return new TestArena();
        }

        private class TestArena
        {
            public NameValueCollection Collection { get; }
            public IDictionary<string, string> Sut { get; }

            public TestArena() : this(false)
            {
            }

            public TestArena(bool isCaseInSensitive)
            {
                Collection = new NameValueCollection();
                Sut = new DictionaryWrappingNameValueCollection(
                    Collection,
                    isCaseInSensitive
                );
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