using System;
using NUnit.Framework;
using PeanutButter.DuckTyping.AutoConversion;

namespace PeanutButter.DuckTyping.Tests.AutoConversion
{
    [TestFixture]
    public class TestConverterLocator: AssertionHelper
    {
        [Test]
        public void GetConverterFor_GivenStringAndGuid_ShouldReturnConverter()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = ConverterLocator.GetConverter(typeof(string), typeof(Guid));
            var result2 = ConverterLocator.GetConverter(typeof(Guid), typeof(string));

            //--------------- Assert -----------------------
            Expect(result1, Is.Not.Null);
            Expect(result2, Is.Not.Null);
            Expect(result1, Is.EqualTo(result2));
            var useful1 = result1 as IConverter<string, Guid>;
            Expect(useful1, Is.Not.Null);
        }

        [Test]
        public void GetConverterFor_GivenIntAndString_ShouldReturnConverter()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = ConverterLocator.GetConverter(typeof(int), typeof(string));
            var result2 = ConverterLocator.GetConverter(typeof(string), typeof(int));

            //--------------- Assert -----------------------
            Expect(result1, Is.Not.Null);
            Expect(result2, Is.Not.Null);
            Expect(result1, Is.EqualTo(result2));
            var useful2 = result1 as IConverter<string, int>;
            Expect(useful2, Is.Not.Null);
        }


    }
}
