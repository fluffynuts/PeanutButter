using System;
using System.Collections.Generic;
using NUnit.Framework;
using NExpect;
using PeanutButter.EasyArgs.Attributes;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using DescriptionAttribute = PeanutButter.EasyArgs.Attributes.DescriptionAttribute;
using IgnoreAttribute = PeanutButter.EasyArgs.Attributes.IgnoreAttribute;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.EasyArgs.Tests;

[TestFixture]
public class TestParserExtensions
{
    [Test]
    public void ShouldParseArgumentBasedOnShortName()
    {
        // Arrange
        var expected = GetRandomInt(1, 32768);
        var args = new[]
        {
            "-p",
            expected.ToString()
        };
        // Act
        var result = args.ParseTo<IArgs>();
        // Assert
        Expect(result.Port)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldParseArgumentBasedOnLongName()
    {
        // Arrange
        var expected = GetRandomInt(1, 32768);
        var args = new[]
        {
            "--listen-port",
            expected.ToString()
        };

        // Act
        var result = args.ParseTo<IArgs>();
        // Assert
        Expect(result.Port)
            .To.Equal(expected);
    }

    [TestCase("--other-property")]
    [TestCase("--otherproperty")]
    [TestCase("--otherProperty")]
    [TestCase("--OtherProperty")]
    public void ShouldFallBackToMatchingPropertyNameForLongName(string arg)
    {
        // Arrange
        var expected = GetRandomInt(1, 32768);
        var args = new[]
        {
            arg,
            expected.ToString()
        };
        // Act
        var result = args.ParseTo<IArgs>();
        // Assert
        Expect(result.OtherProperty)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeCaseSensitiveForShortName()
    {
        // Arrange
        var e1 = GetRandomInt(1, 32768);
        var e2 = GetAnother(e1);
        var args = new[]
        {
            "-p",
            e1.ToString(),
            "-P",
            e2.ToString()
        };

        // Act
        var result = args.ParseTo<IArgs>();
        // Assert
        Expect(result.Port)
            .To.Equal(e1);
        Expect(result.UpperCaseP)
            .To.Equal(e2);
    }

    [Test]
    public void ShouldNotCollectMultipleValuesForNonEnumerableProperty()
    {
        // Arrange
        var expected = GetRandomInt(1, 32768);
        var unexpected = GetAnother(expected);
        var args = new[]
        {
            "-p",
            expected.ToString(),
            unexpected.ToString()
        };
        // Act
        var result = args.ParseTo<IArgs>(out var uncollected);
        // Assert
        Expect(result.Port)
            .To.Equal(expected);
        Expect(uncollected)
            .To.Equal(
                new[]
                {
                    unexpected.ToString()
                }
            );
    }

    [Test]
    public void ShouldCollectMultipleValuesForEnumerableProperty()
    {
        // Arrange
        var args = new[]
        {
            "-v",
            "1",
            "2",
            "3"
        };
        // Act
        var result = args.ParseTo<ISum>();
        // Assert
        Expect(result.Values)
            .To.Equal(
                new[]
                {
                    1,
                    2,
                    3
                }
            );
    }

    [Test]
    public void ShouldSetFlagFalseWhenNotProvided()
    {
        // Arrange
        var args = new string[0];
        // Act
        var result = args.ParseTo<IHasFlags>();
        // Assert
        Expect(result.Frob)
            .To.Be.False();
    }

    [Test]
    public void ShouldSetFlagToTrueWhenProvided()
    {
        // Arrange
        var args = new[]
        {
            "--frob"
        };
        // Act
        var result = args.ParseTo<IHasFlags>();
        // Assert
        Expect(result.Frob)
            .To.Be.True();
    }

    [Test]
    public void ShouldSetFlagToTrueWhenDefaultedTrueAndMissing()
    {
        // Arrange
        var args = new string[0];
        // Act
        var result = args.ParseTo<IHasDefaultTrueFrob>();
        // Assert
        Expect(result.Frob)
            .To.Be.True();
    }

    [Test]
    public void ShouldUnderstandImplicitFlagNegations()
    {
        // Arrange
        var args = new[]
        {
            "--no-frob"
        };
        // Act
        var result = args.ParseTo<IHasDefaultTrueFrob>();
        // Assert
        Expect(result.Frob)
            .To.Be.False();
    }

    [Test]
    public void ShouldErrorAndExitWhenConflictingArgumentsGiven()
    {
        // Arrange
        var args = new[]
        {
            "--flag-1",
            "--flag-2"
        };
        var opts = CreateRecordingParserOptions(out var parserResult);
        // Act
        args.ParseTo<IHasConflictingFlags>(
            out _,
            opts
        );
        // Assert
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .To.Equal(1);
        Expect(lines)
            .To.Contain.Only(1)
            .Matched.By(line => line.Contains("--flag-1 conflicts with --flag-2"));
        Expect(lines)
            .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
    }

    [Test]
    public void ShouldErrorAndExitIfFlagAndNoFlagSpecified()
    {
        // Arrange
        var args = new[]
        {
            "--flag-1",
            "--no-flag-1"
        };
        var opts = CreateRecordingParserOptions(out var parserResult);
        var expected = $"{args[1]} conflicts with {args[0]}";

        // Act
        args.ParseTo<IHasConflictingFlags>(
            out _,
            opts
        );
        // Assert
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .To.Equal(1);
        Expect(lines)
            .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
        Expect(lines)
            .To.Contain.Only(1)
            .Matched.By(
                line => line.Contains(expected),
                () => $"expected: {expected}\nreceieved: {lines.JoinWith("\n")}"
            );
    }

    [Test]
    public void ShouldErrorAndExitIfFlagAndNoFlagSpecifiedReversed()
    {
        // Arrange
        var args = new[]
        {
            "--no-flag-1",
            "--flag-1"
        };
        var opts = CreateRecordingParserOptions(out var parserResult);
        var expected = $"{args[1]} conflicts with {args[0]}";

        // Act
        args.ParseTo<IHasConflictingFlags>(
            out _,
            opts
        );
        // Assert
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .To.Equal(1);
        Expect(lines)
            .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
        Expect(lines)
            .To.Contain.Only(1)
            .Matched.By(
                line => line.Contains(expected),
                () => $"expected: {expected}\nreceieved: {lines.JoinWith("\n")}"
            );
    }

    [Test]
    public void ShouldErrorAndExitIfSingleValueArgAlreadySpecified()
    {
        // Arrange
        var args = new[]
        {
            "--listen-port",
            1.ToString(),
            "-p",
            2.ToString()
        };
        var opts = CreateRecordingParserOptions(out var parserResult);
        // Act
        args.ParseTo<IArgs>(out _, opts);

        // Assert
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .To.Equal(1);
        Expect(lines)
            .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
        Expect(lines)
            .To.Contain.Only(1)
            .Matched.By(
                line => line.Contains("--listen-port specified more than once but only accepts one value")
            );
    }

    [Test]
    public void ShouldErrorAndExitByDefaultWhenEncounteringUnknownSwitch()
    {
        // Arrange
        var args = new[]
        {
            "--flag1",
            "--port"
        };
        var opts = CreateRecordingParserOptions(out var parserResult);

        // Act
        args.ParseTo<IHasConflictingFlags>(out _, opts);
        // Assert
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .To.Equal(1);
        Expect(lines)
            .To.Contain.Only(1).Item(() => lines.JoinWith("\n"));
        Expect(lines)
            .To.Contain.Only(1)
            .Matched.By(
                line => line.Contains("unknown option: --port")
            );
    }

    [Test]
    public void ShouldErrorAndExitIfRequiredArgIsMissing()
    {
        // Arrange
        var args = new string[0];
        var opts = CreateRecordingParserOptions(out var parserResult);
        // Act
        args.ParseTo<IHasRequiredArg>(out _, opts);
        // Assert
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .To.Equal(1);
        Expect(lines)
            .To.Contain.Only(1).Item();
        Expect(lines)
            .To.Contain.Only(1)
            .Matched.By(l => l.Contains("--port is required"));
    }

    [Test]
    public void ShouldVerifyExistingFile()
    {
        // Arrange
        using var tmpFile = new AutoTempFile();
        var validArgs = new[]
        {
            "--existing-file",
            tmpFile.Path
        };
        var invalidArgs = new[]
        {
            "--existing-file",
            $"{tmpFile.Path}.missing"
        };
        var opts = CreateRecordingParserOptions(out var parserResult);
        // Act
        var validParsed = validArgs.ParseTo<IHasFileAttributes>(opts);
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .To.Be.Null(() => "file exists, exit code should not be set");
        Expect(validParsed.ExistingFile)
            .To.Equal(tmpFile.Path);
        Expect(lines)
            .To.Be.Empty();

        var opts2 = CreateRecordingParserOptions(out var parserResult2);
        invalidArgs.ParseTo<IHasFileAttributes>(opts2);
        (exitCode, lines) = parserResult2;
        Expect(exitCode)
            .Not.To.Be.Null()
            .And
            .Not.To.Equal(0);
        Expect(lines)
            .To.Contain.Exactly(1)
            .Matched.By(s => s.Contains(tmpFile.Path));
        // Assert
    }

    [Test]
    public void ShouldVerifyExistingFolder()
    {
        // Arrange
        using var tmpFile = new AutoTempFolder();
        var validArgs = new[]
        {
            "--existing-folder",
            tmpFile.Path
        };
        var invalidArgs = new[]
        {
            "--existing-folder",
            $"{tmpFile.Path}.missing"
        };
        var opts1 = CreateRecordingParserOptions(out var parserResult1);
        var opts2 = CreateRecordingParserOptions(out var parserResult2);
        // Act
        var validParsed = validArgs.ParseTo<IHasFileAttributes>(opts1);
        var (exitCode, lines) = parserResult1;
        Expect(exitCode)
            .To.Be.Null();
        Expect(validParsed.ExistingFolder)
            .To.Equal(tmpFile.Path);
        Expect(lines)
            .To.Be.Empty();

        invalidArgs.ParseTo<IHasFileAttributes>(opts2);
        (exitCode, lines) = parserResult2;
        Expect(exitCode)
            .Not.To.Be.Null()
            .And
            .Not.To.Equal(0);
        Expect(lines)
            .To.Contain.Exactly(1)
            .Matched.By(s => s.Contains(tmpFile.Path));
        // Assert
    }

    [Test]
    public void ShouldErrorOnRequiredExistingFileNotSet()
    {
        // Arrange
        var opts = CreateRecordingParserOptions(out var parserResult);
        // Act
        new string[0].ParseTo<IHasRequiredFileAttributes>(opts);
        // Assert
        var (exitCode, lines) = parserResult;
        Expect(exitCode)
            .Not.To.Be.Null()
            .And
            .Not.To.Equal(0);
        Expect(lines)
            .To.Contain.Exactly(1)
            .Matched.By(s => s.Contains("required"));
    }

    public interface IHasFileAttributes
    {
        [ExistingFile]
        string ExistingFile { get; set; }

        [ExistingFolder]
        string ExistingFolder { get; set; }
    }

    public interface IHasRequiredFileAttributes
    {
        [Required]
        [ExistingFile]
        string AnotherFile { get; set; }
    }

    public interface IHasRequiredArg
    {
        [Required]
        int Port { get; }
    }

    public interface IHasConflictingFlags
    {
        bool Flag1 { get; set; }

        [ConflictsWith(nameof(Flag1))]
        bool Flag2 { get; set; }
    }

    public interface IHasFlags
    {
        bool Frob { get; }
    }

    public interface IHasDefaultTrueFrob
    {
        [Default(true)]
        bool Frob { get; }
    }

    public interface ISum
    {
        int[] Values { get; set; }
    }

    [Test]
    public void ParsingToPOCO()
    {
        // Arrange
        var args = new[]
        {
            "--port",
            "123"
        };
        // Act
        var result = args.ParseTo<PocoArgs>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Port)
            .To.Equal(123);
    }

    [Test]
    public void ShouldBeAbleToConsumeWithNoSwitchesAndOnlyOutArgs()
    {
        // Arrange
        var args = new[]
        {
            "something-something"
        };
        // Act
        var result = args.ParseTo<IFoo>(out var remaining);
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(remaining)
            .To.Equal(args);
    }

    public interface IFoo
    {
        public bool Bar { get; set; }
    }

    public class PocoArgs
    {
        public int Port { get; set; }
    }

    private class ParserResult
    {
        public int? ExitCode { get; set; }
        public string[] Lines => _lines.ToArray();
        private readonly List<string> _lines = new();

        public void AddText(string str)
        {
            _lines.AddRange(
                str.Split(
                    new[]
                    {
                        '\r',
                        '\n'
                    },
                    StringSplitOptions.RemoveEmptyEntries
                )
            );
        }

        public void Deconstruct(out int? exitCode, out string[] lines)
        {
            exitCode = ExitCode;
            lines = Lines;
        }
    }

    private static ParserOptions CreateRecordingParserOptions(
        out ParserResult parserResult
    )
    {
        var myRef = parserResult = new ParserResult();

        return new ParserOptions()
        {
            ExitAction = c => myRef.ExitCode = c,
            LineWriter = str => myRef.AddText(str)
        };
    }


    [TestFixture]
    public class GenerateHelpText
    {
        [Test]
        public void ShouldGenerateHelp()
        {
            // Arrange
            var expected = @"
Some Program Name
This program is designed to make your life so much easier

Usage: someprogram {args} ...files

-f, --flag                the off flag
    --help                shows this help
-h, --host [text]         host to connect to (localhost)
-l, --long-option [text]
-o, --on-flag             the on flag (default: on)
-P, --password [text]
-p, --port [number]
-u, --user [text]         user name to use

Negate any flag argument with --no-{option}

This program was made with love and biscuits.
Exit status codes are 0 for happy and non-zero for unhappy.
Report bugs to <no-one-cares@whatevs.org>
".Trim()
                .Split(
                    new[]
                    {
                        '\r',
                        '\n'
                    },
                    StringSplitOptions.RemoveEmptyEntries
                );
            var args = new[]
            {
                "--help"
            };
            var opts = CreateRecordingParserOptions(out var parserResult);
            // Act
            args.ParseTo<IHelpArgs>(out _, opts);
            // Assert
            var (exitCode, lines) = parserResult;
            Expect(exitCode)
                .To.Equal(2);
            Expect(lines)
                .To.Equal(
                    expected
                );
        }

        [Description(
            @"
Some Program Name
This program is designed to make your life so much easier

Usage: someprogram {args} ...files
"
        )]
        [MoreInfo(
            @"
This program was made with love and biscuits.
Exit status codes are 0 for happy and non-zero for unhappy.
Report bugs to <no-one-cares@whatevs.org>
"
        )]
        public interface IHelpArgs
        {
            [Description("user name to use")]
            public string User { get; set; }

            [Description("host to connect to")]
            [Default("localhost")]
            public string Host { get; set; }

            // should get -p
            public int Port { get; set; }

            // should get -P
            public string Password { get; set; }

            // should get kebab-cased long name
            public string LongOption { get; set; }

            [Description("the off flag")]
            public bool Flag { get; set; }

            [Default(true)]
            [Description("the on flag")]
            public bool OnFlag { get; set; }
        }

        [Test]
        public void ShouldIncludeHelpHeaderAndCopyrightFromParserOptions()
        {
            // Arrange
            var args = new[]
            {
                "--help"
            };
            var collected = new List<string>();
            var opts = new ParserOptions()
            {
                LineWriter = collected.Add,
                Description = new[]
                {
                    GetRandomWords()
                },
                IncludeDefaultDescription = true,
                MoreInfo = new[]
                {
                    GetRandomWords()
                },
                ExitOnError = false,
                ExitAction = _ =>
                {
                }
            };
            // Act
            args.ParseTo<IOpts>(opts);
            // Assert
            Expect(collected.JoinWith("\n").Trim())
                .To.Start.With(opts.Description[0])
                .Then("Usage: ")
                .And
                .To.End.With(opts.MoreInfo[0]);
        }

        public interface IOpts
        {
            int Count { get; set; }
        }

        [Test]
        public void ShouldIncludeAllPropertiesInInterfaceAncestry()
        {
            // Arrange
            var args = new[]
            {
                "--name",
                "bob",
                "--address",
                "here",
                "--color",
                "red",
                "--flag",
                "--value",
                "123"
            };
            var collected = new List<string>();
            // Act
            var result = args.ParseTo<IChild>(
                new ParserOptions()
                {
                    ExitOnError = false,
                    LineWriter = collected.Add
                }
            );
            // Assert
            Expect(collected)
                .To.Be.Empty();
            Expect(result.Name)
                .To.Equal("bob");
            Expect(result.Address)
                .To.Equal("here");
            Expect(result.Color)
                .To.Equal("red");
            Expect(result.Flag)
                .To.Be.True();
            Expect(result.Value)
                .To.Equal(123);
        }

        public interface IGrandParent1
        {
            public string Name { get; set; }
        }

        public interface IGrandParent2
        {
            public string Address { get; set; }
        }

        public interface IParent1 : IGrandParent1, IGrandParent2
        {
            public string Color { get; set; }
        }

        public interface IParent2
        {
            public bool Flag { get; set; }
        }

        public interface IChild : IParent1, IParent2
        {
            public int Value { get; set; }
        }

        [Test]
        public void ShouldNotGenerateShortNameWhenDisabled()
        {
            // Arrange
            var captured = new List<string>();
            var opts = new ParserOptions()
            {
                LineWriter = captured.Add,
                ExitWhenShowingHelp = false
            };
            var args = new[]
            {
                "--help"
            };
            // Act
            args.ParseTo<INoShortName>(opts);
            // Assert
            Expect(captured)
                .Not.To.Contain.Any
                .Matched.By(s => s.StartsWith("-t"));
        }
    }

    public interface IHasFlag
    {
        public bool Flag { get; set; }
    }

    [Test]
    public void ShouldOutputUnknownArgsWhenNotExitingOnError()
    {
        // Arrange
        var args = new[]
        {
            "--flag",
            "some command"
        };
        var opts = new ParserOptions()
        {
            ExitOnError = false
        };
        // Act
        var result = args.ParseTo<IHasFlag>(
            out var uncollected,
            opts
        );
        // Assert
        Expect(result.Flag)
            .To.Be.True();
        Expect(uncollected)
            .To.Equal(
                new[]
                {
                    "some command"
                }
            );
    }

    public interface INoShortName
    {
        [DisableGeneratedShortName]
        public bool TheFlag { get; set; }
    }

    [TestFixture]
    public class FallbackToEnvironmentVariables
    {
        [TestFixture]
        public class WhenEnabled
        {
            [TestFixture]
            public class StringProperty
            {
                [TestCase("RemoteHost")]
                [TestCase("REMOTE_HOST")]
                [TestCase("ReMoTE.Host")]
                public void ShouldObserveEnvironmentVariable_(
                    string varName
                )
                {
                    // Arrange
                    var expected = GetRandomHostname();
                    using var _ = new AutoTempEnvironmentVariable(
                        varName,
                        expected
                    );
                    var args = new string[0];
                    // Act
                    var result = args.ParseTo<Options>(
                        new ParserOptions()
                        {
                            FallbackOnEnvironmentVariables = true
                        }
                    );
                    // Assert
                    Expect(result.RemoteHost)
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class BooleanProperty
            {
                [TestCase("TheFlag")]
                [TestCase("THE_FLAG")]
                [TestCase("ThE.FlAg")]
                public void ShouldObserveEnvironmentVariable_(
                    string varName
                )
                {
                    // Arrange
                    using var _ = new AutoTempEnvironmentVariable(
                        varName,
                        GetRandomFrom(
                            new[]
                            {
                                "1",
                                "true",
                                "yes"
                            }
                        )
                    );
                    var args = new string[0];
                    // Act
                    var result = args.ParseTo<Options>(
                        new ParserOptions()
                        {
                            FallbackOnEnvironmentVariables = true
                        }
                    );
                    // Assert
                    Expect(result.TheFlag)
                        .To.Be.True();
                }
            }

            [Test]
            public void ShouldSolveRequiredMarker()
            {
                // Arrange
                var expected = GetRandomHostname();
                using var _ = new AutoTempEnvironmentVariable(
                    "server",
                    expected
                );
                var captured = -1;
                var args = new string[0];
                // Act
                var result = args.ParseTo<RequiredOptions>(
                    new ParserOptions()
                    {
                        FallbackOnEnvironmentVariables = true,
                        ExitAction = c => captured = c
                    }
                );
                // Assert
                Expect(captured)
                    .To.Equal(-1);
                Expect(result.Server)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldHintForMissingRequiredVars()
            {
                // Arrange
                var args = new string[0];
                var lines = new List<string>();
                // Act
                args.ParseTo<RequiredOptions>(
                    new ParserOptions()
                    {
                        FallbackOnEnvironmentVariables = true,
                        LineWriter = s => lines.Add(s),
                        ExitAction = _ =>
                        {
                        }
                    }
                );
                // Assert
                Expect(lines)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        l => l.EndsWith(
                            "set the SERVER environment variable appropriately"
                        )
                    );
            }

            [Test]
            public void ShouldHintForMissingRequiredVars2()
            {
                // Arrange
                var args = new string[0];
                var lines = new List<string>();
                // Act
                args.ParseTo<RequiredOptions2>(
                    new ParserOptions()
                    {
                        FallbackOnEnvironmentVariables = true,
                        LineWriter = s => lines.Add(s),
                        ExitAction = _ =>
                        {
                        }
                    }
                );
                // Assert
                Expect(lines)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        l => l.EndsWith(
                            "set the REMOTE_HOST environment variable appropriately"
                        )
                    );
            }
        }

        public class Options
        {
            public string RemoteHost { get; set; }
            public bool TheFlag { get; set; }
        }

        public class RequiredOptions
        {
            [Required]
            public string Server { get; set; }
        }

        public class RequiredOptions2
        {
            [Required]
            public string RemoteHost { get; set; }
        }
    }

    [TestFixture]
    public class GenerateArgs
    {
        [Test]
        public void ShouldGenerateTheSingleOption()
        {
            // Arrange
            var opts = new OneOption()
            {
                TheOption = 12
            };
            var expected = new[]
            {
                "--the-option",
                "12"
            };
            // Act
            var result = opts.GenerateArgs();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldGeneratePositiveFlagAppropriately()
        {
            // Arrange
            var positive = new OneFlag()
            {
                TheFlag = true
            };
            var expected = new[]
            {
                "--the-flag"
            };
            // Act
            var result = positive.GenerateArgs();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldGenerateNegativeFlagAppropriately()
        {
            // Arrange
            var positive = new OneFlag()
            {
                TheFlag = false
            };
            var expected = new[]
            {
                "--no-the-flag"
            };
            // Act
            var result = positive.GenerateArgs();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldGenerateTheMultiStringOption()
        {
            // Arrange
            var opts = new MultiStringOption()
            {
                TheOption = new[]
                {
                    "one",
                    "two"
                }
            };
            var expected = new[]
            {
                "--the-option",
                "one",
                "two"
            };
            // Act
            var result = opts.GenerateArgs();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        public class OneOption
        {
            public int TheOption { get; set; }
        }

        public class OneFlag
        {
            public bool TheFlag { get; set; }
        }

        public class MultiStringOption
        {
            public string[] TheOption { get; set; }
        }
    }

    [TestFixture]
    public class Validation
    {
        [TestFixture]
        public class Min
        {
            [Test]
            public void ShouldFailWhenMinValueNotMet()
            {
                // Arrange
                var args = new[]
                {
                    "--some-number",
                    "4"
                };
                var output = new List<string>();
                var exitCode = 0;
                var opts = new ParserOptions()
                {
                    ExitAction = c => exitCode = c,
                    LineWriter = s => output.Add(s)
                };
                // Act
                args.ParseTo<INumericOptions>(
                    out _,
                    opts
                );
                // Assert
                Expect(exitCode)
                    .To.Equal(ExitCodes.ARGUMENT_ERROR);
                Expect(output)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        l => l.Contains("--some-number") &&
                            l.Contains("should be at least 5") &&
                            l.Contains("received: 4")
                    );
            }

            [Test]
            public void ShouldFailWhenMaxValueExceeded()
            {
                // Arrange
                var args = new[]
                {
                    "--some-number",
                    "14"
                };
                var output = new List<string>();
                var exitCode = 0;
                var opts = new ParserOptions()
                {
                    ExitAction = c => exitCode = c,
                    LineWriter = s => output.Add(s)
                };
                // Act
                args.ParseTo<INumericOptions>(
                    out _,
                    opts
                );
                // Assert
                Expect(exitCode)
                    .To.Equal(ExitCodes.ARGUMENT_ERROR);
                Expect(output)
                    .To.Contain.Exactly(1)
                    .Matched.By(
                        l => l.Contains("--some-number") &&
                            l.Contains("should be at most 10") &&
                            l.Contains("received: 14")
                    );
            }
        }

        public interface INumericOptions
        {
            [Min(5)]
            [Max(10)]
            public int SomeNumber { get; set; }
        }
    }

    [TestFixture]
    public class IgnoredProperties
    {
        [Test]
        public void ShouldNotMapThemFromCommandLine()
        {
            // Arrange
            var args = new[]
            {
                "--not-ignored",
                "123",
                "--ignored",
                "456"
            };
            // Act
            var result = args.ParseTo<IOptionsWithIgnoredProperty>(
                new ParserOptions()
                {
                    ExitOnError = false
                }
            );
            // Assert
            Expect(result.NotIgnored)
                .To.Equal(123);
            Expect(result.Ignored)
                .To.Equal(0);
        }

        [Test]
        public void ShouldNotPresentHelpForThem()
        {
            // Arrange
            var args = new[]
            {
                "--help"
            };
            var captured = new List<string>();
            // Act
            args.ParseTo<IOptionsWithIgnoredProperty>(
                new ParserOptions()
                {
                    LineWriter = captured.Add,
                    ExitAction = _ =>
                    {
                        /* intentionally blank - --help should make the program exit */
                    }
                }
            );
            // Assert
            Expect(captured)
                .Not.To.Contain.Any
                .Matched.By(
                    s => s.Contains("--ignored") ||
                        s.Contains("-i ")
                );
        }

        public interface IOptionsWithIgnoredProperty
        {
            [Description("not ignored")]
            int NotIgnored { get; set; }

            [Description("ignored")]
            [Ignore]
            int Ignored { get; set; }
        }
    }

    [TestFixture]
    public class Bugs
    {
        [Test]
        public void ShouldParseNegativeDecimal()
        {
            // Arrange
            var args = new[]
            {
                "--value",
                "-1"
            };
            var exitCode = 0;
            // Act
            var result = args.ParseTo<HasDecimal>(
                out var _,
                new ParserOptions()
                {
                    ExitAction = c => exitCode = c
                }
            );
            // Assert
            Expect(exitCode)
                .To.Equal(0);
            Expect(result.Value)
                .To.Equal(-1);
        }

        [TestFixture]
        public class DefaultValuesOnFlags
        {
            [TestFixture]
            public class WhenFlagDefaultIsTrue
            {
                [TestFixture]
                public class AndFlagNotProvided
                {
                    [Test]
                    public void ShouldBeTrue()
                    {
                        // Arrange
                        var args = Array.Empty<string>();
                        // Act
                        var result = args.ParseTo<HasDefaultPositiveFlag>();
                        // Assert
                        Expect(result.Flag)
                            .To.Be.True();
                    }
                }

                [TestFixture]
                public class AndFlagProvided
                {
                    [TestFixture]
                    public class AsPositiveFlag
                    {
                        [Test]
                        public void ShouldBeTrue()
                        {
                            // Arrange
                            var args = new[]
                            {
                                "--flag"
                            };
                            // Act
                            var result = args.ParseTo<HasDefaultPositiveFlag>();
                            // Assert
                            Expect(result.Flag)
                                .To.Be.True();
                        }
                    }

                    [TestFixture]
                    public class AsNegativeFlag
                    {
                        [Test]
                        public void ShouldBeFalse()
                        {
                            // Arrange
                            var args = new[]
                            {
                                "--no-flag"
                            };
                            // Act
                            var result = args.ParseTo<HasDefaultPositiveFlag>();
                            // Assert
                            Expect(result.Flag)
                                .To.Be.False();
                        }
                    }
                }

                public class HasDefaultPositiveFlag
                {
                    [Default(true)]
                    public bool Flag { get; set; }
                }
            }

            [TestFixture]
            public class WhenFlagDefaultIsFalse
            {
                [TestFixture]
                public class AndFlagNotProvided
                {
                    [Test]
                    public void ShouldBeFalse()
                    {
                        // Arrange
                        var args = Array.Empty<string>();
                        // Act
                        var result = args.ParseTo<HasDefaultNegativeFlag>();
                        // Assert
                        Expect(result.Flag)
                            .To.Be.False();
                    }
                }

                [TestFixture]
                public class AndFlagProvided
                {
                    [TestFixture]
                    public class AsPositiveFlag
                    {
                        [Test]
                        public void ShouldBeTrue()
                        {
                            // Arrange
                            var args = new[]
                            {
                                "--flag"
                            };
                            // Act
                            var result = args.ParseTo<HasDefaultNegativeFlag>();
                            // Assert
                            Expect(result.Flag)
                                .To.Be.True();
                        }
                    }

                    [TestFixture]
                    public class AsNegativeFlag
                    {
                        [Test]
                        public void ShouldBeFalse()
                        {
                            // Arrange
                            var args = new[]
                            {
                                "--no-flag"
                            };
                            // Act
                            var result = args.ParseTo<HasDefaultNegativeFlag>();
                            // Assert
                            Expect(result.Flag)
                                .To.Be.False();
                        }
                    }
                }
            }

            [TestFixture]
            public class EnvironmentalOverridesForDefaults
            {
                [TestFixture]
                public class WhenOverrideSpecified
                {
                    [TestFixture]
                    public class AndDefaultFromEnvironmentAllowed
                    {
                        [Test]
                        public void ShouldSetTheValueFromTheEnvironment()
                        {
                            // Arrange
                            using var e1 = new AutoTempEnvironmentVariable(
                                "SOME_FLAG",
                                GetRandomFrom(
                                    new[]
                                    {
                                        "1",
                                        "yes",
                                        "true"
                                    }
                                )
                            );
                            var name = GetRandomName();
                            using var e2 = new AutoTempEnvironmentVariable(
                                "MOO_COWS",
                                name
                            );

                            // Act
                            var result = new string[0].ParseTo<HasEnvironmentDefaultOption>();
                            // Assert
                            Expect(result.SomeFlag)
                                .To.Be.True();
                            Expect(result.SomeName)
                                .To.Equal(name);
                        }

                        public class HasEnvironmentDefaultOption
                        {
                            [AllowDefaultFromEnvironment]
                            public bool SomeFlag { get; set; }

                            [AllowDefaultFromEnvironment("MOO_COWS")]
                            public string SomeName { get; set; }
                        }
                    }
                }

                [TestFixture]
                public class DecoratingEntireOptionsObjects
                {
                    [Test]
                    public void ShouldReadAllDefaultsFromEnvironment()
                    {
                        // Arrange
                        var expectedNumber = GetRandomInt(1);
                        using var e1 = new AutoTempEnvironmentVariable(
                            "THE_NUMBER",
                            $"{expectedNumber}"
                        );
                        var expectedString = GetRandomString();
                        using var e2 = new AutoTempEnvironmentVariable(
                            "THE.NA_ME",
                            $"{expectedString}"
                        );
                        // Act
                        var result = new string[0].ParseTo<AllFromEnv>();
                        // Assert
                        Expect(result.TheNumber)
                            .To.Equal(expectedNumber);
                        Expect(result.TheName)
                            .To.Equal(expectedString);
                    }

                    [AllowDefaultsFromEnvironment]
                    public interface IOptions
                    {
                        int Number { get; set; }
                    }

                    [AllowDefaultsFromEnvironment]
                    public class AllFromEnv
                    {
                        [Default(1)]
                        public int TheNumber { get; set; }

                        [Default("moo")]
                        public string TheName { get; set; }
                    }
                }
            }

            public class HasDefaultNegativeFlag
            {
                [Default(false)]
                public bool Flag { get; set; }
            }
        }

        [TestFixture]
        public class DefaultValuesForEnumProperties
        {
            [Test]
            public void ShouldParseStringValue()
            {
                // Arrange
                var args = new[]
                {
                    "--type",
                    nameof(VehicleTypes.Car)
                };
                // Act
                var result = args.ParseTo<IVehicle>();
                // Assert
                Expect(result.Type)
                    .To.Equal(VehicleTypes.Car);
            }

            [Test]
            public void ShouldParseDefaultValueFromString()
            {
                // Arrange
                var args = new string[0];
                // Act
                var result = args.ParseTo<IVehicle>();
                // Assert
                Expect(result.Type)
                    .To.Equal(VehicleTypes.Bike);
            }

            public interface IVehicle
            {
                [Default(nameof(VehicleTypes.Bike))]
                VehicleTypes Type { get; set; }
            }

            public enum VehicleTypes
            {
                None,
                Car,
                Bike,
                Plane
            }
        }

        public class HasDecimal
        {
            public decimal Value { get; set; }
        }
    }

    [TestFixture]
    public class DateTimeHandling
    {
        [Test]
        public void ShouldParseYYYMMDD()
        {
            // Arrange
            var date = GetRandomDate();
            var args = new[]
            {
                "--date-time",
                $"{date.Year}/{date.Month}/{date.Day}"
            };
            // Act
            var result = args.ParseTo<HasDateTime>();
            // Assert
            Expect(result.DateTime)
                .To.Equal(date.Date);
        }

        public class HasDateTime
        {
            public DateTime DateTime { get; set; }
        }
    }

    public interface IArgs
    {
        [ShortName('p')]
        [LongName("listen-port")]
        public int Port { get; set; }

        public int OtherProperty { get; set; }

        [ShortName('P')]
        public int UpperCaseP { get; set; }
    }
}