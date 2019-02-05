using System;
using System.Collections.Generic;
using System.Linq;
using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;

// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable RedundantArgumentDefaultValue

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestEnumerableExtensions : TestBase
    {
        [Test]
        public void Randomize_OperatingOnEnumerableOfItems_ShouldGiveBackJumbledCollectionOfSameItems()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var results = new List<decimal>();
            for (var i = 0; i < NORMAL_RANDOM_TEST_CYCLES; i++)
            {
                var input = GetRandomCollection(() => GetRandomString(2, 10), 5, 10).ToArray();
                var jumbled = input.Randomize().ToArray();
                Expect(input).To.Contain.Exactly(jumbled.Length).Items();
                var outOfPlace = input
                    .Where((t, j) => t != jumbled[j])
                    .Count();
                results.Add(outOfPlace / (decimal)input.Length);
            }

            //---------------Test Result -----------------------
            var average = results.Average();
            Expect(average).To.Be.Greater.Than(0.5M);
        }

        [Test]
        public void Randomize_OperatingOnNull_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            List<int> ints = null;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ints.Randomize();

            //---------------Test Result -----------------------
            Expect(result).To.Be.Null();
        }

        [Test]
        public void Randomize_OperatingOnEmptyCollection_ShouldReturnEmptyCollection()
        {
            //---------------Set up test pack-------------------
            var ints = new List<int>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ints.Randomize();

            //---------------Test Result -----------------------
            Expect(result).Not.To.Be.Null();
            Expect(result).To.Be.Empty();
        }

        [Test]
        public void Randomize_ShouldReturnTheSameCollectionInADifferentOrder()
        {
            for (var i = 0;
                i < 10;
                i++) // it's entirely possible that the random output is the same, so let's try up to 10 times
            {
                //---------------Set up test pack-------------------
                var src = GetRandomCollection(() => GetRandomString(), 3);

                //---------------Assert Precondition----------------
                Expect(src).Not.To.Be.Empty();

                //---------------Execute Test ----------------------
                var result = src.Randomize();

                //---------------Test Result -----------------------
                Expect(result).To.Be.Equivalent.To(src);
                try
                {
                    Expect(result).Not.To.Equal(src);
                    return;
                }
                catch (Exception)
                {
                    if (i < 9)
                        continue;
                    throw;
                }
            }
        }

        [Test]
        public void ShouldContainOneDeepEqualTo_OperatingOnEmptyCollection_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var src = new string[] { };
            var seek = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainOneDeepEqualTo(seek))
                .To.Throw<AssertionException>()
                .With.Message.Containing("no matches");

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldContainOneDeepEqualTo_OperatingOnCollectionNotContainingValue_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var src = GetRandomCollection<string>(2);
            var seek = GetAnother(src);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainOneDeepEqualTo(seek))
                .To.Throw<AssertionException>()
                .With.Message.Containing(
                    $"Expected to find one \"{seek}\" in {src.Stringify()}"
                );

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldContainOneDeepEqualTo_OperatingOnCollectionContainingOneMatchingValue_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var src = GetRandomCollection<string>(4);
            var seek = GetRandomFrom(src);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainOneDeepEqualTo(seek))
                .Not.To.Throw();

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldContainOneDeepEqualTo_OperatingOnCollectionContainingTwoMatchingValues_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var stuff = GetRandomCollection<string>(4);
            var seek = GetRandomFrom(stuff);
            var src = stuff.And(seek);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainOneDeepEqualTo(seek))
                .To.Throw<AssertionException>()
                .With.Message.Containing("more than one match");

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldContainAtLeastOneDeepEqualTo_OperatingOnEmptyCollection_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var src = new string[] { };
            var seek = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainAtLeastOneDeepEqualTo(seek))
                .To.Throw<AssertionException>()
                .With.Message.Containing("empty");

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldContainAtLeastOneDeepEqualTo_OperatingOnCollectionNotContainingValue_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var src = GetRandomCollection<string>(2);
            var seek = GetAnother(src);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainAtLeastOneDeepEqualTo(seek))
                .To.Throw<AssertionException>()
                .With.Message.Containing(
                    $"Expected to find \"{seek}\" in {Stringifier.Stringify(src)}"
                );

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldContainAtLeastOneDeepEqualTo_OperatingOnCollectionContainingOneMatchingValue_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var src = GetRandomCollection<string>(4);
            var seek = GetRandomFrom(src);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainAtLeastOneDeepEqualTo(seek))
                .Not.To.Throw();

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldContainAtLeastOneDeepEqualTo_OperatingOnCollectionContainingTwoMatchingValues_ShouldNotThrow()
        {
            //--------------- Arrange -------------------
            var stuff = GetRandomCollection<string>(4);
            var seek = GetRandomFrom(stuff);
            var src = stuff.And(seek);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => src.ShouldContainOneDeepEqualTo(seek))
                .To.Throw<AssertionException>()
                .With.Message.Containing("more than one match");

            //--------------- Assert -----------------------
        }
    }
}