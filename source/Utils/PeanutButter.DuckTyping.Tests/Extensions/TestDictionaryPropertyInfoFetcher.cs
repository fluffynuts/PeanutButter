using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

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
            var sut = Create(input);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.GetProperties(input.GetType(), BindingFlags.Public);

            //--------------- Assert -----------------------
            Expect(result, Is.Empty);
        }


        private IPropertyInfoFetcher Create(Dictionary<string, object> data)
        {
            return new DictionaryPropertyFetcher(data);
        }

    }
}