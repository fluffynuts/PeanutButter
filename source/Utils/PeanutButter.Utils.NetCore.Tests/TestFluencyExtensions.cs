namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestFluencyExtensions
{
    [TestFixture]
    public class With
    {
        [Test]
        public void ShouldApplyTransformAndReturnMutatedObject()
        {
            // Arrange
            var expected = GetRandomInt();
            var poco = new Poco();
            // Act
            var result = poco
                .With(o => o.Id = expected);
            // Assert
            Expect(result)
                .To.Be(poco);
            Expect(result.Id)
                .To.Equal(expected);
        }

        [TestFixture]
        public class WhenMutatorIsNull
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var item = new Poco();
                // Act
                Expect(() => item.With(null))
                    .To.Throw<ArgumentNullException>()
                    .For("mutator");
                // Assert
            }
        }
    }

    [TestFixture]
    public class WithAll
    {
        [TestFixture]
        public class WhenCollectionIsNull
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var collection = null as Container<int>[];
                // Act
                Expect(() => collection.WithAll(o => o.Value++))
                    .To.Throw<ArgumentNullException>()
                    .For("subject");
                // Assert
            }
        }

        [TestFixture]
        public class WhenMutatorIsNull
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var collection = Array.Empty<int>();
                // Act
                Expect(() => collection.WithAll(null))
                    .To.Throw<ArgumentNullException>()
                    .For("mutator");
                // Assert
            }
        }

        [Test]
        public void ShouldApplyTransformToAllElementsOfTheSequenceAndReturnTheSequence1()
        {
            // Arrange
            var items = new[]
            {
                new Container<int>(1),
                new Container<int>(2),
                new Container<int>(3)
            };

            // Act
            var result = items.WithAll(o => o.Value++);
            // Assert
            Expect(result.Select(o => o.Value))
                .To.Equal(
                    new[]
                    {
                        2,
                        3,
                        4
                    }
                );
        }

        [Test]
        public void ShouldApplyTransformToAllElementsOfTheSequenceAndReturnTheSequence2()
        {
            // Arrange
            var items = new List<Container<int>>
            {
                new(1),
                new(2),
                new(3)
            };

            // Act
            var result = items.WithAll(o => o.Value++);
            // Assert
            Expect(result.Select(o => o.Value))
                .To.Equal(
                    new[]
                    {
                        2,
                        3,
                        4
                    }
                );
        }

        public class Container<T>
        {
            public T Value { get; set; }

            public Container(T value)
            {
                Value = value;
            }
        }
    }

    public class Poco
    {
        public int Id { get; set; }
    }
}