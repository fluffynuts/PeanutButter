using System.Collections.Generic;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable PossibleMultipleEnumeration

namespace PeanutButter.TestUtils.Generic.Tests
{
    public class TestEnumerableExtensions
    {
        private class IntWrapper
        {
            public static IntWrapper For(int i)
            {
                return new IntWrapper(i);
            }
            public IntWrapper(int i)
            {
                Value = i;
            }

            public int Value { get; }
        }

        [Test]
        public void ShouldMatchDataIn_OperatingOnCollection_WhenComparisonCollectionIsDifferentSize_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(1), IntWrapper.For(2)};
            var right = new[] {IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => left.ShouldMatchDataIn(right));
            Assert.Throws<AssertionException>(() => right.ShouldMatchDataIn(left));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldMatchDataIn_OperatingOnCollectionsOfSameLengthButDifferentData_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(2) };
            var right = new[] {IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => left.ShouldMatchDataIn(right));
            Assert.Throws<AssertionException>(() => right.ShouldMatchDataIn(left));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldMatchDataIn_OperatingOnCollectionsOfSameLengthAndSameData_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(1) };
            var right = new[] {IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => left.ShouldMatchDataIn(right));
            Assert.DoesNotThrow(() => right.ShouldMatchDataIn(left));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldMatchDataIn_OperatingOnCollectionsOfSameLengthAndSameDataDifferentOrder_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(1), IntWrapper.For(2) };
            var right = new[] { IntWrapper.For(2), IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => left.ShouldMatchDataIn(right));
            Assert.DoesNotThrow(() => right.ShouldMatchDataIn(left));

            //---------------Test Result -----------------------
        }
        [Test]
        public void ShouldMatchDataInAndOrderOf_OperatingOnCollection_WhenComparisonCollectionIsDifferentSize_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(1), IntWrapper.For(2)};
            var right = new[] {IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => left.ShouldMatchDataInAndOrderOf(right));
            Assert.Throws<AssertionException>(() => right.ShouldMatchDataInAndOrderOf(left));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldMatchDataInAndOrderOf_OperatingOnCollectionsOfSameLengthButDifferentData_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(2) };
            var right = new[] {IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => left.ShouldMatchDataInAndOrderOf(right));
            Assert.Throws<AssertionException>(() => right.ShouldMatchDataInAndOrderOf(left));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldMatchDataInAndOrderOf_OperatingOnCollectionsOfSameLengthAndSameData_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(1) };
            var right = new[] {IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => left.ShouldMatchDataInAndOrderOf(right));
            Assert.DoesNotThrow(() => right.ShouldMatchDataInAndOrderOf(left));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldMatchDataInAndOrderOf_OperatingOnCollectionsOfSameLengthAndSameDataDifferentOrder_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var left = new[] {IntWrapper.For(1), IntWrapper.For(2) };
            var right = new[] { IntWrapper.For(2), IntWrapper.For(1) };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => left.ShouldMatchDataInAndOrderOf(right));
            Assert.Throws<AssertionException>(() => right.ShouldMatchDataInAndOrderOf(left));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldMatchDataIn_ShouldOperateOnCollectionsOfDifferentTypeButSameShape()
        {
            //--------------- Arrange -------------------
            var left = new[]
            {
                new { id = 1 }
            };
            var right = new[]
            {
                new { id = 1 }
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() => left.ShouldMatchDataIn(right));

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldMatchDataInAndOrderOf_ShouldOperateOnCollectionsOfDifferentTypeButSameShape()
        {
            //--------------- Arrange -------------------
            var left = new[]
            {
                new { id = 1 }
            };
            var right = new[]
            {
                new { id = 1 }
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.DoesNotThrow(() => left.ShouldMatchDataInAndOrderOf(right));

            //--------------- Assert -----------------------
        }


        [Test]
        public void IsEquivalentTo_GivenTwoNulls_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            IEnumerable<string> left = null;
            IEnumerable<string> right = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = left.IsEquivalentTo(right);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsEquivalentTo_GivenOneNull_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var left = new string[] {};
            IEnumerable<string> right = null;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result1 = left.IsEquivalentTo(right);
            var result2 = right.IsEquivalentTo(left);

            //---------------Test Result -----------------------
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [Test]
        public void IsEquivalentTo_GivenTwoEmptyCollections_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var left = new string[] {};
            var right = new string[] {};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = left.IsEquivalentTo(right);

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsEquivalentTo_GivenOneCollectionWithOneItem_AndAnotherWithNoItems_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var left = new[] {1};
            var right = new int[] {};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result1 = left.IsEquivalentTo(right);
            var result2 = right.IsEquivalentTo(left);

            //---------------Test Result -----------------------
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [Test]
        public void IsEquivalentTo_GivenMisMatchingCollectionsOfSameLength_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var left = new[] {true};
            var right = new[] {false};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result1 = left.IsEquivalentTo(right);
            var result2 = right.IsEquivalentTo(left);

            //---------------Test Result -----------------------
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [Test]
        public void IsEquivalentTo_GivenMisMatchingCollectionsOfDifferentLength_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var left = new[] {true};
            var right = new[] {true, false};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result1 = left.IsEquivalentTo(right);
            var result2 = right.IsEquivalentTo(left);

            //---------------Test Result -----------------------
            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [Test]
        public void IsEquivalentTo_GivenMatchingCollections_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var left = new[] {1.2, 3.4};
            var right = new[] {1.2, 3.4};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result1 = left.IsEquivalentTo(right);
            var result2 = right.IsEquivalentTo(left);

            //---------------Test Result -----------------------
            Assert.IsTrue(result1);
            Assert.IsTrue(result2);
        }

        [Test]
        public void METHOD()
        {
            // Arrange
            // Pre-assert
            // Act
            // Assert
            Assert.Fail("Not yet implemented");
        }

        [Test]
        public void ShouldHaveUnique_WhenHasNoMatches_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var input = new[] { true, true, true };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => input.ShouldHaveUnique(v => v == false));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveUnique_WhenMultipleNoMatches_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var input = new[] { 1.2m, 1.2m, .15m};

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => input.ShouldHaveUnique(v => v == 1.2m));

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldHaveUnique_WhenHaveOneMatch_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var singular = GetRandomString(4,6);
            var plural = GetRandomString(2,3);
            var input = new[] { plural, singular, plural };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => input.ShouldHaveUnique(s => s == singular));

            //---------------Test Result -----------------------
        }

    }
}
