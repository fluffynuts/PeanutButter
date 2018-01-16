using System;
using NExpect;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;

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
            Expect(from).To.Be.Less.Than(to);

            //---------------Execute Test ----------------------
            var sut = Create(from, to);

            //---------------Test Result -----------------------
            Expect(sut.From).To.Equal(from);
            Expect(sut.To).To.Equal(to);
        }

        [Test]
        public void Construct_GivenFromDateAndToDate_WhenFromDateGreaterThanToDate_ShouldSetPropertiesReversed()
        {
            //---------------Set up test pack-------------------
            var from = GetRandomDate();
            var to = GetAnother(from, () => GetRandomDate(), (d1, d2) => d1 >= d2);

            //---------------Assert Precondition----------------
            Expect(from).To.Be.Less.Than(to);

            //---------------Execute Test ----------------------
            var sut = Create(to, from);

            //---------------Test Result -----------------------
            Expect(sut.From).To.Equal(from);
            Expect(sut.To).To.Equal(to);
        }

        private DateRange Create(DateTime fromDate, DateTime toDate)
        {
            return new DateRange(fromDate, toDate);
        }
    }
}