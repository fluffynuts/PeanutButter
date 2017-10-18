using System;
using NUnit.Framework;
using NExpect;
using NExpect.Implementations;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestStringifier
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
            Expect(result).To.Equal(expected);
        }

        private static readonly Tuple<object, string>[] _complexSource = new[]
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

        [TestCaseSource(nameof(_complexSource))]
        public void Stringify_GivenObject_ShouldReturnExpected(Tuple<object, string> data)
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Stringifier.Stringify(data.Item1);

            //--------------- Assert -----------------------
            Expect(result).To.Equal(data.Item2.TrimStart().Replace("\r", ""));
        }

        [Test]
        public void CollectionsOfCollections()
        {
            // Arrange
            // Pre-Assert
            // Act
            Console.WriteLine(new[] {
                new[] { 1, 2 },
                new[] { 5, 6, 7 }
            }.Stringify());
            // Assert
        }

    }
}