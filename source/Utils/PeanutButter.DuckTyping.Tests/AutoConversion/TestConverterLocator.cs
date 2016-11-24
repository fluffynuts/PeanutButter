using System;
using System.Linq;
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

        public class Type1
        {
        }
        public class Type2
        {
        }
        public class Type3
        {
        }
        public class LyingConverter: IConverter<Type1, Type2>
        {
            public Type T1 => typeof(Type1);
            public Type T2 => typeof(Type3);
            public Type1 Convert(Type2 input)
            {
                throw new NotImplementedException();
            }

            public Type2 Convert(Type1 input)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Converters_ShouldNotContainLyingConverters()
        {
            // the T1 and T2 properties are just there to make lookups faster
            //  and should reflect the same types as the generic interface
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = ConverterLocator.Converters.Where(t => t.GetType() == typeof(LyingConverter));

            //--------------- Assert -----------------------
            Expect(result, Is.Empty);
        }

        public class HonestConverter: IConverter<Type1, Type3>
        {
            public Type T1 => typeof(Type1);
            public Type T2 => typeof(Type3);
            public Type1 Convert(Type3 input)
            {
                throw new NotImplementedException();
            }

            public Type3 Convert(Type1 input)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void Converters_ShouldContainHonestConverters()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = ConverterLocator.Converters.Where(t => t.GetType() == typeof(HonestConverter));

            //--------------- Assert -----------------------
            Expect(result, Is.Not.Empty);
        }




    }
}
