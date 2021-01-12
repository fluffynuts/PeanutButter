using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Args.Tests
{
    [TestFixture]
    public class TestArgsParser
    {
        [Test]
        public void ShouldParseArgumentBasedOnShortName()
        {
            // Arrange
            var expected = GetRandomInt(1, 32768);
            var args = new[] { "-p", expected.ToString() };
            // Act
            var result = args.Parse<IArgs>();
            // Assert
            Expect(result.Port)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldParseArgumentBasedOnLongName()
        {
            // Arrange
            var expected = GetRandomInt(1, 32768);
            var args = new[] { "--listen-port", expected.ToString() };

            // Act
            var result = args.Parse<IArgs>();
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
            var args = new[] { arg, expected.ToString() };
            // Act
            var result = args.Parse<IArgs>();
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
            var args = new[] { "-p", e1.ToString(), "-P", e2.ToString() };

            // Act
            var result = args.Parse<IArgs>();
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
            var args = new[] { "-p", expected.ToString(), unexpected.ToString() };
            // Act
            var result = args.Parse<IArgs>(out var uncollected);
            // Assert
            Expect(result.Port)
                .To.Equal(expected);
            Expect(uncollected)
                .To.Equal(new[] { unexpected.ToString() });
        }

        [Test]
        public void ShouldCollectMultipleValuesForEnumerableProperty()
        {
            // Arrange
            var args = new[] { "-v", "1", "2", "3" };
            // Act
            var result = args.Parse<ISum>();
            // Assert
            Expect(result.Values)
                .To.Equal(new[] { 1, 2, 3 });
        }

        [Test]
        public void ShouldSetFlagFalseWhenNotProvided()
        {
            // Arrange
            var args = new string[0];
            // Act
            var result = args.Parse<IHasFlags>();
            // Assert
            Expect(result.Frob)
                .To.Be.False();
        }

        [Test]
        public void ShouldSetFlagToTrueWhenProvided()
        {
            // Arrange
            var args = new[] { "--frob" };
            // Act
            var result = args.Parse<IHasFlags>();
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
            var result = args.Parse<IHasDefaultTrueFrob>();
            // Assert
            Expect(result.Frob)
                .To.Be.True();
        }

        [Test]
        public void ShouldUnderstandImplicitFlagNegations()
        {
            // Arrange
            var args = new[] { "--no-frob" };
            // Act
            var result = args.Parse<IHasDefaultTrueFrob>();
            // Assert
            Expect(result.Frob)
                .To.Be.False();
        }

        [Test]
        public void ShouldErrorAndExitWhenConflictingArgumentsGiven()
        {
            // Arrange
            var args = new[] { "--flag1", "--flag2" };
            int? exitCode = null;
            var lines = new List<string>();
            var opts = new ArgParserOptions()
            {
                ExitAction = c => exitCode = c,
                LineWriter = str => lines.Add(str)
            };

            // Act
            var result = args.Parse<IHasConflictingFlags>(
                out var uncollected,
                opts
            );
            // Assert
            Expect(exitCode)
                .To.Equal(1);
            Expect(lines)
                .To.Contain.Only(1)
                .Matched.By(line => line.Contains("--flag1 conflicts with --flag2"));
            
            // FIXME: this actually produces the inverse message and the message that flag1 conflicts with itself, which comes from the auto-inversion
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

        [TestFixture]
        public class GenerateHelpText
        {
            [Test]
            [Ignore("WIP")]
            public void ShouldGenerateHelp()
            {
                // Arrange
                var expected = @"
Some Program Name
This program is designed to make your life so much easier

Usage: someprogram {args} ...files

-f, --flag                  the flag
    --no-flag               (negated)
-h, --host [text]           host to connect to (localhost)
-l, --long-option [text]    Long option
-P, --password [text]       Password
-p, --port [number]         Port
-u, --user [text]           user name to use

This program was made with love and biscuits.
Exit status codes are 0 for happy and non-zero for unhappy.
Report bugs to <no-one-cares@whatevs.org>
".Trim();
                // Act
                // Assert
            }

            [Description(@"
Some Program Name
This program is designed to make your life so much easier

Usage: someprogram {args} ...files
")]
            [MoreInfo(@"
This program was made with love and biscuits.
Exit status codes are 0 for happy and non-zero for unhappy.
Report bugs to <no-one-cares@whatevs.org>
")]
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

                [Description("the flag")]
                public bool Flag { get; set; }
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
}