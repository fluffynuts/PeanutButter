namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRunUntil
{
    [TestFixture]
    public class SimpleSyncVariant
    {
        [Test]
        public void ShouldRunUntilValidationPassesAndReturnResult()
        {
            // Arrange
            // Act
            var result = Run.Until<int>(i => i > 4, () => GetRandomInt());
            // Assert
            Expect(result)
                .To.Be.Greater.Than(4);
        }
    }

    [TestFixture]
    public class SyncVariant
    {
        [Test]
        public void ShouldRunUntilValidationPassesAndReturnResult()
        {
            // Arrange
            // Act
            var result = Run.Until<int>(i => i > 4, i => i + 1);
            // Assert
            Expect(result)
                .To.Equal(5);
        }
    }

    [TestFixture]
    public class SyncValidatorAsyncGenerator
    {
        [Test]
        public async Task ShouldRunUntilValidationPassesAndReturnResult()
        {
            // Arrange
            // Act
            var result = await Run.Until<int>(
                i => i > 4,
                async i => await Task.FromResult(i + 1)
            );
            // Assert
            Expect(result)
                .To.Equal(5);
        }
    }

    [TestFixture]
    public class SyncValidatorAsyncSimpleGenerator
    {
        [Test]
        public async Task ShouldRunUntilValidationPassesAndReturnResult()
        {
            // Arrange
            var start = 0;
            // Act
            var result = await Run.Until<int>(
                i => i > 4,
                async () => await Task.FromResult(++start)
            );
            // Assert
            Expect(result)
                .To.Equal(5);
        }
    }

    [TestFixture]
    public class AsyncValidatorSyncGenerator
    {
        [Test]
        public async Task ShouldRunUntilValidationPassesAndReturnResult()
        {
            // Arrange
            // Act
            var result = await Run.Until<int>(
                i => Task.FromResult(i > 4),
                i => i + 1
            );
            // Assert
            Expect(result)
                .To.Equal(5);
        }
    }

    [TestFixture]
    public class FullyAsyncVariant
    {
        [Test]
        public async Task ShouldRunUntilValidationPassesAndReturnResult()
        {
            // Arrange
            // Act
            var result = await Run.Until<int>(
                i => Task.FromResult(i > 4),
                async i => await Task.FromResult(i + 1)
            );
            // Assert
            Expect(result)
                .To.Equal(5);
        }
    }

    [TestFixture]
    public class FullyAsyncSimpleVariant
    {
        [Test]
        public async Task ShouldRunUntilValidationPassesAndReturnResult()
        {
            // Arrange
            var start = 0;
            // Act
            var result = await Run.Until<int>(
                i => Task.FromResult(i > 4),
                async i => await Task.FromResult(++start)
            );
            // Assert
            Expect(result)
                .To.Equal(5);
        }
    }
}