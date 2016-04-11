using System;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestDateTimeRange
    {
        [Test]
        public void Construct_GivenFromDateAndToDate_WhenFromDateLessThanToDate_ShouldSetProperties()
        {
            //---------------Set up test pack-------------------
            var from = GetRandomDate();
            var to = GetAnother(from, () => GetRandomDate(), (d1, d2) => d1 >= d2);

            //---------------Assert Precondition----------------
            Assert.That(from, Is.LessThan(to));

            //---------------Execute Test ----------------------
            var sut = Create(from, to);

            //---------------Test Result -----------------------
            Assert.AreEqual(from, sut.From);
            Assert.AreEqual(to, sut.To);
        }

        [Test]
        public void Construct_GivenFromDateAndToDate_WhenFromDateGreaterThanToDate_ShouldSetPropertiesReversed()
        {
            //---------------Set up test pack-------------------
            var from = GetRandomDate();
            var to = GetAnother(from, () => GetRandomDate(), (d1, d2) => d1 >= d2);

            //---------------Assert Precondition----------------
            Assert.That(from, Is.LessThan(to));

            //---------------Execute Test ----------------------
            var sut = Create(to, from);

            //---------------Test Result -----------------------
            Assert.AreEqual(from, sut.From);
            Assert.AreEqual(to, sut.To);
        }

        private DateRange Create(DateTime fromDate, DateTime toDate)
        {
            return new DateRange(fromDate, toDate);
        }
    }
}