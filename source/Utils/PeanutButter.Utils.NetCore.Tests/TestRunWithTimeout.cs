namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRunWithTimeout
{
    [TestFixture]
    public class WhenLogicIsTooSlow
    {
        [Test]
        public void ShouldThrowTimeout()
        {
            // Arrange

            // Act
            Expect(
                async () =>
                    await Run.WithTimeoutAsync(
                        100,
                        _ => Task.Delay(2000)
                    )
            ).To.Throw<TimeoutException>();
            // Assert
        }
    }

    [TestFixture]
    public class WhenLogicIsFastEnough
    {
        [Test]
        public void ShouldNotThrow()
        {
            // Arrange

            // Act
            Expect(
                async () =>
                    await Run.WithTimeoutAsync(
                        500,
                        _ => Task.Delay(100)
                    )
            ).Not.To.Throw();
            // Assert
        }
    }

    [TestFixture]
    public class WhenLogicReturnsValue
    {
        [TestFixture]
        public class InTime
        {
            [Test]
            public async Task ShouldReturnTheValue()
            {
                // Arrange
                var expected = GetRandomInt(1);
                // Act
                var result = await Run.WithTimeoutAsync(
                    1000,
                    token => Task.FromResult(expected)
                );
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    public class AsyncActionOverload
    {
        [TestFixture]
        public class WhenFastEnough
        {
            [Test]
            public void ShouldComplete()
            {
                // Arrange
                // Act
                Expect(
                    async () =>
                        await Run.WithTimeoutAsync(
                            500,
                            async () => await Task.Delay(100)
                        )
                ).Not.To.Throw();
                // Assert
            }
        }

        [TestFixture]
        public class WhenTooSlow
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                // Act
                Expect(
                        async () =>
                            await Run.WithTimeoutAsync(
                                100,
                                async () => await Task.Delay(500)
                            )
                    ).To.Throw<TimeoutException>()
                    .With.Message.Like("unable to complete requested logic within");
                // Assert
            }
        }
    }

    [TestFixture]
    public class ActionOverload
    {
        [TestFixture]
        public class WhenFastEnough
        {
            [Test]
            public void ShouldComplete()
            {
                // Arrange
                // Act
                Expect(
                    () =>
                        Run.WithTimeout(
                            500,
                            () => Thread.Sleep(100)
                        )
                ).Not.To.Throw();
                // Assert
            }
        }

        [TestFixture]
        public class WhenTooSlow
        {
            [Test]
            [Repeat(50)]
            public void ShouldThrow()
            {
                // Arrange
                // Act
                Expect(
                        () =>
                            Run.WithTimeout(
                                100,
                                () => Thread.Sleep(500)
                            )
                    ).To.Throw<TimeoutException>()
                    .With.Message.Like("unable to complete requested logic within");
                // Assert
            }
        }
    }
}