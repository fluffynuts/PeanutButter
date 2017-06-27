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
            var kvp = GetRandom<KeyValuePair<string, object>>();
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
