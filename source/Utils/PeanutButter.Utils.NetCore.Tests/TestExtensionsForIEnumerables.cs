// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestExtensionsForIEnumerables
{
    [TestFixture]
    public class AsHashSet
    {
        [Test]
        public void ShouldProduceAnHashSet()
        {
            // Arrange
            var ints = GetRandomArray<int>(10, 20);

            // Act
            var result = ints.AsHashSet();
            // Assert
            Expect(result)
                .To.Be.An.Instance.Of<HashSet<int>>();
            Expect(result)
                .To.Contain.All.Of(ints);
        }
    }

    [TestFixture]
    public class ForEach
    {
        [TestFixture]
        public class OperatingOnNull
        {
            [Test]
            public void ShouldThrowArgumentNullException()
            {
                // This may seem pointless, but bear with me:
                //  I actually wanted ForEach to just do nothing when given a null collection,
                //  however System.Collections.Generic.List<T> has its own ForEach signature
                //  which doesn't even throw a valid ArgumentNullException -- it just barfs
                //  with a NullReferenceException. So if I made mine different, it would be
                //  inconsistent and surprising. And extension methods can't override actual
                //  methods. So, poo. This test then serves as documentation that I thought
                //  about it and chose consistency over function. You can, however, use
                //  the helper EmptyIfNull() to get the nicer behaviour:
                //  int[] foo = null;
                //  foo.EmptyIfNull().ForEach(i => {});
                //---------------Set up test pack-------------------
                int[] src = null;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(
                    () => src.ForEach(
                        _ =>
                        {
                        }
                    )
                ).To.Throw<NullReferenceException>();

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class OperatingOnCollection
        {
            [Test]
            public void ShouldRunActionOnEachElement()
            {
                //---------------Set up test pack-------------------
                var result = new List<int>();
                var src = new[]
                {
                    GetRandomInt(),
                    GetRandomInt(),
                    GetRandomInt()
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                src.ForEach(item => result.Add(item));

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(src);
            }
        }

        [TestFixture]
        public class GivenActionTakingIndex
        {
            [Test]
            public void ShouldPopulateSecondParameterWithIndex()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomArray<int>(5, 15);
                var collectedIndexes = new List<int>();
                var collectedItems = new List<int>();
                var expectedIndexes = input.Select((_, i) => i).ToList();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                input.ForEach(
                    (item, idx) =>
                    {
                        collectedItems.Add(item);
                        collectedIndexes.Add(idx);
                    }
                );

                //---------------Test Result -----------------------
                Expect(collectedItems)
                    .To.Equal(input);
                Expect(collectedIndexes)
                    .To.Equal(expectedIndexes);
            }
        }
    }

    [TestFixture]
    public class IsSameAs
    {
#pragma warning disable CS0618
        [TestFixture]
        public class WhenBothAreNull
        {
            [Test]
            public void ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                List<int> first = null;
                List<int> second = null;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = first.IsSameAs(second);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenOneIsNull
        {
            [Test]
            public void ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                List<int> first = null;
                var second = new List<int>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = first.IsSameAs(second);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenBothAreEmpty
        {
            [Test]
            public void ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var first = new int[]
                {
                };
                var second = new List<int>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = first.IsSameAs(second);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenBothContainSameElementsInOrder
        {
            [Test]
            public void ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var first = new[]
                {
                    1,
                    2
                };
                var second = new List<int>(
                    new[]
                    {
                        1,
                        2
                    }
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = first.IsSameAs(second);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenBothContainSameElementsOutOfOrder
        {
            [Test]
            public void ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var first = new[]
                {
                    1,
                    2
                };
                var second = new List<int>(
                    new[]
                    {
                        2,
                        1
                    }
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = first.IsSameAs(second);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenBothContainDifferentElements
        {
            [Test]
            public void ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var first = new[]
                {
                    1
                };
                var second = new List<int>(
                    new[]
                    {
                        2,
                        1
                    }
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = first.IsSameAs(second);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.False();
            }
        }
#pragma warning restore CS0618
    }

    [TestFixture]
    public class JoinWith
    {
        [Test]
        public void ShouldReturnEmptyStringForNullCollection()
        {
            // Arrange
            var collection = null as int[];
            // Act
            var result = collection.JoinWith("");
            // Assert
            Expect(result)
                .To.Equal("");
        }

        [TestFixture]
        public class OperatingOnStringCollection
        {
            [TestFixture]
            public class WhenCollectionIsEmpty
            {
                [Test]
                public void ShouldReturnEmptyString()
                {
                    //---------------Set up test pack-------------------
                    var src = new List<string>();

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = src.JoinWith(",");

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Be.Empty();
                }
            }

            [TestFixture]
            public class WhenCollectionHasItems
            {
                [Test]
                public void ShouldReturnJoinedString()
                {
                    // Arrange
                    var src = new[]
                    {
                        "a",
                        "b",
                        "c"
                    };
                    var delimiter = ";";
                    var expected = "a;b;c";

                    // Act
                    var result = src.JoinWith(delimiter);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }
        }

        [TestFixture]
        public class OperatingOnNumericCollection
        {
            [TestFixture]
            public class WhenCollectionIsEmpty
            {
                [Test]
                public void ShouldReturnEmptyString()
                {
                    //---------------Set up test pack-------------------
                    var src = new List<int>();

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = src.JoinWith(",");

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Be.Empty();
                }
            }

            [TestFixture]
            public class WhenCollectionIsNotEmpty
            {
                [Test]
                public void ShouldReturnCollectionJoinedWithGivenDelimiter()
                {
                    //---------------Set up test pack-------------------
                    var src = new[]
                    {
                        1,
                        2,
                        3
                    };
                    var delimiter = GetRandomString(2, 3);
                    var expected = "1" + delimiter + "2" + delimiter + "3";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = src.JoinWith(delimiter);

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Equal(expected);
                }
            }
        }

        [TestFixture]
        public class OperatingOnObjects
        {
            [Test]
            public void ShouldReturnValuesConvertedToStringsJoinedWithDelimiter()
            {
                // Arrange
                var items = GetRandomArray<Person>(2, 3);
                var delimiter = ";;";
                var expected = "";
                foreach (var item in items)
                {
                    if (expected.Length > 0)
                    {
                        expected += delimiter;
                    }

                    expected += item.ToString();
                }

                // Act
                var result = items.JoinWith(delimiter);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldNotBreakOnNullValues()
            {
                // Arrange
                var items = GetRandomArray<Person>(3, 4);
                items[1] = null;
                var delimiter = ";;";
                var expected = "";
                foreach (var item in items)
                {
                    if (expected.Length > 0)
                    {
                        expected += delimiter;
                    }

                    expected += $"{item}";
                }

                // Act
                var result = items.JoinWith(delimiter);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            public class Person
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public override string ToString()
                {
                    return $"{Id}::{Name}";
                }
            }
        }
    }

    [TestFixture]
    public class EmptyIfNull
    {
        [TestFixture]
        public class WhenCollectionIsNull
        {
            [Test]
            public void ShouldReturnNewEmptyCollection()
            {
                //---------------Set up test pack-------------------
                List<int> src = null;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result1 = src.EmptyIfNull();
                var result2 = src.EmptyIfNull();

                //---------------Test Result -----------------------
                Expect(result1)
                    .Not.To.Be.Null();
                Expect(result1)
                    .To.Be.An.Instance.Of<int[]>();
                Expect(result1)
                    .To.Be.Empty();
                Expect(result2)
                    .To.Be.Empty();
                Expect(result2)
                    .Not.To.Be(result1);
            }
        }

        [TestFixture]
        public class WhenCollectionIsNotNull
        {
            [Test]
            public void ShouldReturnOriginalCollection()
            {
                // Arrange
                var collection = GetRandomCollection<int>();
                // Act
                var result = collection.EmptyIfNull();
                // Assert
                Expect(result)
                    .To.Be(collection);
            }
        }
    }

    [TestFixture]
    public class And
    {
        [Test]
        public void ShouldReturnNewArrayWithAddedItems()
        {
            //---------------Set up test pack-------------------
            var src = new[]
            {
                1,
                2,
                3
            };
            var expected = new[]
            {
                1,
                2,
                3,
                4,
                5
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.And(4, 5);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToAcceptIEnumerable()
        {
            // Arrange
            var src = new[]
            {
                1,
                2,
                3
            };
            // Act
            var result = src.And(Generator());
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        1,
                        2,
                        3,
                        4,
                        5
                    }
                );

            IEnumerable<int> Generator()
            {
                yield return 4;
                yield return 5;
            }
        }

        [Test]
        public void ShouldReturnNewArrayWithAllAddedItems()
        {
            //---------------Set up test pack-------------------
            var src = new[]
            {
                1,
                2,
                3,
                4,
                5
            };
            var expected = new[]
            {
                1,
                2,
                3,
                4,
                5,
                4,
                5
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.And(4, 5);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }

        [TestFixture]
        public class OperatingOnList
        {
            [Test]
            public void ShouldReturnTheList()
            {
                // Arrange
                var list = new List<string>();
                // Act
                var next = list.And("foo");
                // Assert
                Expect(next)
                    .Not.To.Be(list);
                Expect(next)
                    .To.Equal(
                        new[]
                        {
                            "foo"
                        }
                    );
            }

            [Test]
            public void ShouldAcceptIEnumerable()
            {
                // Arrange
                var src = new List<int>(
                    new[]
                    {
                        1,
                        2,
                        3
                    }
                );
                // Act
                var result = src.And(Generator());
                // Assert
                Expect(result)
                    .To.Equal(
                        new[]
                        {
                            1,
                            2,
                            3,
                            4,
                            5
                        }
                    );

                IEnumerable<int> Generator()
                {
                    yield return 4;
                    yield return 5;
                }
            }
        }

        [TestFixture]
        public class OperatingOnListInterface
        {
            [Test]
            public void ShouldReturnTheList()
            {
                // Arrange
                var list = new List<string>() as IList<string>;
                // Act
                var next = list.And("foo");
                // Assert
                Expect(next)
                    .To.Be(list);
                Expect(next)
                    .To.Equal(
                        new[]
                        {
                            "foo"
                        }
                    );
            }
        }

        [TestFixture]
        public class OperatingOnIEnumerable
        {
            IEnumerable<string> MakeEnumerable()
            {
                yield return "foo";
            }

            [Test]
            public void ShouldReturnArray()
            {
                // Arrange
                var list = MakeEnumerable();
                // Act
                var result = list.And("another");
                // Assert
                Expect(result)
                    .To.Equal(
                        new[]
                        {
                            "foo",
                            "another"
                        }
                    );
                Expect(result)
                    .To.Be.An.Instance.Of<string[]>();
            }
        }
    }

    [TestFixture]
    public class ButNot
    {
        [Test]
        public void ShouldReturnNewArrayWithoutRemovedItems()
        {
            //---------------Set up test pack-------------------
            var src = new[]
            {
                1,
                2,
                3
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.ButNot(2);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        1,
                        3
                    }
                );
        }
    }

    [TestFixture]
    public class Flatten
    {
        [TestFixture]
        public class OperatingOnEmptyCollection
        {
            [Test]
            public void ShouldReturnEmptyCollection()
            {
                //---------------Set up test pack-------------------
                var input = new List<List<int>>();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.Flatten();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class WhenOneItemInSubCollection
        {
            [Test]
            public void ShouldReturnFlattened()
            {
                //---------------Set up test pack-------------------
                var input = new List<IEnumerable<int>>();
                var expected = GetRandomArray<int>(1, 1);
                input.Add(expected);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.Flatten();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenMultipleItemsInSubCollections
        {
            [Test]
            public void ShouldReturnFlattened()
            {
                //---------------Set up test pack-------------------
                var input = new List<IEnumerable<int>>();
                var part1 = GetRandomArray<int>();
                var part2 = GetRandomArray<int>();
                var part3 = GetRandomArray<int>();
                var expected = new List<int>();
                expected.AddRange(part1);
                expected.AddRange(part2);
                expected.AddRange(part3);
                input.AddRange(
                    new[]
                    {
                        part1,
                        part2,
                        part3
                    }
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.Flatten();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.Equivalent.To(expected);
            }
        }
    }

    [TestFixture]
    public class Second
    {
        [TestFixture]
        public class WhenOnlyHaveOneItemInCollection
        {
            [Test]
            public void ShouldThrow()
            {
                //---------------Set up test pack-------------------
                var input = new[]
                {
                    1
                };
                var expectedMessage = ReadDefaultOutOfRangeMessage();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(() => input.Second())
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Equal.To(expectedMessage);

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class WhenTwoOrMoreItemsInCollection
        {
            [Test]
            public void ShouldReturnSecond()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomArray<string>(2);
                var expected = collection[1];

                //---------------Assert Precondition----------------
                Assert.That(collection.Count(), Is.GreaterThanOrEqualTo(2));

                //---------------Execute Test ----------------------
                var item = collection.Second();

                //---------------Test Result -----------------------
                Expect(item)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class Third
    {
        [TestFixture]
        public class WhenCollectionLessThanThreeItems
        {
            [TestCase(1)]
            [TestCase(2)]
            public void ShouldThrowFor_(int howMany)
            {
                //---------------Set up test pack-------------------
                var input = GetRandomArray<int>(howMany, howMany);
                var expectedMessage = ReadDefaultOutOfRangeMessage();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(() => input.Third())
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Equal.To(expectedMessage);

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class WhenThreeOrMoreItemsInCollection
        {
            [Test]
            public void ShouldReturnThird()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomArray<string>(3);
                var expected = collection[2];

                //---------------Assert Precondition----------------
                Expect(collection)
                    .To.Contain.At.Least(3)
                    .Items();

                //---------------Execute Test ----------------------
                var item = collection.Third();

                //---------------Test Result -----------------------
                Expect(item)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class Fourth
    {
        [TestFixture]
        public class WhenCollectionLessThanFourItems
        {
            [TestCase(1)]
            [TestCase(2)]
            [TestCase(3)]
            public void ShouldThrowFor_(int howMany)
            {
                //---------------Set up test pack-------------------
                var input = GetRandomArray<int>(howMany, howMany);
                var expectedMessage = ReadDefaultOutOfRangeMessage();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(() => input.Fourth())
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Equal.To(expectedMessage);

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class WhenThreeOrMoreItemsInCollection
        {
            [Test]
            public void ShouldReturnThird()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomArray<string>(4);
                var expected = collection[3];

                //---------------Assert Precondition----------------
                Expect(collection)
                    .To.Contain.At.Least(3)
                    .Items();

                //---------------Execute Test ----------------------
                var item = collection.Fourth();

                //---------------Test Result -----------------------
                Expect(item)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class Nth
    {
        [TestFixture]
        public class WhenNOrMoreItemsInCollection
        {
            [Test]
            public void ShouldReturnTheItemAtThatIndex()
            {
                // Arrange
                var n = GetRandomInt(5, 15);
                var collection = GetRandomArray<string>(n + 1);
                var expected = collection[n];

                // Act
                var result = collection.Nth(n);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class GivenNIsZero
        {
            [Test]
            public void ShouldReturnTheZerothItemInTheCollection()
            {
                // Arrange
                var collection = GetRandomArray<string>(1);
                var expected = collection[0];
                // Act
                var result = collection.Nth(0);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenNExceedsCollectionMaxIndex
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var n = GetRandomInt(5, 10);
                var collection = GetRandomArray<string>(n, n);
                // Act
                Expect(() => collection.Nth(n))
                    .To.Throw<InvalidOperationException>();
                // Assert
            }
        }
    }

    [TestFixture]
    public class At
    {
        [TestFixture]
        public class WhenNOrMoreItemsInCollection
        {
            [Test]
            public void ShouldReturnTheItemAtThatIndex()
            {
                // Arrange
                var n = GetRandomInt(5, 15);
                var collection = GetRandomArray<string>(n + 1);
                var expected = collection[n];

                // Act
                var result = collection.At(n);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class GivenNIsZero
        {
            [Test]
            public void ShouldReturnTheZerothItemInTheCollection()
            {
                // Arrange
                var collection = GetRandomArray<string>(1);
                var expected = collection[0];
                // Act
                var result = collection.At(0);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenNExceedsCollectionMaxIndex
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var n = GetRandomInt(5, 10);
                var collection = GetRandomArray<string>(n, n);
                // Act
                Expect(() => collection.At(n))
                    .To.Throw<InvalidOperationException>();
                // Assert
            }
        }
    }

    [TestFixture]
    public class FirstAfter
    {
        [TestFixture]
        public class WhenCollectionIsInsufficient
        {
            [Test]
            public void ShouldThrow()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomArray<int>(2, 5);
                var skip = GetRandomInt(6, 100);
                var expectedMessage = ReadDefaultOutOfRangeMessage();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(() => collection.FirstAfter(skip))
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Equal.To(expectedMessage);

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class WhenSkipIsZero
        {
            [Test]
            public void ShouldReturnFirstElement()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomCollection<int>(10, 20).ToArray();
                var expected = collection[0];

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = collection.FirstAfter(0);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenCollectionIsSufficientForRequestedIndex
        {
            [Test]
            public void ShouldReturnRequestedElement()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomCollection<int>(10, 20).ToArray();
                var skip = GetRandomInt(2, 8);
                var expected = collection[skip];

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = collection.FirstAfter(skip);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class FirstOrDefaultAfter
    {
        [TestFixture]
        public class WhenIntCollectionIsInsufficient
        {
            [TestCase(default(int))]
            public void ShouldReturn_(int expected)
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomCollection<int>(2, 5);
                var skip = GetRandomInt(6, 100);

                //---------------Assert Precondition----------------
                Assert.That(skip, Is.GreaterThan(collection.Count()));

                //---------------Execute Test ----------------------
                var result = collection.FirstOrDefaultAfter(skip);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenRefTypeCollectionIsInsufficient
        {
            [Test]
            public void ShouldReturnDefaultForType()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomCollection<SomeType>(2, 5);
                var skip = GetRandomInt(6, 100);
                var expected = default(SomeType);

                //---------------Assert Precondition----------------
                Assert.That(skip, Is.GreaterThan(collection.Count()));

                //---------------Execute Test ----------------------
                var result = collection.FirstOrDefaultAfter(skip);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }

            public class SomeType;
        }


        [TestFixture]
        public class WhenCollectionIsSufficient
        {
            [Test]
            public void ShouldReturnRequestedElement()
            {
                //---------------Set up test pack-------------------
                var collection = GetRandomArray<int>(10, 20);
                var skip = GetRandomInt(2, 8);
                var expected = collection[skip];

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = collection.FirstOrDefaultAfter(skip);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    private static string ReadDefaultOutOfRangeMessage()
    {
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        var reference = Assert.Throws<InvalidOperationException>(() => new int[0].First());
        return reference?.Message ?? throw new Exception("Message not retrieved");
    }


    private class ItemWithNullableId
    {
        // ReSharper disable once MemberCanBePrivate.Local
        public int? Id { get; set; }

        public static ItemWithNullableId For(int? value)
        {
            return new ItemWithNullableId()
            {
                Id = value
            };
        }
    }

    [TestFixture]
    public class SelectNonNull
    {
        [Test]
        public void ShouldReturnOnlyNonNullValues()
        {
            //---------------Set up test pack-------------------
            var id1 = GetRandomInt();
            var id2 = GetRandomInt();
            var expected = new[]
            {
                id1,
                id2
            };
            var input = new[]
            {
                ItemWithNullableId.For(id1),
                ItemWithNullableId.For(null),
                ItemWithNullableId.For(id2),
                ItemWithNullableId.For(null)
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.SelectNonNull(o => o.Id);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }


        public class Thing;

        public class ItemWithNullableThing
        {
            public Thing Thing { get; set; }

            public static ItemWithNullableThing For(Thing thing)
            {
                return new ItemWithNullableThing()
                {
                    Thing = thing
                };
            }
        }

        [Test]
        public void ShouldReturnOnlyNonNullValues2()
        {
            //---------------Set up test pack-------------------
            var id1 = GetRandom<Thing>();
            var id2 = GetRandom<Thing>();
            var expected = new[]
            {
                id1,
                id2
            };
            var input = new[]
            {
                ItemWithNullableThing.For(id1),
                ItemWithNullableThing.For(null),
                ItemWithNullableThing.For(id2),
                ItemWithNullableThing.For(null)
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.SelectNonNull(o => o.Thing);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class AsText
    {
        [TestFixture]
        public class OperatingOnStringArray
        {
            [Test]
            public void ShouldReturnTextBlockWithEnvironmentNewlines()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomArray<string>(2, 4);
                var expected = string.Join(Environment.NewLine, input);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsText();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class OperatingOnOtherTypes
        {
            [Test]
            public void ShouldReturnTextBlockWithStringRepresentations()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomList<SomethingWithNiceToString>(2, 4);
                var expected = string.Join(Environment.NewLine, input);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsText();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }

            public class SomethingWithNiceToString
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public override string ToString()
                {
                    return $"{Id} :: {Name}";
                }
            }
        }

        [TestFixture]
        public class WhenGivenAlternativeDelimiter
        {
            [Test]
            public void ShouldUseIt()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomArray<int>(3, 6);
                var delimiter = GetRandomString(1);
                var expected = string.Join(delimiter, input);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsText(delimiter);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class AsTextList
    {
        [TestFixture]
        public class WhenEmpty
        {
            [Test]
            public void ShouldReturnEmpty()
            {
                // Arrange
                var input = new string[0];
                // Act
                var result = input.AsTextList();
                // Assert
                Expect(result)
                    .To.Equal("<empty>");
            }
        }

        [TestFixture]
        public class WhenHaveOneItem
        {
            [Test]
            public void ShouldReturnListOfOne()
            {
                // Arrange
                var input = new[]
                {
                    "cat"
                };
                var expected = "- cat";
                // Act
                var result = input.AsTextList();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenHaveMultipleItems
        {
            [Test]
            public void ShouldReturnList()
            {
                // Arrange
                var input = new[]
                {
                    "cat",
                    "dog",
                    "cow"
                };
                var expected = "- cat\n- dog\n- cow";
                // Act
                var result = input.AsTextList();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class AsTextListWithHeader
    {
        [TestFixture]
        public class WhenEmpty
        {
            [Test]
            public void ShouldReturnEmpty()
            {
                // Arrange
                var input = new string[0];
                // Act
                var result = input.AsTextListWithHeader("items", "- ", "");
                // Assert
                Expect(result)
                    .To.Equal("");
            }
        }

        [TestFixture]
        public class WhenHaveOneItem
        {
            [Test]
            public void ShouldReturnListOfOne()
            {
                // Arrange
                var input = new[]
                {
                    "cat"
                };
                var expected = "items\n- cat";
                // Act
                var result = input.AsTextListWithHeader("items", "- ", "");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenHaveMultipleItems
        {
            [Test]
            public void ShouldReturnList()
            {
                // Arrange
                var input = new[]
                {
                    "cat",
                    "dog",
                    "cow"
                };
                var expected = "items\n- cat\n- dog\n- cow";
                // Act
                var result = input.AsTextListWithHeader("items", "- ", "");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class IsIn
    {
        [TestFixture]
        public class WhenSubjectIsNotInCollection
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var test = GetRandomArray<string>(4);
                var search = GetAnother(test);
                // Act
                var result = search.IsIn(test);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldWorkOnNull()
            {
                // Arrange
                var test = GetRandomArray<string>(4);
                // Act
                var result = (null as string).IsIn(test);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenSubjectIsInCollection
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var test = GetRandomArray<string>(4);
                var search = GetRandomFrom(test);
                // Act
                var result = search.IsIn(test);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [Test]
        [Explicit("Run to see that one-off hashsets are slower than just using .Contains")]
        public void HashSetSpeedVsLinqContains()
        {
            // Arrange
            var size = 1000000;
            var data = GetRandomArray<string>(size);
            var foo = GetRandomString();
            // ReSharper disable once NotAccessedVariable
            bool result;
            // Act
            var hashSetTime = Benchmark.Time(
                () =>
                {
                    var hashSet = new HashSet<string>(data);
                    result = hashSet.Contains(foo);
                }
            );
            var containsTime = Benchmark.Time(
                () =>
                {
                    result = data.Contains(foo);
                }
            );
            Console.WriteLine($"contains: {containsTime}");
            Console.WriteLine($"hashset:  {hashSetTime}");
            // Assert
        }
    }

    [TestFixture]
    public class HasUnique
    {
        [TestFixture]
        public class WhenNoMatch
        {
            [Test]
            public void ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var input = new[]
                {
                    1,
                    2,
                    3
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.HasUnique(i => i == 4);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenHaveMultipleMatches
        {
            [Test]
            public void ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var input = new[]
                {
                    "a",
                    "a",
                    "b",
                    "c"
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.HasUnique(i => i == "a");

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenOnlyOneMatch
        {
            [Test]
            public void ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var input = new[]
                {
                    "a",
                    "a",
                    "b",
                    "c"
                };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.HasUnique(i => i == "b");

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.True();
            }
        }
    }

    [TestFixture]
    public class TimesDo
    {
        [TestFixture]
        public class OperatingOnZero
        {
            [Test]
            public void ShouldNotRunAction()
            {
                //---------------Set up test pack-------------------
                var calls = 0;
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                0.TimesDo(() => calls++);

                //---------------Test Result -----------------------
                Expect(calls)
                    .To.Equal(0);
            }
        }

        [TestFixture]
        public class OperatingOnPositiveInteger
        {
            [Test]
            public void ShouldRunActionThatManyTimes()
            {
                //---------------Set up test pack-------------------
                var calls = 0;
                var howMany = GetRandomInt(1, 20);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                howMany.TimesDo(() => calls++);

                //---------------Test Result -----------------------
                Expect(calls)
                    .To.Equal(howMany);
            }

            [TestFixture]
            public class GivenActionAcceptingIndex
            {
                [Test]
                public void ShouldFeedIndex()
                {
                    //---------------Set up test pack-------------------
                    var howMany = GetRandomInt();
                    var result = new List<int>();
                    var expected = new List<int>();
                    for (var i = 0; i < howMany; i++)
                        expected.Add(i);
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    howMany.TimesDo(i => result.Add(i));

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Equal(expected);
                }
            }
        }

        [TestFixture]
        public class OperatingOnNegativeInteger
        {
            [Test]
            public void ShouldThrowArgumentException()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(
                        () => (-1).TimesDo(
                            () =>
                            {
                            }
                        )
                    ).To.Throw<ArgumentException>()
                    .For("howMany")
                    .With.Message.Containing("positive integer");

                //---------------Test Result -----------------------
            }

            [TestFixture]
            public class GivenActionAcceptingIndex
            {
                [Test]
                public void ShouldThrowArgumentException()
                {
                    //---------------Set up test pack-------------------

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    Expect(
                            () => (-1).TimesDo(
                                _ =>
                                {
                                }
                            )
                        ).To.Throw<ArgumentException>()
                        .For("howMany")
                        .With.Message.Containing("positive integer");

                    //---------------Test Result -----------------------
                }
            }
        }
    }

    [TestFixture]
    public class None
    {
        [TestFixture]
        public class WithTestFn
        {
            [Test]
            public void ShouldReturnTrueForNullCollection()
            {
                // Arrange
                var input = null as IEnumerable<int>;
                // Pre-assert
                // Act
                var result = input.None(i => i == 1);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueForEmptyCollection()
            {
                // Arrange
                var input = new int[0];
                // Pre-assert
                // Act
                var result = input.None(i => i == GetRandomInt());
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenAllFail()
            {
                // Arrange
                var input = new[]
                {
                    1,
                    2,
                    3
                };
                // Pre-assert
                // Act
                var result = input.None(i => i > 4);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseWhenAnyOnePasses()
            {
                // Arrange
                var input = new[]
                {
                    2,
                    4,
                    6,
                    7
                };
                // Pre-assert
                // Act
                var result = input.None(i => i % 2 == 1);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WithNoTestFn
        {
            [Test]
            public void ShouldReturnTrueForNullCollection()
            {
                // Arrange
                var input = null as IEnumerable<int>;
                // Act
                var result = input.None();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldBeTrueForEmptyCollection()
            {
                // Arrange
                var input = new int[0];
                // Act
                var result = input.None();
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldBeFalseForAnySizeCollection()
            {
                // Arrange
                var input = GetRandomArray<int>(1);
                // Act
                var result = input.None();
                // Assert
                Expect(result).To.Be.False();
            }
        }
    }

    [TestFixture]
    public class FindDuplicates
    {
        [TestFixture]
        public class WithImpliedDiscriminator
        {
            [TestFixture]
            public class OperatingOnEmptyCollection
            {
                [Test]
                public void ShouldReturnEmptyCollection()
                {
                    // Arrange
                    var input = new int[0];
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates();
                    // Assert
                    Expect(result)
                        .To.Be.Empty();
                }
            }

            [TestFixture]
            public class OperatingOnCollectionOfOne
            {
                [Test]
                public void ShouldReturnEmptyCollection()
                {
                    // Arrange
                    var input = new[]
                    {
                        GetRandomInt()
                    };
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates();
                    // Assert
                    Expect(result)
                        .To.Be.Empty();
                }
            }

            [TestFixture]
            public class OperatingOnDistinctCollection
            {
                [Test]
                public void ShouldReturnEmptyCollection()
                {
                    // Arrange
                    var input = GetRandomCollection<string>(10, 20).Distinct();
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates();
                    // Assert
                    Expect(result)
                        .To.Be.Empty();
                }
            }

            [TestFixture]
            public class OperatingOnCollectionWithDuplicates
            {
                [Test]
                public void ShouldReturnDuplicatesOnceEach()
                {
                    // Arrange
                    var input = new[]
                    {
                        1,
                        2,
                        3,
                        3,
                        3,
                        2
                    };
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates();
                    // Assert
                    Expect(result)
                        .To.Contain.Exactly(2).Items();
                    Expect(result)
                        .To.Be.Equivalent.To(
                            new[]
                            {
                                2,
                                3
                            }
                        );
                }
            }
        }

        [TestFixture]
        public class WithProvidedDiscriminator
        {
            [TestFixture]
            public class OperatingOnEmptyCollection
            {
                [Test]
                public void ShouldReturnEmptyCollection()
                {
                    // Arrange
                    var input = new object[0];
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates(o => o.GetHashCode());
                    // Assert
                    Expect(result).To.Be.Empty();
                }
            }

            [TestFixture]
            public class OperatingOnCollectionOfOne
            {
                [Test]
                public void ShouldReturnEmptyCollection()
                {
                    // Arrange
                    var input = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "bob"
                        }
                    };
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates(o => o.id);
                    // Assert
                    Expect(result).To.Be.Empty();
                }
            }

            [TestFixture]
            public class OperatingOnDistinctCollection
            {
                [Test]
                public void ShouldReturnEmptyCollection()
                {
                    // Arrange
                    var idx = 0;
                    var input = GetRandomCollection(
                        () => new
                        {
                            name = $"snowflake #{idx}",
                            id = idx++
                        }
                    );
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates(o => o.id);
                    // Assert
                    Expect(result).To.Be.Empty();
                }
            }

            [TestFixture]
            public class OperatingOnCollectionWithDuplicates
            {
                [Test]
                public void ShouldReturnDuplicatesOnceEach()
                {
                    // Arrange
                    var input = new[]
                    {
                        new
                        {
                            id = 1,
                            name = "bob"
                        },
                        new
                        {
                            id = 2,
                            name = "andrew"
                        },
                        new
                        {
                            id = 3,
                            name = "posh"
                        },
                        new
                        {
                            id = 3,
                            name = "scary"
                        },
                        new
                        {
                            id = 3,
                            name = "baby"
                        },
                        new
                        {
                            id = 3,
                            name = "sporty"
                        },
                        new
                        {
                            id = 3,
                            name = "ginger"
                        },
                        new
                        {
                            id = 2,
                            name = "dave"
                        }
                    };
                    // Pre-Assert
                    // Act
                    var result = input.FindDuplicates(o => o.id);
                    // Assert
                    Expect(result).To.Contain.Exactly(2).Items();
                    var spiceGirls = result.Where(o => o.Key == 3).Select(o => o.Items).Single()
                        .Select(o => o.name);
                    Expect(spiceGirls).To.Contain.Exactly(5).Items();
                    Expect(spiceGirls)
                        .To.Be.Equivalent.To(
                            new[]
                            {
                                "posh",
                                "scary",
                                "baby",
                                "sporty",
                                "ginger"
                            }
                        );
                    var adoringFans = result.Where(r => r.Key == 2).Select(r => r.Items).Single();
                    Expect(adoringFans).To.Contain.Exactly(2).Items();
                }
            }
        }
    }

    [TestFixture]
    public class IsEmpty
    {
        [TestFixture]
        public class WhenCollectionIsNull
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var collection = null as int[];
                // Act
                var result = collection.IsEmpty();
                // Assert
                Expect(result).To.Be.True();
            }
        }

        [TestFixture]
        public class WhenCollectionIsEmpty
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var collection = new int[0];
                // Act
                var result = collection.IsEmpty();
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenCollectionIsNotEmpty
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var collection = GetRandomCollection<int>(1);
                // Act
                var result = collection.IsEmpty();
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }
    }

    [TestFixture]
    public class TrimmingStringCollections
    {
        [Test]
        public void TrimAll()
        {
            // Arrange
            var input = new[]
            {
                " one",
                "two ",
                " three "
            };
            var expected = new[]
            {
                "one",
                "two",
                "three"
            };
            // Act
            var result = input.Trim();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void TrimStartAll()
        {
            // Arrange
            var input = new[]
            {
                " one",
                "two ",
                " three "
            };
            var expected = new[]
            {
                "one",
                "two ",
                "three "
            };
            // Act
            var result = input.TrimStart();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void TrimEndAll()
        {
            // Arrange
            var input = new[]
            {
                " one",
                "two ",
                " three "
            };
            var expected = new[]
            {
                " one",
                "two",
                " three"
            };
            // Act
            var result = input.TrimEnd();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class Padding
    {
        [Test]
        public void PadLeftAll()
        {
            // Arrange
            var input = new[]
            {
                "one",
                "two",
                "three"
            };
            var expected = new[]
            {
                "   one",
                "   two",
                " three"
            };
            // Act
            var result = input.PadLeft(6);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void PadLeftAllDefaultsToPaddingToLongestString()
        {
            // Arrange
            var input = new[]
            {
                "one",
                "two",
                "three"
            };
            var expected = new[]
            {
                "  one",
                "  two",
                "three"
            };
            // Act
            var result = input.PadLeft();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldPadLeftWithProvidedChar()
        {
            // Arrange
            var input = new[]
            {
                "one",
                "two",
                "three"
            };
            var expected = new[]
            {
                "%%one",
                "%%two",
                "three"
            };
            // Act
            var result = input.PadLeft('%');
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void PadRightAll()
        {
            // Arrange
            var input = new[]
            {
                "one",
                "two",
                "three"
            };
            var expected = new[]
            {
                "one   ",
                "two   ",
                "three "
            };
            // Act
            var result = input.PadRight(6);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void PadRightAllDefaultsToPaddingToLongestString()
        {
            // Arrange
            var input = new[]
            {
                "one",
                "two",
                "three"
            };
            var expected = new[]
            {
                "one  ",
                "two  ",
                "three"
            };
            // Act
            var result = input.PadRight();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldPadRightWithProvidedChar()
        {
            // Arrange
            var input = new[]
            {
                "one",
                "two",
                "three"
            };
            var expected = new[]
            {
                "one^^",
                "two^^",
                "three"
            };
            // Act
            var result = input.PadRight('^');
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [TestFixture]
        public class IsEqualTo
        {
            [Test]
            public void ShouldReturnTrueForTwoEmptyCollections()
            {
                // Arrange
                var a = new int[0];
                var b = new int[0];
                // Act
                var result = a.IsEqualTo(b);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueForTwoEqualCollectionsOfSameType()
            {
                // Arrange
                var a = new[]
                {
                    1
                };
                var b = new[]
                {
                    1
                };
                // Act
                var result = a.IsEqualTo(b);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseForMismatchedSize()
            {
                // Arrange
                var a = new[]
                {
                    1
                };
                var b = new[]
                {
                    1,
                    2
                };
                // Act
                var result = a.IsEqualTo(b);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseForSameSizeDifferentValues()
            {
                // Arrange
                var a = new[]
                {
                    1
                };
                var b = new[]
                {
                    2
                };
                // Act
                var result = a.IsEqualTo(b);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class IsEquivalentTo
        {
            [Test]
            public void ShouldReturnTrueWhenBothCollectionsAreNull()
            {
                // Arrange
                var left = null as int[];
                var right = null as int[];
                // Act
                var result = left.IsEquivalentTo(right);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseWhenLeftIsNullAndRightIsNot()
            {
                // Arrange
                var left = null as int[];
                var right = new[]
                {
                    1
                };
                // Act
                var result = left.IsEquivalentTo(right);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseWhenLeftIsNotNullAndRightIsNull()
            {
                // Arrange
                var left = new[]
                {
                    GetRandomInt()
                };
                var right = null as int[];
                // Act
                var result = left.IsEquivalentTo(right);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnTrueForTwoEmptyCollections()
            {
                // Arrange
                var a = new int[0];
                var b = new int[0];
                // Act
                var result = a.IsEquivalentTo(b);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueForEqualCollections()
            {
                // Arrange
                var a = new[]
                {
                    1
                };
                var b = new[]
                {
                    1
                };
                // Act
                var result = a.IsEquivalentTo(b);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueForOutOfOrderEquivalentCollections()
            {
                // Arrange
                var a = new[]
                {
                    1,
                    2
                };
                var b = new[]
                {
                    2,
                    1
                };
                // Act
                var result = a.IsEquivalentTo(b);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueForOutOfOrderEquivalenceWithRepeatedValues()
            {
                // Arrange
                var a = new[]
                {
                    1,
                    2,
                    1
                };
                var b = new[]
                {
                    2,
                    1,
                    1
                };
                // Act
                var result = a.IsEquivalentTo(b);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseForRepeatedValuesWithNoEquivalence()
            {
                // Arrange
                var a = new[]
                {
                    1,
                    2,
                    1,
                    1,
                    1
                };
                var b = new[]
                {
                    2,
                    1,
                    1,
                    2,
                    1
                };
                // Act
                var result = a.IsEquivalentTo(b);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseForMismatchedSize()
            {
                // Arrange
                var a = new[]
                {
                    1,
                    2
                };
                var b = new[]
                {
                    1,
                    2,
                    3
                };
                // Act
                var result = a.IsEquivalentTo(b);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseForSameSizeDifferentEntries()
            {
                // Arrange
                var a = new[]
                {
                    1
                };
                var b = new[]
                {
                    2
                };
                // Act
                var result = a.IsEquivalentTo(b);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class FindOrAdd
        {
            [Test]
            public void SimpleTypesSimpleEquality()
            {
                // Arrange
                var expected = GetRandomString();
                var collection = new List<string>();
                // Act
                var result1 = collection.FindOrAdd(expected);
                var result2 = collection.FindOrAdd(expected);
                // Assert
                Expect(result1)
                    .To.Equal(expected);
                Expect(result1)
                    .To.Equal(result2);
                Expect(collection)
                    .To.Equal(
                        new[]
                        {
                            expected
                        }
                    );
            }

            [Test]
            public void ComplexTypesWithParameterlessConstructors()
            {
                // Arrange
                var collection = new List<Poco>();
                var item1 = new Poco()
                {
                    Id = 1,
                    Name = GetRandomString(12)
                };
                // Act
                var result1 = collection.FindOrAdd(
                    o => o.Id == item1.Id
                );
                result1.Id = 1;
                var result2 = collection.FindOrAdd(
                    o => o.Id == item1.Id
                );
                // Assert
                Expect(result1)
                    .Not.To.Be.Null();
                Expect(result1)
                    .To.Be(result2);
                // should have a new poco
                Expect(result1)
                    .Not.To.Be(item1);
            }

            [Test]
            public void ShouldReturnExistingComplexItemViaEqualsOverride()
            {
                // Arrange
                var collection = new List<Poco>();
                var item = GetRandom<Poco>();
                collection.Add(item);
                // Act
                var result = collection.FindOrAdd(item);
                // Assert
                Expect(result)
                    .To.Be(item);
            }

            [Test]
            public void GivenMatcherAndGenerator()
            {
                // Arrange
                var collection = new List<Poco>();
                var id = GetRandomInt();
                var name = GetRandomString();
                var expected = new
                {
                    Id = id,
                    Name = name
                };
                // Act
                var result1 = collection.FindOrAdd(
                    o => o.Id == id,
                    () => new Poco()
                    {
                        Id = id,
                        Name = name
                    }
                );
                var result2 = collection.FindOrAdd(
                    o => o.Id == id,
                    () => new Poco()
                    {
                        Id = id,
                        Name = name
                    }
                );
                // Assert
                Expect(result1)
                    .To.Be(result2);
                Expect(result1)
                    .To.Deep.Equal(expected);
                Expect(collection)
                    .To.Contain.Only(1)
                    .Matched.By(o => o.DeepEquals(expected));
            }

            public class Poco
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public override bool Equals(object obj)
                {
                    var asPoco = obj as Poco;
                    if (asPoco is null)
                    {
                        return false;
                    }

                    return asPoco.Id == Id && asPoco.Name == Name;
                }

                public override int GetHashCode()
                {
                    // ReSharper disable NonReadonlyMemberInGetHashCode
                    return (Id, Name).GetHashCode();
                    // ReSharper enable NonReadonlyMemberInGetHashCode
                }
            }
        }

        public static IEnumerable<(bool, int[], string[], int, char)> NonStringTestCases()
        {
            var start = new[]
            {
                1,
                11,
                101
            };
            var isLeft = true;
            var isRight = false;
            yield return (isLeft, start, new[]
            {
                "  1",
                " 11",
                "101"
            }, 3, ' ');
            yield return (isLeft, start, new[]
            {
                "%%%1",
                "%%11",
                "%101"
            }, 4, '%');
            yield return (isRight, start, new[]
            {
                "1  ",
                "11 ",
                "101"
            }, 3, ' ');
            yield return (isRight, start, new[]
            {
                "1&&&",
                "11&&",
                "101&"
            }, 4, '&');
        }

        [TestCaseSource(nameof(NonStringTestCases))]
        public void PaddingNonStrings(
            (bool isLeft, int[] start, string[] expected, int requiredLength, char padChar) testCase
        )
        {
            // Arrange
            var (isLeft, start, expected, requiredLength, padChar) = testCase;
            // Act
            var result = isLeft
                ? start.PadLeft(requiredLength, padChar)
                : start.PadRight(requiredLength, padChar);

            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class SelectArray
    {
        [TestFixture]
        public class GivenNull
        {
            [Test]
            public void ShouldReturnEmptyArray()
            {
                // Arrange
                var input = null as List<int>;
                var expected = new int[0];
                // Act
                var result = input.Map(i => i);
                // Assert
                Expect(result)
                    .To.Equal(expected);
                Expect(result)
                    .To.Be(Array.Empty<int>());
            }
        }

        [Test]
        public void ShouldReturnMappedCollectionAsArray()
        {
            // Arrange
            var input = new List<int>()
            {
                1,
                2,
                3
            };
            var expected = new[]
            {
                2,
                4,
                6
            };
            // Act
            var result = input.Map(i => i * 2);
            // Assert
            Expect(result)
                .To.Equal(expected);
            // ReSharper disable once ConvertTypeCheckToNullCheck
            Expect(result is int[])
                .To.Be.True();
        }
    }

    [TestFixture]
    public class SelectList
    {
        [TestFixture]
        public class GivenNull
        {
            [Test]
            public void ShouldReturnEmptyArray()
            {
                // Arrange
                var input = null as int[];
                var expected = new List<int>();
                // Act
                var result = input.MapList(i => i);
                // Assert
                Expect(result)
                    .To.Equal(expected);
                Expect(result is List<int>)
                    .To.Be.True();
            }
        }

        [Test]
        public void ShouldReturnMappedCollectionAsArray()
        {
            // Arrange
            var input = new List<int>()
            {
                1,
                2,
                3
            };
            var expected = new[]
            {
                2,
                4,
                6
            };
            // Act
            var result = input.MapList(i => i * 2);
            // Assert
            Expect(result)
                .To.Equal(expected);
            // ReSharper disable once ConvertTypeCheckToNullCheck
            Expect(result is List<int>)
                .To.Be.True();
        }
    }

    [TestFixture]
    public class Filter
    {
        [TestFixture]
        public class OperatingOnNull
        {
            [Test]
            public void ShouldReturnEmptyArray()
            {
                // Arrange
                var input = null as List<int>;
                var expected = Array.Empty<int>();
                // Act
                var result = input.Filter(_ => true);
                // Assert
                Expect(result)
                    .To.Be(expected);
            }
        }

        [TestFixture]
        public class OperatingOnCollection
        {
            [Test]
            public void ShouldFilterInput()
            {
                // Arrange
                var input = new List<int>()
                {
                    1,
                    2,
                    3,
                    4,
                    5,
                    6
                };
                var expected = new[]
                {
                    2,
                    4,
                    6
                };
                // Act
                var result = input.Filter(i => i % 2 == 0);
                // Assert
                // ReSharper disable once ConvertTypeCheckToNullCheck
                Expect(result is int[])
                    .To.Be.True();
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class FilterList
    {
        [TestFixture]
        public class OperatingOnNull
        {
            [Test]
            public void ShouldReturnEmptyArray()
            {
                // Arrange
                var input = null as int[];
                // Act
                var result = input.FilterList(_ => true);
                // Assert
                Expect(result)
                    .To.Be.Empty();
                Expect(result is List<int>)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class OperatingOnCollection
        {
            [Test]
            public void ShouldFilterListInput()
            {
                // Arrange
                var input = new[]
                {
                    1,
                    2,
                    3,
                    4,
                    5,
                    6
                };
                var expected = new List<int>()
                {
                    2,
                    4,
                    6
                };
                // Act
                var result = input.FilterList(i => i % 2 == 0);
                // Assert
                // ReSharper disable once ConvertTypeCheckToNullCheck
                Expect(result is List<int>)
                    .To.Be.True();
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class FindRepeatedValues
    {
        [TestFixture]
        public class OperatingOnNull
        {
            [Test]
            public void ShouldReturnEmptySet()
            {
                // Arrange
                var collection = null as IEnumerable<int>;
                // Act
                var result = collection.FindRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class OperatingOnEmptyCollection
        {
            [Test]
            public void ShouldReturnEmptySet()
            {
                // Arrange
                var collection = Array.Empty<bool>();
                // Act
                var result = collection.FindRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class OperatingOnNonEmptyCollections
        {
            [Test]
            public void ShouldFindTheValuesRepeatedOnce()
            {
                // Arrange
                var collection = new[]
                {
                    1,
                    2,
                    3,
                    3,
                    1
                };
                // Act
                var result = collection.FindRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Equivalent.To(
                        new[]
                        {
                            1,
                            3
                        }
                    );
            }

            [Test]
            public void ShouldReturnRepeatedValuesOnlyOnce()
            {
                // Arrange
                var collection = new[]
                {
                    1,
                    2,
                    3,
                    3,
                    1,
                    3,
                    3,
                    3,
                    1,
                    3,
                    1
                };
                // Act
                var result = collection.FindRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Equivalent.To(
                        new[]
                        {
                            1,
                            3
                        }
                    );
            }
        }

        [TestFixture]
        public class OperatingOnComplexValue
        {
            [Test]
            public void TwoIdenticalInstancesShouldHaveSameHashCodeAndReportEqual()
            {
                // Arrange
                var method = GetRandom<HttpMethod>();
                var route = GetRandomPath();
                var first = new RouteAndMethod(route, method);
                var second = first.Clone();
                // Act
                Expect(first)
                    .Not.To.Be(second);
                Expect(first.GetHashCode())
                    .To.Equal(second.GetHashCode());
                Expect(first.Equals(second))
                    .To.Be.True();
                Expect(second.Equals(first))
                    .To.Be.True();
                // Assert
            }

            [Test]
            public void ShouldFindDuplicates1()
            {
                // Arrange
                var method = GetRandom<HttpMethod>();
                var route = GetRandomPath();
                var first = new RouteAndMethod(route, method);
                var second = first.Clone();
                var collection = new[]
                {
                    first,
                    second
                };
                // Act
                var result = collection.FindDuplicates();
                // Assert
                Expect(result)
                    .To.Contain.Only(1)
                    .Deep.Equal.To(first);
            }

            public enum HttpMethod
            {
                Get,
                Put,
                Delete,
                Post,
                Head,
                Trace,
                Patch,
                Connect,
                Options,
            }

            public class RouteAndMethod : IComparable<RouteAndMethod>
            {
                public string Route { get; }
                public HttpMethod Method { get; }

                public RouteAndMethod(
                    string route,
                    HttpMethod method
                )
                {
                    Route = route;
                    Method = method;
                }

                public RouteAndMethod Clone()
                {
                    return new(Route, Method);
                }

                public override string ToString()
                {
                    return $"[{Method}] {Route}";
                }

                public override bool Equals(object obj)
                {
                    var other = obj as RouteAndMethod;
                    return Equals(other);
                }

                protected bool Equals(RouteAndMethod other)
                {
                    return other is not null &&
                        Route == other.Route &&
                        Method == other.Method;
                }

                public override int GetHashCode()
                {
                    return Tuple.Create(Route, (int)Method).GetHashCode();
                }

                public int CompareTo(RouteAndMethod other)
                {
                    if (ReferenceEquals(this, other))
                    {
                        return 0;
                    }

                    if (ReferenceEquals(null, other))
                    {
                        return 1;
                    }

                    var routeComparison = string.Compare(
                        Route,
                        other.Route,
                        StringComparison.OrdinalIgnoreCase
                    );
                    if (routeComparison != 0)
                    {
                        return routeComparison;
                    }

                    return Method.CompareTo(other.Method);
                }
            }
        }
    }

    [TestFixture]
    public class FindAllRepeatedValues
    {
        [TestFixture]
        public class OperatingOnNull
        {
            [Test]
            public void ShouldReturnEmptySet()
            {
                // Arrange
                var collection = null as IEnumerable<int>;
                // Act
                var result = collection.FindAllRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class OperatingOnEmptyCollection
        {
            [Test]
            public void ShouldReturnEmptySet()
            {
                // Arrange
                var collection = Array.Empty<bool>();
                // Act
                var result = collection.FindAllRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class OperatingOnNonEmptyCollections
        {
            [Test]
            public void ShouldFindTheValuesRepeatedOnce()
            {
                // Arrange
                var collection = new[]
                {
                    1,
                    2,
                    3,
                    3,
                    1
                };
                // Act
                var result = collection.FindAllRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Equivalent.To(
                        new[]
                        {
                            1,
                            3
                        }
                    );
            }

            [Test]
            public void ShouldReturnRepeatedValuesOnlyOnce()
            {
                // Arrange
                var collection = new[]
                {
                    1,
                    2,
                    3,
                    3,
                    1,
                    3,
                    3,
                    3,
                    1,
                    3,
                    1
                };
                // Act
                var result = collection.FindAllRepeatedValues();
                // Assert
                Expect(result)
                    .To.Be.Equivalent.To(
                        new[]
                        {
                            1,
                            1,
                            1,
                            3,
                            3,
                            3,
                            3,
                            3
                        }
                    );
            }
        }
    }

    [TestFixture]
    public class FindUniqueValues
    {
        [TestFixture]
        public class OperatingOnNullCollection
        {
            [Test]
            public void ShouldReturnEmptySet()
            {
                // Arrange
                var collection = null as IEnumerable<string>;
                // Act
                var result = collection.FindUniqueValues();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class OperatingOnEmptyCollection
        {
            [Test]
            public void ShouldReturnEmptySet()
            {
                // Arrange
                var collection = Array.Empty<int>();
                // Act
                var result = collection.FindUniqueValues();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class OperatingOnCollection
        {
            [Test]
            public void ShouldReturnUniqueValues()
            {
                // Arrange
                var collection = new[]
                {
                    1,
                    7,
                    3,
                    5,
                    3,
                    9,
                    9
                };
                var expected = new[]
                {
                    1,
                    5,
                    7
                };
                // Act
                var result = collection.FindUniqueValues();
                // Assert
                Expect(result)
                    .To.Be.Equivalent.To(expected);
            }
        }
    }

    [TestFixture]
    public class AsArray
    {
        [TestFixture]
        public class GivenArray
        {
            [Test]
            public void ShouldReturnThatArray()
            {
                // Arrange
                var collection = new[]
                {
                    1,
                    2,
                    3
                };
                // Act
                var result = ExtensionsForIEnumerables.AsArray(collection);
                // Assert
                Expect(result)
                    .To.Be(collection);
            }
        }

        [TestFixture]
        public class GivenNull
        {
            [Test]
            public void ShouldReturnEmptyArray()
            {
                // Arrange
                var collection = null as IEnumerable<string>;
                // Act
                var result = collection.AsArray();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        public class GivenNonEmptyEnumerable
        {
            [Test]
            public void ShouldReturnAnArrayWithTheItems()
            {
                // Arrange
                var numbers = GetRandomArray<int>(3, 5);
                var enumerated = 0;
                var collection = GenerateNumbers();
                // Act
                var result = collection.AsArray();
                // Assert
                Expect(enumerated)
                    .To.Equal(1);
                Expect(result)
                    .To.Equal(collection);

                IEnumerable<int> GenerateNumbers()
                {
                    enumerated++;
                    foreach (var number in numbers)
                    {
                        yield return number;
                    }
                }
            }
        }
    }

    [TestFixture]
    public class JoinPath
    {
        [Test]
        public void ShouldJoinWindowsPathOnDemand()
        {
            // Arrange
            var items = GetRandomArray<string>(3);
            var expected = string.Join("\\", items);
            // Act
            var result = items.JoinPath(PathType.Windows);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldJoinUnixPathOnDemand()
        {
            // Arrange
            var items = GetRandomArray<string>(3);
            var expected = string.Join("/", items);
            // Act
            var result = items.JoinPath(PathType.Unix);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldJoinBasedOnEnvironmentByDefault()
        {
            // Arrange
            var items = GetRandomArray<string>(3);
            var expected = Platform.IsUnixy
                ? string.Join("/", items)
                : string.Join("\\", items);
            // Act
            var result = items.JoinPath();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldJoinBasedOnEnvironmentOnDemand()
        {
            // Arrange
            var items = GetRandomArray<string>(3);
            var expected = Platform.IsUnixy
                ? string.Join("/", items)
                : string.Join("\\", items);
            // Act
            var result = items.JoinPath(PathType.Auto);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldUnifyPathDelimiters()
        {
            // Arrange
            var basePath = "C:/foo/bar";
            var addPath = "moo\\quux";
            var expected = "C:/foo/bar/moo/quux";
            // Act
            var result = new[]
            {
                basePath,
                addPath
            }.JoinPath(PathType.Unix);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldThrowForNull()
        {
            // Arrange
            // Act
            Expect(() => (null as string[]).JoinPath())
                .To.Throw<ArgumentNullException>()
                .For("parts");
            // Assert
        }
    }

    [TestFixture]
    public class FilterNulls
    {
        [TestFixture]
        public class GivenNullCollection
        {
            [Test]
            public void ShouldReturnEmptyCollection()
            {
                // Arrange
                var collection = null as int?[];
                // Act
                var result = collection.FilterNulls();
                // Assert
                Expect(result)
                    .To.Be.Empty();
                Expect(result)
                    .To.Be.An.Instance.Of<int[]>();
            }
        }

        [TestFixture]
        public class GivenEmptyCollection
        {
            [Test]
            public void ShouldReturnEmptyCollection()
            {
                // Arrange
                var collection = new int?[0];
                // Act
                var result = collection.FilterNulls();
                // Assert
                Expect(result)
                    .To.Be.Empty();
                Expect(result)
                    .To.Be.An.Instance.Of<int[]>();
            }
        }

        [TestFixture]
        public class GivenNoNullValues
        {
            [Test]
            public void ShouldReturnAllOriginalValuesNonNullable()
            {
                // Arrange
                var collection = new int?[]
                {
                    1,
                    2,
                    3
                };
                var expected = new[]
                {
                    1,
                    2,
                    3
                };

                // Act
                var result = collection.FilterNulls();

                // Assert
                Expect(result)
                    .To.Equal(expected);
                Expect(result)
                    .To.Be.An.Instance.Of<int[]>();
            }
        }

        [TestFixture]
        public class GivenCollectionWithNullValues
        {
            [Test]
            public void ShouldFilterThemOut()
            {
                // Arrange
                var collection = new int?[]
                {
                    1,
                    null,
                    2,
                    3,
                    null
                };
                var expected = new[]
                {
                    1,
                    2,
                    3
                };
                // Act
                var result = collection.FilterNulls();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class OperatingOnStrings
        {
            [Test]
            public void ShouldFilterOutNulls()
            {
                // Arrange
                var collection = new[]
                {
                    "a",
                    null,
                    "b",
                    "C"
                };
                var expected = new[]
                {
                    "a",
                    "b",
                    "C"
                };
                // Act
                var result = collection.FilterNulls();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class OperatingOnClassTypes
        {
            [Test]
            public void ShouldFilterOutNulls()
            {
                // Arrange
                var collection = new[]
                {
                    new Person(1, "bob"),
                    null,
                    new Person(2, "sally")
                };

                var expected = new[]
                {
                    new Person(1, "bob"),
                    new Person(2, "sally")
                };

                // Act
                var result = collection.FilterNulls();

                // Assert
                Expect(result)
                    .To.Deep.Equal(expected);
            }

            public class Person
            {
                public int Id { get; set; }
                public string Name { get; set; }

                public Person(
                    int id,
                    string name
                )
                {
                    Id = id;
                    Name = name;
                }
            }
        }
    }

    [TestFixture]
    public class Seek
    {
        [Test]
        public void ShouldFindTheFirstItemOfThatType()
        {
            // Arrange
            var items = ObjectArray(
                1,
                "foo",
                3,
                true,
                "bob",
                false
            );
            // Act
            var result1 = items.Seek<bool>();
            var result2 = items.Seek<string>();
            var result3 = items.Seek<int>();
            // Assert
            Expect(result1)
                .To.Be.True();
            Expect(result2)
                .To.Equal("foo");
            Expect(result3)
                .To.Equal(1);
        }

        [Test]
        public void ShouldFindTheFirstItemOfThatTypeAfterTheSkip()
        {
            // Arrange
            var items = ObjectArray(
                1,
                "foo",
                3,
                true,
                "bob",
                false
            );
            // Act
            var result1 = items.Seek<bool>(1);
            var result2 = items.Seek<string>(1);
            var result3 = items.Seek<int>(1);
            // Assert
            Expect(result1)
                .To.Be.False();
            Expect(result2)
                .To.Equal("bob");
            Expect(result3)
                .To.Equal(3);
        }

        [Test]
        public void ShouldFindTheFirstMatchingItemOfThatTypeAfterTheSkip()
        {
            // Arrange
            var items = ObjectArray(1, "aaa1", "aaa2", true, "aaa3");
            var skip = 2;
            var expected = "aaa3";
            // Act
            var result = items.Seek<string>(
                skip,
                s => s.StartsWith("aaa")
            );
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToFindByPredicateOnly()
        {
            // Arrange
            var items = ObjectArray(
                1,
                "one",
                2,
                "two",
                3,
                "three"
            );
            // Act
            var result = items.Seek<string>(s => s.Contains("ee"));
            // Assert
            Expect(result)
                .To.Equal("three");
        }

        [Test]
        public void ShouldThrowWhenNotFound()
        {
            // Arrange
            var items = ObjectArray(1, true, 2, DateTime.Now);
            // Act
            Expect(() => items.Seek<string>())
                .To.Throw<ElementNotFoundException>();
            // Assert
        }

        [TestFixture]
        public class FindOrDefault
        {
            [Test]
            public void ShouldReturnTheDefaultValueWhenNotFound1()
            {
                // Arrange
                var items = ObjectArray(1, true, 2, DateTime.Now);
                // Act
                var result = items.SeekOrDefault<string>();
                // Assert
                Expect(result)
                    .To.Be.Null();
            }

            [Test]
            public void ShouldReturnTheDefaultValueWhenNotFound2()
            {
                // Arrange
                var items = ObjectArray("1", true, "2", DateTime.Now);
                // Act
                var result = items.SeekOrDefault<int>();
                // Assert
                Expect(result)
                    .To.Equal(0);
            }
        }

        [TestFixture]
        public class AddRange
        {
            [Test]
            public void ShouldBeAbleToAddRangeOnHashSet()
            {
                // Arrange
                var hashset = GetRandomArray(() => GetRandomString(20, 30), 5)
                    .AsHashSet();
                var toAdd = GetRandomArray(() => GetRandomString(20, 30), 5);

                // Act
                hashset.AddRange(toAdd);
                // Assert
                Expect(hashset)
                    .To.Contain.All.Of(toAdd);
            }

            [Test]
            public void ShouldBeAbleToAddRangeOnHashSetWithParams()
            {
                // Arrange
                var hashset = GetRandomArray(() => GetRandomString(20, 30), 5)
                    .AsHashSet();
                var toAdd = GetRandomArray(() => GetRandomString(20, 30), 3, 3);

                // Act
                hashset.AddRange(toAdd[0], toAdd[1], toAdd[2]);
                // Assert
                Expect(hashset)
                    .To.Contain.All.Of(toAdd);
            }
        }

        private static object[] ObjectArray(
            params object[] args
        )
        {
            return args;
        }
    }
}