using NUnit.Framework;

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

            public int Value { get; private set; }
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
    }
}
