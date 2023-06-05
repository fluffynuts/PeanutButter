using System;
using System.Collections.Generic;
using System.Threading;
using NExpect;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestTextStatusSteps
{
    [TestFixture]
    public class WhenActionCompletesOk
    {
        [Test]
        public void ShouldWriteLabelWithPrefix_ThenRunAction_ThenSuccessLabel()
        {
            // Arrange
            var prefix = $"{RandomValueGen.GetRandomString()}: ";
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
                () => collected.Add("-- flush --")
            );
            var label = RandomValueGen.GetRandomString();

            // Act
            sut.Run(
                label,
                () => collected.Add("-- action --")
            );
            // Assert
            var s1 = $"{expectedStartMarker} {prefix}{label}";
            Expectations.Expect(collected)
                .To.Equal(
                    new[]
                    {
                        s1,
                        "-- flush --",
                        $"-- action --",
                        $"\r{new String(' ', s1.Length)}\r",
                        $"{ok} {prefix}{label}\n",
                        "-- flush --"
                    }
                );
        }

        [Test]
        public void ShouldUseLengthOfLongestMarkerForSpacing()
        {
            // Arrange
            var prefix = $"{RandomValueGen.GetRandomString()}: ";
            var collected = new List<string>();
            var ok = "ok";
            var fail = "oh noe";
            var sut = Create(
                prefix,
                "",
                ok,
                fail,
                s => collected.Add(s),
                () => collected.Add("-- flush --")
            );
            var label = RandomValueGen.GetRandomString();

            // Act
            sut.Run(
                label,
                () => collected.Add("-- action --")
            );
            // Assert
            var s1 = $"{new String(' ', fail.Length + 1)}{prefix}{label}";
            Expectations.Expect(collected)
                .To.Equal(
                    new[]
                    {
                        s1,
                        "-- flush --",
                        $"-- action --",
                        $"\r{new String(' ', s1.Length)}\r",
                        $"{ok} {prefix}{label}\n",
                        "-- flush --"
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
            var prefix = $"{RandomValueGen.GetRandomString()}: ";
            var collected = new List<string>();
            var ok = "okie dokie";
            var fail = "oh noe";
            var sut = Create(
                prefix,
                "",
                ok,
                fail,
                s => collected.Add(s),
                () => collected.Add("-- flush --")
            );
            var label = RandomValueGen.GetRandomString();

            // Act
            Expectations.Expect(
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
            Expectations.Expect(collected)
                .To.Equal(
                    new[]
                    {
                        s1,
                        "-- flush --",
                        $"\r{new String(' ', s1.Length)}\r",
                        $"{fail} {prefix}{label}\n",
                        "-- flush --"
                    }
                );
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