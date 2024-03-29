using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestTextStatusSteps
{
    [TestFixture]
    public class Run
    {
        [TestFixture]
        public class WhenActionCompletesOk
        {
            [Test]
            public void ShouldWriteLabelWithPrefix_ThenRunAction_ThenSuccessLabel()
            {
                // Arrange
                var prefix = $"{GetRandomString()}: ";
                var collected = new List<string>();
                var start = "start:";
                var ok = "okie dokie";
                var expectedStartMarker = "start:    ";
                var fail = "oh noe";
                var sut = Create(
                    prefix,
                    start,
                    ok,
                    fail,
                    s => collected.Add(s),
                    () => collected.Add(FLUSH_MARKER)
                );
                var label = GetRandomString();

                // Act
                sut.Run(
                    label,
                    () => collected.Add("-- action --")
                );
                // Assert
                var s1 = $"{expectedStartMarker} {prefix}{label}";
                Expect(collected)
                    .To.Equal(
                        new[]
                        {
                            s1,
                            FLUSH_MARKER,
                            $"-- action --",
                            $"\r{new String(' ', s1.Length)}\r",
                            $"{ok} {prefix}{label}\n",
                            FLUSH_MARKER
                        }
                    );
            }

            [Test]
            public void ShouldUseLengthOfLongestMarkerForSpacing()
            {
                // Arrange
                var prefix = $"{GetRandomString()}: ";
                var collected = new List<string>();
                var ok = "ok";
                var fail = "oh noe";
                var sut = Create(
                    prefix,
                    "",
                    ok,
                    fail,
                    s => collected.Add(s),
                    () => collected.Add(FLUSH_MARKER)
                );
                var label = GetRandomString();

                // Act
                sut.Run(
                    label,
                    () => collected.Add("-- action --")
                );
                // Assert
                var s1 = $"{new String(' ', fail.Length + 1)}{prefix}{label}";
                Expect(collected)
                    .To.Equal(
                        new[]
                        {
                            s1,
                            FLUSH_MARKER,
                            $"-- action --",
                            $"\r{new String(' ', s1.Length)}\r",
                            $"{ok} {prefix}{label}\n",
                            FLUSH_MARKER
                        }
                    );
            }
        }

        [TestFixture]
        public class WhenActionFails
        {
            [Test]
            public void ShouldWriteLabelWithPrefix_ThenRunAction_ThenFailLabel_ThenThrow()
            {
                // Arrange
                var prefix = $"{GetRandomString()}: ";
                var collected = new List<string>();
                var ok = "okie dokie";
                var fail = "oh noe";
                var sut = Create(
                    prefix,
                    "",
                    ok,
                    fail,
                    s => collected.Add(s),
                    () => collected.Add(FLUSH_MARKER)
                );
                var label = GetRandomString();

                // Act
                Expect(
                        () =>
                        {
                            sut.Run(
                                label,
                                () => throw new ApplicationException("moo cow")
                            );
                        }
                    ).To.Throw<ApplicationException>()
                    .With.Message.Equal.To("moo cow");
                // Assert
                var s1 = $"{new String(' ', ok.Length + 1)}{prefix}{label}";
                Expect(collected)
                    .To.Equal(
                        new[]
                        {
                            s1,
                            FLUSH_MARKER,
                            $"\r{new String(' ', s1.Length)}\r",
                            $"{fail} {prefix}{label}\n",
                            FLUSH_MARKER
                        }
                    );
            }
        }
    }

    [TestFixture]
    public class RunAsync
    {
        [TestFixture]
        public class WhenActionCompletesOk
        {
            [Test]
            public async Task ShouldWriteLabelWithPrefix_ThenRunAction_ThenSuccessLabel()
            {
                // Arrange
                var prefix = $"{GetRandomString()}: ";
                var collected = new List<string>();
                var start = "start:";
                var ok = "okie dokie";
                var expectedStartMarker = "start:    ";
                var fail = "oh noe";
                var sut = Create(
                    prefix,
                    start,
                    ok,
                    fail,
                    s => collected.Add(s),
                    () => collected.Add(FLUSH_MARKER)
                );
                var label = GetRandomString();

                // Act
                await sut.RunAsync(
                    label,
                    MakeAsync(() => collected.Add("-- action --"))
                );
                // Assert
                var s1 = $"{expectedStartMarker} {prefix}{label}";
                Expect(collected)
                    .To.Equal(
                        new[]
                        {
                            s1,
                            FLUSH_MARKER,
                            $"-- action --",
                            $"\r{new String(' ', s1.Length)}\r",
                            $"{ok} {prefix}{label}\n",
                            FLUSH_MARKER
                        }
                    );
            }

            [Test]
            public async Task ShouldUseLengthOfLongestMarkerForSpacing()
            {
                // Arrange
                var prefix = $"{GetRandomString()}: ";
                var collected = new List<string>();
                var ok = "ok";
                var fail = "oh noe";
                var sut = Create(
                    prefix,
                    "",
                    ok,
                    fail,
                    s => collected.Add(s),
                    () => collected.Add(FLUSH_MARKER)
                );
                var label = GetRandomString();

                // Act
                await sut.RunAsync(
                    label,
                    MakeAsync(() => collected.Add("-- action --"))
                );
                // Assert
                var s1 = $"{new String(' ', fail.Length + 1)}{prefix}{label}";
                Expect(collected)
                    .To.Equal(
                        new[]
                        {
                            s1,
                            FLUSH_MARKER,
                            $"-- action --",
                            $"\r{new String(' ', s1.Length)}\r",
                            $"{ok} {prefix}{label}\n",
                            FLUSH_MARKER
                        }
                    );
            }
        }

        [TestFixture]
        public class WhenActionFails
        {
            [Test]
            public void ShouldWriteLabelWithPrefix_ThenRunAction_ThenFailLabel_ThenThrow()
            {
                // Arrange
                var prefix = $"{GetRandomString()}: ";
                var collected = new List<string>();
                var ok = "okie dokie";
                var fail = "oh noe";
                var sut = Create(
                    prefix,
                    "",
                    ok,
                    fail,
                    s => collected.Add(s),
                    () => collected.Add(FLUSH_MARKER)
                );
                var label = GetRandomString();

                // Act
                Expect(
                        async () =>
                        {
                            await sut.RunAsync(
                                label,
                                MakeAsync(() => throw new ApplicationException("moo cow"))
                            );
                        }
                    ).To.Throw<ApplicationException>()
                    .With.Message.Equal.To("moo cow");
                // Assert
                var s1 = $"{new String(' ', ok.Length + 1)}{prefix}{label}";
                Expect(collected)
                    .To.Equal(
                        new[]
                        {
                            s1,
                            FLUSH_MARKER,
                            $"\r{new String(' ', s1.Length)}\r",
                            $"{fail} {prefix}{label}\n",
                            FLUSH_MARKER
                        }
                    );
            }
        }

        private static Func<Task> MakeAsync(Action action)
        {
            return new Func<Task>(
                async () =>
                {
                    // give up control
                    await Task.Delay(0);
                    action();
                    // and again
                    await Task.Delay(0);
                }
            );
        }
    }

    private const string FLUSH_MARKER = "-- flush --";

    [TestFixture]
    public class PureLogging
    {
        [Test]
        public void ShouldLogViaWriterAndFlush()
        {
            // Arrange
            var prefix = GetRandomString();
            var captured = new List<string>();
            var sut = Create(
                prefix,
                GetRandomString(),
                GetRandomString(),
                GetRandomString(),
                s => captured.Add(s),
                () => captured.Add("-- flush --")
            );
            var toLog = GetRandomString();
            var expected = new[]
            {
                $"{prefix}{toLog}",
                FLUSH_MARKER
            };
            // Act
            sut.Log(toLog);
            // Assert
            Expect(captured)
                .To.Equal(expected);
        }

        [Test]
        public async Task ShouldLogViaWriterAndFlushAsync()
        {
            // Arrange
            var prefix = GetRandomString();
            var captured = new List<string>();
            var sut = Create(
                prefix,
                GetRandomString(),
                GetRandomString(),
                GetRandomString(),
                s => captured.Add(s),
                () => captured.Add("-- flush --")
            );
            var toLog = GetRandomString();
            var expected = new[]
            {
                $"{prefix}{toLog}",
                FLUSH_MARKER
            };
            // Act
            await sut.LogAsync(toLog);
            // Assert
            Expect(captured)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    [Explicit("all integration tests requiring eyeballing")]
    public class ConsoleWriter
    {
        [Test]
        public void BasicTestingSteps()
        {
            // Arrange
            var steps = ConsoleSteps.ForTesting("", Console.Error);
            // Act
            steps.Run(
                "Milk the cow",
                () =>
                {
                    Thread.Sleep(1000);
                }
            );
            // Assert
        }
    }

    private static TextStatusSteps Create(
        string prefix,
        string started,
        string completed,
        string failed,
        Action<string> writer,
        Action flushAction
    )
    {
        return new TextStatusSteps(
            prefix,
            started,
            completed,
            failed,
            writer,
            flushAction
        );
    }
}