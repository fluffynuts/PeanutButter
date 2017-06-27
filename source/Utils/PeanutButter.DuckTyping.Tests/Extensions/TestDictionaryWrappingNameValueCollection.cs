using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

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
