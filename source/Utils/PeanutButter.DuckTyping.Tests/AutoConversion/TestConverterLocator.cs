using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping.AutoConversion;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.DuckTyping.Tests.AutoConversion
{
    [TestFixture]
    public class TestConverterLocator
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
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Equal(result2);
            var useful1 = result1 as IConverter<string, Guid>;
            Expect(useful1).Not.To.Be.Null();
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
            Expect(result1).Not.To.Be.Null();
            Expect(result2).Not.To.Be.Null();
            Expect(result1).To.Equal(result2);
            var useful2 = result1 as IConverter<string, int>;
            Expect(useful2).Not.To.Be.Null();
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

        public class HonestConverter : IConverter<Type1, Type3>
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

            public bool CanConvert(Type t1, Type t2)
            {
                var types = new HashSet<Type>(new[] { T1, T2 });
                return types
                    .Contains(t1) && types.Contains(t2);
            }

            public bool IsInitialised => true;
        }

        [Test]
        public void Converters_ShouldContainHonestConverters()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = ConverterLocator.Converters.Values.Where(t => t.GetType() == typeof(HonestConverter));

            //--------------- Assert -----------------------
            Expect(result).Not.To.Be.Empty();
        }


        public interface IConvertMe1
        {
        }

        public interface IConvertMe2
        {
        }

        public class ConverterWithConstructorParameters
            : IConverter<IConvertMe1, IConvertMe2>
        {
            public Type T1 => typeof(IConvertMe1);
            public Type T2 => typeof(IConvertMe2);
            public string Moo { get; }

            public ConverterWithConstructorParameters(string moo)
            {
                Moo = moo;
            }

            public IConvertMe1 Convert(IConvertMe2 input)
            {
                throw new NotImplementedException();
            }

            public IConvertMe2 Convert(IConvertMe1 input)
            {
                throw new NotImplementedException();
            }

            public bool CanConvert(Type t1, Type t2)
            {
                return (t1 == T1 || t1 == T2) &&
                    (t2 == T1 || t2 == T2) &&
                    t1 != t2;
            }

            public bool IsInitialised => true;
        }

        [Test]
        public void ShouldNotExplodeWhenUnableToLoadConverter()
        {
            //--------------- Arrange -------------------
            IConverter[] converters = null;
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => converters = ConverterLocator.Converters.Values.Distinct().ToArray())
                .Not.To.Throw();

            //--------------- Assert -----------------------
            Expect(
                    converters.Any(c => c is ConverterWithConstructorParameters)
                )
                .To.Be.False();
        }
    }
}