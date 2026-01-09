using System;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.RandomGenerators.Tests;

[TestFixture]
public class TestDateTimeRange
{
    [Test]
    public void Construct_GivenFromDateAndToDate_WhenFromDateLessThanToDate_ShouldSetProperties()
    {
        //---------------Set up test pack-------------------
        var from = GetRandomDate();
        var to = GetAnother(from, () => GetRandomDate(from.AddYears(1)), (d1, d2) => d1 >= d2);

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
        var to = GetAnother(from, () => GetRandomDate(from.AddYears(1)), (d1, d2) => d1 >= d2);

        //---------------Assert Precondition----------------
        Expect(from).To.Be.Less.Than(to);

        //---------------Execute Test ----------------------
        var sut = Create(to, from);

        //---------------Test Result -----------------------
        Expect(sut.From).To.Equal(from);
        Expect(sut.To).To.Equal(to);
    }

    [TestFixture]
    public class InRange
    {
        [Test]
        public void WhenGivenDateEqualToStart_ShouldReturnTrue()
        {
            // Arrange
            var from = GetRandomDate();
            var to = GetRandomDate(from.AddMinutes(1));
            var sut = Create(from, to);
            // Pre-assert
            // Act
            var result = sut.InRange(from);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void WhenGivenDateEqualToEnd_ShouldReturnTrue()
        {
            // Arrange
            var from = GetRandomDate();
            var to = GetRandomDate(from.AddMinutes(1));
            var sut = Create(from, to);
            // Pre-assert
            // Act
            var result = sut.InRange(to);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void WhenGivenDateShouldReturnTrue()
        {
            // Arrange
            var from = GetRandomDate();
            var to = GetRandomDate(from.AddMinutes(1));
            var test = GetRandomDate(from, to);
            var sut = Create(from, to);
            // Pre-assert
            // Act
            var result = sut.InRange(test);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void WhenGivenDateOutOfRange_ShouldReturnFalse()
        {
            // Arrange
            var from = GetRandomDate();
            var to = GetRandomDate(from.AddMinutes(1));
            var test = GetRandomDate();
            while (test >= from && test <= to)
            {
                test = GetRandomDate();
            }

            var sut = Create(from, to);
            // Pre-assert
            // Act
            var result = sut.InRange(test);
            // Assert
            Expect(result).To.Be.False();
        }
    }

    private static DateRange Create(DateTime fromDate, DateTime toDate)
    {
        return new DateRange(fromDate, toDate);
    }
}