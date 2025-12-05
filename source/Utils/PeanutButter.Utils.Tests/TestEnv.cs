namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestEnv
    {
        [TestFixture]
        public class String
        {
            [Test]
            public void ShouldReturnTheValueWhenSet()
            {
                // Arrange
                var envVar = GetRandomString(10);
                var expected = GetRandomString(10);
                using var _ = new AutoTempEnvironmentVariable(
                    envVar,
                    expected
                );
                // Act
                var result = Env.String(envVar, "foo");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldReturnTheFallbackWhenNotSet()
            {
                // Arrange
                var envVar = GetRandomString(10);
                var expected = GetRandomString(10);
                // Act
                var result = Env.String(envVar, expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class Integer
        {
            [Test]
            public void ShouldReturnTheEnvValueWhenSet()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = GetRandomInt(10, 100);
                using var _ = new AutoTempEnvironmentVariable(
                    varname,
                    $"{expected}"
                );
                // Act
                var result = Env.Integer(varname, 1337);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldReturnTheFallbackWhenNotSet()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = GetRandomInt(10, 100);
                // Act
                var result = Env.Integer(varname, expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldClipMin()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = 11;
                using var _ = new AutoTempEnvironmentVariable(
                    varname,
                    "3"
                );
                // Act
                var result = Env.Integer(varname, 1000, min: expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldClipMax()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = 11;
                using var _ = new AutoTempEnvironmentVariable(
                    varname,
                    "30"
                );
                // Act
                var result = Env.Integer(varname, 1000, max: expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class Flag
        {
            [Test]
            public void ShouldReturnTheEnvValueWhenSet()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = GetRandomBoolean();
                using var _ = new AutoTempEnvironmentVariable(
                    varname,
                    $"{expected}"
                );
                // Act
                var result = Env.Flag(varname, !expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldReturnTheFallbackWhenNotSet()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = GetRandomBoolean();
                // Act
                var result = Env.Flag(varname, expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class Decimal
        {
            [Test]
            public void ShouldReturnTheEnvValueWhenSet()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = GetRandomDecimal(10, 100);
                using var _ = new AutoTempEnvironmentVariable(
                    varname,
                    $"{expected}"
                );
                // Act
                var result = Env.Decimal(varname, 1337);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldReturnTheFallbackWhenNotSet()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = GetRandomDecimal(10, 100);
                // Act
                var result = Env.Decimal(varname, expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldClipMin()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = 11;
                using var _ = new AutoTempEnvironmentVariable(
                    varname,
                    "3"
                );
                // Act
                var result = Env.Decimal(varname, 1000, min: expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldClipMax()
            {
                // Arrange
                var varname = GetRandomString(10);
                var expected = 11;
                using var _ = new AutoTempEnvironmentVariable(
                    varname,
                    "30"
                );
                // Act
                var result = Env.Decimal(varname, 1000, max: expected);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }
}