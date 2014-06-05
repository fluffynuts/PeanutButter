using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestRandomValueGen : TestBase
    {

        [TestCase(1, 100)]
        [TestCase(101, 250)]
        public void GetRandomInt_GivenRangeOfIntegers_ReturnsRandomIntWithinRange(int min, int max)
        {
            //---------------Set up test pack-------------------
            var ints = new List<int>();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < RANDOM_TEST_CYCLES; i++)
            {
                ints.Add(RandomValueGen.GetRandomInt(min, max));
            }

            //---------------Test Result -----------------------
            Assert.IsTrue(ints.All(i => i >= min));
            Assert.IsTrue(ints.All(i => i <= max));
            Assert.IsTrue(ints.Distinct().Count() > 1);
            Assert.IsTrue(ints.Count(i => i == max) > 0);
        }

        [TestCase(1, 100)]
        [TestCase(101, 250)]
        public void GetRandomLong_GivenRangeOfIntegers_ReturnsRandomIntWithinRange(int min, int max)
        {
            //---------------Set up test pack-------------------
            var ints = new List<long>();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < RANDOM_TEST_CYCLES; i++)
            {
                ints.Add(RandomValueGen.GetRandomLong(min, max));
            }

            //---------------Test Result -----------------------
            Assert.IsTrue(ints.All(i => i >= min));
            Assert.IsTrue(ints.All(i => i <= max));
            Assert.IsTrue(ints.Distinct().Count() > 1);
        }

        [TestCase(50, 100)]
        [TestCase(150, 400)]
        public void GetRandomString_GivenLengthLimits_ReturnsRandomStringsWithinLengthRange(int minLength, int maxLength)
        {
            //---------------Set up test pack-------------------
            var strings = new List<string>();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < RANDOM_TEST_CYCLES; i++)
            {
                strings.Add(RandomValueGen.GetRandomString(minLength, maxLength));
            }

            //---------------Test Result -----------------------
            Assert.IsTrue(strings.All(s => s.Length >= minLength));
            Assert.IsTrue(strings.All(s => s.Length <= maxLength));
            Assert.IsTrue(strings.Distinct().Count() > 1);
        }


        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        [Test]
        public void GetRandomEnum_WhenGivenEnumTypeSpecifier_ShouldReturnRandomValueFromEnumSelection()
        {
            //---------------Set up test pack-------------------
            var results = new List<TestEnum>();
            const int runs = RANDOM_TEST_CYCLES;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < runs; i++)
            {
                results.Add(RandomValueGen.GetRandomEnum<TestEnum>());
            }
            //---------------Test Result -----------------------
            var onePercent = (100 * results.Count(i => i == TestEnum.One)) / runs;
            var twoPercent = (100 * results.Count(i => i == TestEnum.Two)) / runs;
            var threePercent = (100 * results.Count(i => i == TestEnum.Three)) / runs;

            var d1 = Math.Abs(twoPercent - onePercent);
            var d2 = Math.Abs(threePercent - twoPercent);
            var d3 = Math.Abs(threePercent - onePercent);

            Assert.That(d1, Is.LessThan(10));
            Assert.That(d2, Is.LessThan(10));
            Assert.That(d3, Is.LessThan(10));
        }

        [Test]
        public void GetRandomFrom_WhenGivenIEnumerableOfItems_ShouldReturnARandomItemFromTheCollection()
        {
            //---------------Set up test pack-------------------
            var o1 = new object();
            var o2 = new object();
            var o3 = new object();
            var items = new[] {o1, o2, o3};
            var results = new List<object>();
            const int runs = RANDOM_TEST_CYCLES;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < runs; i++)
            {
                results.Add(RandomValueGen.GetRandomFrom(items));
            }

            //---------------Test Result -----------------------
        }



    }

}
