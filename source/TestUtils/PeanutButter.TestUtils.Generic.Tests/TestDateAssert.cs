using System;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestDateAssert
    {
        [Test]
        public void IsInRange_GivenMinAndMaxDate_WhenInputIsWithinRange_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(2011, 1, 1);
            var max = new DateTime(2022, 2, 2);
            var test = new DateTime(2016, 1, 6);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => DateTimeAssert.IsInRange(test, min, max));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsInRange_GivenMinAndMaxDate_WhenInputIsTooLarge_ShouldAssertFail()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(2011, 1, 1);
            var max = new DateTime(2022, 2, 2);
            var test = new DateTime(2026, 1, 6);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => DateTimeAssert.IsInRange(test, min, max));

            //---------------Test Result -----------------------
            Assert.AreEqual(test + " does not fall in the range " + min + " - " + max, ex.Message);

        }

        [Test]
        public void IsInRange_GivenMinAndMaxDate_WhenInputIsTooSmall_ShouldAssertFail()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(2011, 1, 1);
            var max = new DateTime(2022, 2, 2);
            var test = new DateTime(2006, 1, 6);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => DateTimeAssert.IsInRange(test, min, max));

            //---------------Test Result -----------------------
            Assert.AreEqual(test + " does not fall in the range " + min + " - " + max, ex.Message);

        }

        [Test]
        public void IsInRange_WhenTestValueEqualsMin_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(2011, 1, 1);
            var max = new DateTime(2022, 2, 2);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => DateTimeAssert.IsInRange(min, min, max));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsInRange_WhenTestValueEqualsMax_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(2011, 1, 1);
            var max = new DateTime(2022, 2, 2);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => DateTimeAssert.IsInRange(max, min, max));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsInRange_CanGetDatesAsIntegers()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(2011, 1, 1);
            var max = new DateTime(2022, 2, 2);
            var test = new DateTime(2016, 6, 6);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => DateTimeAssert.IsInRange(test, min.Year, min.Month, min.Day, max.Year, max.Month, max.Day));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsInTimeRange_WhenTestValueWithinTimeRange_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(1900, 1, 1, 2, 0, 0);
            var max = new DateTime(1900, 1, 1, 4, 0, 0);
            var test = new DateTime(2011, 1, 1, 3, 0, 0);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => DateTimeAssert.IsInTimeRange(test, min, max));

            //---------------Test Result -----------------------
        }

        [Test]
        public void IsInTimeRange_WhenTestValueGreaterThanTimeRange_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var min = new DateTime(1900, 1, 1, 2, 0, 0);
            var max = new DateTime(1900, 1, 1, 4, 0, 0);
            var test = new DateTime(2011, 1, 1, 6, 0, 0);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var ex = Assert.Throws<AssertionException>(() => DateTimeAssert.IsInTimeRange(test, min, max));

            //---------------Test Result -----------------------
            Assert.AreEqual("Time of " + test + " does not fall within expected range: " + min.AsTimeString() + " - " + max.AsTimeString(), ex.Message);
        }
    }
}
