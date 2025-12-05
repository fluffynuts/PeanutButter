namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestStrictComparer
    {
        [TestFixture]
        public class EqualsOverride
        {
            [Test]
            public void Given_Null_Null_ShouldReturnTrue()
            {
                // Arrange
                var sut = Create<object>();
                // Pre-Assert
                // Act
                var result = sut.Equals(null, null);
                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            public void WhenValuesAreNotEqual_ShouldReturnFalse()
            {
                // Arrange
                var x = GetRandomInt();
                var y = GetAnother(x);
                var sut = Create<int>();
                // Pre-Assert
                // Act
                var result = sut.Equals(x, y);
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            public void WhenValuesAreEqual_ShouldReturnTrue()
            {
                // Arrange
                var value = GetRandomString();
                var sut = Create<string>();
                // Pre-Assert
                // Act
                var result = sut.Equals(value, value);
                // Assert
                Expect(result).To.Be.True();
            }
        }

        [TestFixture]
        public class GetHashCodeOverride
        {
            [Test]
            public void Given_Null_ShouldReturn_0()
            {
                // Arrange
                var sut = Create<string>();
                // Pre-Assert
                // Act
                // ReSharper disable once AssignNullToNotNullAttribute
                var result = sut.GetHashCode(null);
                // Assert
                Expect(result).To.Equal(0);
            }

            [Test]
            public void Given_NotNull_ShouldReturnThatObjectsHashCode()
            {
                // Arrange
                var str = GetRandomString(2);
                var expected = str.GetHashCode();
                var sut = Create<string>();
                // Pre-Assert
                // Act
                var result = sut.GetHashCode(str);
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        private static StrictComparer<T> Create<T>()
        {
            return new StrictComparer<T>();
        }
    }
}