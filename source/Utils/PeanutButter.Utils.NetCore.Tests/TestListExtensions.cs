using System.Collections;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestListExtensions
{
    [TestFixture]
    public class Shift
    {
        [Test]
        public void ShouldReturnFirstValueFromList()
        {
            // Arrange
            var list = new List<int>()
            {
                1,
                2,
                3
            };
            // Act
            var result = list.Shift();
            // Assert
            Expect(result)
                .To.Equal(1);
        }

        [Test]
        public void ShouldRemoveTheItemFromTheList()
        {
            // Arrange
            var list = new List<int>()
            {
                1,
                1,
                1
            };
            // Act
            var result = list.Shift();
            // Assert
            Expect(result)
                .To.Equal(1);
            Expect(list)
                .To.Equal(
                    new[]
                    {
                        1,
                        1
                    }
                );
        }

        [Test]
        public void ShouldThrowWhenNoElements()
        {
            // Arrange
            var list = new List<int>();
            // Act
            Expect(() => list.Shift())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("List contains no elements");
            // Assert
        }
    }

    [TestFixture]
    public class TryPop
    {
        [TestFixture]
        public class WhenNoElements
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var l = new List<string>();
                // Act
                var result = l.TryPop(out _);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenHaveElements
        {
            [Test]
            public void ShouldPopLastOne()
            {
                // Arrange
                var elements = GetRandomArray<string>(3);
                var l = new List<string>(elements);
                // Act
                var result = l.TryPop(out var element);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(element)
                    .To.Equal(elements.Last());
            }
        }
    }

    [TestFixture]
    public class TryShift
    {
        [TestFixture]
        public class WhenNoElements
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var l = new List<string>();
                // Act
                var result = l.TryShift(out _);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenHaveElements
        {
            [Test]
            public void ShouldShiftFirstOne()
            {
                // Arrange
                var elements = GetRandomArray<string>(3);
                var l = new List<string>(elements);
                // Act
                var result = l.TryShift(out var element);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(element)
                    .To.Equal(elements.First());
            }
        }
    }

    [TestFixture]
    public class Pop
    {
        [Test]
        public void ShouldReturnLastValueFromList()
        {
            // Arrange
            var list = new List<string>()
            {
                "a",
                "b",
                "c"
            };
            // Act
            var result = list.Pop();
            // Assert
            Expect(result)
                .To.Equal("c");
        }

        [Test]
        public void ShouldRemoveValueFromList()
        {
            // Arrange
            var list = new List<string>()
            {
                "a",
                "b",
                "c"
            };
            // Act
            var result = list.Pop();
            // Assert
            Expect(result)
                .To.Equal("c");
            Expect(list)
                .To.Equal(
                    new[]
                    {
                        "a",
                        "b"
                    }
                );
        }

        [Test]
        public void ShouldThrowIfListIsEmpty()
        {
            // Arrange
            var list = new List<string>();
            // Act
            Expect(() => list.Pop())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("List contains no elements");
            // Assert
        }
    }

    [TestFixture]
    public class Unshift
    {
        [Test]
        public void ShouldAddElementToStartOfList()
        {
            // Arrange
            var list = new List<int>()
            {
                1,
                2,
                3
            };
            // Act
            list.Unshift(4);
            // Assert
            Expect(list)
                .To.Equal(
                    new[]
                    {
                        4,
                        1,
                        2,
                        3
                    }
                );
        }
    }

    [TestFixture]
    public class Push
    {
        [Test]
        public void ShadowsAddForCompleteness()
        {
            // Arrange
            var list = new List<bool>()
            {
                true,
                false
            };
            // Act
            list.Push(true);
            // Assert
            Expect(list)
                .To.Equal(
                    new[]
                    {
                        true,
                        false,
                        true
                    }
                );
        }
    }

    [TestFixture]
    public class AddAll
    {
        [Test]
        public void ShouldAddAllTheItems()
        {
            // Arrange
            var list = new List<int>();
            // Act
            var result = list.AddAll(1, 2, 3);
            // Assert
            Expect(list)
                .To.Equal(
                    new[]
                    {
                        1,
                        2,
                        3
                    }
                );
            Expect(result)
                .To.Be(list);
        }

        [Test]
        public void ShouldAddAllTheItemsAlt()
        {
            // Arrange
            var list = new MyList<int>();
            // Act
            var result = list.AddAll(1, 2, 3);
            // Assert
            Expect(list as IEnumerable<int>)
                .To.Equal(
                    new[]
                    {
                        1,
                        2,
                        3
                    }
                );
            Expect(result)
                .To.Be(list);
        }


        public class MyList<T> : IList<T>
        {
            private readonly List<T> _actual;

            public MyList()
            {
                _actual = new List<T>();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _actual.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(T item)
            {
                _actual.Add(item);
            }

            public void Clear()
            {
                _actual.Clear();
            }

            public bool Contains(T item)
            {
                return _actual.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _actual.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                return _actual.Remove(item);
            }

            public int Count => _actual.Count;
            public bool IsReadOnly => false;

            public int IndexOf(T item)
            {
                return _actual.IndexOf(item);
            }

            public void Insert(int index, T item)
            {
                _actual.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _actual.RemoveAt(index);
            }

            public T this[int index]
            {
                get => _actual[index];
                set => _actual[index] = value;
            }
        }
    }

    [TestFixture]
    public class TryEjectFirst
    {
        [TestFixture]
        public class WhenEmpty
        {
            [Test]
            public void ShouldReturnFalseAndSetResultDefault()
            {
                // Arrange
                var ints = new List<int>();
                var strings = new List<string>();
                // Act
                var intEjected = ints.TryEjectFirst(o => o == 1, out var intResult);
                var stringEjected = strings.TryEjectFirst(o => o == "foo", out var stringResult);
                // Assert
                Expect(intEjected)
                    .To.Be.False();
                Expect(intResult)
                    .To.Equal(default(int));
                Expect(stringEjected)
                    .To.Be.False();
                Expect(stringResult)
                    .To.Equal(default(string));
            }
        }

        [TestFixture]
        public class WhenContainsOneItem
        {
            [Test]
            public void ShouldReturnDefaultForNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1
                };
                // Act
                var ejected = data.TryEjectFirst(i => i % 2 == 0, out var result);
                // Assert
                Expect(data)
                    .Not.To.Be.Empty();
                Expect(ejected)
                    .To.Be.False();
                Expect(result)
                    .To.Equal(default(int));
            }

            [Test]
            public void ShouldReturnFirstMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    3
                };
                // Act
                var ejected = data.TryEjectFirst(i => i % 2 == 1, out var result);
                // Assert
                Expect(ejected)
                    .To.Be.True();
                Expect(data)
                    .To.Be.Empty();
                Expect(result)
                    .To.Equal(3);
            }
        }

        [TestFixture]
        public class WhenContainsManyItems
        {
            [Test]
            public void ShouldReturnDefaultForNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    5
                };
                // Act
                var ejected = data.TryEjectFirst(i => i % 2 == 0, out var result);
                // Assert
                Expect(ejected)
                    .To.Be.False();
                Expect(data)
                    .To.Equal(
                        new[]
                        {
                            1,
                            3,
                            5
                        }
                    );
                Expect(result)
                    .To.Equal(default(int));
            }

            [Test]
            public void ShouldReturnFirstMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    5
                };
                // Act
                var ejected = data.TryEjectFirst(i => i % 2 == 1 && i > 2, out var result);
                // Assert
                Expect(ejected)
                    .To.Be.True();
                Expect(data)
                    .Not.To.Contain(3);
                Expect(data)
                    .To.Equal(
                        new[]
                        {
                            1,
                            5
                        }
                    );
                Expect(result)
                    .To.Equal(3);
            }
        }
    }

    [TestFixture]
    public class EjectFirst
    {
        [TestFixture]
        public class WhenEmpty
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var ints = new List<int>();
                // Act
                Expect(() => ints.EjectFirst(i => i > 1))
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Containing(
                        "contains no elements"
                    );
                // Assert
            }
        }

        [TestFixture]
        public class WhenContainsOneItem
        {
            [Test]
            public void ShouldThrowOnNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1
                };
                // Act
                Expect(() => data.EjectFirst(i => i % 2 == 0))
                    .To.Throw<NotFoundException>();
                // Assert
            }

            [Test]
            public void ShouldReturnFirstMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    3
                };
                // Act
                var result = data.EjectFirst(i => i % 2 == 1);
                // Assert
                Expect(data)
                    .To.Be.Empty();
                Expect(result)
                    .To.Equal(3);
            }
        }

        [TestFixture]
        public class WhenContainsManyItems
        {
            [Test]
            public void ShouldThrowForNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    5
                };
                // Act
                Expect(() => data.EjectFirst(i => i % 2 == 0))
                    .To.Throw<NotFoundException>();
                // Assert
            }

            [Test]
            public void ShouldReturnFirstMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    5
                };
                // Act
                var result = data.EjectFirst(i => i % 2 == 1 && i > 2);
                // Assert
                Expect(data)
                    .Not.To.Contain(3);
                Expect(data)
                    .To.Equal(
                        new[]
                        {
                            1,
                            5
                        }
                    );
                Expect(result)
                    .To.Equal(3);
            }
        }
    }

    [TestFixture]
    public class TryEjectLast
    {
        [TestFixture]
        public class WhenEmpty
        {
            [Test]
            public void ShouldReturnFalseAndSetResultDefault()
            {
                // Arrange
                var ints = new List<int>();
                var strings = new List<string>();
                // Act
                var intEjected = ints.TryEjectLast(o => o == 1, out var intResult);
                var stringEjected = strings.TryEjectLast(o => o == "foo", out var stringResult);
                // Assert
                Expect(intEjected)
                    .To.Be.False();
                Expect(intResult)
                    .To.Equal(default(int));
                Expect(stringEjected)
                    .To.Be.False();
                Expect(stringResult)
                    .To.Equal(default(string));
            }
        }

        [TestFixture]
        public class WhenContainsOneItem
        {
            [Test]
            public void ShouldReturnDefaultForNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1
                };
                // Act
                var ejected = data.TryEjectLast(i => i % 2 == 0, out var result);
                // Assert
                Expect(data)
                    .Not.To.Be.Empty();
                Expect(ejected)
                    .To.Be.False();
                Expect(result)
                    .To.Equal(default(int));
            }

            [Test]
            public void ShouldReturnOnlyMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    3
                };
                // Act
                var ejected = data.TryEjectLast(i => i % 2 == 1, out var result);
                // Assert
                Expect(ejected)
                    .To.Be.True();
                Expect(data)
                    .To.Be.Empty();
                Expect(result)
                    .To.Equal(3);
            }
        }

        [TestFixture]
        public class WhenContainsManyItems
        {
            [Test]
            public void ShouldReturnDefaultForNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    5
                };
                // Act
                var ejected = data.TryEjectLast(i => i % 2 == 0, out var result);
                // Assert
                Expect(ejected)
                    .To.Be.False();
                Expect(data)
                    .To.Equal(
                        new[]
                        {
                            1,
                            3,
                            5
                        }
                    );
                Expect(result)
                    .To.Equal(default(int));
            }

            [Test]
            public void ShouldReturnOnlyMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    6
                };
                // Act
                var ejected = data.TryEjectLast(i => i % 2 == 1 && i > 2, out var result);
                // Assert
                Expect(ejected)
                    .To.Be.True();
                Expect(data)
                    .Not.To.Contain(3);
                Expect(data)
                    .To.Equal(
                        new[]
                        {
                            1,
                            6
                        }
                    );
                Expect(result)
                    .To.Equal(3);
            }

            [Test]
            public void ShouldReturnFirstMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    6,
                    8
                };
                // Act
                var ejected = data.TryEjectLast(i => i % 2 == 1, out var result);
                // Assert
                Expect(ejected)
                    .To.Be.True();
                Expect(data)
                    .Not.To.Contain(3);
                Expect(data)
                    .To.Equal(
                        new[]
                        {
                            1,
                            6,
                            8
                        }
                    );
                Expect(result)
                    .To.Equal(3);
            }
        }
    }

    [TestFixture]
    public class EjectLast
    {
        [TestFixture]
        public class WhenEmpty
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var ints = new List<int>();
                // Act
                Expect(() => ints.EjectLast(i => i > 1))
                    .To.Throw<InvalidOperationException>()
                    .With.Message.Containing(
                        "contains no elements"
                    );
                // Assert
            }
        }

        [TestFixture]
        public class WhenContainsOneItem
        {
            [Test]
            public void ShouldThrowOnNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1
                };
                // Act
                Expect(() => data.EjectLast(i => i % 2 == 0))
                    .To.Throw<NotFoundException>();
                // Assert
            }

            [Test]
            public void ShouldReturnLastMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    3
                };
                // Act
                var result = data.EjectLast(i => i % 2 == 1);
                // Assert
                Expect(data)
                    .To.Be.Empty();
                Expect(result)
                    .To.Equal(3);
            }
        }

        [TestFixture]
        public class WhenContainsManyItems
        {
            [Test]
            public void ShouldThrowForNoMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    5
                };
                // Act
                Expect(() => data.EjectLast(i => i % 2 == 0))
                    .To.Throw<NotFoundException>();
                // Assert
            }

            [Test]
            public void ShouldReturnLastMatch()
            {
                // Arrange
                var data = new List<int>()
                {
                    1,
                    3,
                    6
                };
                // Act
                var result = data.EjectLast(i => i % 2 == 1 && i > 2);
                // Assert
                Expect(data)
                    .Not.To.Contain(3);
                Expect(data)
                    .To.Equal(
                        new[]
                        {
                            1,
                            6
                        }
                    );
                Expect(result)
                    .To.Equal(3);
            }
        }
    }

    [TestFixture]
    public class Reversed
    {
        [Test]
        public void ShouldReturnANewListButReversed()
        {
            // Arrange
            var input = new[]
            {
                1,
                2,
                3
            };
            // Act
            var result = input.Reversed();
            // Assert
            Expect(result)
                .Not.To.Be(input);
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        3,
                        2,
                        1
                    }
                );
            Expect(() => result.Add(123))
                .Not.To.Throw(
                    "should return a List<T> for easier consumer access"
                );
        }
    }
}