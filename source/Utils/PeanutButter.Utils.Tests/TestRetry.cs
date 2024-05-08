using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRetry
{
    [TestFixture]
    public class RetryMax
    {
        [TestFixture]
        public class ExceptionFiltering
        {
            [Test]
            public void ShouldOnlyRetryForAllowedFailures1()
            {
                // Arrange
                var lastAttempt = 0;
                // Act
                Expect(
                    () =>
                    {
                        Retry.Max(5).Times(
                            Logic,
                            e => e is ArgumentNullException
                                ? ExceptionHandlingStrategies.Suppress
                                : ExceptionHandlingStrategies.Throw
                        );
                    }
                ).To.Throw<ArgumentException>();
                // Assert
                Expect(lastAttempt)
                    .To.Equal(4);

                void Logic(int attempt)
                {
                    // it seems that when the captured code
                    // passed into Times explicitly:
                    // 1. expects an input parameter (attempt)
                    // 2. throws an exception inside the block
                    // then the compiler is matching this to the
                    //  signature that takes Func<int, Task>, which
                    //  causes the exception to be swallowed in
                    //  a task result, because we're not awaiting,
                    //  and we shouldn't have to
                    lastAttempt = attempt;
                    if (lastAttempt > 3)
                    {
                        throw new ArgumentException("nope");
                    }

                    throw new ArgumentNullException("meh");
                }
            }

            [Test]
            public void ShouldOnlyRetryForAllowedFailures2()
            {
                // Arrange
                var lastAttempt = 0;
                // Act
                Expect(
                    () =>
                    {
                        Retry.Max(5).Times(
                            Logic,
                            e => e is ArgumentNullException
                                ? ExceptionHandlingStrategies.Suppress
                                : ExceptionHandlingStrategies.Throw
                        );
                    }
                ).To.Throw<InvalidOperationException>();
                // Assert
                Expect(lastAttempt)
                    .To.Equal(4);

                void Logic()
                {
                    // it seems that when the captured code
                    // passed into Times explicitly:
                    // 1. expects an input parameter (attempt)
                    // 2. throws an exception inside the block
                    // then the compiler is matching this to the
                    //  signature that takes Func<int, Task>, which
                    //  causes the exception to be swallowed in
                    //  a task result, because we're not awaiting,
                    //  and we shouldn't have to
                    lastAttempt++;
                    if (lastAttempt > 3)
                    {
                        throw new InvalidOperationException("nope");
                    }

                    throw new ArgumentNullException("meh");
                }
            }
        }

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

    [TestFixture]
    public class RetryUntil
    {
        [Test]
        public void ShouldBeAbleToRetryUntilDateTime()
        {
            // Arrange
            var timeout = DateTime.Now.AddSeconds(1);
            var attempts = 0;
            // Act
            var runTime = Benchmark.Time(
                () =>
                {
                    Expect(
                            () =>
                                Retry.Until(timeout).Do(
                                    () =>
                                    {
                                        ++attempts;
                                        throw new Exception($"nope {attempts}");
                                    }
                                )
                        ).To.Throw<TimeoutException>()
                        .With.Message.Containing($"nope {attempts}");
                }
            );
            // Assert
            Expect(runTime)
                .To.Be.Greater.Than(TimeSpan.FromSeconds(1))
                .And
                .To.Be.Less.Than(TimeSpan.FromSeconds(2));
            Expect(attempts)
                .To.Be.Greater.Than(2);
        }

        [Test]
        public void ShouldBeAbleToRetryUntilTimeSpan()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(1);
            var attempts = 0;
            // Act
            var runTime = Benchmark.Time(
                () =>
                {
                    Expect(
                            () =>
                                Retry.Until(timeout).Do(
                                    () =>
                                    {
                                        ++attempts;
                                        throw new Exception($"nope {attempts}");
                                    }
                                )
                        ).To.Throw<TimeoutException>()
                        .With.Message.Containing($"nope {attempts}");
                }
            );
            // Assert
            Expect(runTime)
                .To.Be.Greater.Than(TimeSpan.FromSeconds(1))
                .And
                .To.Be.Less.Than(TimeSpan.FromSeconds(2));
            Expect(attempts)
                .To.Be.Greater.Than(2);
        }

        [Test]
        public void ShouldBeAbleToRetryWithCustomBackoffStrategy()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(1);
            var attempts = 0;
            var strategy = new CountingBackoffStrategy();
            // Act
            var runTime = Benchmark.Time(
                () =>
                {
                    Expect(
                            () =>
                                Retry.Until(timeout).Do(
                                    () =>
                                    {
                                        ++attempts;
                                        throw new Exception($"nope {attempts}");
                                    },
                                    strategy
                                )
                        ).To.Throw<TimeoutException>()
                        .With.Message.Containing($"nope {attempts}");
                }
            );
            // Assert
            Expect(runTime)
                .To.Be.Greater.Than(TimeSpan.FromSeconds(1))
                .And
                .To.Be.Less.Than(TimeSpan.FromSeconds(2));
            Expect(attempts)
                .To.Be.Greater.Than(2);
            Expect(strategy.Attempts)
                .To.Equal(attempts - 1);
        }

        [Test]
        public void ShouldBeAbleToRetryWithCustomDefaultBackoffStrategy()
        {
            // Arrange
            var timeout = TimeSpan.FromSeconds(1);
            var attempts = 0;
            var strategy = new CountingBackoffStrategy();
            Retry.InstallDefaultBackoffStrategy(strategy);
            // Act
            var runTime = Benchmark.Time(
                () =>
                {
                    Expect(
                            () =>
                                Retry.Until(timeout).Do(
                                    () =>
                                    {
                                        ++attempts;
                                        throw new Exception($"nope {attempts}");
                                    }
                                )
                        ).To.Throw<TimeoutException>()
                        .With.Message.Containing($"nope {attempts}");
                }
            );
            // Assert
            Expect(runTime)
                .To.Be.Greater.Than(TimeSpan.FromSeconds(1))
                .And
                .To.Be.Less.Than(TimeSpan.FromSeconds(2));
            Expect(attempts)
                .To.Be.Greater.Than(2);
            Expect(strategy.Attempts)
                .To.Equal(attempts - 1);
        }

        public class CountingBackoffStrategy : IBackoffStrategy
        {
            public int Attempts { get; private set; }

            public void Backoff(int attempt)
            {
                Attempts = attempt;
                Thread.Sleep(100);
            }
        }
    }
}