using System;
using NUnit.Framework;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestStringifier: AssertionHelper
    {
        [TestCase("foo", "\"foo\"")]
        [TestCase(1, "1")]
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        [TestCase(null, "null")]
        public void Stringify_GivenPrimitive_ShouldReturnExpected(object value, string expected)
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Stringifier.Stringify(value);

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }

        private static readonly Tuple<object, string>[] ComplexSource = new[]
        {
            Tuple.Create(new { foo = 1 } as object, @"
{
  foo: 1
}"),
            Tuple.Create(new { foo = new { bar = 1 } } as object, @"
{
  foo: {
    bar: 1
  }
}") 
        };

        [TestCaseSource(nameof(ComplexSource))]
        public void Stringify_GivenObject_ShouldReturnExpected(Tuple<object, string> data)
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Stringifier.Stringify(data.Item1);

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(data.Item2.TrimStart().Replace("\r", "")));
        }


    }
}