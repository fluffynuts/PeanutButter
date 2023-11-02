using System;
using System.Collections.Generic;
using System.Threading;
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
            public class WhenFuncReturnsValue
            {
                [Test]
                public void ShouldEventuallyReceiveThatValue()
                {
                    // Arrange
                    var runs = 0;
                    // Act
                    var result = Retry.Max(5).Times(
                        () =>
                        {
                            if (++runs > 3)
                            {
                                return 42;
                            }

                            throw new Exception("nope");
                        }
                    );
                    // Assert
                    Expect(result)
                        .To.Equal(42);
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

        [TestFixture]
        public class Backoff
        {
            [Test]
            public void ShouldBackOffWithDefaultStrategyByDefault()
            {
                // Arrange
                var collected = new List<DateTime>();
                var maxAttempts = GetRandomInt(4, 6);
                // Act
                var attempt = 0;
                Retry.Max(maxAttempts).Times(
                    () =>
                    {
                        collected.Add(DateTime.Now);
                        if (++attempt == maxAttempts)
                        {
                            return;
                        }

                        throw new Exception("nope");
                    }
                );
                // Assert

                for (var i = 1; i < maxAttempts; i++)
                {
                    var previous = collected[i - 1];
                    var current = collected[i];
                    var delta = current - previous;
                    Expect(delta)
                        .To.Be.Greater.Than.Or.Equal.To(
                            TimeSpan.FromMilliseconds(
                                DefaultBackoffStrategy.BACKOFF_MILLISECONDS
                            )
                        );
                }
            }

            [Test]
            public void ShouldUseTheProvidedBackoffStrategy()
            {
                // Arrange
                var maxAttempts = GetRandomInt(4, 6);
                var expected = maxAttempts - 1; // should pass on the final attempt
                var backoff = new SimpleBackoffStrategy();
                // Act
                var attempt = 0;
                Retry.Max(maxAttempts).Times(
                    () =>
                    {
                        if (++attempt == maxAttempts)
                        {
                            return;
                        }

                        throw new Exception("nuh-uh");
                    },
                    backoff
                );
                // Assert
                Expect(backoff.Attempts)
                    .To.Equal(expected);
            }

            public class SimpleBackoffStrategy : IBackoffStrategy
            {
                public int Attempts { get; private set; }

                // ReSharper disable once MemberHidesStaticFromOuterClass
                public void Backoff(int attempt)
                {
                    Attempts = attempt;
                    Thread.Sleep(attempt * 100);
                }
            }

            [Test]
            public void ShouldBeAbleToInstallADefaultStrategy()
            {
                // Arrange
                var backoff = new SimpleBackoffStrategy();
                using var _ = AutoResetter.Create(
                    () => Retry.InstallDefaultBackoffStrategy(backoff),
                    Retry.ForgetDefaultBackoffStrategy
                );
                var maxAttempts = GetRandomInt(4, 6);
                var expected = maxAttempts - 1; // should pass on the final attempt
                // Act
                var attempt = 0;
                Retry.Max(maxAttempts).Times(
                    () =>
                    {
                        if (++attempt == maxAttempts)
                        {
                            return;
                        }

                        throw new Exception("nuh-uh");
                    }
                );
                // Assert
                Expect(backoff.Attempts)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldBeAbleToInstallADefaultStrategyViaFactory()
            {
                // Arrange
                var backoff = new SimpleBackoffStrategy();
                using var _ = AutoResetter.Create(
                    () => Retry.InstallDefaultBackoffStrategy(() => backoff),
                    Retry.ForgetDefaultBackoffStrategy
                );
                var maxAttempts = GetRandomInt(4, 6);
                var expected = maxAttempts - 1; // should pass on the final attempt
                // Act
                var attempt = 0;
                Retry.Max(maxAttempts).Times(
                    () =>
                    {
                        if (++attempt == maxAttempts)
                        {
                            return;
                        }

                        throw new Exception("nuh-uh");
                    }
                );
                // Assert
                Expect(backoff.Attempts)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldRevertDefaultBackoffStrategyWhenForgetting()
            {
                // Arrange
                var backoff = new SimpleBackoffStrategy();
                using var _ = AutoResetter.Create(
                    () => Retry.InstallDefaultBackoffStrategy(() => backoff),
                    Retry.ForgetDefaultBackoffStrategy
                );
                var maxAttempts = GetRandomInt(4, 6);
                var expected = maxAttempts - 1; // should pass on the final attempt
                
                var attempt = 0;
                Retry.Max(maxAttempts).Times(
                    () =>
                    {
                        // ReSharper disable once AccessToModifiedClosure
                        if (++attempt == maxAttempts)
                        {
                            return;
                        }

                        throw new Exception("nuh-uh");
                    }
                );
                // Pre-Assert
                Expect(backoff.Attempts)
                    .To.Equal(expected);
                
                // Act
                Retry.ForgetDefaultBackoffStrategy();
                attempt = 0;
                Retry.Max(maxAttempts).Times(
                    () =>
                    {
                        if (++attempt == maxAttempts)
                        {
                            return;
                        }

                        throw new Exception("nuh-uh");
                    }
                );
                // Assert
                Expect(backoff.Attempts)
                    .To.Equal(expected, "Should not have used the custom backoff strategy");
                
            }
        }
    }
}