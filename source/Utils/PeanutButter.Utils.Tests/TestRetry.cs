using System;
using System.Threading.Tasks;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestRetry
    {
        [TestFixture]
        public class SynchronousOperations
        {
            [TestFixture]
            public class WhenCodeCompletesOnTheFirstAttempt
            {
                [Test]
                public void ShouldCompleteSuccessfully()
                {
                    // Arrange
                    var runs = 0;
                    // Act
                    Retry.Max(5).Times(
                        () =>
                        {
                            runs++;
                        }
                    );
                    // Assert
                    Expect(runs)
                        .To.Equal(1);
                }
            }

            [TestFixture]
            public class WhenCompletesBeforeMaxAttempts
            {
                [Test]
                public void ShouldCompleteSuccessfully()
                {
                    // Arrange
                    var runs = 1;
                    // Act
                    Retry.Max(5).Times(
                        () =>
                        {
                            runs++;
                            if (runs < 5)
                            {
                                throw new Exception("nope");
                            }
                        }
                    );
                    // Assert
                    Expect(runs)
                        .To.Equal(5);
                }
            }

            [TestFixture]
            public class WhenFailsEveryTime
            {
                [Test]
                public void ShouldThrow()
                {
                    // Arrange
                    var runs = 0;
                    // Act
                    Expect(
                            () =>
                                Retry.Max(5).Times(
                                    () =>
                                    {
                                        throw new InvalidOperationException(
                                            $"nope: {++runs}"
                                        );
                                    }
                                )
                        ).To.Throw<RetriesExceededException>()
                        .With.Property(e => e.InnerException)
                        .Matched.By(
                            e =>
                                e is InvalidOperationException ioe &&
                                ioe.Message == $"nope: {runs}"
                        );
                    // Assert
                }
            }
        }

        [TestFixture]
        public class AsynchronousOperations
        {
            [TestFixture]
            public class WhenCodeCompletesOnTheFirstAttempt
            {
                [Test]
                public async Task ShouldCompleteSuccessfully()
                {
                    // Arrange
                    var runs = 0;
                    // Act
                    await Retry.Max(5).Times(
                        async () =>
                        {
                            await Task.Delay(0);
                            runs++;
                        }
                    );
                    // Assert
                    Expect(runs)
                        .To.Equal(1);
                }
            }

            [TestFixture]
            public class WhenCompletesBeforeMaxAttempts
            {
                [Test]
                public async Task ShouldCompleteSuccessfully()
                {
                    // Arrange
                    var runs = 0;
                    var failUntil = 3;
                    // Act
                    await Retry.Max(failUntil).Times(
                        async () =>
                        {
                            await Task.Delay(0);
                            runs++;
                            if (runs < (failUntil - 1))
                            {
                                throw new Exception("nope");
                            }
                        }
                    );
                    // Assert
                    Expect(runs)
                        .To.Equal(failUntil - 1);
                }
            }

            [TestFixture]
            public class WhenFailsEveryTime
            {
                [Test]
                public void ShouldThrow()
                {
                    // Arrange
                    var runs = 0;
                    // Act
                    Expect(
                            async () =>
                                await Retry.Max(5).Times(
                                    async () =>
                                    {
                                        await Task.Delay(0);
                                        throw new InvalidOperationException(
                                            $"nope: {++runs}"
                                        );
                                    }
                                )
                        ).To.Throw<RetriesExceededException>()
                        .With.Property(e => e.InnerException)
                        .Matched.By(
                            e =>
                                e is InvalidOperationException ioe &&
                                ioe.Message == $"nope: {runs}"
                        );
                    // Assert
                }
            }
        }
    }
}