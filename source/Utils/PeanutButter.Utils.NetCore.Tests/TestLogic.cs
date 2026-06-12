using static PeanutButter.Utils.Logic;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestLogic
{
    [TestFixture]
    public class AllOfTests
    {
        [TestFixture]
        public class RawValues
        {
            [Test]
            public void ShouldReturnTrueWhenAllTrue()
            {
                // Arrange
                // Act
                var result = AllOf(
                    true,
                    true,
                    true,
                    true,
                    true
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseIfAnyFalse()
            {
                // Arrange
                // Act
                var result = AllOf(
                    true,
                    true,
                    true,
                    false,
                    true
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyFalseShortCircuit1()
            {
                // Arrange
                // Act
                var result = AllOf(
                    false,
                    true,
                    true,
                    true,
                    true
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyFalseShortCircuit2()
            {
                // Arrange
                // Act
                var result = AllOf(
                    true,
                    false,
                    true,
                    true,
                    true
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class Funcs
        {
            [Test]
            public void ShouldReturnTrueWhenAllTrue()
            {
                // Arrange
                // Act
                var result = AllOf(
                    () => true,
                    () => true,
                    () => true,
                    () => true,
                    () => true
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseIfAnyFalse()
            {
                // Arrange
                // Act
                var result = AllOf(
                    () => true,
                    () => true,
                    () => true,
                    () => true,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
            [Test]
            public void ShouldReturnFalseIfAnyFalseShortCircuit1()
            {
                // Arrange
                // Act
                var result = AllOf(
                    () => false,
                    () => true,
                    () => true,
                    () => true,
                    () => true
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
            [Test]
            public void ShouldReturnFalseIfAnyFalseShortCircuite2()
            {
                // Arrange
                // Act
                var result = AllOf(
                    () => true,
                    () => false,
                    () => true,
                    () => true,
                    () => true
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }
    }

    [TestFixture]
    public class AnyOfTests
    {
        [TestFixture]
        public class RawValues
        {
            [Test]
            public void ShouldReturnTrueWhenAnyTrue()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    false,
                    false,
                    false,
                    false,
                    true
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenAnyTrueShortCircuit1()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    true,
                    false,
                    false,
                    false,
                    false
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenAnyTrueShortCircuit2()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    false,
                    true,
                    false,
                    false,
                    false
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseIfAllFalse()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    false,
                    false,
                    false,
                    false,
                    false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class Funcs
        {
            [Test]
            public void ShouldReturnTrueWhenAnyTrue()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    () => false,
                    () => false,
                    () => false,
                    () => false,
                    () => true
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenAnyTrueShortCircuit1()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    () => true,
                    () => false,
                    () => false,
                    () => false,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenAnyTrueShortCircuit2()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    () => false,
                    () => true,
                    () => false,
                    () => false,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseIfAllFalse()
            {
                // Arrange
                // Act
                var result = AnyOf(
                    () => false,
                    () => false,
                    () => false,
                    () => false,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }
    }

    [TestFixture]
    public class NoneOfTests
    {
        [TestFixture]
        public class RawValues
        {
            [Test]
            public void ShouldReturnTrueWhenAllFalse()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    false,
                    false,
                    false,
                    false,
                    false
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrue()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    false,
                    false,
                    false,
                    false,
                    true
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrueShortCircuit1()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    true,
                    false,
                    false,
                    false,
                    false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrueShortCircuit2()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    false,
                    true,
                    false,
                    false,
                    false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrueShortCircuit()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    true,
                    false,
                    true,
                    false,
                    false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class Funcs
        {
            [Test]
            public void ShouldReturnTrueWhenAllFalse()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    () => false,
                    () => false,
                    () => false,
                    () => false,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrue()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    () => false,
                    () => false,
                    () => false,
                    () => false,
                    () => true
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrueShortCircuit1()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    () => true,
                    () => false,
                    () => false,
                    () => false,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrueShortCircuit2()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    () => false,
                    () => true,
                    () => false,
                    () => false,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseIfAnyTrueShortCircuit()
            {
                // Arrange
                // Act
                var result = NoneOf(
                    () => false,
                    () => false,
                    () => true,
                    () => false,
                    () => false
                );
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }
    }
}