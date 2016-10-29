using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestDictionaryPropertyInfoFetcher: AssertionHelper
    {
        [Test]
        public void Type_ShouldImplement_IPropertyFetcher()
        {
            //--------------- Arrange -------------------
            var sut = typeof(DictionaryPropertyFetcher);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldImplement<IPropertyInfoFetcher>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void GetProperties_GivenEmptyDictionary_ShouldReturnEmptyArray()
        {
            //--------------- Arrange -------------------
            var input = new Dictionary<string, object>();
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertiesFor(input, BindingFlags.Public);

            //--------------- Assert -----------------------
            Expect(result, Is.Empty);
        }

        [Test]
        public void GetProperties_GivenDictionaryWithOneIntProperty_ShouldReturnMatchingProperty()
        {
            //--------------- Arrange -------------------
            var propertyName = GetRandomString();
            var propertyValue = GetRandomInt();
            var input = new Dictionary<string, object>()
            {
                { propertyName, propertyValue }
            };
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetPropertiesFor(input, BindingFlags.Public);

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Empty);
            Expect(result, Has.Length.EqualTo(1));
            var propInfo = result[0];
            Expect(propInfo.Name, Is.EqualTo(propertyName));
            Expect(propInfo.PropertyType, Is.EqualTo(propertyValue.GetType()));
            Expect(propInfo.CanRead, Is.True);
            Expect(propInfo.CanWrite, Is.True);
        }

        private IPropertyInfoFetcher Create()
        {
            return new DictionaryPropertyFetcher();
        }

    }
}