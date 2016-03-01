using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
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

        [Test]
        public void GetRandomIntGeneric_GivenRangeOfIntegers_ReturnsRandomIntWithinRange()
        {
            //---------------Set up test pack-------------------
            var ints = new List<int>();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => ints.Add(RandomValueGen.GetRandom<int>()));

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(ints);
            Assert.IsTrue(ints.All(i => i >= RandomValueGen.DefaultRanges.MIN_INT_VALUE));
            Assert.IsTrue(ints.All(i => i <= RandomValueGen.DefaultRanges.MAX_INT_VALUE));
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

        [Test]
        public void GetRandomLongGeneric_GivenRangeOfIntegers_ReturnsRandomIntWithinRange()
        {
            //---------------Set up test pack-------------------
            var ints = new List<long>();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => ints.Add(RandomValueGen.GetRandom<long>()));

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(ints);
            Assert.IsTrue(ints.All(i => i >= RandomValueGen.DefaultRanges.MIN_LONG_VALUE));
            Assert.IsTrue(ints.All(i => i <= RandomValueGen.DefaultRanges.MAX_LONG_VALUE));
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

        [TestCase(50, 100)]
        [TestCase(150, 400)]
        public void GetRandomStringGeneric_GivenLengthLimits_ReturnsRandomStringsWithinLengthRange(int minLength, int maxLength)
        {
            //---------------Set up test pack-------------------
            var strings = new List<string>();
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() => strings.Add(RandomValueGen.GetRandom<string>()));

            //---------------Test Result -----------------------
            Assert.IsTrue(strings.All(s => s.Length >= RandomValueGen.DefaultRanges.MINLENGTH_STRING));
            Assert.IsTrue(strings.All(s => s.Length <= RandomValueGen.DefaultRanges.MINLENGTH_STRING + RandomValueGen.DefaultRanges.MINLENGTH_STRING));
            Assert.IsTrue(strings.Distinct().Count() > 1);
        }


        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        [Test]
        public void GetRandomEnum_GENERIC_WhenGivenEnumTypeSpecifier_ShouldReturnRandomValueFromEnumSelection()
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
        public void GetRandom_GENERIC_WithEnumType_WhenGivenEnumTypeSpecifier_ShouldReturnRandomValueFromEnumSelection()
        {
            //---------------Set up test pack-------------------
            var results = new List<TestEnum>();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            RunCycles(() => results.Add(RandomValueGen.GetRandom<TestEnum>()));
            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(results);
        }

        [Test]
        public void GetRandomEnum_WITHTYPE_WhenGivenEnumTypeSpecifier_ShouldReturnRandomValueFromEnumSelection()
        {
            //---------------Set up test pack-------------------
            var results = new List<TestEnum>();
            var type = typeof (TestEnum);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            RunCycles(() => results.Add((TestEnum)RandomValueGen.GetRandomEnum(type)));
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
            Assert.IsTrue(results.All(r => items.Contains(r)));
            Assert.IsTrue(items.All(i => results.Any(r => r == i)));
        }

        [Test]
        public void GetRandomSelectionFrom_WhenGivenIEnumerableOfItems_ShouldReturnARandomCollectionFromTheCollection()
        {
            //---------------Set up test pack-------------------
            var o1 = new object();
            var o2 = new object();
            var o3 = new object();
            var items = new[] {o1, o2, o3};
            var results = new List<IEnumerable<object>>();
            const int runs = RANDOM_TEST_CYCLES;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < runs; i++)
            {
                results.Add(RandomValueGen.GetRandomSelectionFrom(items));
            }

            //---------------Test Result -----------------------
            var flattened = results.SelectMany(r => r);
            Assert.IsTrue(flattened.All(f => items.Contains(f)));
            var averageCount = results.Select(r => r.Count()).Average();
            Assert.That(averageCount, Is.GreaterThan(1));
            Assert.That(averageCount, Is.LessThan(items.Count()));
        }

        [Test]
        public void GetRandomSelectionFrom_ShouldNotRepeatItems()
        {
            //---------------Set up test pack-------------------
            var o1 = new object();
            var o2 = new object();
            var o3 = new object();
            var items = new[] {o1, o2, o3};
            const int runs = RANDOM_TEST_CYCLES;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < runs; i++)
            {
                var result = RandomValueGen.GetRandomSelectionFrom(items);
                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(result, result.Distinct());
            }
        }

        [Test]
        public void GetRandomSelectionFrom_ShouldProvideCollectionWithinRequiredRangeOfSize()
        {
            //---------------Set up test pack-------------------
            var o1 = new object();
            var o2 = new object();
            var o3 = new object();
            var o4 = new object();
            var o5 = new object();
            var o6 = new object();
            var items = new[] {o1, o2, o3, o4, o5, o6};
            var min = RandomValueGen.GetRandomInt(1, 3);
            var max = RandomValueGen.GetRandomInt(3, items.Length);
            const int runs = RANDOM_TEST_CYCLES;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < runs; i++)
            {
                var result = RandomValueGen.GetRandomSelectionFrom(items, min, max);
                //---------------Test Result -----------------------
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(min));
                Assert.That(result.Count(), Is.LessThanOrEqualTo(max));
            }
        }

        [Test]
        public void GetRandomCollection_GivenFactoryFunction_ShouldInvokeItToCreateItems()
        {
            //---------------Set up test pack-------------------
            const int runs = RANDOM_TEST_CYCLES;
            var generatedValues = new List<int>();
            Func<int> factory = () =>
            {
                var thisValue = RandomValueGen.GetRandomInt();
                generatedValues.Add(thisValue);
                return thisValue;
            };
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < runs; i++)
            {
                var result = RandomValueGen.GetRandomCollection(factory);
                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(generatedValues, result);
                generatedValues.Clear();
            }
        }

        [Test]
        public void GetRandomCollection_GenericInvoke_ShouldUseNinjaSuperPowersToCreateCollection()
        {
            //---------------Set up test pack-------------------
            var minItems = RandomValueGen.GetRandomInt(1, 10);
            var maxItems = RandomValueGen.GetRandomInt(11, 20);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = RandomValueGen.GetRandomCollection<SomePOCO>(minItems, maxItems);

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            CollectionAssert.IsNotEmpty(result);
            Assert.IsTrue(result.All(r => r != null));
            Assert.IsTrue(result.All(r => r.GetType() == typeof(SomePOCO)));
            VarianceAssert.IsVariant<SomePOCO, int>(result, "Id");
            VarianceAssert.IsVariant<SomePOCO, string>(result, "Name");
            VarianceAssert.IsVariant<SomePOCO, DateTime>(result, "Date");
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
            RunCycles(() => results.Add(RandomValueGen.GetRandomDate(minTime: minTime, maxTime: maxTime)));

            //---------------Test Result -----------------------
            var outOfRangeLeft = results.Where(d => d.Ticks < minTime.Ticks).ToArray();
            var outOfRangeRight = results.Where(d => d.Ticks < maxTime.Ticks).ToArray();
            Assert.IsFalse(outOfRangeLeft.Any() && outOfRangeRight.Any(), 
                                    GetErrorHelpFor(outOfRangeLeft, outOfRangeRight, minTime, maxTime));
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
        public void GetRandomCollection_GivenGeneratorFunctionAndBoundaries_ShouldReturnListOfRandomSizeContainingOutputOfGeneratorPerItem()
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
                var result = RandomValueGen.GetRandomCollection(() => fill, min, max);


                //---------------Test Result -----------------------
                Assert.That(result.Count(), Is.GreaterThanOrEqualTo(min));
                Assert.That(result.Count(), Is.LessThanOrEqualTo(max));
                Assert.IsTrue(result.All(item => item == fill));
            }
        }

        [Test]
        public void GetRandomCollection_WhenMinEqualsMax_ShouldReturnExactlyThatSize()
        {
            //---------------Set up test pack-------------------
            int max;
            var min = max = RandomValueGen.GetRandomInt();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = RandomValueGen.GetRandomCollection(() => RandomValueGen.GetRandomInt(), min, max);

            //---------------Test Result -----------------------
            Assert.AreEqual(min, result.Count());
        }


        [Test]
        public void GetRandomAlphaNumericString_ShouldProduceRandomStringWithOnlyAlphaNumericCharacters()
        {
            var allResults = new List<Tuple<string, int, int>>();
            RunCycles(() =>
            {
                //---------------Set up test pack-------------------
                var minLength = RandomValueGen.GetRandomInt(1, 50);
                var maxLength = RandomValueGen.GetRandomInt(minLength, minLength + 50);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = RandomValueGen.GetRandomAlphaNumericString(minLength, maxLength);

                allResults.Add(Tuple.Create(result, minLength, maxLength));

            });
            //---------------Test Result -----------------------
            CollectionAssert.IsNotEmpty(allResults);
            // collisions are possible, but should occur < 1%
            var total = allResults.Count();
            var unique = allResults.Select(o => o.Item1).Distinct().Count();
            var delta = (decimal) (total - unique)/(decimal) total;
            Assert.That(delta, Is.LessThan(1));

            var tooShort = allResults.Where(r => r.Item1.Length < r.Item2);
            var tooLong = allResults.Where(r => r.Item1.Length > r.Item3);
            var alphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var invalidCharacters = allResults.Where(r => r.Item1.Any(c => !alphaNumericChars.Contains(c)));
            Assert.IsFalse(tooShort.Any() && tooLong.Any() && invalidCharacters.Any(), BuildErrorMessageFor(tooShort, tooLong, invalidCharacters));
        }

        [Test]
        public void GetRandomAlphaString_ShouldProduceRandomStringWithOnlyAlphaCharacters()
        {
            var allResults = new List<Tuple<string, int, int>>();
            RunCycles(() =>
            {
                //---------------Set up test pack-------------------
                var minLength = RandomValueGen.GetRandomInt(1, 50);
                var maxLength = RandomValueGen.GetRandomInt(minLength, minLength + 50);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = RandomValueGen.GetRandomAlphaString(minLength, maxLength);

                allResults.Add(Tuple.Create(result, minLength, maxLength));

            });
            //---------------Test Result -----------------------
            CollectionAssert.IsNotEmpty(allResults);
            // collisions are possible, but should occur < 1%
            var total = allResults.Count();
            var unique = allResults.Select(o => o.Item1).Distinct().Count();
            var delta = (decimal) (total - unique)/(decimal) total;
            Assert.That(delta, Is.LessThan(1));

            var tooShort = allResults.Where(r => r.Item1.Length < r.Item2);
            var tooLong = allResults.Where(r => r.Item1.Length > r.Item3);
            var alphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var invalidCharacters = allResults.Where(r => r.Item1.Any(c => !alphaNumericChars.Contains(c)));
            Assert.IsFalse(tooShort.Any() && tooLong.Any() && invalidCharacters.Any(), BuildErrorMessageFor(tooShort, tooLong, invalidCharacters));
        }

        [Test]
        public void GetRandomNumericString_ShouldProduceRandomStringWithOnlyNumericCharacters()
        {
            var allResults = new List<Tuple<string, int, int>>();
            RunCycles(() =>
            {
                //---------------Set up test pack-------------------
                var minLength = RandomValueGen.GetRandomInt(1, 50);
                var maxLength = RandomValueGen.GetRandomInt(minLength, minLength + 50);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = RandomValueGen.GetRandomNumericString(minLength, maxLength);

                allResults.Add(Tuple.Create(result, minLength, maxLength));

            });
            //---------------Test Result -----------------------
            CollectionAssert.IsNotEmpty(allResults);
            // collisions are possible, but should occur < 1%
            var total = allResults.Count();
            var unique = allResults.Select(o => o.Item1).Distinct().Count();
            var delta = (decimal) (total - unique)/(decimal) total;
            Assert.That(delta, Is.LessThan(1));

            var tooShort = allResults.Where(r => r.Item1.Length < r.Item2);
            var tooLong = allResults.Where(r => r.Item1.Length > r.Item3);
            var alphaNumericChars = "1234567890";
            var invalidCharacters = allResults.Where(r => r.Item1.Any(c => !alphaNumericChars.Contains(c)));
            Assert.IsFalse(tooShort.Any() && tooLong.Any() && invalidCharacters.Any(), BuildErrorMessageFor(tooShort, tooLong, invalidCharacters));
        }

        [Test]
        public void GetAnother_GivenOriginalValueAndGenerator_WhenCanGenerateNewValue_ShouldReturnANewValue()
        {
            RunCycles(() =>
            {
                //---------------Set up test pack-------------------
                var notThis = RandomValueGen.GetRandomString(1,1);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = RandomValueGen.GetAnother(notThis, () => RandomValueGen.GetRandomString(1, 1));

                //---------------Test Result -----------------------
                Assert.AreNotEqual(notThis, result);
            });
        }

        [Test]
        public void GetAnother_GivenOriginalValueAndNoGenerator_WhenCanGenerateNewValue_ShouldReturnANewValue()
        {
            RunCycles(() =>
            {
                //---------------Set up test pack-------------------
                var notThis = RandomValueGen.GetRandomString(1,1);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = RandomValueGen.GetAnother(notThis);

                //---------------Test Result -----------------------
                Assert.AreNotEqual(notThis, result);
            });
        }

        [Test]
        public void GetAnother_GivenOriginalValueAndGenerator_WhenCannotGenerateNewValue_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var notThis = RandomValueGen.GetRandomString(1, 1);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<CannotGetAnotherDifferentRandomValueException<string>>(() => RandomValueGen.GetAnother(notThis, () => notThis));

            //---------------Test Result -----------------------

        }

        [Test]
        public void GetAnother_GivenOriginalValueAndGenerator_WhenCannotGenerateNewValueBecauseOfComparisonFunc_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var notThis = RandomValueGen.GetRandomString(1, 1);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<CannotGetAnotherDifferentRandomValueException<string>>(() => RandomValueGen.GetAnother(notThis, 
                                                                                            () => RandomValueGen.GetRandomString(),
                                                                                            (left, right) => true));

            //---------------Test Result -----------------------

        }

        [Test]
        public void GetAnother_GivenOriginalValueCollectionAndGenerator_WhenCanGenerateNewValue_ShouldReturnThatValue()
        {
            RunCycles(() =>
            {
                //---------------Set up test pack-------------------
                var notThis = "abcdefghijklmnopqrstuvwABCDEFGHIJKLMNOPQRSTUVW".ToCharArray().Select(c => c.ToString());

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = RandomValueGen.GetAnother(notThis, () => RandomValueGen.GetRandomString(1, 1));

                //---------------Test Result -----------------------
                Assert.IsFalse(notThis.Any(i => i == result));
            });
        }

        [Test]
        public void GetAnother_GivenOriginalValueCollectionAndNoGenerator_WhenCanGenerateNewValue_ShouldReturnThatValue()
        {
            RunCycles(() =>
            {
                //---------------Set up test pack-------------------
                var notThis = "abcdefghijklmnopqrstuvwABCDEFGHIJKLMNOPQRSTUVW".ToCharArray().Select(c => c.ToString());

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = RandomValueGen.GetAnother(notThis);

                //---------------Test Result -----------------------
                Assert.IsFalse(notThis.Any(i => i == result));
            });
        }


        [Test]
        public void GetAnother_GivenOriginalValueCollectionAndGenerator_WhenCannotGenerateNewValueBecauseOfComparisonFunc_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var notAnyOfThese = RandomValueGen.GetRandomCollection(() => RandomValueGen.GetRandomString(), 2);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<CannotGetAnotherDifferentRandomValueException<IEnumerable<string>>>(() => RandomValueGen.GetAnother(notAnyOfThese, 
                                                                                            () => RandomValueGen.GetRandomString(),
                                                                                            (left, right) => true));

            //---------------Test Result -----------------------

        }

        [Test]
        public void GetAnother_GivenNullValue_ShouldReturnValueFromGenerator()
        {
            //---------------Set up test pack-------------------
            var strings = new Stack<string>();
            var expected = RandomValueGen.GetRandomString();
            var unexpected = RandomValueGen.GetAnother(expected, () => RandomValueGen.GetRandomString());
            strings.Push(unexpected);
            strings.Push(expected);
            strings.Push(null);


            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = RandomValueGen.GetAnother((string)null, () => strings.Pop());

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }


        public class SomePOCO
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            public DateTime? Date { get; set; }
        }

        [Test]
        public void GetRandom_GivenPOCOType_ShouldUseOnTheFlyGenericBuilderToGiveBackRandomItem()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var item = RandomValueGen.GetRandom<SomePOCO>();

            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            Assert.IsInstanceOf<SomePOCO>(item);
            // assert that *something* was set
            Assert.IsNotNull(item.Id);
            Assert.IsNotNull(item.Name);
            Assert.IsNotNull(item.Date);
        }

        [Test]
        public void GetRandomValue_GivenPOCOTypeArgument_ShouldUseOnTheFlyGenericBuilderToGiveBackRandomItem()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var item = RandomValueGen.GetRandomValue(typeof(SomePOCO)) as SomePOCO;

            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            Assert.IsInstanceOf<SomePOCO>(item);
            // assert that *something* was set
            Assert.IsNotNull(item.Id);
            Assert.IsNotNull(item.Name);
            Assert.IsNotNull(item.Date);
        }

        [TestCase(typeof(int))]
        [TestCase(typeof(byte))]
        [TestCase(typeof(char))]
        [TestCase(typeof(long))]
        [TestCase(typeof(float))]
        [TestCase(typeof(double))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(string))]
        [TestCase(typeof(bool))]
        public void GetRandomValue_GivenPrimitiveTypeArgument_ShouldUseRegularRVGMethods(Type type)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var item = RandomValueGen.GetRandomValue(type);

            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            Assert.IsInstanceOf(type, item);
        }

        [TestCase(typeof(int?))]
        [TestCase(typeof(byte?))]
        [TestCase(typeof(char?))]
        [TestCase(typeof(long?))]
        [TestCase(typeof(float?))]
        [TestCase(typeof(double?))]
        [TestCase(typeof(decimal?))]
        [TestCase(typeof(DateTime?))]
        [TestCase(typeof(bool?))]
        public void GetRandomValue_GivenNullablePrimitiveTypeArgument_ShouldUseRegularRVGMethods(Type type)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var item = RandomValueGen.GetRandomValue(type);

            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            Assert.IsInstanceOf(type, item);
        }


        [Test]
        public void GetRandomValue_GivenPOCOType_ShouldHaveVariance()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var items = RandomValueGen.GetRandomCollection(RandomValueGen.GetRandom<SomePOCO>, RANDOM_TEST_CYCLES, RANDOM_TEST_CYCLES);

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant<SomePOCO,int>(items, "Id");
            VarianceAssert.IsVariant<SomePOCO,string>(items, "Name");
            VarianceAssert.IsVariant<SomePOCO,DateTime>(items, "Date");
        }

        public class SomePOCOWithBuilder: SomePOCO
        {
        }
        public class SomePOCOWithBuilderBuilder: GenericBuilder<SomePOCOWithBuilderBuilder, SomePOCOWithBuilder>
        {
            public override SomePOCOWithBuilderBuilder WithRandomProps()
            {
                return base.WithRandomProps()
                        .WithProp(o => o.Id = RandomValueGen.GetRandomInt(1000, 2000));
            }
        }

        [Test]
        public void GetRandomValue_GivenPOCOWithBuilderType_ShouldUseExistingBuilder()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var item = RandomValueGen.GetRandom<SomePOCOWithBuilder>();

            //---------------Test Result -----------------------
            Assert.IsNotNull(item);
            Assert.IsInstanceOf<SomePOCOWithBuilder>(item);
            // assert that *something* was set
            Assert.IsNotNull(item.Id);
            Assert.That(item.Id.Value, Is.GreaterThanOrEqualTo(1000));
            Assert.That(item.Id.Value, Is.LessThanOrEqualTo(2000));
            Assert.IsNotNull(item.Name);
            Assert.IsNotNull(item.Date);
        }

        [Test]
        [Ignore("Discovery purposes only; answer is that non-printables are fine")]
        public void Discovery_WhatDoesEncodingGetString_DoWithNonPrintableCharacters()
        {
            //---------------Set up test pack-------------------
            var bytes = RandomValueGen.GetRandomCollection(() => RandomValueGen.GetRandomInt(0, 255))
                .Select(i => (byte) i)
                .ToArray();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = Encoding.UTF8.GetString(bytes);

            //---------------Test Result -----------------------
            Console.WriteLine(result);

        }


        [Test]
        public void GetRandomAlphaString_GivenMinLength_ShouldReturnValueOfAtLeastThatLength()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() =>
            {
                var minLength = RandomValueGen.GetRandomInt(10, 20);
                var result = RandomValueGen.GetRandomAlphaNumericString(minLength);
                Assert.That(result.Length, Is.GreaterThanOrEqualTo(minLength));
            });

            //---------------Test Result -----------------------
        }

        [Test]
        public void GetRandomBytes_ShouldReturnBytesAcrossFullRange()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            for (var i = 0; i < 20; i++)
            {
                // look for full-range variance across an 8k block
                var result = RandomValueGen.GetRandomBytes(8192, 8192);
                if (result.Distinct().Count() == 256)
                    return;
            }
            //---------------Test Result -----------------------
            Assert.Fail("Couldn't find full range of bytes");

        }

        [Test]
        public void GetRandomString_ShouldProduceStringInRequiredLengthRange()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() =>
            {
                var minLength = RandomValueGen.GetRandomInt(10, 20);
                var maxLength = RandomValueGen.GetRandomInt(21, 30);
                var result = RandomValueGen.GetRandomString(minLength, maxLength);
                Assert.That(result.Length, Is.GreaterThanOrEqualTo(minLength));
                Assert.That(result.Length, Is.LessThanOrEqualTo(maxLength));
            });

            //---------------Test Result -----------------------
        }

        [Test]
        public void GetRandomString_GivenMinAndMaxLengthsSwapped_ShouldProduceStringWithinRequiredLengthRange()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            RunCycles(() =>
            {
                var minLength = RandomValueGen.GetRandomInt(10, 20);
                var maxLength = RandomValueGen.GetRandomInt(21, 30);
                var result = RandomValueGen.GetRandomString(maxLength, minLength);
                Assert.That(result.Length, Is.GreaterThanOrEqualTo(minLength));
                Assert.That(result.Length, Is.LessThanOrEqualTo(maxLength));
            });

            //---------------Test Result -----------------------
        }

        [Test]
        public void GetRandomIPv4Address_ShouldReturnValidIPV4Addresses()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var allResults = new List<string>();
            RunCycles(() =>
            {
                var result = RandomValueGen.GetRandomIPv4Address();
                allResults.Add(result);
                var parts = result.Split('.');
                Assert.AreEqual(4, parts.Length);
                var ints = parts.Select(int.Parse);
                Assert.IsTrue(ints.All(i => i >= 0 && i < 265));
            });

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(allResults);
        }

        [Test]
        public void GetRandomHostName_ShouldReturnRandomHostName()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var allResults = new List<string>();
            RunCycles(() =>
            {
                var result = RandomValueGen.GetRandomHostname();
                Assert.IsNotNull(result);
                var parts = result.Split('.');
                Assert.IsTrue(parts.All(p => p.Length > 0));
            });
            
            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(allResults);
        }

        public void GetRandomVersionString_GivenNoParameters_ShouldReturnVersionWithThreeIntegerParts()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var allResults = new List<string>();
            RunCycles(() =>
            {
                var result = RandomValueGen.GetRandomVersionString();
                var parts = result.Split('.');
                Assert.AreEqual(3, parts.Length);
                Assert.IsTrue(parts.All(p => p.IsInteger()));
            });

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(allResults);
        }

        [Test]
        public void GetRandomVersionString_GivenPartsCount_ShouldReturnVersionWithThatManyParts()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var allResults = new List<string>();
            RunCycles(() =>
            {
                var partCount = RandomValueGen.GetRandomInt(2, 7);
                var result = RandomValueGen.GetRandomVersionString(partCount);
                var parts = result.Split('.');
                Assert.AreEqual(partCount, parts.Length);
                Assert.IsTrue(parts.All(p => p.IsInteger()));
            });

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(allResults);
        }

        [Test]
        public void GetRandomVersion_ShouldReturnRandomDotNetVersionInfo()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var allResults = new List<Version>();
            RunCycles(() =>
            {
                var result = RandomValueGen.GetRandomVersion();
                allResults.Add(result);
            });

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(allResults);
        }

        [Test]
        public void GetRandomFoldername_ShouldProduceVariantPath()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var allResults = new List<string>();
            RunCycles(() =>
            {
                var thisResult = RandomValueGen.GetRandomWindowsPath();
                var parts = thisResult.Split('\\');
                Assert.That(parts.Length, Is.GreaterThan(1));
                Assert.That(parts.Length, Is.LessThan(6));
                Assert.That(thisResult.Length, Is.LessThan(248));
                Assert.That(parts[0].Length == 2);
                Assert.That(parts[0].EndsWith(":"));
                StringAssert.Contains(parts[0].First().ToString(), "ABCDEGHIJKLMNOPQRSTUVWXYZ");
            });

            //---------------Test Result -----------------------
            VarianceAssert.IsVariant(allResults);
        }



        private string BuildErrorMessageFor(IEnumerable<Tuple<string, int, int>> tooShort, IEnumerable<Tuple<string, int, int>> tooLong, IEnumerable<Tuple<string, int, int>> invalidCharacters)
        {
            var message = new List<string>();
            if (tooShort.Any())
            {
                message.Add(string.Join("\n",
                    "Some results were too short:",
                    string.Join("\n\t", tooShort.Take(5).Select(i => $"{i.Item1}  (<{i.Item2})"))));
            }
            if (tooLong.Any())
            {
                message.Add(string.Join("\n",
                    "Some results were too long:",
                    string.Join("\n\t", tooLong.Take(5).Select(i => $"{i.Item1}  (>{i.Item3})"))));
            }
            if (invalidCharacters.Any())
            {
                message.Add(string.Join("\n",
                    "Some results contained invalid characters:",
                    string.Join("\n\t", invalidCharacters.Take(5).Select(i => i.Item1))));
            }
            return message.JoinWith("\n");
        }


        private void RunCycles(Action toRun)
        {
            for (var i = 0; i < RANDOM_TEST_CYCLES; i++)
                toRun();
        }

        private string GetErrorHelpFor(IEnumerable<DateTime> outOfRangeLeft, 
            IEnumerable<DateTime> outOfRangeRight,
            DateTime minTime,
            DateTime maxTime)
        {
            var message = "";
            if (outOfRangeLeft.Any())
            {
                message = string.Join("\n", new[]
                {
                    "One or more results had a time that was too early:",
                    "minTime: " + minTime.ToString("yyyy/MM/dd HH:mm:ss.ttt"),
                    "bad values: " + string.Join(",", outOfRangeLeft.Take(5))
                });
            }
            if (outOfRangeRight.Any())
            {
                message += string.Join("\n", new[]
                {
                    "One or more results had a time that was too late:",
                    "maxTime: " + maxTime.ToString("yyyy/MM/dd HH:mm:ss.ttt"),
                    "bad values: " + string.Join(",", outOfRangeLeft.Take(5))
                });
            }
            return message;
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
    }
}
