using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    }

}
