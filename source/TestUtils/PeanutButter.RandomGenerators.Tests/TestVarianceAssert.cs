using NUnit.Framework;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestVarianceAssert
    {
        [Test]
        public void IsVariant_OperatingOnEmptyCollection_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => VarianceAssert.IsVariant(new string[] { }));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsVariant_OperatingOnCollectionOfOne_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => VarianceAssert.IsVariant(new[] { RandomValueGen.GetRandomString() }));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsVariant_OperatingOnCollectionOfTwo_WhenHaveDifferentValues_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var value1 = RandomValueGen.GetRandomString();
            var value2 = RandomValueGen.GetAnother(value1);
            var input = new[] { value1, value2 };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => VarianceAssert.IsVariant(input));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsVariant_OperatingOnCollectionOfTwo_WhenHaveSameValues_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var value = RandomValueGen.GetRandomString();
            var input = new[] { value, value };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<AssertionException>(() => VarianceAssert.IsVariant(input));

            //---------------Test Result -----------------------
        }




    }
}