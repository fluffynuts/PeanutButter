using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

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
            RunCycles(() => ints.Add(RandomValueGen.GetRandomInt(min, max)));

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
            RunCycles(() => ints.Add(RandomValueGen.GetRandomLong(min, max)));

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
            RunCycles(() => strings.Add(RandomValueGen.GetRandomString(minLength, maxLength)));

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
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            RunCycles(() => results.Add(RandomValueGen.GetRandomEnum<TestEnum>()));
            //---------------Test Result -----------------------
            var runs = results.Count;
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

        private void RunCycles(Action toRun)
        {
            for (var i = 0; i < RANDOM_TEST_CYCLES; i++)
                toRun();
        }

        private class DateTimeRange
        {
            public DateTime MaxDate { get; private set; }
            public DateTime MinDate { get; private set; }

            public DateTimeRange(int minYear, int minMonth, int minDay, int maxYear, int maxMonth, int maxDay)
            {
                MinDate = new DateTime(minYear, minMonth, minDay, 0, 0, 0);
                MaxDate = new DateTime(maxYear, maxMonth, maxDay, 0, 0, 0);
                if (MinDate > MaxDate)
                {
                    var swap = MinDate;
                    MinDate = MaxDate;
                    MaxDate = swap;
                }
            }

            public bool InRange(DateTime value)
            {
                return value >= MinDate && value <= MaxDate;
            }
        }

        [TestCase(1984, 4, 4, 2001, 1, 1)]
        [TestCase(1914, 4, 4, 2011, 1, 1)]
        [TestCase(2001, 4, 4, 2001, 1, 1)]
        public void GetRandomDate_GivenDateOnlyIsTrue_ShouldReturnDateTimeWithNoTimeComponent(int minYear, int minMonth, int minDay, int maxYear, int maxMonth, int maxDay)
        {
            //---------------Set up test pack-------------------
            var results = new List<DateTime>();
            var range = new DateTimeRange(minYear, minMonth, minDay, maxYear, maxMonth, maxDay);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => results.Add(RandomValueGen.GetRandomDate(range.MinDate, range.MaxDate, dateOnly:true)));

            //---------------Test Result -----------------------
            Assert.AreEqual(RANDOM_TEST_CYCLES, results.Count);
            Assert.IsTrue(results.All(range.InRange), "One or more generated value is out of range");
            Assert.IsTrue(results.All(d => d.Hour == 0), "Hours are not all zeroed");
            Assert.IsTrue(results.All(d => d.Minute == 0), "Minutes are not all zeroed");
            Assert.IsTrue(results.All(d => d.Second == 0), "Seconds are not all zeroed");
            Assert.IsTrue(results.All(d => d.Millisecond == 0), "Seconds are not all zeroed");
        }

        [TestCase(1984, 4, 4, 2001, 1, 1)]
        [TestCase(1914, 4, 4, 2011, 1, 1)]
        [TestCase(2001, 4, 4, 2001, 1, 1)]
        public void GetRandomDate_ShouldReturnDatesWithinRange(int minYear, int minMonth, int minDay, int maxYear, int maxMonth, int maxDay)
        {
            //---------------Set up test pack-------------------
            var results = new List<DateTime>();
            var range = new DateTimeRange(minYear, minMonth, minDay, maxYear, maxMonth, maxDay);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => results.Add(RandomValueGen.GetRandomDate(range.MinDate, range.MaxDate)));

            //---------------Test Result -----------------------
            Assert.AreEqual(RANDOM_TEST_CYCLES, results.Count);
            Assert.IsTrue(results.All(d => d >= range.MinDate), "One or more results is less than the minimum date");
            Assert.IsTrue(results.All(d => d <= range.MaxDate), "One or more results is greater than the maximum date");
            Assert.IsTrue(results.All(d => d.Millisecond == 0), "Milliseconds should be zeroed on random dates");
        }

        [Test]
        public void GetRandomTimeOn_GivenDate_ShouldReturnRandomDateTimeOnThatDay()
        {
            //---------------Set up test pack-------------------
            var theDate = RandomValueGen.GetRandomDate();
            var min = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
            var max = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
            max = max.AddDays(1).AddMilliseconds(-1);
            var results = new List<DateTime>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => results.Add(RandomValueGen.GetRandomTimeOn(theDate)));

            //---------------Test Result -----------------------
            Assert.IsTrue(results.All(d => d >= min));
            Assert.IsTrue(results.All(d => d <= max));

        }

        [Test]
        public void GetRandomDate_GivenMinTime_ShouldProduceRandomDatesWithTimesGreaterOrEqual()
        {
            //---------------Set up test pack-------------------
            var minTime = new DateTime(1900, 1, 1, RandomValueGen.GetRandomInt(0, 12), RandomValueGen.GetRandomInt(0, 59), RandomValueGen.GetRandomInt(0, 59));
            var maxTime = new DateTime(minTime.Year, minTime.Month, minTime.Day, 0, 0, 0);
            maxTime = maxTime.AddDays(1).AddMilliseconds(-1);
            var results = new List<DateTime>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => results.Add(RandomValueGen.GetRandomDate(minTime: minTime)));

            //---------------Test Result -----------------------
            var outOfRange = results.Where(d => d.MillisecondsSinceStartOfDay() < minTime.MillisecondsSinceStartOfDay()).ToArray();
            Assert.IsFalse(outOfRange.Any(), "One or more results had a time that was too early.");
        }

        [Test]
        public void GetRandomDate_GivenMaxTime_ShouldProduceRandomDatesWithTimesLessThanOrEqual()
        {
            //---------------Set up test pack-------------------
            var maxTime = new DateTime(1900, 1, 1, RandomValueGen.GetRandomInt(12, 23), RandomValueGen.GetRandomInt(0, 59), RandomValueGen.GetRandomInt(0, 59));
            var results = new List<DateTime>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => results.Add(RandomValueGen.GetRandomDate(maxTime: maxTime)));

            //---------------Test Result -----------------------
            var outOfRange = results.Where(d => d.MillisecondsSinceStartOfDay() > maxTime.MillisecondsSinceStartOfDay()).ToArray();
            Assert.IsFalse(outOfRange.Any(), "One or more results had a time that was too late.");
        }

        [Test]
        public void GetRandomList_GivenGeneratorFunctionAndBoundaries_ShouldReturnListOfRandomSizeContainingOutputOfGeneratorPerItem()
        {
            //---------------Set up test pack-------------------
            const int runs = RANDOM_TEST_CYCLES;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < runs; i++)
            {
                var min = RandomValueGen.GetRandomInt(10, 100);
                var max = RandomValueGen.GetRandomInt(10, 100);
                if (min > max)
                {
                    var swap = min;
                    min = max;
                    max = swap;
                }
                var fill = RandomValueGen.GetRandomInt(1, 1024);
                var result = RandomValueGen.GetRandomList(() => fill, min, max);


                //---------------Test Result -----------------------
                Assert.That(result.Count, Is.GreaterThanOrEqualTo(min));
                Assert.That(result.Count, Is.LessThanOrEqualTo(max));
                Assert.IsTrue(result.All(item => item == fill));
            }
        }

    }

}
