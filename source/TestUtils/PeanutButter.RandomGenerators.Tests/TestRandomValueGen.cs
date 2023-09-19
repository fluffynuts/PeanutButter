using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using GenericBuilderTestArtifactBuilders;
using GenericBuilderTestArtifactEntities;
using NExpect;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using NUnit.Framework;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;
using static PeanutButter.Utils.PyLike;
using TimeSpan = System.TimeSpan;
using static PeanutButter.RandomGenerators.Tests.RandomTestCycles;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestRandomValueGen
    {
        [TestFixture]
        public class GetRandomInt
        {
            [TestCase(
                1,
                100
            )]
            [TestCase(
                101,
                250
            )]
            [TestCase(
                4,
                10
            )]
            public void GivenRangeOfIntegers_ReturnsRandomIntWithinRange(
                int min,
                int max
            )
            {
                //---------------Set up test pack-------------------
                var ints = new List<int>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => ints.Add(
                        GetRandomInt(
                            min,
                            max
                        )
                    ),
                    HIGH_RANDOM_TEST_CYCLES
                );

                //---------------Test Result -----------------------
                Expect(ints.All(i => i >= min))
                    .To.Be.True(() => $"Numbers < min {min}: {ints.Where(i => i < min).JoinWith(",")}");
                Expect(ints.All(i => i <= max))
                    .To.Be.True(() => $"Numbers > max {max}: {ints.Where(i => i > max).JoinWith(",")}");
                Expect(ints.Distinct().Count() > 1)
                    .To.Be.True(() => $"Distinct count of numbers: {ints.Distinct().Count()}");
            }

            [Test]
            public void GivenRange_ShouldFindMinNumber()
            {
                // Arrange
                var min = GetRandomInt(
                    1,
                    10
                );
                var max = GetRandomInt(
                    11,
                    20
                );
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    var result = GetRandomInt(
                        min,
                        max
                    );
                    if (result == min)
                    {
                        Assert.Pass();
                        return;
                    }
                }

                // Assert
                Assert.Fail($"Unable to find {min} in range {min} - {max} over {HIGH_RANDOM_TEST_CYCLES} attempts");
            }

            [Test]
            public void GetRandomInt_GivenRange_ShouldFindMaxNumber()
            {
                // Arrange
                var min = GetRandomInt(
                    1,
                    10
                );
                var max = GetRandomInt(
                    11,
                    20
                );
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    var result = GetRandomInt(
                        min,
                        max
                    );
                    if (result == max)
                    {
                        Assert.Pass();
                        return;
                    }
                }

                // Assert
                Assert.Fail($"Unable to find {max} in range {min} - {max} over {HIGH_RANDOM_TEST_CYCLES} attempts");
            }
        }

        [TestFixture]
        public class GetRandomDouble
        {
            [Test]
            public void ShouldProvideSpreadWithinRange()
            {
                // Arrange
                var min = GetRandomDouble(
                    1,
                    10
                );
                var max = GetRandomDouble(
                    11,
                    20
                );
                var collected = new List<double>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomDouble(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
            }
        }

        [TestFixture]
        public class GetRandomDecimal
        {
            [Test]
            public void ShouldProvideSpreadWithinRange()
            {
                // Arrange
                var min = GetRandomDecimal(
                    1,
                    10
                );
                var max = GetRandomDecimal(
                    11,
                    20
                );
                var collected = new List<decimal>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomDecimal(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
            }
        }

        [TestFixture]
        public class GetRandomMoney
        {
            [Test]
            public void ShouldProvideSpreadWithinRange()
            {
                // Arrange
                var min = GetRandomMoney(
                    1,
                    10
                );
                var max = GetRandomMoney(
                    11,
                    20
                );
                var collected = new List<decimal>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomMoney(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
                Expect(collected)
                    .To.Contain.All
                    .Matched.By(HasMaxTwoDecimalPlaces);

                bool HasMaxTwoDecimalPlaces(
                    decimal value
                )
                {
                    var str = value.ToString();
                    var parts = str.Split('.');
                    if (parts.Length < 2)
                    {
                        return (int)value == value;
                    }

                    return parts[1].Length <= 2;
                }
            }

            [TestCase(
                10,
                100
            )]
            public void ShouldDefaultToBeWithinRange_(
                int min,
                int max
            )
            {
                // Arrange
                var collected = new List<decimal>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomMoney(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
            }
        }

        [TestFixture]
        public class GetRandomTaxRate
        {
            [Test]
            public void ShouldProvideSpreadWithinRange()
            {
                // Arrange
                var min = GetRandomTaxRate(
                    1,
                    10
                );
                var max = GetRandomTaxRate(
                    11,
                    20
                );
                var collected = new List<decimal>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomTaxRate(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
                Expect(collected)
                    .To.Contain.All
                    .Matched.By(HasMaxTwoDecimalPlaces);

                bool HasMaxTwoDecimalPlaces(
                    decimal value
                )
                {
                    var str = value.ToString();
                    var parts = str.Split('.');
                    if (parts.Length < 2)
                    {
                        return (int)value == value;
                    }

                    return parts[1].Length <= 2;
                }
            }

            [TestCase(
                3,
                20
            )]
            public void ShouldDefaultToBeWithinRange_(
                int min,
                int max
            )
            {
                // Arrange
                var collected = new List<decimal>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomTaxRate(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
            }
        }

        [TestFixture]
        public class GetRandomInterestRate
        {
            [Test]
            public void ShouldProvideSpreadWithinRange()
            {
                // Arrange
                var min = GetRandomInterestRate(
                    1,
                    10
                );
                var max = GetRandomInterestRate(
                    11,
                    20
                );
                var collected = new List<decimal>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomInterestRate(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
                Expect(collected)
                    .To.Contain.All
                    .Matched.By(HasMaxTwoDecimalPlaces);

                bool HasMaxTwoDecimalPlaces(
                    decimal value
                )
                {
                    var str = value.ToString();
                    var parts = str.Split('.');
                    if (parts.Length < 2)
                    {
                        return (int)value == value;
                    }

                    return parts[1].Length <= 2;
                }
            }

            [TestCase(
                3,
                20
            )]
            public void ShouldDefaultToBeWithinRange_(
                int min,
                int max
            )
            {
                // Arrange
                var collected = new List<decimal>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomInterestRate(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
            }
        }

        [TestFixture]
        public class GetRandomFloat
        {
            [Test]
            public void ShouldProvideSpreadWithinRange()
            {
                // Arrange
                var min = GetRandomFloat(
                    1,
                    10
                );
                var max = GetRandomFloat(
                    11,
                    20
                );
                var collected = new List<float>();
                // Pre-assert
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(
                        GetRandomFloat(
                            min,
                            max
                        )
                    );
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(d => d >= min);
                Expect(collected).To.Contain.All
                    .Matched.By(d => d <= max);
            }
        }

        [TestFixture]
        public class GetRandomTimeOfDay
        {
            [Test]
            public void ShouldReturnTimespanWithin24HourRange()
            {
                // Arrange
                var collected = new List<TimeSpan>();
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(GetRandomTimeOfDay());
                }

                // Assert
                Expect(collected).To.Contain.All
                    .Matched.By(t => t >= TimeSpan.Zero);
                var oneDay = TimeSpan.FromSeconds(86400);
                Expect(collected).To.Contain.All
                    .Matched.By(t => t <= oneDay);
            }
        }

        [TestFixture]
        public class GetRandomLong
        {
            [TestCase(
                1,
                100
            )]
            [TestCase(
                101,
                250
            )]
            public void GivenRangeOfIntegers_ReturnsRandomIntWithinRange(
                int min,
                int max
            )
            {
                //---------------Set up test pack-------------------
                var ints = new List<long>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => ints.Add(
                        GetRandomLong(
                            min,
                            max
                        )
                    )
                );

                //---------------Test Result -----------------------
                Assert.IsTrue(ints.All(i => i >= min));
                Assert.IsTrue(ints.All(i => i <= max));
                Assert.IsTrue(ints.Distinct().Count() > 1);
            }
        }

        [TestFixture]
        public class GetRandomString
        {
            [TestCase(
                50,
                100
            )]
            [TestCase(
                150,
                400
            )]
            public void GivenLengthLimits_ReturnsRandomStringsWithinLengthRange(
                int minLength,
                int maxLength
            )
            {
                //---------------Set up test pack-------------------
                var strings = new List<string>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => strings.Add(
                        GetRandomString(
                            minLength,
                            maxLength
                        )
                    )
                );

                //---------------Test Result -----------------------
                Assert.IsTrue(strings.All(s => s.Length >= minLength));
                Assert.IsTrue(strings.All(s => s.Length <= maxLength));
                Assert.IsTrue(strings.Distinct().Count() > 1);
            }

            [Test]
            public void ShouldProduceStringInRequiredLengthRange()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var minLength = GetRandomInt(
                            10,
                            20
                        );
                        var maxLength = GetRandomInt(
                            21,
                            30
                        );
                        var result = GetRandomString(
                            minLength,
                            maxLength
                        );
                        Assert.That(
                            result.Length,
                            Is.GreaterThanOrEqualTo(minLength)
                        );
                        Assert.That(
                            result.Length,
                            Is.LessThanOrEqualTo(maxLength)
                        );
                    }
                );

                //---------------Test Result -----------------------
            }

            [Test]
            public void GivenMinAndMaxLengthsSwapped_ShouldProduceStringWithinRequiredLengthRange()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var minLength = GetRandomInt(
                            10,
                            20
                        );
                        var maxLength = GetRandomInt(
                            21,
                            30
                        );
                        var result = GetRandomString(
                            maxLength,
                            minLength
                        );
                        Assert.That(
                            result.Length,
                            Is.GreaterThanOrEqualTo(minLength)
                        );
                        Assert.That(
                            result.Length,
                            Is.LessThanOrEqualTo(maxLength)
                        );
                    }
                );

                //---------------Test Result -----------------------
            }
        }


        private enum TestEnum
        {
            One,
            Two,
            Three
        }

        [TestFixture]
        public class GetRandomEnum
        {
            [Retry(5)] // can fail when the odds are really against us
            public void ShouldReturnRandomValueFromEnumSelection()
            {
                //---------------Set up test pack-------------------
                var results = new List<TestEnum>();
                var type = typeof(TestEnum);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------

                RunCycles(() => results.Add((TestEnum)GetRandomEnum(type)));
                //---------------Test Result -----------------------
                var runs = results.Count;
                var onePercent = (100 * results.Count(i => i == TestEnum.One)) / runs;
                var twoPercent = (100 * results.Count(i => i == TestEnum.Two)) / runs;
                var threePercent = (100 * results.Count(i => i == TestEnum.Three)) / runs;

                var d1 = Math.Abs(twoPercent - onePercent);
                var d2 = Math.Abs(threePercent - twoPercent);
                var d3 = Math.Abs(threePercent - onePercent);

                Assert.That(
                    d1,
                    Is.LessThan(30)
                );
                Assert.That(
                    d2,
                    Is.LessThan(30)
                );
                Assert.That(
                    d3,
                    Is.LessThan(30)
                );
            }
        }

        [TestFixture]
        public class GetRandomFrom
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnARandomItemFromTheListCollection()
            {
                //---------------Set up test pack-------------------
                var o1 = new object();
                var o2 = new object();
                var o3 = new object();
                var items = new List<object>
                {
                    o1,
                    o2,
                    o3
                };
                var results = new List<object>();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                // warm up
                var _ = GetRandomFrom(items);
                var time = Benchmark.Time(
                    () =>
                    {
                        for (var i = 0; i < RIDICULOUS_RANDOM_TEST_CYCLES; i++)
                        {
                            results.Add(GetRandomFrom(items));
                        }
                    }
                );

                Console.Error.WriteLine(time);

                //---------------Test Result -----------------------
                Assert.IsTrue(results.All(r => items.Contains(r)));
                Assert.IsTrue(items.All(i => results.Any(r => r == i)));
            }

            [Test]
            [Parallelizable]
            public void ShouldReturnARandomItemFromTheArrayCollection()
            {
                //---------------Set up test pack-------------------
                var o1 = new object();
                var o2 = new object();
                var o3 = new object();
                var items = new[]
                {
                    o1,
                    o2,
                    o3
                };
                var results = new List<object>();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                // warm up
                var _ = GetRandomFrom(items);
                var time = Benchmark.Time(
                    () =>
                    {
                        for (var i = 0; i < RIDICULOUS_RANDOM_TEST_CYCLES; i++)
                        {
                            results.Add(GetRandomFrom(items));
                        }
                    }
                );

                Console.Error.WriteLine(time);

                //---------------Test Result -----------------------
                Assert.IsTrue(results.All(r => items.Contains(r)));
                Assert.IsTrue(items.All(i => results.Any(r => r == i)));
            }

            [Test]
            [Parallelizable]
            public void ShouldReturnARandomItemFromTheEnumerableCollection()
            {
                //---------------Set up test pack-------------------
                var o1 = new object();
                var o2 = new object();
                var o3 = new object();
                var items = new ConcurrentBag<object>(
                    new[]
                    {
                        o1,
                        o2,
                        o3
                    }
                );
                var results = new List<object>();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                // warm up
                var _ = GetRandomFrom(items);
                var time = Benchmark.Time(
                    () =>
                    {
                        for (var i = 0; i < RIDICULOUS_RANDOM_TEST_CYCLES; i++)
                        {
                            results.Add(GetRandomFrom(items));
                        }
                    }
                );

                Console.Error.WriteLine(time);

                //---------------Test Result -----------------------
                Assert.IsTrue(results.All(r => items.Contains(r)));
                Assert.IsTrue(items.All(i => results.Any(r => r == i)));
            }

            [Test]
            [Parallelizable]
            public void ShouldReturnARandomItemFromTheLazyEnumerableCollection()
            {
                //---------------Set up test pack-------------------
                var o1 = new object();
                var o2 = new object();
                var o3 = new object();
                var items = new[]
                {
                    o1
                }.Union(
                    new[]
                    {
                        o2,
                        o3
                    }
                );
                var results = new List<object>();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                // warm up
                var _ = GetRandomFrom(items);
                var time = Benchmark.Time(
                    () =>
                    {
                        for (var i = 0; i < RIDICULOUS_RANDOM_TEST_CYCLES; i++)
                        {
                            results.Add(GetRandomFrom(items));
                        }
                    }
                );

                Console.Error.WriteLine(time);

                //---------------Test Result -----------------------
                Assert.IsTrue(results.All(r => items.Contains(r)));
                Assert.IsTrue(items.All(i => results.Any(r => r == i)));
            }

            [Test]
            [Parallelizable]
            public void ParamsSignatureForPrettyCode()
            {
                // Arrange
                // Act
                var result = OneOf(
                    1,
                    2,
                    3
                );
                // Assert
                Expect(
                        new[]
                        {
                            1,
                            2,
                            3
                        }
                    )
                    .To.Contain(result);
            }
        }

        [TestFixture]
        public class GetRandomSelectionFrom
        {
            [Test]
            public void ShouldReturnARandomCollectionFromTheOriginalCollection()
            {
                //---------------Set up test pack-------------------
                var o1 = new object();
                var o2 = new object();
                var o3 = new object();
                var items = new[]
                {
                    o1,
                    o2,
                    o3
                };
                var results = new List<IEnumerable<object>>();
                const int runs = NORMAL_RANDOM_TEST_CYCLES;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < runs; i++)
                {
                    results.Add(GetRandomSelectionFrom(items));
                }

                //---------------Test Result -----------------------
                var flattened = results.SelectMany(
                    r =>
                    {
                        var collections = r as object[] ?? r.ToArray();
                        return collections;
                    }
                );
                Assert.IsTrue(flattened.All(f => items.Contains(f)));
                var averageCount = results.Select(r => r.Count()).Average();
                Assert.That(
                    averageCount,
                    Is.GreaterThan(1)
                );
                Assert.That(
                    averageCount,
                    Is.LessThan(items.Length)
                );
            }

            [Test]
            public void ShouldNotRepeatItems()
            {
                //---------------Set up test pack-------------------
                var o1 = new object();
                var o2 = new object();
                var o3 = new object();
                var items = new[]
                {
                    o1,
                    o2,
                    o3
                };
                const int runs = NORMAL_RANDOM_TEST_CYCLES;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < runs; i++)
                {
                    var result = GetRandomSelectionFrom(items);
                    //---------------Test Result -----------------------
                    CollectionAssert.AreEqual(
                        result,
                        result.Distinct()
                    );
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
                var items = new[]
                {
                    o1,
                    o2,
                    o3,
                    o4,
                    o5,
                    o6
                };
                var min = GetRandomInt(
                    1,
                    3
                );
                var max = GetRandomInt(
                    3,
                    items.Length
                );
                const int runs = NORMAL_RANDOM_TEST_CYCLES;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < runs; i++)
                {
                    var result = GetRandomSelectionFrom(
                        items,
                        min,
                        max
                    );
                    //---------------Test Result -----------------------
                    Assert.That(
                        result.Count(),
                        Is.GreaterThanOrEqualTo(min)
                    );
                    Assert.That(
                        result.Count(),
                        Is.LessThanOrEqualTo(max)
                    );
                }
            }
        }

        [TestFixture]
        public class GetRandomArray
        {
            [Test]
            public void GivenFactoryFunction_ShouldInvokeItToCreateItems()
            {
                //---------------Set up test pack-------------------
                const int runs = NORMAL_RANDOM_TEST_CYCLES;
                var generatedValues = new List<int>();
                Func<int> factory = () =>
                {
                    var thisValue = GetRandomInt();
                    generatedValues.Add(thisValue);
                    return thisValue;
                };
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < runs; i++)
                {
                    var result = GetRandomArray(factory);
                    //---------------Test Result -----------------------
                    CollectionAssert.AreEqual(
                        generatedValues,
                        result
                    );
                    generatedValues.Clear();
                }
            }

            [Test]
            public void GenericInvoke_ShouldUseNinjaSuperPowersToCreateArray()
            {
                //---------------Set up test pack-------------------
                var minItems = GetRandomInt(5);
                var maxItems = GetRandomInt(
                    11,
                    20
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomArray<SomePOCO>(
                    minItems,
                    maxItems
                );

                //---------------Test Result -----------------------
                result.ForEach(o => Console.WriteLine(o.ToString()));
                Assert.IsNotNull(result);
                CollectionAssert.IsNotEmpty(result);
                Assert.IsTrue(result.All(r => r != null));
                Assert.IsTrue(result.All(r => r.GetType() == typeof(SomePOCO)));
                VarianceAssert.IsVariant<SomePOCO, int>(
                    result,
                    "Id"
                );
                VarianceAssert.IsVariant<SomePOCO, string>(
                    result,
                    "Name"
                );
                VarianceAssert.IsVariant<SomePOCO, DateTime>(
                    result,
                    "Date"
                );
            }
        }

        [TestFixture]
        public class GetRandomList
        {
            [Test]
            public void GivenFactoryFunction_ShouldInvokeItToCreateItems()
            {
                //---------------Set up test pack-------------------
                const int runs = NORMAL_RANDOM_TEST_CYCLES;
                var generatedValues = new List<int>();
                Func<int> factory = () =>
                {
                    var thisValue = GetRandomInt();
                    generatedValues.Add(thisValue);
                    return thisValue;
                };
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < runs; i++)
                {
                    var result = GetRandomList(factory);
                    //---------------Test Result -----------------------
                    CollectionAssert.AreEqual(
                        generatedValues,
                        result
                    );
                    generatedValues.Clear();
                }
            }

            [Test]
            public void GenericInvoke_ShouldUseNinjaSuperPowersToCreateList()
            {
                //---------------Set up test pack-------------------
                var minItems = GetRandomInt(5);
                var maxItems = GetRandomInt(
                    11,
                    20
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomList<SomePOCO>(
                    minItems,
                    maxItems
                );

                //---------------Test Result -----------------------
                result.ForEach(o => Console.WriteLine(o.ToString()));
                Assert.IsNotNull(result);
                CollectionAssert.IsNotEmpty(result);
                Assert.IsTrue(result.All(r => r != null));
                Assert.IsTrue(result.All(r => r.GetType() == typeof(SomePOCO)));
                VarianceAssert.IsVariant<SomePOCO, int>(
                    result,
                    "Id"
                );
                VarianceAssert.IsVariant<SomePOCO, string>(
                    result,
                    "Name"
                );
                VarianceAssert.IsVariant<SomePOCO, DateTime>(
                    result,
                    "Date"
                );
            }
        }

        [TestFixture]
        public class FillingInNaturalValues
        {
            [Test]
            public void ShouldFillComplementaryValues()
            {
                // Arrange
                // Act
                var result = GetRandom<SomePOCO>();
                // Assert
                Console.WriteLine(result);
                Expect(result.Name)
                    .To.Contain(result.FirstName);
                Expect(result.Name)
                    .To.Contain(result.LastName);
                Expect(result.Login)
                    .To.Contain(result.FirstName.ToLower());
                Expect(result.Email)
                    .To.Contain(result.FirstName.ToLower());
            }
        }

        [TestFixture]
        public class GetRandomDate
        {
            [Test]
            public void ShouldReturnRandomDateTimeForDefaultCall()
            {
                // Arrange
                var collected = new List<DateTime>();
                // Act
                for (var i = 0; i < HIGH_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(GetRandomDate());
                }

                // Assert
                var unique = collected.Distinct();
                Expect(unique)
                    .To.Be.Equivalent.To(collected);
            }

            [TestCase(
                1984,
                4,
                4,
                2001,
                1,
                1
            )]
            [TestCase(
                1914,
                4,
                4,
                2011,
                1,
                1
            )]
            [TestCase(
                2001,
                4,
                4,
                2001,
                1,
                1
            )]
            public void GivenDateOnlyIsTrue_ShouldReturnDateTimeWithNoTimeComponent(
                int minYear,
                int minMonth,
                int minDay,
                int maxYear,
                int maxMonth,
                int maxDay
            )
            {
                //---------------Set up test pack-------------------
                var results = new List<DateTime>();
                var range = new DateTimeRangeContainer(
                    minYear,
                    minMonth,
                    minDay,
                    maxYear,
                    maxMonth,
                    maxDay
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => results.Add(
                        GetRandomDate(
                            range.From,
                            range.To,
                            true
                        )
                    )
                );

                //---------------Test Result -----------------------
                Assert.AreEqual(
                    NORMAL_RANDOM_TEST_CYCLES,
                    results.Count
                );
                Assert.IsTrue(
                    results.All(range.InRange),
                    "One or more generated value is out of range"
                );
                Assert.IsTrue(
                    results.All(d => d.Hour == 0),
                    "Hours are not all zeroed"
                );
                Assert.IsTrue(
                    results.All(d => d.Minute == 0),
                    "Minutes are not all zeroed"
                );
                Assert.IsTrue(
                    results.All(d => d.Second == 0),
                    "Seconds are not all zeroed"
                );
                Assert.IsTrue(
                    results.All(d => d.Millisecond == 0),
                    "Seconds are not all zeroed"
                );
            }

            [Test]
            public void GivenMinTimeOrMaxTime_AndDateOnlyIsTrue_ShouldIgnoreTheGivenTimes()
            {
                //---------------Set up test pack-------------------
                var ticksInOneDay = TimeSpan.FromDays(1).Ticks;
                var minTicks = GetRandomLong(
                    1,
                    ticksInOneDay - 1
                );
                var maxTicks = GetRandomLong(
                    minTicks,
                    ticksInOneDay - 1
                );
                var minTime = GetRandomDate().StartOfDay().AddTicks(minTicks);
                var maxTime = GetRandomDate().StartOfDay().AddTicks(maxTicks);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomDate(
                    dateOnly: true,
                    minTime: minTime,
                    maxTime: maxTime
                );

                //---------------Test Result -----------------------
                Assert.AreEqual(
                    result.StartOfDay(),
                    result
                );
            }

            [TestCase(
                1984,
                4,
                4,
                2001,
                1,
                1
            )]
            [TestCase(
                1914,
                4,
                4,
                2011,
                1,
                1
            )]
            [TestCase(
                2001,
                4,
                4,
                2001,
                1,
                1
            )]
            public void ShouldReturnDatesWithinRange(
                int minYear,
                int minMonth,
                int minDay,
                int maxYear,
                int maxMonth,
                int maxDay
            )
            {
                //---------------Set up test pack-------------------
                var results = new List<DateTime>();
                var range = new DateTimeRangeContainer(
                    minYear,
                    minMonth,
                    minDay,
                    maxYear,
                    maxMonth,
                    maxDay
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => results.Add(
                        GetRandomDate(
                            range.From,
                            range.To
                        )
                    )
                );

                //---------------Test Result -----------------------
                Assert.AreEqual(
                    NORMAL_RANDOM_TEST_CYCLES,
                    results.Count
                );
                Assert.IsTrue(
                    results.All(d => d >= range.From),
                    "One or more results is less than the minimum date"
                );
                Assert.IsTrue(
                    results.All(d => d <= range.To),
                    "One or more results is greater than the maximum date"
                );
                Assert.IsTrue(
                    results.All(d => d.Microseconds() == 0),
                    "Microseconds should be zeroed on random dates"
                );
            }

            [Test]
            public void GetRandomTimeOn_GivenDate_ShouldReturnRandomDateTimeOnThatDay()
            {
                //---------------Set up test pack-------------------
                var theDate = GetRandomDate();
                var min = new DateTime(
                    theDate.Year,
                    theDate.Month,
                    theDate.Day,
                    0,
                    0,
                    0
                );
                var max = new DateTime(
                    theDate.Year,
                    theDate.Month,
                    theDate.Day,
                    0,
                    0,
                    0
                );
                max = max.AddDays(1).AddMilliseconds(-1);
                var results = new List<DateTime>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(() => results.Add(GetRandomTimeOn(theDate)));

                //---------------Test Result -----------------------
                Assert.IsTrue(results.All(d => d >= min));
                Assert.IsTrue(results.All(d => d <= max));
            }

            [Test]
            public void GivenMinTime_ShouldProduceRandomDatesWithTimesGreaterOrEqual()
            {
                //---------------Set up test pack-------------------
                var minTime = new DateTime(
                    1900,
                    1,
                    1,
                    GetRandomInt(
                        0,
                        12
                    ),
                    GetRandomInt(
                        0,
                        59
                    ),
                    GetRandomInt(
                        0,
                        59
                    )
                );
                var maxTime = new DateTime(
                    minTime.Year,
                    minTime.Month,
                    minTime.Day,
                    0,
                    0,
                    0
                );
                maxTime = maxTime.AddDays(1).AddMilliseconds(-1);
                var results = new List<DateTime>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => results.Add(
                        GetRandomDate(
                            minTime: minTime,
                            maxTime: maxTime
                        )
                    )
                );

                //---------------Test Result -----------------------
                var outOfRangeLeft = results.Where(d => d.Ticks < minTime.Ticks).ToArray();
                var outOfRangeRight = results.Where(d => d.Ticks < maxTime.Ticks).ToArray();
                Assert.IsFalse(
                    outOfRangeLeft.Any() && outOfRangeRight.Any(),
                    GetErrorHelpFor(
                        outOfRangeLeft,
                        outOfRangeRight,
                        minTime,
                        maxTime
                    )
                );
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void GivenMinDateOnly_ShouldProduceRandomDatesWithin30YearPeriod()
            {
                //---------------Set up test pack-------------------
                var minDate = new DateTime(
                    GetRandomInt(
                        1,
                        3000
                    ),
                    GetRandomInt(
                        1,
                        12
                    ),
                    GetRandomInt(
                        1,
                        28
                    )
                );
                var maxDate = minDate.AddYears(30);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomDate(minDate: minDate);

                //---------------Test Result -----------------------
                Assert.That(
                    result,
                    Is.GreaterThanOrEqualTo(minDate)
                        .And.LessThanOrEqualTo(maxDate)
                );
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void GivenMaxDateOnly_ShouldProduceRandomDatesWithin30YearPeriod()
            {
                //---------------Set up test pack-------------------
                var maxDate = new DateTime(
                    GetRandomInt(
                        31,
                        3000
                    ),
                    GetRandomInt(
                        1,
                        12
                    ),
                    GetRandomInt(
                        1,
                        28
                    )
                );
                var minDate = maxDate.AddYears(-30);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomDate(maxDate: maxDate);

                //---------------Test Result -----------------------
                Assert.That(
                    result,
                    Is.GreaterThanOrEqualTo(minDate)
                        .And.LessThanOrEqualTo(maxDate)
                );
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void
                GivenMinAndMaxTimeWithFractionsOfSeconds_ShouldTruncateTimesToMilliseconds_AndProduceRandomDateWithTimeBetweenMinAndMax()
            {
                //---------------Set up test pack-------------------
                var r = GetRandomDate();
                var baseDateTime = new DateTime(
                    r.Year,
                    r.Month,
                    r.Day,
                    r.Hour,
                    r.Minute,
                    r.Second
                );
                var ticksInOneSecond = TimeSpan.FromSeconds(1).Ticks;
                var minTicks = GetRandomLong(
                    1,
                    ticksInOneSecond - 1
                );
                var maxTicks = GetRandomLong(
                    minTicks,
                    ticksInOneSecond - 1
                );
                var minTime = baseDateTime.AddTicks(minTicks);
                var maxTime = baseDateTime.AddTicks(maxTicks);
                var expectedMinTime = minTime.TruncateMicroseconds();
                var expectedMaxTime = maxTime.TruncateMicroseconds();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomDate(
                    minTime: minTime,
                    maxTime: maxTime
                );

                //---------------Test Result -----------------------
                Assert.That(
                    result.TimeOfDay,
                    Is.GreaterThanOrEqualTo(expectedMinTime.TimeOfDay)
                        .And.LessThanOrEqualTo(expectedMaxTime.TimeOfDay)
                );
            }

            [Test]
            public void GivenMaxTime_ShouldProduceRandomDatesWithTimesLessThanOrEqual()
            {
                //---------------Set up test pack-------------------
                var maxTime = new DateTime(
                    1900,
                    1,
                    1,
                    GetRandomInt(
                        12,
                        23
                    ),
                    GetRandomInt(
                        0,
                        59
                    ),
                    GetRandomInt(
                        0,
                        59
                    )
                );
                var results = new List<DateTime>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                        results.Add(GetRandomDate(maxTime: maxTime))
                );

                //---------------Test Result -----------------------
                var outOfRange = results
                    .Where(d => d.MillisecondsSinceStartOfDay() > maxTime.MillisecondsSinceStartOfDay())
                    .ToArray();
                Assert.IsFalse(
                    outOfRange.Any(),
                    $"One or more results had a time that was too late for {maxTime}.{Environment.NewLine}{Print(outOfRange)}"
                );
            }

            private string Print(
                DateTime[] outOfRange
            )
            {
                return string.Join(
                    Environment.NewLine,
                    outOfRange
                );
            }

            [Test]
            public void GivenMinDateTimeAndMaxDateTime_WhenDateOnlySpecified_ShouldReturnDateWithinRange()
            {
                //---------------Set up test pack-------------------
                var minDate = new DateTime(
                    2011,
                    1,
                    1,
                    23,
                    30,
                    0
                );
                var maxDate = new DateTime(
                    2011,
                    1,
                    2,
                    00,
                    30,
                    0
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomDate(
                            minDate,
                            maxDate,
                            true
                        );
                        Assert.AreEqual(
                            new DateTime(
                                2011,
                                1,
                                2,
                                0,
                                0,
                                0
                            ),
                            result
                        );
                    }
                );

                //---------------Test Result -----------------------
            }

            [Test]
            public void
                GivenMinDateTimeAndMaxDateTime_WhenDateOnlySpecified_AndMinMaxOnSameDay_ShouldGiveThatDay()
            {
                //---------------Set up test pack-------------------
                var minDate = new DateTime(
                    2011,
                    1,
                    1,
                    12,
                    00,
                    0
                );
                var maxDate = new DateTime(
                    2011,
                    1,
                    1,
                    12,
                    30,
                    0
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomDate(
                            minDate,
                            maxDate,
                            true
                        );
                        Assert.AreEqual(
                            new DateTime(
                                2011,
                                1,
                                1,
                                0,
                                0,
                                0
                            ),
                            result
                        );
                    }
                );

                //---------------Test Result -----------------------
            }
        }


        [TestFixture]
        public class GetRandomUtcDate
        {
            [TestCase(
                1984,
                4,
                4,
                2001,
                1,
                1
            )]
            [TestCase(
                1914,
                4,
                4,
                2011,
                1,
                1
            )]
            [TestCase(
                2001,
                4,
                4,
                2001,
                1,
                1
            )]
            public void GetRandomUtcDate_GivenDateOnlyIsTrue_ShouldReturnDateTimeWithNoTimeComponent(
                int minYear,
                int minMonth,
                int minDay,
                int maxYear,
                int maxMonth,
                int maxDay
            )
            {
                //---------------Set up test pack-------------------
                var results = new List<DateTime>();
                var range = new DateTimeRangeContainer(
                    minYear,
                    minMonth,
                    minDay,
                    maxYear,
                    maxMonth,
                    maxDay
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => results.Add(
                        GetRandomUtcDate(
                            range.From,
                            range.To,
                            true
                        )
                    )
                );

                //---------------Test Result -----------------------
                Expect(results).To.Contain.Only(NORMAL_RANDOM_TEST_CYCLES).Items();
                Expect(results).To.Contain.All
                    .Matched.By(dt => dt.Kind == DateTimeKind.Utc);
                Expect(results).To.Contain.All
                    .Matched.By(
                        range.InRange,
                        "One or more generated value is out of range"
                    );
                Expect(results).To.Contain.All
                    .Matched.By(
                        d => d.Hour == 0,
                        "Hours are not all zeroed"
                    );
                Expect(results).To.Contain.All
                    .Matched.By(
                        d => d.Minute == 0,
                        "Minutes are not all zeroed"
                    );
                Expect(results).To.Contain.All
                    .Matched.By(
                        d => d.Second == 0,
                        "Seconds are not all zeroed"
                    );
                Expect(results).To.Contain.All
                    .Matched.By(
                        d => d.Millisecond == 0,
                        "Milliseconds are not all zeroed"
                    );
            }

            [Test]
            public void GetRandomUtcDate_GivenMinTimeOrMaxTime_AndDateOnlyIsTrue_ShouldIgnoreTheGivenTimes()
            {
                //---------------Set up test pack-------------------
                var ticksInOneDay = TimeSpan.FromDays(1).Ticks;
                var minTicks = GetRandomLong(
                    1,
                    ticksInOneDay - 1
                );
                var maxTicks = GetRandomLong(
                    minTicks,
                    ticksInOneDay - 1
                );
                var minTime = GetRandomUtcDate().StartOfDay().AddTicks(minTicks);
                var maxTime = GetRandomUtcDate().StartOfDay().AddTicks(maxTicks);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDate(
                    dateOnly: true,
                    minTime: minTime,
                    maxTime: maxTime
                );

                //---------------Test Result -----------------------
                Assert.AreEqual(
                    result.StartOfDay(),
                    result
                );
            }

            [TestCase(
                1984,
                4,
                4,
                2001,
                1,
                1
            )]
            [TestCase(
                1914,
                4,
                4,
                2011,
                1,
                1
            )]
            [TestCase(
                2001,
                4,
                4,
                2001,
                1,
                1
            )]
            public void GetRandomUtcDate_ShouldReturnDatesWithinRange(
                int minYear,
                int minMonth,
                int minDay,
                int maxYear,
                int maxMonth,
                int maxDay
            )
            {
                //---------------Set up test pack-------------------
                var results = new List<DateTime>();
                var range = new DateTimeRangeContainer(
                    minYear,
                    minMonth,
                    minDay,
                    maxYear,
                    maxMonth,
                    maxDay
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => results.Add(
                        GetRandomUtcDate(
                            range.From,
                            range.To
                        )
                    )
                );

                //---------------Test Result -----------------------
                Assert.AreEqual(
                    NORMAL_RANDOM_TEST_CYCLES,
                    results.Count
                );
                Assert.IsTrue(
                    results.All(d => d >= range.From),
                    "One or more results is less than the minimum date"
                );
                Assert.IsTrue(
                    results.All(d => d <= range.To),
                    "One or more results is greater than the maximum date"
                );
                Assert.IsTrue(
                    results.All(d => d.Microseconds() == 0),
                    "Microseconds should be zeroed on random dates"
                );
            }

            [Test]
            public void GetRandomTimeOn_GivenDate_ShouldReturnRandomDateTimeOnThatDay()
            {
                //---------------Set up test pack-------------------
                var theDate = GetRandomUtcDate();
                var min = new DateTime(
                    theDate.Year,
                    theDate.Month,
                    theDate.Day,
                    0,
                    0,
                    0
                );
                var max = new DateTime(
                    theDate.Year,
                    theDate.Month,
                    theDate.Day,
                    0,
                    0,
                    0
                );
                max = max.AddDays(1).AddMilliseconds(-1);
                var results = new List<DateTime>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(() => results.Add(GetRandomTimeOn(theDate)));

                //---------------Test Result -----------------------
                Assert.IsTrue(results.All(d => d >= min));
                Assert.IsTrue(results.All(d => d <= max));
            }

            [Test]
            public void GetRandomUtcDate_GivenMinTime_ShouldProduceRandomDatesWithTimesGreaterOrEqual()
            {
                //---------------Set up test pack-------------------
                var minTime = new DateTime(
                    1900,
                    1,
                    1,
                    GetRandomInt(
                        0,
                        12
                    ),
                    GetRandomInt(
                        0,
                        59
                    ),
                    GetRandomInt(
                        0,
                        59
                    )
                );
                var maxTime = new DateTime(
                    minTime.Year,
                    minTime.Month,
                    minTime.Day,
                    0,
                    0,
                    0
                );
                maxTime = maxTime.AddDays(1).AddMilliseconds(-1);
                var results = new List<DateTime>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () => results.Add(
                        GetRandomUtcDate(
                            minTime: minTime,
                            maxTime: maxTime
                        )
                    )
                );

                //---------------Test Result -----------------------
                var outOfRangeLeft = results.Where(d => d.Ticks < minTime.Ticks).ToArray();
                var outOfRangeRight = results.Where(d => d.Ticks < maxTime.Ticks).ToArray();
                Assert.IsFalse(
                    outOfRangeLeft.Any() && outOfRangeRight.Any(),
                    GetErrorHelpFor(
                        outOfRangeLeft,
                        outOfRangeRight,
                        minTime,
                        maxTime
                    )
                );
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void GetRandomUtcDate_GivenMinDateOnly_ShouldProduceRandomDatesWithin30YearPeriod()
            {
                //---------------Set up test pack-------------------
                var minDate = new DateTime(
                    GetRandomInt(
                        1,
                        3000
                    ),
                    GetRandomInt(
                        1,
                        12
                    ),
                    GetRandomInt(
                        1,
                        28
                    )
                );
                var maxDate = minDate.AddYears(30);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDate(minDate: minDate);

                //---------------Test Result -----------------------
                Assert.That(
                    result,
                    Is.GreaterThanOrEqualTo(minDate)
                        .And.LessThanOrEqualTo(maxDate)
                );
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void GetRandomUtcDate_GivenMaxDateOnly_ShouldProduceRandomDatesWithin30YearPeriod()
            {
                //---------------Set up test pack-------------------
                var maxDate = new DateTime(
                    GetRandomInt(
                        31,
                        3000
                    ),
                    GetRandomInt(
                        1,
                        12
                    ),
                    GetRandomInt(
                        1,
                        28
                    )
                );
                var minDate = maxDate.AddYears(-30);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDate(maxDate: maxDate);

                //---------------Test Result -----------------------
                Assert.That(
                    result,
                    Is.GreaterThanOrEqualTo(minDate)
                        .And.LessThanOrEqualTo(maxDate)
                );
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void
                GetRandomUtcDate_GivenMinAndMaxTimeWithFractionsOfSeconds_ShouldTruncateTimesToMilliseconds_AndProduceRandomDateWithTimeBetweenMinAndMax()
            {
                //---------------Set up test pack-------------------
                var r = GetRandomUtcDate();
                var baseDateTime = new DateTime(
                    r.Year,
                    r.Month,
                    r.Day,
                    r.Hour,
                    r.Minute,
                    r.Second
                );
                var ticksInOneSecond = TimeSpan.FromSeconds(1).Ticks;
                var minTicks = GetRandomLong(
                    1,
                    ticksInOneSecond - 1
                );
                var maxTicks = GetRandomLong(
                    minTicks,
                    ticksInOneSecond - 1
                );
                var minTime = baseDateTime.AddTicks(minTicks);
                var maxTime = baseDateTime.AddTicks(maxTicks);
                var expectedMinTime = minTime.TruncateMicroseconds();
                var expectedMaxTime = maxTime.TruncateMicroseconds();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDate(
                    minTime: minTime,
                    maxTime: maxTime
                );

                //---------------Test Result -----------------------
                Assert.That(
                    result.TimeOfDay,
                    Is.GreaterThanOrEqualTo(expectedMinTime.TimeOfDay)
                        .And.LessThanOrEqualTo(expectedMaxTime.TimeOfDay)
                );
            }

            [Test]
            public void GetRandomUtcDate_GivenMaxTime_ShouldProduceRandomDatesWithTimesLessThanOrEqual()
            {
                //---------------Set up test pack-------------------
                var maxTime = new DateTime(
                    1900,
                    1,
                    1,
                    GetRandomInt(
                        12,
                        23
                    ),
                    GetRandomInt(
                        0,
                        59
                    ),
                    GetRandomInt(
                        0,
                        59
                    )
                );
                var results = new List<DateTime>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(() => results.Add(GetRandomUtcDate(maxTime: maxTime)));

                //---------------Test Result -----------------------
                var outOfRange = results
                    .Where(d => d.MillisecondsSinceStartOfDay() > maxTime.MillisecondsSinceStartOfDay())
                    .ToArray();
                Assert.IsFalse(
                    outOfRange.Any(),
                    $"One or more results had a time that was too late for {maxTime}.{Environment.NewLine}{Print(outOfRange)}"
                );
            }

            private string Print(
                DateTime[] outOfRange
            )
            {
                return string.Join(
                    Environment.NewLine,
                    outOfRange
                );
            }

            [Test]
            public void
                GetRandomUtcDate_GivenMinDateTimeAndMaxDateTime_WhenDateOnlySpecified_ShouldReturnDateWithinRange()
            {
                //---------------Set up test pack-------------------
                var minDate = new DateTime(
                    2011,
                    1,
                    1,
                    23,
                    30,
                    0
                );
                var maxDate = new DateTime(
                    2011,
                    1,
                    2,
                    00,
                    30,
                    0
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomUtcDate(
                            minDate,
                            maxDate,
                            true
                        );
                        Assert.AreEqual(
                            new DateTime(
                                2011,
                                1,
                                2,
                                0,
                                0,
                                0
                            ),
                            result
                        );
                    }
                );

                //---------------Test Result -----------------------
            }

            [Test]
            public void
                GetRandomUtcDate_GivenMinDateTimeAndMaxDateTime_WhenDateOnlySpecified_AndMinMaxOnSameDay_ShouldGiveThatDay()
            {
                //---------------Set up test pack-------------------
                var minDate = new DateTime(
                    2011,
                    1,
                    1,
                    12,
                    00,
                    0
                );
                var maxDate = new DateTime(
                    2011,
                    1,
                    1,
                    12,
                    30,
                    0
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomUtcDate(
                            minDate,
                            maxDate,
                            true
                        );
                        Assert.AreEqual(
                            new DateTime(
                                2011,
                                1,
                                1,
                                0,
                                0,
                                0
                            ),
                            result
                        );
                    }
                );

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class GetRandomTimespan
        {
            [TestFixture]
            public class WhenContextIsMilliseconds
            {
                [Test]
                public void ShouldReturnValueInRange()
                {
                    // Arrange
                    // Pre-assert
                    // Act
                    var collected = Range(
                            0,
                            NORMAL_RANDOM_TEST_CYCLES
                        )
                        .Select(
                            _ =>
                            {
                                var result = GetRandomTimeSpan(
                                    1,
                                    10,
                                    TimeSpanContexts.Milliseconds
                                );
                                // Assert
                                Expect(result.Ticks).To.Be
                                    .Greater.Than(TimeSpan.FromMilliseconds(1).Ticks - 1)
                                    .And
                                    .Less.Than(TimeSpan.FromMilliseconds(10).Ticks + 1);
                                return result;
                            }
                        ).ToArray();
                    Expect(collected)
                        .To.Vary();
                }

                [Test]
                public void ShouldReturnValueInRange2()
                {
                    // Arrange
                    // Pre-assert
                    // Act
                    var collected = Range(
                            0,
                            NORMAL_RANDOM_TEST_CYCLES
                        )
                        .Select(
                            _ =>
                            {
                                var result = GetRandomTimeSpan(TimeSpan.FromSeconds(2));
                                // Assert
                                Expect(result).To.Be
                                    .Greater.Than
                                    .Or.Equal.To(TimeSpan.FromSeconds(2));
                                return result;
                            }
                        ).ToArray();
                    Expect(collected)
                        .To.Vary();
                }
            }

            [TestFixture]
            public class WhenContextIsMinutes
            {
                [Test]
                public void ShouldReturnValueInRange()
                {
                    // Arrange
                    // Pre-assert
                    // Act
                    var collected = Range(
                            0,
                            NORMAL_RANDOM_TEST_CYCLES
                        )
                        .Select(
                            _ =>
                            {
                                var result = GetRandomTimeSpan(
                                    1,
                                    10,
                                    TimeSpanContexts.Minutes
                                );
                                // Assert
                                Expect(result.Ticks).To.Be
                                    .Greater.Than(TimeSpan.FromMinutes(1).Ticks - 1)
                                    .And
                                    .Less.Than(TimeSpan.FromMinutes(10).Ticks + 1);
                                return result;
                            }
                        ).ToArray();
                    Expect(collected)
                        .To.Vary();
                }
            }

            [TestFixture]
            public class WhenContextIsHours
            {
                [Test]
                public void ShouldReturnValueInRange()
                {
                    // Arrange
                    // Pre-assert
                    // Act
                    var collected = Range(
                            0,
                            NORMAL_RANDOM_TEST_CYCLES
                        )
                        .Select(
                            _ =>
                            {
                                var result = GetRandomTimeSpan(
                                    1,
                                    10,
                                    TimeSpanContexts.Hours
                                );
                                // Assert
                                Expect(result.Ticks).To.Be
                                    .Greater.Than(TimeSpan.FromHours(1).Ticks - 1)
                                    .And
                                    .Less.Than(TimeSpan.FromHours(10).Ticks + 1);
                                return result;
                            }
                        );

                    Expect(collected)
                        .To.Vary();
                }
            }

            [TestFixture]
            public class WhenContextIsDays
            {
                [Test]
                public void ShouldReturnValueInRange()
                {
                    // Arrange
                    // Pre-assert
                    // Act
                    var collected = Range(
                            0,
                            NORMAL_RANDOM_TEST_CYCLES
                        )
                        .Select(
                            _ =>
                            {
                                var result = GetRandomTimeSpan(
                                    1,
                                    10,
                                    TimeSpanContexts.Days
                                );
                                // Assert
                                Expect(result.Ticks).To.Be
                                    .Greater.Than(TimeSpan.FromDays(1).Ticks - 1)
                                    .And
                                    .Less.Than(TimeSpan.FromDays(10).Ticks + 1);
                                return result;
                            }
                        );
                    Expect(collected)
                        .To.Vary();
                }
            }

            [TestFixture]
            public class WhenContextIsSeconds
            {
                [Test]
                public void ShouldReturnValueInRange()
                {
                    // Arrange
                    // Pre-assert
                    // Act
                    var collected = Range(
                            0,
                            NORMAL_RANDOM_TEST_CYCLES
                        )
                        .Select(
                            _ =>
                            {
                                var result = GetRandomTimeSpan(
                                    1,
                                    10,
                                    TimeSpanContexts.Seconds
                                );
                                // Assert
                                Expect(result.Ticks).To.Be
                                    .Greater.Than(TimeSpan.FromSeconds(1).Ticks - 1)
                                    .And
                                    .Less.Than(TimeSpan.FromSeconds(10).Ticks + 1);
                                return result;
                            }
                        );
                    Expect(collected)
                        .To.Vary();
                }
            }

            [TestFixture]
            public class WhenGivenTimeSpans
            {
                [Test]
                public void ShouldProduceTimeSpanInRange()
                {
                    // Arrange
                    var min = TimeSpan.FromSeconds(
                        GetRandomInt(
                            1,
                            100
                        )
                    );
                    var max = TimeSpan.FromSeconds(
                        GetRandomInt(
                            200,
                            300
                        )
                    );
                    // Act
                    var collected =
                        Range(
                            0,
                            NORMAL_RANDOM_TEST_CYCLES
                        ).Aggregate(
                            new List<TimeSpan>(),
                            (
                                acc,
                                cur
                            ) =>
                            {
                                acc.Add(
                                    GetRandomTimeSpan(
                                        min,
                                        max
                                    )
                                );
                                return acc;
                            }
                        );
                    // Assert
                    Expect(collected).To.Contain.All
                        .Matched.By(t => t >= min && t <= max);
                    Expect(collected)
                        .To.Vary();
                }
            }

            [Test]
            public void GivenNoArguments_ShouldProduceTimeSpanWithinAWeek()
            {
                // Arrange
                var expectedMin = TimeSpan.FromDays(-7);
                var expectedMax = TimeSpan.FromDays(7);
                // Act
                var collected = Range(
                        0,
                        NORMAL_RANDOM_TEST_CYCLES
                    )
                    .Select(
                        _ =>
                        {
                            var result = GetRandomTimeSpan();
                            // Assert
                            Expect(result)
                                .To.Be.Greater.Than.Or.Equal.To(expectedMin);
                            Expect(result)
                                .To.Be.Less.Than.Or.Equal.To(expectedMax);
                            return result;
                        }
                    );
                Expect(collected)
                    .To.Vary();
            }
        }

        [TestFixture]
        public class GetRandomDateRange
        {
            [Test]
            public void GivenNoArguments_ShouldReturnRandomDateRangeDefaultedLocal()
            {
                //---------------Set up test pack-------------------
                var allResults = new List<DateRange>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var thisResult = GetRandomDateRange();
                        Assert.IsNotNull(thisResult);
                        Assert.That(
                            thisResult.From,
                            Is.LessThanOrEqualTo(thisResult.To)
                        );
                        allResults.Add(thisResult);
                    }
                );

                //---------------Test Result -----------------------
                var froms = allResults.Select(o => o.From);
                var tos = allResults.Select(o => o.To);
                var deltas = allResults.Select(o => o.To - o.From);
                VarianceAssert.IsVariant(froms);
                VarianceAssert.IsVariant(tos);
                VarianceAssert.IsVariant(deltas);
                Expect(allResults).To.Contain.All
                    .Matched.By(
                        dt => dt.From.Kind == DateTimeKind.Local &&
                            dt.To.Kind == DateTimeKind.Local
                    );
            }

            [Test]
            public void GivenMinDate_ShouldReturnRangeWithBothDatesGreaterThanMinDate()
            {
                //---------------Set up test pack-------------------
                var minDate = GetRandomDate();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomDateRange(minDate);

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.That(
                    result.From,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.To,
                    Is.GreaterThanOrEqualTo(minDate)
                );
            }

            [Test]
            public void GivenMinDateAndMaxDate_ShouldReturnRangeWithinMinAndMaxRange()
            {
                //---------------Set up test pack-------------------
                var minDate = GetRandomDate();
                var maxDate = minDate.AddDays(
                    GetRandomInt(
                        1,
                        12
                    )
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomDateRange(
                    minDate,
                    maxDate
                );

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.That(
                    result.From,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.To,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.From,
                    Is.LessThanOrEqualTo(maxDate)
                );
                Assert.That(
                    result.To,
                    Is.LessThanOrEqualTo(maxDate)
                );
            }

            [Test]
            public void GivenMinDateAndMaxDateAndDateOnlyTrue_ShouldReturnRangeWithinMinAndMaxRangeWithNoTimes()
            {
                //---------------Set up test pack-------------------
                var minDate = GetRandomDate();
                var maxDate = minDate.AddDays(
                    GetRandomInt(
                        1,
                        12
                    )
                );


                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomDateRange(
                    minDate,
                    maxDate,
                    true
                );

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.That(
                    result.From,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.To,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.From,
                    Is.LessThanOrEqualTo(maxDate)
                );
                Assert.That(
                    result.To,
                    Is.LessThanOrEqualTo(maxDate)
                );
                Assert.AreEqual(
                    result.From.StartOfDay(),
                    result.From
                );
                Assert.AreEqual(
                    result.To.StartOfDay(),
                    result.To
                );
            }

            [Test]
            public void GivenMinTime_ShouldEnsureBothDatesAreAfterThatTime()
            {
                //---------------Set up test pack-------------------
                var minTime = GetRandomDate();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomDateRange(minTime: minTime);

                        //---------------Test Result -----------------------
                        Assert.That(
                            result.From.TimeOfDay,
                            Is.GreaterThanOrEqualTo(minTime.TimeOfDay)
                        );
                        Assert.That(
                            result.To.TimeOfDay,
                            Is.GreaterThanOrEqualTo(minTime.TimeOfDay)
                        );
                    }
                );
            }

            [Test]
            public void GivenMaxTime_ShouldEnsureBothDatesAreBeforeThatTime()
            {
                //---------------Set up test pack-------------------
                var maxTime = GetRandomDate();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomDateRange(maxTime: maxTime);

                        //---------------Test Result -----------------------
                        Assert.That(
                            result.From.TimeOfDay,
                            Is.LessThanOrEqualTo(maxTime.TimeOfDay)
                        );
                        Assert.That(
                            result.To.TimeOfDay,
                            Is.LessThanOrEqualTo(maxTime.TimeOfDay)
                        );
                    }
                );
            }

            [Test]
            public void GivenDateKind_ShouldReturnBothDatesWithThatKind()
            {
                RunCycles(
                    () =>
                    {
                        //---------------Set up test pack-------------------
                        var expected = GetRandom<DateTimeKind>();

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        var result = GetRandomDateRange(expected);
                        //---------------Test Result -----------------------
                        Assert.AreEqual(
                            expected,
                            result.From.Kind
                        );
                        Assert.AreEqual(
                            expected,
                            result.To.Kind
                        );
                    }
                );
            }
        }

        [TestFixture]
        public class GetRandomUtcDateRange
        {
            [Test]
            public void GivenNoArguments_ShouldReturnRandomDateRange()
            {
                //---------------Set up test pack-------------------
                var allResults = new List<DateRange>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var thisResult = GetRandomUtcDateRange();
                        Assert.IsNotNull(thisResult);
                        Assert.That(
                            thisResult.From,
                            Is.LessThanOrEqualTo(thisResult.To)
                        );
                        allResults.Add(thisResult);
                    }
                );

                //---------------Test Result -----------------------
                var froms = allResults.Select(o => o.From);
                var tos = allResults.Select(o => o.To);
                var deltas = allResults.Select(o => o.To - o.From);
                VarianceAssert.IsVariant(froms);
                VarianceAssert.IsVariant(tos);
                VarianceAssert.IsVariant(deltas);
            }

            [Test]
            public void GivenMinDate_ShouldReturnRangeWithBothDatesGreaterThanMinDate()
            {
                //---------------Set up test pack-------------------
                var minDate = GetRandomDate();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDateRange(minDate);

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.That(
                    result.From,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.To,
                    Is.GreaterThanOrEqualTo(minDate)
                );
            }

            [Test]
            public void GivenMinDateAndMaxDate_ShouldReturnRangeWithinMinAndMaxRange()
            {
                //---------------Set up test pack-------------------
                var minDate = GetRandomDate();
                var maxDate = minDate.AddDays(
                    GetRandomInt(
                        1,
                        12
                    )
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDateRange(
                    minDate,
                    maxDate
                );

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.That(
                    result.From,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.To,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.From,
                    Is.LessThanOrEqualTo(maxDate)
                );
                Assert.That(
                    result.To,
                    Is.LessThanOrEqualTo(maxDate)
                );
            }

            [Test]
            public void GivenMinDateAndMaxDateAndDateOnlyTrue_ShouldReturnRangeWithinMinAndMaxRangeWithNoTimes()
            {
                //---------------Set up test pack-------------------
                var minDate = GetRandomDate();
                var maxDate = minDate.AddDays(
                    GetRandomInt(
                        1,
                        12
                    )
                );


                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDateRange(
                    minDate,
                    maxDate,
                    true
                );

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.That(
                    result.From,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.To,
                    Is.GreaterThanOrEqualTo(minDate)
                );
                Assert.That(
                    result.From,
                    Is.LessThanOrEqualTo(maxDate)
                );
                Assert.That(
                    result.To,
                    Is.LessThanOrEqualTo(maxDate)
                );
                Assert.AreEqual(
                    result.From.StartOfDay(),
                    result.From
                );
                Assert.AreEqual(
                    result.To.StartOfDay(),
                    result.To
                );
            }

            [Test]
            public void GivenMinTime_ShouldEnsureBothDatesAreAfterThatTime()
            {
                //---------------Set up test pack-------------------
                var minTime = GetRandomDate();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomUtcDateRange(minTime: minTime);

                        //---------------Test Result -----------------------
                        Assert.That(
                            result.From.TimeOfDay,
                            Is.GreaterThanOrEqualTo(minTime.TimeOfDay)
                        );
                        Assert.That(
                            result.To.TimeOfDay,
                            Is.GreaterThanOrEqualTo(minTime.TimeOfDay)
                        );
                    }
                );
            }

            [Test]
            public void GivenMaxTime_ShouldEnsureBothDatesAreBeforeThatTime()
            {
                //---------------Set up test pack-------------------
                var maxTime = GetRandomDate();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var result = GetRandomUtcDateRange(maxTime: maxTime);

                        //---------------Test Result -----------------------
                        Assert.That(
                            result.From.TimeOfDay,
                            Is.LessThanOrEqualTo(maxTime.TimeOfDay)
                        );
                        Assert.That(
                            result.To.TimeOfDay,
                            Is.LessThanOrEqualTo(maxTime.TimeOfDay)
                        );
                    }
                );
            }

            [Test]
            public void ShouldSetUtc()
            {
                //---------------Set up test pack-------------------
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomUtcDateRange();

                //---------------Test Result -----------------------
                Assert.AreEqual(
                    result.From.Kind,
                    DateTimeKind.Utc
                );
                Assert.AreEqual(
                    result.To.Kind,
                    DateTimeKind.Utc
                );
            }
        }

        [TestFixture]
        public class GetRandomCollection
        {
            [Test]
            public void
                GivenGeneratorFunctionAndBoundaries_ShouldReturnListOfRandomSizeContainingOutputOfGeneratorPerItem()
            {
                //---------------Set up test pack-------------------
                const int runs = NORMAL_RANDOM_TEST_CYCLES;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < runs; i++)
                {
                    var min = GetRandomInt(
                        10,
                        100
                    );
                    var max = GetRandomInt(
                        10,
                        100
                    );
                    if (min > max)
                    {
                        var swap = min;
                        min = max;
                        max = swap;
                    }

                    var fill = GetRandomInt(
                        1,
                        1024
                    );
                    var result = GetRandomCollection(
                        () => fill,
                        min,
                        max
                    );


                    //---------------Test Result -----------------------
                    Assert.That(
                        result.Count(),
                        Is.GreaterThanOrEqualTo(min)
                    );
                    Assert.That(
                        result.Count(),
                        Is.LessThanOrEqualTo(max)
                    );
                    Assert.IsTrue(result.All(item => item == fill));
                }
            }

            [Test]
            public void WhenMinEqualsMax_ShouldReturnExactlyThatSize()
            {
                //---------------Set up test pack-------------------
                int max;
                var min = max = GetRandomInt();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomCollection(
                    () => GetRandomInt(),
                    min,
                    max
                );

                //---------------Test Result -----------------------
                Assert.AreEqual(
                    min,
                    result.Count()
                );
            }

            [Test]
            public void ShouldInvokeProvidedFactoryFunction()
            {
                //---------------Set up test pack-------------------
                const int runs = NORMAL_RANDOM_TEST_CYCLES;
                var generatedValues = new List<int>();
                Func<int> factory = () =>
                {
                    var thisValue = GetRandomInt();
                    generatedValues.Add(thisValue);
                    return thisValue;
                };
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < runs; i++)
                {
                    var result = GetRandomCollection(factory);
                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Equal(generatedValues);
                    generatedValues.Clear();
                }
            }

            [Test]
            public void GenericInvoke_ShouldGenerateCollectionOfRequestedType()
            {
                //---------------Set up test pack-------------------
                var minItems = GetRandomInt(5);
                var maxItems = GetRandomInt(
                    11,
                    20
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandomCollection<SomePOCO>(
                    minItems,
                    maxItems
                );

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                CollectionAssert.IsNotEmpty(result);
                Assert.IsTrue(result.All(r => r != null));
                Assert.IsTrue(result.All(r => r.GetType() == typeof(SomePOCO)));
                VarianceAssert.IsVariant<SomePOCO, int>(
                    result,
                    "Id"
                );
                VarianceAssert.IsVariant<SomePOCO, string>(
                    result,
                    "Name"
                );
                VarianceAssert.IsVariant<SomePOCO, DateTime>(
                    result,
                    "Date"
                );
            }
        }


        [TestFixture]
        public class GetRandomNonAlphaNumericString
        {
            [Test]
            public void ShouldProduceRandomStringWithOnlyAlphaNumericCharacters()
            {
                var allResults = new List<Tuple<string, int, int>>();
                RunCycles(
                    () =>
                    {
                        //---------------Set up test pack-------------------
                        var minLength = GetRandomInt(
                            1,
                            50
                        );
                        var maxLength = GetRandomInt(
                            minLength,
                            minLength + 50
                        );

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        var result = GetRandomAlphaNumericString(
                            minLength,
                            maxLength
                        );

                        allResults.Add(
                            Tuple.Create(
                                result,
                                minLength,
                                maxLength
                            )
                        );
                    }
                );
                //---------------Test Result -----------------------
                CollectionAssert.IsNotEmpty(allResults);
                // collisions are possible, but should occur < 1%
                var total = allResults.Count;
                var unique = allResults.Select(o => o.Item1).Distinct().Count();
                var delta = (total - unique) / (decimal)total;
                Assert.That(
                    delta,
                    Is.LessThan(1)
                );

                var tooShort = allResults.Where(r => r.Item1.Length < r.Item2);
                var tooLong = allResults.Where(r => r.Item1.Length > r.Item3);
                var alphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                var invalidCharacters = allResults.Where(r => r.Item1.Any(c => !alphaNumericChars.Contains(c)));
                Assert.IsFalse(
                    tooShort.Any() && tooLong.Any() && invalidCharacters.Any(),
                    BuildErrorMessageFor(
                        tooShort,
                        tooLong,
                        invalidCharacters
                    )
                );
            }

            [Test]
            public void ShouldProduceNonAlphaNumericStrings()
            {
                // Arrange
                var results = new List<string>();
                // Pre-assert
                // Act
                RunCycles(() => results.Add(GetRandomNonAlphaNumericString(1)));
                // Assert
                Expect(results.None(r => r.IsAlphanumeric()));
            }

            [Test]
            public void ShouldProduceRandomStringWithOnlyAlphaCharacters()
            {
                var allResults = new List<Tuple<string, int, int>>();
                RunCycles(
                    () =>
                    {
                        //---------------Set up test pack-------------------
                        var minLength = GetRandomInt(
                            1,
                            50
                        );
                        var maxLength = GetRandomInt(
                            minLength,
                            minLength + 50
                        );

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        var result = GetRandomAlphaString(
                            minLength,
                            maxLength
                        );

                        allResults.Add(
                            Tuple.Create(
                                result,
                                minLength,
                                maxLength
                            )
                        );
                    }
                );
                //---------------Test Result -----------------------
                CollectionAssert.IsNotEmpty(allResults);
                // collisions are possible, but should occur < 1%
                var total = allResults.Count;
                var unique = allResults.Select(o => o.Item1).Distinct().Count();
                var delta = (total - unique) / (decimal)total;
                Assert.That(
                    delta,
                    Is.LessThan(1)
                );

                var tooShort = allResults.Where(r => r.Item1.Length < r.Item2);
                var tooLong = allResults.Where(r => r.Item1.Length > r.Item3);
                var alphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var invalidCharacters = allResults.Where(r => r.Item1.Any(c => !alphaNumericChars.Contains(c)));
                Assert.IsFalse(
                    tooShort.Any() && tooLong.Any() && invalidCharacters.Any(),
                    BuildErrorMessageFor(
                        tooShort,
                        tooLong,
                        invalidCharacters
                    )
                );
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceStringWithinRequiredLengthRange()
            {
                // Arrange
                var min = GetRandomInt(
                    1,
                    100
                );
                var max = GetRandomInt(
                    min,
                    100
                );
                // Act
                var result = GetRandomNonAlphaNumericString(
                    min,
                    max
                );
                // Assert
                Expect(result.Length)
                    .To.Be.Greater.Than.Or.Equal.To(min)
                    .And
                    .Less.Than.Or.Equal.To(max);
            }
        }

        [TestFixture]
        public class GetRandomNumericString
        {
            [Test]
            public void ShouldProduceRandomStringWithOnlyNumericCharacters()
            {
                var allResults = new List<Tuple<string, int, int>>();
                RunCycles(
                    () =>
                    {
                        //---------------Set up test pack-------------------
                        var minLength = GetRandomInt(
                            1,
                            50
                        );
                        var maxLength = GetRandomInt(
                            minLength,
                            minLength + 50
                        );

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        var result = GetRandomNumericString(
                            minLength,
                            maxLength
                        );

                        allResults.Add(
                            Tuple.Create(
                                result,
                                minLength,
                                maxLength
                            )
                        );
                    }
                );
                //---------------Test Result -----------------------
                CollectionAssert.IsNotEmpty(allResults);
                // collisions are possible, but should occur < 1%
                var total = allResults.Count;
                var unique = allResults.Select(o => o.Item1).Distinct().Count();
                var delta = (total - unique) / (decimal)total;
                Assert.That(
                    delta,
                    Is.LessThan(1)
                );

                var tooShort = allResults.Where(r => r.Item1.Length < r.Item2);
                var tooLong = allResults.Where(r => r.Item1.Length > r.Item3);
                var alphaNumericChars = "1234567890";
                var invalidCharacters = allResults.Where(r => r.Item1.Any(c => !alphaNumericChars.Contains(c)));
                Assert.IsFalse(
                    tooShort.Any() && tooLong.Any() && invalidCharacters.Any(),
                    BuildErrorMessageFor(
                        tooShort,
                        tooLong,
                        invalidCharacters
                    )
                );
            }
        }

        [TestFixture]
        public class GetAnother
        {
            [TestFixture]
            public class WhenCanGenerateANewValue
            {
                [Test]
                public void GivenOriginalValueCollectionAndNoGenerator_ShouldReturnThatValue()
                {
                    RunCycles(
                        () =>
                        {
                            //---------------Set up test pack-------------------
                            var notThis = "abcdefghijklmnopqrstuvwABCDEFGHIJKLMNOPQRSTUVW".ToCharArray()
                                .Select(c => c.ToString());

                            //---------------Assert Precondition----------------

                            //---------------Execute Test ----------------------
                            var result = GetAnother(notThis);

                            //---------------Test Result -----------------------
                            Assert.IsFalse(notThis.Any(i => i == result));
                        }
                    );
                }

                [Test]
                public void GivenOriginalValueAndGenerator_ShouldReturnANewValue()
                {
                    RunCycles(
                        () =>
                        {
                            //---------------Set up test pack-------------------
                            var notThis = GetRandomString(
                                1,
                                1
                            );

                            //---------------Assert Precondition----------------

                            //---------------Execute Test ----------------------
                            var result = GetAnother(
                                notThis,
                                () => GetRandomString(
                                    1,
                                    1
                                )
                            );

                            //---------------Test Result -----------------------
                            Assert.AreNotEqual(
                                notThis,
                                result
                            );
                        }
                    );
                }

                [Test]
                public void GivenOriginalValueAndNoGenerator_ShouldReturnANewValue()
                {
                    RunCycles(
                        () =>
                        {
                            //---------------Set up test pack-------------------
                            var notThis = GetRandomString(
                                1,
                                1
                            );

                            //---------------Assert Precondition----------------

                            //---------------Execute Test ----------------------
                            var result = GetAnother(notThis);

                            //---------------Test Result -----------------------
                            Assert.AreNotEqual(
                                notThis,
                                result
                            );
                        }
                    );
                }

                [Test]
                public void GivenNullValue_ShouldReturnValueFromGenerator()
                {
                    //---------------Set up test pack-------------------
                    var strings = new Stack<string>();
                    var expected = GetRandomString();
                    var unexpected = GetAnother(
                        expected,
                        () => GetRandomString()
                    );
                    strings.Push(unexpected);
                    strings.Push(expected);
                    strings.Push(null);


                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = GetAnother(
                        (string)null,
                        () => strings.Pop()
                    );

                    //---------------Test Result -----------------------
                    Assert.AreEqual(
                        expected,
                        result
                    );
                }
            }

            [TestFixture]
            public class WhenCannotGenerateOutputDueToFailingValidationFunc
            {
                [Test]
                public void ShouldThrow_Variant1()
                {
                    //---------------Set up test pack-------------------
                    var notThis = GetRandomString(
                        1,
                        1
                    );

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    Assert.Throws<CannotGetAnotherDifferentRandomValueException<string>>(
                        () => GetAnother(
                            notThis,
                            () => notThis
                        )
                    );

                    //---------------Test Result -----------------------
                }

                [Test]
                public void ShouldThrow_Variant2()
                {
                    //---------------Set up test pack-------------------
                    var notAnyOfThese = GetRandomCollection(
                        () => GetRandomString(),
                        2
                    );

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    Assert.Throws<CannotGetAnotherDifferentRandomValueException<string[]>>(
                        () => GetAnother(
                            notAnyOfThese,
                            () => GetRandomString(),
                            (
                                left,
                                right
                            ) => true
                        )
                    );

                    //---------------Test Result -----------------------
                }
            }

            [TestFixture]
            public class WhenCanGenerateNewValues
            {
                [Test]
                public void ShouldReturnThatValue()
                {
                    RunCycles(
                        () =>
                        {
                            //---------------Set up test pack-------------------
                            var notThis = "abcdefghijklmnopqrstuvwABCDEFGHIJKLMNOPQRSTUVW".ToCharArray()
                                .Select(c => c.ToString());

                            //---------------Assert Precondition----------------

                            //---------------Execute Test ----------------------
                            var result = GetAnother(
                                notThis,
                                () => GetRandomString(
                                    1,
                                    1
                                )
                            );

                            //---------------Test Result -----------------------
                            Assert.IsFalse(notThis.Any(i => i == result));
                        }
                    );
                }
            }
        }

        [TestFixture]
        public class GenericGetRandom
        {
            [Test]
            public void ForInt_ReturnsRandomIntWithinDefaultRange()
            {
                //---------------Set up test pack-------------------
                var ints = new List<int>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(() => ints.Add(GetRandom<int>()));

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(ints);
                Assert.IsTrue(ints.All(i => i >= DefaultRanges.MIN_INT_VALUE));
                Assert.IsTrue(ints.All(i => i <= DefaultRanges.MAX_INT_VALUE));
            }

            [Test]
            public void ForLong_ReturnsRandomIntWithinRange()
            {
                //---------------Set up test pack-------------------
                var ints = new List<long>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(() => ints.Add(GetRandom<long>()));

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(ints);
                Assert.IsTrue(ints.All(i => i >= DefaultRanges.MIN_LONG_VALUE));
                Assert.IsTrue(ints.All(i => i <= DefaultRanges.MAX_LONG_VALUE));
            }

            [Test]
            public void ForString_ReturnsRandomStringsWithinDefaultRanges()
            {
                //---------------Set up test pack-------------------
                var strings = new List<string>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(() => strings.Add(GetRandom<string>()));

                //---------------Test Result -----------------------
                Assert.IsTrue(strings.All(s => s.Length >= DefaultRanges.MINLENGTH_STRING));
                Assert.IsTrue(
                    strings.All(
                        s => s.Length <=
                            DefaultRanges.MINLENGTH_STRING +
                            DefaultRanges.MINLENGTH_STRING
                    )
                );
                Assert.IsTrue(strings.Distinct().Count() > 1);
            }

            [Test]
            public void ForEnum_ShouldReturnRandomValueFromEnumSelection()
            {
                //---------------Set up test pack-------------------
                var results = new List<TestEnum>();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------

                RunCycles(() => results.Add(GetRandom<TestEnum>()));
                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(results);
            }

            [Test]
            // statistically, the ratios can be out
            [Retry(5)]
            public void ForEnum_ShouldReturnRandomValueFromEnumSelectionWithReasonableSpread()
            {
                //---------------Set up test pack-------------------
                var results = new List<TestEnum>();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------

                RunCycles(() => results.Add(GetRandomEnum<TestEnum>()));
                //---------------Test Result -----------------------
                var runs = results.Count;
                var onePercent = (100 * results.Count(i => i == TestEnum.One)) / runs;
                var twoPercent = (100 * results.Count(i => i == TestEnum.Two)) / runs;
                var threePercent = (100 * results.Count(i => i == TestEnum.Three)) / runs;

                var d1 = Math.Abs(twoPercent - onePercent);
                var d2 = Math.Abs(threePercent - twoPercent);
                var d3 = Math.Abs(threePercent - onePercent);

                Assert.That(
                    d1,
                    Is.LessThan(20)
                );
                Assert.That(
                    d2,
                    Is.LessThan(20)
                );
                Assert.That(
                    d3,
                    Is.LessThan(20)
                );
            }

            [Test]
            public void ShouldUseOnTheFlyGenericBuilderToGiveBackRandomItem()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var item = GetRandom<SomePOCO>();

                //---------------Test Result -----------------------
                Assert.IsNotNull(item);
                Assert.IsInstanceOf<SomePOCO>(item);
                // assert that *something* was set
                Assert.IsNotNull(item.Id);
                Assert.IsNotNull(item.Name);
                Assert.IsNotNull(item.Date);
            }

            [Test]
            public void WhenOperatingOnInternalTypeFromAnotherAssembly()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(
                    () => GetRandom<InternalClass>()
                );

                //---------------Test Result -----------------------
            }

            [Test]
            public void WhenOperatingOnInternalTypeNotShared()
            {
                //---------------Set up test pack-------------------
                if (Type.GetType("Mono.Runtime") != null)
                {
                    Assert.Ignore("Mono (erroneously) allows access to internal types, so this test is skipped");
                    return;
                }

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<UnableToCreateDynamicBuilderException>(
                    () => GetRandom<AnotherInternalClass>()
                );

                //---------------Test Result -----------------------
            }

            [Test]
            public void
                GetRandomOfType_GivenInterfaceType_WhenImplementationWithDefaultConstructorCanBefound_ShouldReturnInstance_AndPreferAccessModifiersFromInstance()
            {
                //---------------Set up test pack-------------------
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = GetRandom<IInterfaceToGetRandomOf>();

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Name);
            }

            [Test]
            public void GetRandom_GivenAValidatorFunction_ShouldReturnADifferentValue()
            {
                RunCycles(
                    () =>
                    {
                        //--------------- Arrange -------------------
                        var first = GetRandom<IHasAnId>();

                        //--------------- Assume ----------------

                        //--------------- Act ----------------------
                        var other = GetRandom<IHasAnId>(
                            test =>
                            {
                                return test.Id != first.Id;
                            }
                        );

                        //--------------- Assert -----------------------
                        Expect(other).Not.To.Be.Null();
                        Expect(other).Not.To.Equal(first);
                        Expect(other.Id).Not.To.Equal(first.Id);
                    }
                );
            }

            [Test]
            public void GetRandom_GivenAValidatorAndGenerator_ShouldUseTheGeneratorToReturnADifferentValue()
            {
                //--------------- Arrange -------------------
                var first = GetRandom<IHasAnId>();
                var expected = GetRandom<IHasAnId>(o => o.Id != first.Id);

                //--------------- Assume ----------------
                // rather fail early if we're about to enter an infinite loop
                Expect(first.Id).Not.To.Equal(expected.Id);

                //--------------- Act ----------------------
                var result = GetRandom(
                    o => o.Id != first.Id,
                    () => expected
                );

                //--------------- Assert -----------------------
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void SuccessfullySelectingASub()
            {
                // Arrange
                // Act
                var result = GetRandom<IPublicFoo>();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.GetType().Name)
                    .To.Start.With("ObjectProxy");
            }

            public interface IPublicFoo
            {
                void DoThing();
            }

            [Test]
            public void GetRandomOfType_WhenTypeHasSimpleParameteredConstructor_ShouldAttemptToConstruct()
            {
                // Arrange
                // Pre-Assert
                // Act
                var result = GetRandom<HasConstructorWithParameter>();
                // Assert
                Expect(result.Parameter).Not.To.Be.Null();
            }

            [Test]
            public void GetRandomOfTypeKeyValuePair_ShouldReturnKeyValuePairWithData()
            {
                // Arrange
                // Pre-Assert
                // Act
                var result = GetRandom<KeyValuePair<string, string>>();
                var attempts = 0;
                while (result.Key == "" || result.Value == "")
                {
                    if (++attempts > 10)
                        Assert.Fail("Unable to get non-empty key or value");
                    result = GetRandom<KeyValuePair<string, string>>();
                }

                // Assert
                Expect(result).Not.To.Be.Null();
                Expect(result.Key).Not.To.Be.Null();
                Expect(result.Value).Not.To.Be.Null();
            }

            [Test]
            public void ShouldPreferTheParameterlessConstructor()
            {
                // Arrange
                // Pre-Assert
                // Act
                var sut = GetRandom<HasTwoConstructors>();
                // Assert
                Expect(sut.ParameterlessConstructorUsed).To.Be.True();
            }

            [Test]
            public void WhenPropModsInvokeWithProp_ShouldNotThrowCollectionModifiedException()
            {
                // Arrange

                // Pre-assert

                // Act
                Expect(
                        () =>
                        {
                            var parent = GetRandom<Parent>();
                            Expect(parent.Children).Not.To.Be.Empty();
                        }
                    )
                    .Not.To.Throw();

                // Assert
            }

            [Test]
            public void WhenTypeHasTimeSpanProperty_ShouldNotExplode()
            {
                // Arrange
                // Pre-assert
                // Act
                Expect(GetRandom<PocoWithTimeSpan>)
                    .Not.To.Throw();
                // Assert
            }

            [Test]
            public void WhenPropModsInvokeWithProp_ShouldNotStoreLastBuildTimePropMods()
            {
                // Arrange
                var builder = ParentBuilder.Create().WithRandomProps();

                // Pre-assert

                // Act
                builder.Build();
                Expect(builder.WithChildrenCallCount).To.Equal(1);
                builder.Build();

                // Assert
                Expect(builder.WithChildrenCallCount).To.Equal(2);
            }

            [Test]
            public void ShouldInvokeAProvidedMutator()
            {
                // Arrange
                // Act
                var result = GetRandom<SomePOCO>(
                    o =>
                    {
                        o.Id = -42;
                    }
                );
                // Assert
                Expect(result.Id)
                    .To.Equal(-42);
            }
        }

        [TestFixture]
        public class GetRandomValue
        {
            [Test]
            public void GivenPOCOType_ShouldUseOnTheFlyGenericBuilderToGiveBackRandomItem()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var item = GetRandom(typeof(SomePOCO)) as SomePOCO;

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
            public void GivenPrimitiveType_ShouldUseRegularRVGMethods(
                Type type
            )
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var item = GetRandom(type);

                //---------------Test Result -----------------------
                Assert.IsNotNull(item);
                Assert.IsInstanceOf(
                    type,
                    item
                );
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
            public void GivenNullablePrimitiveType_ShouldUseRegularRVGMethods(
                Type type
            )
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var item = GetRandom(type);

                //---------------Test Result -----------------------
                Assert.IsNotNull(item);
                Assert.IsInstanceOf(
                    type,
                    item
                );
            }

            [Test]
            public void GivenPOCOType_ShouldHaveVariance()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var items = GetRandomCollection(
                    GetRandom<SomePOCO>,
                    NORMAL_RANDOM_TEST_CYCLES,
                    NORMAL_RANDOM_TEST_CYCLES
                );

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant<SomePOCO, int>(
                    items,
                    "Id"
                );
                VarianceAssert.IsVariant<SomePOCO, string>(
                    items,
                    "Name"
                );
                VarianceAssert.IsVariant<SomePOCO, DateTime>(
                    items,
                    "Date"
                );
            }

            [Test]
            public void GivenPOCOWithBuilderType_ShouldUseExistingBuilder()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var item = GetRandom<SomePOCOWithBuilder>();

                //---------------Test Result -----------------------
                Assert.IsNotNull(item);
                Assert.IsInstanceOf<SomePOCOWithBuilder>(item);
                // assert that *something* was set
                Assert.IsNotNull(item.Id);
                Assert.That(
                    item.Id.Value,
                    Is.GreaterThanOrEqualTo(1000)
                );
                Assert.That(
                    item.Id.Value,
                    Is.LessThanOrEqualTo(2000)
                );
                Assert.IsNotNull(item.Name);
                Assert.IsNotNull(item.Date);
            }
        }


        public class SomePOCOWithBuilder : SomePOCO
        {
        }

        public class SomePOCOWithBuilderBuilder : GenericBuilder<SomePOCOWithBuilderBuilder, SomePOCOWithBuilder>
        {
            public override SomePOCOWithBuilderBuilder WithRandomProps()
            {
                return base.WithRandomProps()
                    .WithProp(
                        o => o.Id = GetRandomInt(
                            1000,
                            2000
                        )
                    );
            }
        }

        [TestFixture]
        [Explicit("Discovery tests")]
        public class Discovery
        {
            [Test]
            public void EncodingNonPrintableCharacters_ShouldNotThrow()
            {
                //---------------Set up test pack-------------------
                var bytes = GetRandomCollection(
                        () => GetRandomInt(
                            0,
                            255
                        )
                    )
                    .Select(i => (byte)i)
                    .ToArray();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = Encoding.UTF8.GetString(bytes);

                //---------------Test Result -----------------------
                Console.WriteLine(result);
            }
        }


        [TestFixture]
        public class GetRandomAlphaString
        {
            [Test]
            public void GivenMinLength_ShouldReturnValueOfAtLeastThatLength()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                RunCycles(
                    () =>
                    {
                        var minLength = GetRandomInt(
                            10,
                            20
                        );
                        var result = GetRandomAlphaNumericString(minLength);
                        Assert.That(
                            result.Length,
                            Is.GreaterThanOrEqualTo(minLength)
                        );
                    }
                );

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class GetRandomBytes
        {
            [Test]
            public void ShouldReturnBytesAcrossFullRange()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                for (var i = 0; i < 20; i++)
                {
                    // look for full-range variance across an 8k block
                    var result = GetRandomBytes(
                        8192,
                        8192
                    );
                    if (result.Distinct().Count() == 256)
                        return;
                }

                //---------------Test Result -----------------------
                Assert.Fail("Couldn't find full range of bytes");
            }
        }

        [TestFixture]
        public class GetRandomIPV4Address
        {
            [Test]
            public void ShouldReturnValidIPV4Addresses()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var allResults = new List<string>();
                RunCycles(
                    () =>
                    {
                        var result = GetRandomIPv4Address();
                        allResults.Add(result);
                        var parts = result.Split('.');
                        Assert.AreEqual(
                            4,
                            parts.Length
                        );
                        var ints = parts.Select(int.Parse);
                        Assert.IsTrue(ints.All(i => i >= 0 && i < 265));
                    }
                );

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(allResults);
            }
        }

        [TestFixture]
        public class RandomHttpMethods
        {
            [Test]
            public void ShouldReturnValidHttpMethod()
            {
                // Arrange
                var allowed = new HashSet<string>(
                    new[]
                    {
                        HttpMethod.Delete.ToString(),
                        HttpMethod.Get.ToString(),
                        HttpMethod.Head.ToString(),
                        HttpMethod.Options.ToString(),
                        HttpMethod.Post.ToString(),
                        HttpMethod.Put.ToString(),
                        HttpMethod.Trace.ToString()
                    }
                );
                var collected = new List<string>();

                // Act
                for (var i = 0; i < NORMAL_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(GetRandomHttpMethod());
                }

                // Assert
                Expect(collected)
                    .To.Contain.All
                    .Matched.By(s => allowed.Contains(s));
            }

            [Test]
            public void ShouldBeAbleToReturnCommonHttpMethod()
            {
                // Arrange
                var allowed = new HashSet<string>(
                    new[]
                    {
                        HttpMethod.Get.ToString(),
                        HttpMethod.Post.ToString(),
                        HttpMethod.Delete.ToString(),
                        HttpMethod.Put.ToString(),
                    }
                );
                var collected = new List<string>();

                // Act
                for (var i = 0; i < NORMAL_RANDOM_TEST_CYCLES; i++)
                {
                    collected.Add(GetRandomCommonHttpMethod());
                }

                // Assert
                Expect(collected)
                    .To.Contain.All
                    .Matched.By(s => allowed.Contains(s));
                var counts = collected.GroupBy(s => s)
                    .ToDictionary(
                        o => o.Key,
                        o => o.Count()
                    );
                Expect(counts["GET"])
                    .To.Be.Greater.Than(counts["PUT"])
                    .And
                    .To.Be.Greater.Than(counts["POST"])
                    .And
                    .To.Be.Greater.Than(counts["DELETE"]);
                Expect(counts["POST"])
                    .To.Be.Greater.Than(counts["PUT"]);
            }
        }

        [TestFixture]
        public class GetRandomHostName
        {
            [Test]
            public void ShouldReturnRandomHostName()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var allResults = new List<string>();
                var re = new Regex("^[A-Za-z0-9-]+$");
                RunCycles(
                    () =>
                    {
                        var result = GetRandomHostname();
                        Expect(result)
                            .Not.To.Be.Null
                            .Or.Whitespace();
                        Expect(result.Length)
                            .To.Be.Less.Than(254);
                        Assert.IsNotNull(result);
                        var parts = result.Split('.');
                        Expect(parts)
                            .To.Contain.All
                            .Matched.By(
                                s =>
                                    re.IsMatch(s) &&
                                    s.Length < 64
                            );
                        allResults.Add(result);
                    },
                    1024
                );

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(allResults);
            }

            [Test]
            public void ShouldReturnAtLeastMinRequiredParts()
            {
                // Arrange
                // Act
                RunCycles(
                    () =>
                    {
                        var min = GetRandomInt(
                            3,
                            5
                        );
                        var result = GetRandomHostname(min);
                        var parts = result.Split('.');
                        Expect(parts.Length)
                            .To.Be.Greater.Than.Or.Equal.To(min);
                    }
                );
                // Assert
            }

            [Test]
            public void ShouldReturnPartsWithinFullySpecifiedRange()
            {
                // Arrange
                // Act
                RunCycles(
                    () =>
                    {
                        var min = GetRandomInt(
                            4,
                            6
                        );
                        var max = GetRandomInt(
                            8,
                            12
                        );
                        var result = GetRandomHostname(
                            min,
                            max
                        );
                        var parts = result.Split('.');
                        Expect(parts.Length)
                            .To.Be.Greater.Than.Or.Equal.To(min)
                            .And
                            .To.Be.Less.Than.Or.Equal.To(max);
                    }
                );
                // Assert
            }
        }

        [TestFixture]
        public class GeneratingRandomVersions
        {
            [Test]
            public void Default_ShouldReturnVersionWithThreeIntegerParts()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var allResults = new List<string>();
                RunCycles(
                    () =>
                    {
                        var result = GetRandomVersionString();
                        var parts = result.Split('.');
                        Assert.AreEqual(
                            3,
                            parts.Length
                        );
                        Assert.IsTrue(parts.All(p => p.IsInteger()));
                        allResults.Add(result);
                    }
                );

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(allResults);
            }

            [Test]
            public void GivenPartsCount_ShouldReturnVersionWithThatManyParts()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var allResults = new List<string>();
                RunCycles(
                    () =>
                    {
                        var partCount = GetRandomInt(
                            2,
                            7
                        );
                        var result = GetRandomVersionString(partCount);
                        var parts = result.Split('.');
                        Assert.AreEqual(
                            partCount,
                            parts.Length
                        );
                        Assert.IsTrue(parts.All(p => p.IsInteger()));
                        allResults.Add(result);
                    }
                );

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(allResults);
            }

            [Test]
            public void ShouldReturnRandomDotNetVersionInfo()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var allResults = new List<Version>();
                RunCycles(
                    () =>
                    {
                        var result = GetRandomVersion();
                        allResults.Add(result);
                    }
                );

                //---------------Test Result -----------------------
                VarianceAssert.IsVariant(allResults);
            }
        }

        [TestFixture]
        public class GeneratingRandomPaths
        {
            [Test]
            public void ShouldProduceVariantWindowsPath()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var allResults = new List<string>();
                RunCycles(
                    () =>
                    {
                        var thisResult = GetRandomWindowsPath();
                        var parts = thisResult.Split('\\');
                        Expect(parts)
                            .To.Contain.At.Least(2).Items();
                        Expect(parts)
                            .To.Contain.At.Most(5).Items();
                        Expect(thisResult.Length)
                            .To.Be.Less.Than(
                                248,
                                () => $"path should work on windows, but this is too long:\n{thisResult}"
                            );
                        Expect(parts.First())
                            .To.Match(
                                new Regex(
                                    "^[A-Z]{1}:$"
                                )
                            );
                        allResults.Add(thisResult);
                    }
                );

                //---------------Test Result -----------------------
                Expect(allResults)
                    .To.Be.Distinct(() => allResults.Stringify());
            }
        }

        [TestFixture]
        public class CreatingRandomFolders
        {
            [Test]
            public void GivenExistingPath_ShouldCreateFolderInThere()
            {
                //---------------Set up test pack-------------------
                using (var folder = new AutoTempFolder())
                {
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = CreateRandomFolderIn(folder.Path);

                    //---------------Test Result -----------------------
                    Assert.IsNotNull(result);
                    Assert.IsTrue(
                        Directory.Exists(
                            Path.Combine(
                                folder.Path,
                                result
                            )
                        )
                    );
                }
            }


            [Test]
            public void GivenPath_ShouldCreateSomeRandomFoldersThereAndReturnTheRelativePaths()
            {
                //---------------Set up test pack-------------------
                using (var folder = new AutoTempFolder())
                {
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = CreateRandomFoldersIn(folder.Path);

                    //---------------Test Result -----------------------
                    Expect(result).Not.To.Be.Null();
                    Expect(result).Not.To.Be.Empty();
                    Expect(result).To.Have.Unique.Items();

                    Expect(result).To.Be.FoldersUnder(folder.Path);
                    Expect(result).To.Be.TheOnlyFoldersUnder(folder.Path);
                }
            }

            [Test]
            public void
                CreateRandomFoldersIn_GivenPathAndDepth_ShouldCreateSomeRandomFoldersThereAndReturnTheRelativePaths()
            {
                //---------------Set up test pack-------------------
                using (var folder = new AutoTempFolder())
                {
                    //---------------Assert Precondition----------------
                    var depth = GetRandomInt(
                        2,
                        3
                    );

                    //---------------Execute Test ----------------------
                    var result = CreateRandomFoldersIn(
                        folder.Path,
                        depth
                    );

                    //---------------Test Result -----------------------
                    Expect(result).Not.To.Be.Null();
                    Expect(result).Not.To.Be.Empty();
                    Expect(result).To.Be.FoldersUnder(folder.Path);
                    Expect(result).To.Have.Unique.Items();

                    var depths = result
                        .Select(r => r.Split(Path.DirectorySeparatorChar).Length)
                        .ToArray();
                    Expect(depths.All(d => d <= depth)).To.Be.True();
                }
            }
        }

        [TestFixture]
        public class CreatingRandomFiles
        {
            [Test]
            public void CreateRandomFileIn_GivenPath_ShouldReturnNameOfFileCreatedThereWithRandomContents()
            {
                //---------------Set up test pack-------------------
                using (var folder = new AutoTempFolder())
                {
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = CreateRandomFileIn(folder.Path);

                    //---------------Test Result -----------------------
                    Assert.IsNotNull(result);
                    Assert.IsTrue(
                        File.Exists(
                            Path.Combine(
                                folder.Path,
                                result
                            )
                        )
                    );
                    CollectionAssert.IsNotEmpty(
                        File.ReadAllBytes(
                            Path.Combine(
                                folder.Path,
                                result
                            )
                        )
                    );
                }
            }

            [Test]
            public void CreateRandomTextFileIn_GivenPath_ShouldReturnNameOfFileCreatedThereWithRandomContents()
            {
                //---------------Set up test pack-------------------
                using (var folder = new AutoTempFolder())
                {
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = CreateRandomTextFileIn(folder.Path);

                    //---------------Test Result -----------------------
                    Assert.IsNotNull(result);
                    Assert.IsTrue(
                        File.Exists(
                            Path.Combine(
                                folder.Path,
                                result
                            )
                        )
                    );
                    var lines = File.ReadAllLines(
                        Path.Combine(
                            folder.Path,
                            result
                        )
                    );
                    CollectionAssert.IsNotEmpty(lines);
                    Assert.IsTrue(
                        lines.All(
                            l =>
                            {
                                return l.All(c => !char.IsControl(c));
                            }
                        )
                    );
                }
            }

            [Test]
            public void CreateRandomFileTreeIn_GivenPath_ShouldCreateFilesAndFoldersAndReturnTheirRelativePaths()
            {
                //---------------Set up test pack-------------------
                using (var folder = new AutoTempFolder())
                {
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = CreateRandomFileTreeIn(folder.Path);

                    //---------------Test Result -----------------------
                    Assert.IsNotNull(result);
                    CollectionAssert.IsNotEmpty(result);
                    Assert.IsTrue(
                        result.Any(
                            r => PathExists(
                                Path.Combine(
                                    folder.Path,
                                    r
                                )
                            )
                        )
                    );
                    Assert.IsTrue(
                        result.Any(
                            r => File.Exists(
                                Path.Combine(
                                    folder.Path,
                                    r
                                )
                            )
                        ),
                        "No files found"
                    );
                    Assert.IsTrue(
                        result.Any(
                            r => Directory.Exists(
                                Path.Combine(
                                    folder.Path,
                                    r
                                )
                            )
                        ),
                        "No folders found"
                    );
                }
            }
        }

        [TestFixture]
        public class RangeCheckTimeOnRandomDate
        {
            [Test]
            public void WhenGivenDateWithTimeExceedingMaxTime_ShouldReturnDateWithTimeAtMaxTime()
            {
                RunCycles(
                    () =>
                    {
                        //---------------Set up test pack-------------------
                        var input = new DateTime(
                            2011,
                            1,
                            1,
                            12,
                            30,
                            0
                        );
                        var maxTime = new DateTime(
                            2011,
                            1,
                            1,
                            9,
                            30,
                            0
                        );

                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------
                        var result = RangeCheckTimeOnRandomDate(
                            null,
                            maxTime,
                            input
                        );

                        //---------------Test Result -----------------------
                        Assert.That(
                            result.TimeOfDay,
                            Is.LessThanOrEqualTo(maxTime.TimeOfDay)
                        );
                    }
                );
            }
        }

        public interface IInterfaceToGetRandomOf
        {
            string Name { get; }
        }

        public class ImplementingType : IInterfaceToGetRandomOf
        {
            public string Name { get; set; }
        }

        public interface IHasAnId
        {
            int Id { get; set; }
        }

        public class HasConstructorWithParameter
        {
            public string Parameter { get; }

            public HasConstructorWithParameter(
                string parameter
            )
            {
                Parameter = parameter;
            }
        }

        public class HasTwoConstructors
        {
            public bool ParameterlessConstructorUsed { get; }
            public string Parameter { get; }

            public HasTwoConstructors()
            {
                ParameterlessConstructorUsed = true;
            }

            public HasTwoConstructors(
                string parameter
            )
            {
                ParameterlessConstructorUsed = false;
                Parameter = parameter;
            }
        }

        public class PocoWithTimeSpan
        {
            public TimeSpan Moo { get; set; }
        }


        public class Parent
        {
            public string Name { get; set; }
            public ChildNode[] Children { get; set; }
        }

        public class ChildNode
        {
            public string Name { get; set; }
        }

        public class ParentBuilder : GenericBuilder<ParentBuilder, Parent>
        {
            public int WithChildrenCallCount { get; private set; }

            public override ParentBuilder WithRandomProps()
            {
                return base.WithRandomProps()
                    .WithRandomChildren();
            }

            public ParentBuilder WithRandomChildren()
            {
                return WithProp(
                    o => WithChildren(
                        GetRandomCollection<ChildNode>(
                            2,
                            4
                        ).ToArray()
                    )
                );
            }

            public ParentBuilder WithChildren(
                params ChildNode[] nodes
            )
            {
                WithChildrenCallCount++;
                return WithProp(o => o.Children = o.Children.EmptyIfNull().And(nodes));
            }
        }

        private static bool PathExists(
            string path
        )
        {
            return File.Exists(path) || Directory.Exists(path);
        }


        private static string BuildErrorMessageFor(
            IEnumerable<Tuple<string, int, int>> tooShort,
            IEnumerable<Tuple<string, int, int>> tooLong,
            IEnumerable<Tuple<string, int, int>> invalidCharacters
        )
        {
            var message = new List<string>();
            if (tooShort.Any())
            {
                message.Add(
                    string.Join(
                        "\n",
                        "Some results were too short:",
                        string.Join(
                            "\n\t",
                            tooShort.Take(5).Select(i => $"{i.Item1}  (<{i.Item2})")
                        )
                    )
                );
            }

            if (tooLong.Any())
            {
                message.Add(
                    string.Join(
                        "\n",
                        "Some results were too long:",
                        string.Join(
                            "\n\t",
                            tooLong.Take(5).Select(i => $"{i.Item1}  (>{i.Item3})")
                        )
                    )
                );
            }

            if (invalidCharacters.Any())
            {
                message.Add(
                    string.Join(
                        "\n",
                        "Some results contained invalid characters:",
                        string.Join(
                            "\n\t",
                            invalidCharacters.Take(5).Select(i => i.Item1)
                        )
                    )
                );
            }

            return message.JoinWith("\n");
        }


        private static void RunCycles(
            Action toRun,
            int? cycles = null
        )
        {
            cycles = cycles ?? NORMAL_RANDOM_TEST_CYCLES;
            for (var i = 0; i < NORMAL_RANDOM_TEST_CYCLES; i++)
                toRun();
        }

        private static string GetErrorHelpFor(
            IEnumerable<DateTime> outOfRangeLeft,
            IEnumerable<DateTime> outOfRangeRight,
            DateTime minTime,
            DateTime maxTime
        )
        {
            var message = "";
            if (outOfRangeLeft.Any())
            {
                message = string.Join(
                    "\n",
                    "One or more results had a time that was too early:",
                    "minTime: " + minTime.ToString("yyyy/MM/dd HH:mm:ss.ttt"),
                    "bad values: " + string.Join(
                        ",",
                        outOfRangeLeft.Take(5)
                    )
                );
            }

            if (outOfRangeRight.Any())
            {
                message += string.Join(
                    "\n",
                    "One or more results had a time that was too late:",
                    "maxTime: " + maxTime.ToString("yyyy/MM/dd HH:mm:ss.ttt"),
                    "bad values: " + string.Join(
                        ",",
                        outOfRangeLeft.Take(5)
                    )
                );
            }

            return message;
        }

        private class DateTimeRangeContainer
        {
            public DateTime From { get; }
            public DateTime To { get; }

            public DateTimeRangeContainer(
                int minYear,
                int minMonth,
                int minDay,
                int maxYear,
                int maxMonth,
                int maxDay
            )
            {
                From = new DateTime(
                    minYear,
                    minMonth,
                    minDay,
                    0,
                    0,
                    0
                );
                To = new DateTime(
                    maxYear,
                    maxMonth,
                    maxDay,
                    0,
                    0,
                    0
                );
                if (From > To)
                {
                    var swap = From;
                    From = To;
                    To = swap;
                }
            }

            public bool InRange(
                DateTime value
            )
            {
                return value >= From && value <= To;
            }
        }

        [TestFixture]
        public class GettingRandomHttpUrls
        {
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldReturnAValidUrl()
            {
                // Arrange
                // Pre-assert
                // Act
                var url = GetRandomHttpUrl();
                // Assert
                Expect(url)
                    .Not.To.Be.Null.Or.Empty();
                var uri = new Uri(url);
                Expect(uri.Scheme)
                    .To.Equal("http");
                Expect(uri.Query)
                    .To.Be.Empty();
                Expect(uri.AbsolutePath)
                    .To.Equal("/");
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldReturnUrlInLowerCaseOnly()
            {
                // Arrange
                // Pre-assert
                // Act
                var url = GetRandomHttpUrl();
                // Assert
                Expect(url)
                    .To.Equal(url.ToLowerInvariant());
                var uri = new Uri(url);
                Expect(uri.Scheme)
                    .To.Equal("http");
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldProvideUrlWithPathOnDemand()
            {
                // Arrange
                // Act
                var url = GetRandomHttpUrlWithPath();
                // Assert
                var uri = new Uri(url);
                Expect(uri.AbsolutePath)
                    .Not.To.Be.Empty();
                Expect(uri.Query)
                    .To.Be.Empty();
                Expect(uri.Scheme)
                    .To.Equal("http");
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldProvideUrlWithParametersAndNoPathOnDemand()
            {
                // Arrange
                // Act
                var url = GetRandomHttpUrlWithParameters();
                // Assert
                var uri = new Uri(url);
                Expect(uri.AbsolutePath)
                    .To.Equal("/");
                Expect(uri.Query)
                    .Not.To.Be.Empty();
                Expect(uri.Scheme)
                    .To.Equal("http");
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldProvideUrlWithParametersAndPathOnDemand()
            {
                // Arrange
                // Act
                var url = GetRandomHttpUrlWithPathAndParameters();
                // Assert
                var uri = new Uri(url);
                Expect(uri.AbsolutePath)
                    .Not.To.Equal("/");
                Expect(uri.Query)
                    .Not.To.Be.Empty();
                Expect(uri.Scheme)
                    .To.Equal("http");
            }
        }

        [TestFixture]
        public class GettingRandomHttpsUrls
        {
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldReturnAValidUrl()
            {
                // Arrange
                // Pre-assert
                // Act
                var url = GetRandomHttpsUrl();
                // Assert
                Expect(url)
                    .Not.To.Be.Null.Or.Empty();
                var uri = new Uri(url);
                Expect(uri.Scheme)
                    .To.Equal("https");
                Expect(uri.Query)
                    .To.Be.Empty();
                Expect(uri.AbsolutePath)
                    .To.Equal("/");
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldReturnUrlInLowerCaseOnly()
            {
                // Arrange
                // Pre-assert
                // Act
                var url = GetRandomHttpsUrl();
                // Assert
                Expect(url)
                    .To.Equal(url.ToLowerInvariant());
                var uri = new Uri(url);
                Expect(uri.Scheme)
                    .To.Equal("https");
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldProvideUrlWithPathOnDemand()
            {
                // Arrange
                // Act
                var url = GetRandomHttpsUrlWithPath();
                // Assert
                var uri = new Uri(url);
                Expect(uri.AbsolutePath)
                    .Not.To.Be.Empty();
                Expect(uri.Query)
                    .To.Be.Empty();
                Expect(uri.Scheme)
                    .To.Equal("https");
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldProvideUrlWithParametersAndNoPathOnDemand()
            {
                // Arrange
                // Act
                var url = GetRandomHttpsUrlWithParameters();
                // Assert
                var uri = new Uri(url);
                Expect(uri.AbsolutePath)
                    .To.Equal("/");
                Expect(uri.Query)
                    .Not.To.Be.Empty();
                Expect(uri.Scheme)
                    .To.Equal("https");
            }
        }

        [TestFixture]
        public class GettingRandomHttpUrlsWithParameters
        {
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldReturnAValidUrl()
            {
                // Arrange
                // Pre-assert
                // Act
                var url = GetRandomHttpUrlWithParameters();
                // Assert
                Expect(() => new Uri(url))
                    .Not.To.Throw();
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldReturnUrlInLowerCaseOnly()
            {
                // Arrange
                // Pre-assert
                // Act
                var url = GetRandomHttpUrlWithParameters();
                // Assert
                var uri = new Uri(url);
                var schemeHostPath = uri.ToString().Replace(
                    uri.Query,
                    ""
                );
                Expect(schemeHostPath)
                    .To.Equal(schemeHostPath.ToLowerInvariant());
            }

            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            [Test]
            public void ShouldHaveAtLeastOneParameter()
            {
                // Arrange
                // Pre-assert
                // Act
                var url = GetRandomHttpUrlWithParameters();
                // Assert
                var uri = new Uri(url);
                Expect(uri.Query).Not.To.Be.Null.Or.Empty();
                var parameters = uri.Query.Substring(1)
                    .Split(
                        new[]
                        {
                            "&"
                        },
                        StringSplitOptions.RemoveEmptyEntries
                    );
                Expect(parameters).Not.To.Be.Empty();
            }
        }

        [Test]
        public void GetRandomOfTShouldNotClobberStaticFields()
        {
            // Arrange
            var zero = TimeSpan.Zero;
            GetRandom<TimeSpan>();
            GetRandom<TimeSpan>();
            // Act
            Expect(TimeSpan.Zero)
                .To.Equal(zero);
            // Assert
        }

        [TestFixture]
        public class GetRandomPlaceName
        {
            [Test]
            public void ShouldReturnRandomisedData()
            {
                // Arrange
                var collected = new List<string>();
                // Act
                RunCycles(() => collected.Add(GetRandomPlaceName()));
                // Assert
                Expect(collected)
                    .To.Be.Mostly.Distinct();
            }
        }

        [TestFixture]
        public class WhenGivenLowerBoundOnly
        {
            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceIntWithinRange()
            {
                // Arrange
                var lowerBound = GetRandomInt(
                    1000,
                    10000
                );
                // Act
                var result = GetRandomInt(lowerBound);
                // Assert
                Expect(result)
                    .To.Be.Greater.Than
                    .Or.Equal.To(lowerBound);
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceLongWithinRange()
            {
                // Arrange
                var lowerBound = GetRandomLong(
                    1000,
                    10000
                );
                // Act
                var result = GetRandomLong(lowerBound);
                // Assert
                Expect(result)
                    .To.Be.Greater.Than
                    .Or.Equal.To(lowerBound);
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceDoubleWithinRange()
            {
                // Arrange
                var lowerBound = GetRandomDouble(
                    1000,
                    10000
                );
                // Act
                var result = GetRandomDouble(lowerBound);
                // Assert
                Expect(result)
                    .To.Be.Greater.Than
                    .Or.Equal.To(lowerBound);
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceDecimalWithinRange()
            {
                // Arrange
                var lowerBound = GetRandomDecimal(
                    1000,
                    10000
                );
                // Act
                var result = GetRandomDecimal(lowerBound);
                // Assert
                Expect(result)
                    .To.Be.Greater.Than
                    .Or.Equal.To(lowerBound);
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceDecimalWithinRangeForDoubleInput()
            {
                // Arrange
                var lowerBound = GetRandomDecimal(
                    1000.0,
                    10000.0
                );
                // Act
                var result = GetRandomDecimal(lowerBound);
                // Assert
                Expect(result)
                    .To.Be.Greater.Than
                    .Or.Equal.To(lowerBound);
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceDecimalWithinRangeForDecimalInput()
            {
                // Arrange
                var lowerBound = GetRandomDecimal(
                    1000.0M,
                    10000.0M
                );
                // Act
                var result = GetRandomDecimal(lowerBound);
                // Assert
                Expect(result)
                    .To.Be.Greater.Than
                    .Or.Equal.To(lowerBound);
            }

            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void ShouldProduceFloatWithinRange()
            {
                // Arrange
                var lowerBound = GetRandomFloat(
                    1000,
                    10000
                );
                // Act
                var result = GetRandomFloat(lowerBound);
                // Assert
                Expect(result)
                    .To.Be.Greater.Than
                    .Or.Equal.To(lowerBound);
            }
        }

        [TestFixture]
        public class GetRandomType
        {
            [Test]
            public void ShouldReturnARandomLoadedType()
            {
                // Arrange
                // Act
                var result1 = GetRandomType();
                var result2 = GetRandom<Type>();
                // Assert
                Expect(result1)
                    .Not.To.Be.Null();
                Expect(result1)
                    .To.Be.An.Instance.Of<Type>();
                Expect(result2)
                    .Not.To.Be.Null();
                Expect(result2)
                    .To.Be.An.Instance.Of<Type>();
            }

            [TestCase(nameof(SomePOCO.Price))]
            [TestCase(nameof(SomePOCO.Discount))]
            [TestCase(nameof(SomePOCO.Cost))]
            public void ShouldSetMonetaryValueFor_(
                string prop
            )
            {
                // Arrange
                // Act
                var result = GetRandom<SomePOCO>();
                // Assert
                var propValue = result.Get<decimal>(prop);
                var stringValue = $"{propValue}";
                var parts = stringValue.Split('.');
                if (parts.Length == 1)
                {
                    Expect(propValue)
                        .To.Equal((int)propValue);
                }
                else
                {
                    Expect(parts[1].Length)
                        .To.Be.At.Most(2);
                }

                Expect(propValue)
                    .To.Be.At.Least(DefaultRanges.MIN_MONEY_VALUE)
                    .And
                    .To.Be.At.Most(DefaultRanges.MAX_MONEY_VALUE);
            }

            [TestCase(nameof(SomePOCO.DiscountPercent))]
            [TestCase(nameof(SomePOCO.VATRate))]
            [TestCase(nameof(SomePOCO.TaxPercent))]
            [TestCase(nameof(SomePOCO.InterestPerc))]
            public void ShouldSetInterestOrTaxRateFor_(
                string prop
            )
            {
                // Arrange
                // Act
                var result = GetRandom<SomePOCO>();
                // Assert
                var propValue = result.Get<decimal>(prop);
                var stringValue = $"{propValue}";
                var parts = stringValue.Split('.');
                if (parts.Length == 1)
                {
                    Expect(propValue)
                        .To.Equal((int)propValue);
                }
                else
                {
                    Expect(parts[1].Length)
                        .To.Be.At.Most(2);
                }

                Expect(propValue)
                    .To.Be.At.Least(DefaultRanges.MIN_TAX_VALUE)
                    .And
                    .To.Be.At.Most(DefaultRanges.MAX_TAX_VALUE);
            }
        }

        [TestFixture]
        public class RandomEmails
        {
            [Test]
            public void ShouldProduceFairlyUniqueResults()
            {
                // Arrange
                var emails = new List<string>();
                // Act
                RunCycles(() => emails.Add(GetRandomEmail()));
                // Assert
                Expect(emails.Distinct())
                    .To.Contain.At.Least(emails.Count() / 2).Items();
                Expect(emails.Select(e => e.ToLower()))
                    .To.Equal(emails);
                emails.ForEach(email => Console.WriteLine(email));
            }
        }

        [TestFixture]
        public class RandomIPAddress
        {
            [Test]
            public void ShouldProduceRandomIpAddressValues()
            {
                // Arrange
                // Act
                var result1 = GetRandom<IPAddress>();
                var result2 = GetRandom<IPAddress>();
                // Assert
                Expect(result1.ToString())
                    .Not.To.Equal(result2.ToString());
            }
        }

        [TestFixture]
        public class RandomDictionaries
        {
            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void GenerateRandomDictionaryOfStringString()
            {
                // Arrange
                // Act
                var result = GetRandom<Dictionary<string, string>>();
                // Assert
                Expect(result)
                    .To.Contain.At.Least(1)
                    .Item();
                Expect(result.Values)
                    .To.Equal(result.Values.Distinct());
            }
        }

        [TestFixture]
        public class RandomNameValueCollection
        {
            [Test]
            [Repeat(NORMAL_RANDOM_TEST_CYCLES)]
            public void GenerateRandomDictionaryOfStringString()
            {
                // Arrange
                // Act
                var result = GetRandom<NameValueCollection>();
                // Assert
                Expect(result)
                    .To.Contain.At.Least(1)
                    .Item();
                var dict = result.ToDictionary();
                Expect(dict.Values)
                    .To.Equal(dict.Values.Distinct());
            }
        }
    }

    [TestFixture]
    public class WildIssues
    {
        [Test]
        public void ShouldPopulateLongAndNullableLongProps()
        {
            // Arrange
            var collected = new List<IMooCakes>();
            // Act
            for (var i = 0; i < NORMAL_RANDOM_TEST_CYCLES; i++)
            {
                collected.Add(GetRandom<IMooCakes>());
            }

            // Assert
            // random values for long include zero (the default of long)
            // -> so we just need to affirm that that's not _always_ set
            Expect(collected)
                .To.Contain.Any
                .Matched.By(o => o.Id != 0);
            var first = collected.First();
            Expect(first.ReferenceId)
                .Not.To.Be.Null();
            Expect(first.OverallRunTime)
                .Not.To.Be.Null();
            Expect(first.StepRunTime)
                .Not.To.Be.Null();
        }

        public interface IMooCakes
        {
            long Id { get; set; }

            string Channel { get; set; }

            Guid UniqueIdentifier { get; set; }

            long? ReferenceId { get; set; }

            string Type { get; set; }

            string Info { get; set; }

            string Meta { get; set; }

            DateTime DateTime { get; set; }

            long? OverallRunTime { get; set; }

            long? StepRunTime { get; set; }
        }
    }

    internal static class Matchers
    {
        internal static IMore<IEnumerable<T>> Vary<T>(
            this ICollectionTo<T> continuation
        )
        {
            return continuation.AddMatcher(
                actual =>
                {
                    if (actual is null)
                    {
                        return new EnforcedMatcherResult(
                            false,
                            "collection is null"
                        );
                    }

                    if (actual.Count() < 2)
                    {
                        return new EnforcedMatcherResult(
                            false,
                            "collection must contain at least 2 items to test variance"
                        );
                    }

                    var distinctCount = actual.Distinct().Count();
                    var passed = distinctCount > 1;

                    return new MatcherResult(
                        passed,
                        () => $"Expected some variance in the collection:\n{actual.Stringify()}"
                    );
                }
            );
        }

        internal static void FoldersUnder(
            this ICollectionBe<string> be,
            string basePath
        )
        {
            be.Compose(
                actual =>
                {
                    actual.ForEach(
                        sub =>
                            Expect(
                                Path.Combine(
                                    basePath,
                                    sub
                                )
                            ).To.Be.A.Folder()
                    );
                }
            );
        }

        internal static void TheOnlyFoldersUnder(
            this ICollectionBe<string> be,
            string folder
        )
        {
            be.Compose(
                actual =>
                {
                    var existing = Directory.EnumerateDirectories(
                            folder,
                            "*",
                            SearchOption.AllDirectories
                        )
                        .Select(
                            p => p.Substring(folder.Length + 1)
                        );
                    Expect(existing).To.Be.Equivalent.To(actual);
                }
            );
        }

        internal static void Folder(
            this IA<string> a
        )
        {
            a.Compose(
                path =>
                    Expect(Directory.Exists(path)).To.Be.True()
            );
        }
    }

    public class SomePOCO
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string StreetAddress { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string CountryCode { get; set; }
        public DateTime? Date { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal Tax { get; set; }
        public decimal VATRate { get; set; }
        public decimal? InterestPerc { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal? Discount { get; set; }
        public decimal DiscountPercent { get; set; }

        public override string ToString()
        {
            var parts = new[]
            {
                $"Id: {Id}",
                $"Name: {Name}",
                $"FirstName: {FirstName}",
                $"LastName: {LastName}",
                $"Login: {Login}",
                $"Email: {Email}",
                $"Street: {StreetAddress}",
                $"City: {City}",
                $"PostalCode: {PostalCode}",
                $"Address: {Address}",
                $"Country: {Country}",
                $"Country Code: {CountryCode}"
            };
            return string.Join(
                Environment.NewLine,
                parts
            );
        }
    }

    public class SomePOCOBuilder : GenericBuilder<SomePOCOBuilder, SomePOCO>
    {
    }
}