using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable StringLiteralTypo

// ReSharper disable ExpressionIsAlwaysNull

namespace PeanutButter.Utils.Tests;

[TestFixture]
[Parallelizable]
public class TestStringExtensions
{
    [TestCase("Hello World", "^Hello", "Goodbye", "Goodbye World")]
    [TestCase("Hello World", "Wor.*", "Goodbye", "Hello Goodbye")]
    [TestCase("Hello World", "Hello$", "Goodbye", "Hello World")]
    [Parallelizable]
    public void RegexReplace_ShouldReplaceAccordingToRegexWithSuppliedValue(
        string input,
        string re,
        string replaceWith,
        string expected
    )
    {
        //---------------Set up test pack-------------------

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = input.RegexReplace(re, replaceWith);


        //---------------Test Result -----------------------
        Expect(result)
            .To.Equal(expected);
    }

    [TestFixture]
    public class Window
    {
        [TestFixture]
        public class GivenEmptyStringOrNull
        {
            [TestCase("")]
            [TestCase(null)]
            public void ShouldReturnEmptyString(
                string subject
            )
            {
                // Arrange
                // Act
                var result = subject.Window(1, 1);
                // Assert
                Expect(result)
                    .To.Equal("");
            }
        }

        [TestFixture]
        public class GivenCenterOutsideOfString
        {
            [Test]
            public void ShouldThrow()
            {
                // Arrange
                var str = "123";
                var idx = 3;
                // Act
                Expect(() => str.Window(idx, 1))
                    .To.Throw<ArgumentOutOfRangeException>()
                    .For("centeredOn");
                // Assert
            }
        }

        [TestFixture]
        public class GivenNegativeContext
        {
            [Test]
            public void ShouldAssumeZero()
            {
                // Arrange
                // Act
                var result = "123".Window(1, -1);
                // Assert
                Expect(result)
                    .To.Equal("2");
            }
        }

        [TestFixture]
        public class Given3CharStringWithZeroContext
        {
            [Test]
            public void ShouldReturnMiddleChar()
            {
                // Arrange
                var str = "123";
                var idx = 1;
                var ctx = 0;
                // Act
                var result = str.Window(idx, ctx);
                // Assert
                Expect(result)
                    .To.Equal("2");
            }
        }

        [TestFixture]
        public class GivenStringWithMaxCharsOutsideString
        {
            [Test]
            public void ShouldReturnTheFullString()
            {
                // Arrange
                var str = "12345";
                var idx = 2;
                // Act
                var result = str.Window(idx, int.MaxValue);
                // Assert
                Expect(result)
                    .To.Equal(str);
            }
        }

        [TestFixture]
        public class GivenStringWithExtraCharsOutsideWindow
        {
            [Test]
            public void ShouldReturnTheRequiredWindow()
            {
                // Arrange
                var str = "0123456789";
                var idx = 4;
                var chars = 3;
                var expected = "1234567";
                // Act
                var result = str.Window(idx, chars);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WhenCenteredOnIsLastChar
        {
            [Test]
            public void ShouldWindowBackFromTheEnd()
            {
                // Arrange
                var str = "aaa";
                // Act
                var result = str.Window(2, 100);
                // Assert
                Expect(result)
                    .To.Equal(str);
            }
        }
    }

    [TestFixture]
    public class SplitLines
    {
        [Test]
        public void ShouldReturnSingleElementArrayForTextWithNoNewlines()
        {
            // Arrange
            var input = GetRandomWords();
            // Act
            var result = input.SplitIntoLines();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        input
                    }
                );
        }

        [Test]
        public void ShouldSplitUnixLines()
        {
            // Arrange
            var str = "foo\nbar";
            // Act
            var result = str.SplitIntoLines();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        "foo",
                        "bar"
                    }
                );
        }

        [Test]
        public void ShouldSplitDOSLines()
        {
            // Arrange
            var str = "foo\r\nbar";
            // Act
            var result = str.SplitIntoLines();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        "foo",
                        "bar"
                    }
                );
        }

        [Test]
        public void ShouldSplitOldSchoolMacLines()
        {
            // Arrange
            var str = "foo\rbar";
            // Act
            var result = str.SplitIntoLines();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        "foo",
                        "bar"
                    }
                );
        }

        [Test]
        public void ShouldSplitMixedLines()
        {
            // Arrange
            var str = "foo\nbar\r\nquuz\rwibbles\n";
            var expected = new[]
            {
                "foo",
                "bar",
                "quuz",
                "wibbles",
                ""
            };
            // Act
            var result = str.SplitIntoLines();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ReplaceAll
    {
        [TestFixture]
        [Parallelizable]
        public class WorkingWithChars
        {
            [Test]
            [Parallelizable]
            public void ShouldReplaceAllParamsCharsWithReplacementChar()
            {
                // Arrange
                var input = "foo.bar-quux_wat";
                var expected = "foo bar quux wat";

                // Act
                var result = input.ReplaceAll(
                    new[]
                    {
                        '.',
                        '-',
                        '_'
                    },
                    ' '
                );
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WorkingWithStrings
        {
            [Test]
            [Parallelizable]
            public void ShouldReplaceAllParamsCharsWithReplacementChar()
            {
                // Arrange
                var input = "foo.bar-quux_wat";
                var expected = "foo bar quux wat";

                // Act
                var result = input.ReplaceAll(
                    new[]
                    {
                        ".",
                        "-",
                        "_"
                    },
                    " "
                );
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class Performance
        {
            [Test]
            [Parallelizable]
            public void SingleCharStringsVsChars()
            {
                // Arrange
                var input = "foo.bar-quux_wat";
                var chars = new[]
                {
                    '.',
                    '-',
                    '_'
                };
                var strings = new[]
                {
                    ".",
                    "-",
                    "_"
                };
                var iterations = 10000000;
                string foo = null;

                // Act
                Benchmark.Time(
                    () =>
                    {
                        foo = input.ReplaceAll(strings, " ");
                    },
                    100
                );
                Benchmark.Time(
                    () =>
                    {
                        foo = input.ReplaceAll(chars, ' ');
                    },
                    100
                );
                var stringTime = Benchmark.Time(
                    () =>
                    {
                        foo = input.ReplaceAll(strings, " ");
                    },
                    iterations
                );
                var charTime = Benchmark.Time(
                    () =>
                    {
                        foo = input.ReplaceAll(chars, ' ');
                    },
                    iterations
                );
                // Assert
                Expect(foo)
                    .Not.To.Be.Null();
                Console.WriteLine($"chars:   {charTime}");
                Console.WriteLine($"strings: {stringTime}");
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class RemoveAll
    {
        [Test]
        [Parallelizable]
        public void ShouldRemoveAllTheProvidedChars()
        {
            // Arrange
            var input = "foo-bar.quux_wat";
            // ReSharper disable once StringLiteralTypo
            var expected = "foobarquuxwat";
            // Act
            var result = input.RemoveAll(
                '-',
                '.',
                '_',
                ':'
            );
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }


    [TestFixture]
    [Parallelizable]
    public class RegexReplaceAll
    {
        [Test]
        [Parallelizable]
        public void ShouldReplaceSinglePatternAllMatches()
        {
            // Arrange
            var input = "-aBc123-%";
            // Act
            var result = input.RegexReplaceAll(
                "",
                "[^a-zA-Z0-9]"
            );
            // Assert
            Expect(result)
                .To.Equal("aBc123");
        }

        [Test]
        [Parallelizable]
        public void ShouldReplaceMultiplePatterns()
        {
            // Arrange
            var input = "_-123abc";

            // Act
            var result = input.RegexReplaceAll(
                "",
                "[_-]",
                "[a-z]"
            );
            // Assert
            Expect(result)
                .To.Equal("123");
        }
    }

    [TestFixture]
    [Parallelizable]
    public class Or
    {
        [TestCase(null)]
        [TestCase("")]
        [Parallelizable]
        public void ShouldReturnOtherStringWhenPrimaryIs_(
            string value
        )
        {
            // think equivalent to Javascript:
            //  var left = null;
            //  var right = 'foo';
            //  var empty = '';
            //  var result = left || right; // result is 'foo'
            //  var other = empty || right; // result is 'foo'
            //---------------Set up test pack-------------------
            string src = null;
            var other = GetRandomString(1, 20);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.Or(other);

            //---------------Test Result -----------------------
            Expect(result).To.Equal(other);
        }

        [Test]
        [Parallelizable]
        public void ShouldChainUntilFirstValidValue()
        {
            //---------------Set up test pack-------------------
            string start = null;
            var expected = GetRandomString(1, 10);
            var unexpected = GetRandomString(11, 20);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = start.Or("").Or(null).Or(expected).Or(unexpected);

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class AsBoolean
    {
        [TestCase("Yes", true)]
        [TestCase("Y", true)]
        [TestCase("yes", true)]
        [TestCase("y", true)]
        [TestCase("1", true)]
        [TestCase("True", true)]
        [TestCase("TRUE", true)]
        [TestCase("true", true)]
        [TestCase("On", true)]
        [TestCase("oFf", false)]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase("\t", false)]
        [TestCase(null, false)]
        [Parallelizable]
        public void ShouldResolveToBooleanValue(
            string input,
            bool expected
        )
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsBoolean();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class AsString
    {
        [TestFixture]
        [Parallelizable]
        public class OperatingOnByteArray
        {
            [Test]
            [Parallelizable]
            public void GivenNull_ShouldReturnNull()
            {
                // Arrange
                byte[] src = null;
                // Pre-Assert
                // Act
                var result = src.AsString();
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }

            [Test]
            [Parallelizable]
            public void GivenNoEncoding_ShouldReturnUTF8String()
            {
                // Arrange
                var src = GetRandomString(4);
                var data = src.AsBytes();
                // Pre-Assert
                // Act
                var result = data.AsString();
                // Assert
                Expect(result).To.Equal(src);
            }

            [Test]
            [Parallelizable]
            public void GivenEncoding_ShouldReturnFromThatEncoding()
            {
                // Arrange
                var src = GetRandomString(4);
                var encoding = Encoding.BigEndianUnicode;
                var data = src.AsBytes(encoding);
                // Pre-Assert
                // Act
                var result = data.AsString(encoding);
                // Assert
                Expect(result).To.Equal(src);
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class AsBytes
    {
        [Test]
        [Parallelizable]
        public void WhenStringIsNull_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ((string)null).AsBytes();

            //---------------Test Result -----------------------
            Expect((object)result).To.Be.Null();
        }

        [Test]
        [Parallelizable]
        public void OperatingOnEmptyString_ShouldReturnEmptyByteArray()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = string.Empty.AsBytes();

            //---------------Test Result -----------------------
            Expect((object)result).Not.To.Be.Null();
            Assert.That(result, Is.Empty);
        }

        [Test]
        [Parallelizable]
        public void OperatingOnNonEmptyString_ShouldReturnStringEncodedAsBytesFromUTF8()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomString(50, 100);
            var expected = Encoding.UTF8.GetBytes(input);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsBytes();

            //---------------Test Result -----------------------
            Assert.That(result, Is.EqualTo(expected));
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private static Encoding[] Encodings { get; } =
        [
            Encoding.UTF8,
            Encoding.ASCII,
            Encoding.UTF32,
            Encoding.UTF7
        ];

        [TestCaseSource(nameof(Encodings))]
        [Parallelizable]
        public void
            OperatingOnNonEmptyString_WhenGivenEncoding_ShouldReturnStringEncodedAsBytesFromEncoding(
                Encoding encoding
            )
        {
            //---------------Set up test pack-------------------
            var input = GetRandomString(50, 100);
            var expected = encoding.GetBytes(input);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsBytes(encoding);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class AsStream
    {
        [Test]
        [Parallelizable]
        public void ActingOnNull_ShouldReturnNull()
        {
            // Arrange
            string src = null;
            // Pre-Assert
            // Act
            var result = src.AsStream();
            // Assert
            Expect(result).To.Be.Null();
        }

        [Test]
        [Parallelizable]
        public void ActingOnEmptyString_ShouldReturnEmptyStream()
        {
            // Arrange
            var src = "";
            // Pre-Assert
            // Act
            var result = src.AsStream();
            // Assert
            var bytes = result.ReadAllBytes();
            Expect(bytes).To.Be.Empty();
        }

        [Test]
        [Parallelizable]
        public void ActingOnNonEmptyString_ShouldReturnStreamContainingData()
        {
            // Arrange
            var src = GetRandomString(2);
            // Pre-Assert
            // Act
            var result = src.AsStream();
            // Assert
            var asString = Encoding.UTF8.GetString(result.ReadAllBytes());
            Expect(asString).To.Equal(src);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class IsInteger
    {
        [TestFixture]
        [Parallelizable]
        public class WhenStringIsInteger
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnTrue()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomInt().ToString();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.IsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenStringIsNotInteger
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomAlphaString();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.IsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.False();
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class AsInteger
    {
        [TestFixture]
        [Parallelizable]
        public class WhenStringIsInteger
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnThatIntegerValue()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomInt(10, 100);
                var input = expected.ToString();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenStringIsFloatingPointWithPeriod
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnTruncatedIntPart()
            {
                //---------------Set up test pack-------------------
                var input = "1.2";

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(1);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenStringIsFloatingPointWithComma
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnTruncatedIntPart()
            {
                //---------------Set up test pack-------------------
                var input = "1,2";

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(1);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenStringContainsAlphaChars
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnIntPartOfBeginning()
            {
                //---------------Set up test pack-------------------
                var input = "2ab4";
                var expected = 2;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenStringHasLeadingAlphaPart
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnIntPartOfBeginning()
            {
                //---------------Set up test pack-------------------
                var input = "woof42meow4";
                var expected = 42;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenStringIsNotAnInteger
        {
            [TestCase("a")]
            [TestCase("")]
            [TestCase("\r\n")]
            [TestCase(null)]
            [Parallelizable]
            public void ShouldReturnZeroFor_(string input)
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsInteger();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(0);
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class IsNullOrWhitespace
    {
        [TestFixture]
        [Parallelizable]
        public class WhenOperatingOnNullOrWhitespace
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static readonly string[] NullOrWhitespaceStrings =
            [
                null,
                "\t",
                "\r",
                "\n"
            ];

            [TestCaseSource(nameof(NullOrWhitespaceStrings))]
            [Parallelizable]
            public void ShouldReturnTrueFor_(
                string src
            )
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = src.IsNullOrWhiteSpace();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenOperatingOnNonWhitespaceString
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var src = GetRandomString();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = src.IsNullOrWhiteSpace();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.False();
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class IsNullOrEmpty
    {
        [TestCase(null)]
        [TestCase("")]
        [Parallelizable]
        public void ShouldReturnTrue_For_(
            string src
        )
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsNullOrEmpty();

            //---------------Test Result -----------------------
            Expect(result)
                .To.Be.True();
        }

        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r")]
        [Parallelizable]
        public void ShouldReturnFalse_For_(
            string src
        )
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsNullOrEmpty();

            //---------------Test Result -----------------------
            Expect(result)
                .To.Be.False();
        }

        [TestFixture]
        [Parallelizable]
        public class WhenStringIsNotWhitespaceOrNull
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnFalse()
            {
                //---------------Set up test pack-------------------
                var input = GetRandomString();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.IsNullOrEmpty();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.False();
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ContainsOneOf
    {
        [Test]
        [Parallelizable]
        public void GivenNoNeedles_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsOneOf())
                .To.Throw<ArgumentException>();
            //--------------- Assert -----------------------
        }

        [Test]
        [Parallelizable]
        public void GivenNullNeedle_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsOneOf(null, "foo"))
                .To.Throw<ArgumentException>();
            //--------------- Assert -----------------------
        }

        [Test]
        [Parallelizable]
        public void OperatingOnNull_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = (null as string).ContainsOneOf("foo");
            //--------------- Assert -----------------------
            Expect(result)
                .To.Be.False();
        }

        [Test]
        [Parallelizable]
        public void OperatingOnStringContainingNoneOfTheNeedles_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var input = "foo";
            var search = new[]
            {
                "bar",
                "quuz",
                "wibbles"
            };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsOneOf(search);

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        [Parallelizable]
        public void OperatingOnStringContainingOnneOfTheNeedles_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var input = "foo";
            var search = new[]
            {
                "bar",
                "quuz",
                "oo",
                "wibbles"
            }.Randomize().ToArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsOneOf(search);

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ContainsAllOf
    {
        [Test]
        [Parallelizable]
        public void GivenNoNeedles_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsAllOf())
                .To.Throw<ArgumentException>();
            //--------------- Assert -----------------------
        }

        [Test]
        [Parallelizable]
        public void GivenNullNeedle_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => "foo".ContainsAllOf(null, "foo"))
                .To.Throw<ArgumentException>();
            //--------------- Assert -----------------------
        }

        [Test]
        [Parallelizable]
        public void OperatingOnNull_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = (null as string).ContainsAllOf("foo");
            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        [Parallelizable]
        public void WhenHaystackContainsAllConstituents_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var input = "hello, world";
            var search = new[]
            {
                "hello",
                ", ",
                "world"
            }.Randomize().ToArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsAllOf(search);

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        [Test]
        [Parallelizable]
        public void WhenHaystackMissingNeedle_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var input = "hello, world";
            var search = new[]
            {
                "hello",
                ", ",
                "there"
            }.Randomize().ToArray();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ContainsAllOf(search);

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }
    }

    [TestFixture]
    [Parallelizable]
    public class IsWhiteSpace
    {
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r")]
        [TestCase("\n")]
        [Parallelizable]
        public void ShouldReturnTrueFor_(string input)
        {
            // Arrange
            // Act
            var result = input.IsWhiteSpace();
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("a")]
        [TestCase(" a")]
        [TestCase("a ")]
        [Parallelizable]
        public void ShouldReturnFalseFor_(string input)
        {
            // Arrange
            // Act
            var result = input.IsWhiteSpace();
            // Assert
            Expect(result)
                .To.Be.False();
        }
    }

    [TestFixture]
    [Parallelizable]
    public class IsEmptyOrWhiteSpace
    {
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\r")]
        [TestCase("\n")]
        [Parallelizable]
        public void ShouldReturnTrueFor_(string input)
        {
            // Arrange
            // Act
            var result = input.IsEmptyOrWhiteSpace();
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [TestCase(null)]
        [TestCase("a")]
        [TestCase(" a")]
        [TestCase("a ")]
        [Parallelizable]
        public void ShouldReturnFalseFor_(string input)
        {
            // Arrange
            // Act
            var result = input.IsEmptyOrWhiteSpace();
            // Assert
            Expect(result)
                .To.Be.False();
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ToBase64
    {
        [Test]
        [Parallelizable]
        public void OperatingOnNullString_ShouldReturnNull()
        {
            //--------------- Arrange -------------------
            string input = null;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            // ReSharper disable once ExpressionIsAlwaysNull
            var result = input.ToBase64();

            //--------------- Assert -----------------------
            Expect(result).To.Be.Null();
        }

        [Test]
        [Parallelizable]
        public void OperatingOnEmptyString()
        {
            //--------------- Arrange -------------------
            var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(""));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = "".ToBase64();

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        [Parallelizable]
        public void OperatingOnNonEmptyString()
        {
            //--------------- Arrange -------------------
            var input = GetRandomString(5, 10);
            var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = input.ToBase64();

            //--------------- Assert -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ToKebabCase
    {
        [Test]
        [Parallelizable]
        public void OperatingOnNull_ShouldReturnNull()
        {
            // Arrange
            var input = null as string;

            // Pre-Assert

            // Act
            // ReSharper disable once ExpressionIsAlwaysNull
            var result = input.ToKebabCase();

            // Assert
            Expect(result).To.Be.Null();
        }

        [TestCase("Moo", "moo")]
        [TestCase("MooCow", "moo-cow")]
        [TestCase("i_am_snake", "i-am-snake")]
        [TestCase("is-already-kebabed", "is-already-kebabed")]
        [TestCase("has-12345-numeric", "has-12345-numeric")]
        [TestCase("has-ABBREVIATION-inside", "has-abbreviation-inside")]
        [TestCase("Today_Is_Great", "today-is-great")]
        [TestCase("foo--bar", "foo-bar")]
        [TestCase("Some Service 2", "some-service-2")]
        [TestCase("some-service-2", "some-service-2")]
        [TestCase("Some Service 2 Moo", "some-service-2-moo")]
        [Parallelizable]
        public void ShouldConvert_(
            string from,
            string expected
        )
        {
            // Arrange

            // Pre-Assert

            // Act
            var result = from.ToKebabCase();

            // Assert
            Expect(result).To.Equal(expected);
        }

        [Test]
        [Parallelizable]
        public void ShouldPreserveGuids()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var s = $"some_guid_{guid}";
            var expected = $"some-guid-{guid.ToString().ToLower()}";
            // Act
            var result = s.ToKebabCase();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ToSnakeCase
    {
        [Test]
        [Parallelizable]
        public void OperatingOnNull_ShouldReturnEmptyString()
        {
            // Arrange
            var input = null as string;

            // Pre-Assert

            // Act
            var result = input.ToSnakeCase();

            // Assert
            Expect(result)
                .To.Be.Empty();
        }

        [TestCase("Moo", "moo")]
        [TestCase("MooCow", "moo_cow")]
        [TestCase("i_am_snake", "i_am_snake")]
        [TestCase("is-already-kebabed", "is_already_kebabed")]
        [TestCase("DBIndex", "db_index")]
        [Parallelizable]
        public void ShouldConvert_(
            string from,
            string expected
        )
        {
            // Arrange

            // Pre-Assert

            // Act
            var result = from.ToSnakeCase();

            // Assert
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ToPascalCase
    {
        [Test]
        [Parallelizable]
        public void OperatingOnNull_ShouldReturnNull()
        {
            // Arrange
            var input = null as string;

            // Pre-Assert

            // Act
            var result = input.ToPascalCase();

            // Assert
            Expect(result)
                .To.Be.Empty();
        }

        public static IEnumerable<(string input, string expected)> PascalCaseTestCases()
        {
            yield return ("Moo", "Moo");
            yield return ("MooCow", "MooCow");
            yield return ("i_am_snake", "IAmSnake");
            yield return ("i am words", "IAmWords");
            yield return ("is-already-kebabed", "IsAlreadyKebabed");
            yield return ("DBIndex", "DBIndex");
        }

        [TestCaseSource(nameof(PascalCaseTestCases))]
        [Parallelizable]
        public void ShouldConvert_((string input, string expected) testCase)
        {
            // Arrange
            var (from, expected) = testCase;

            // Pre-Assert

            // Act
            var result = from.ToPascalCase();

            // Assert
            Expect(result).To.Equal(expected);
        }

        [TestCaseSource(nameof(PascalCaseTestCases))]
        [Parallelizable]
        public void ToTitleCaseAlias_ShouldConvert_((string input, string expected) testCase)
        {
            // Arrange
            var (from, expected) = testCase;

            // Pre-Assert

            // Act
            var result = from.ToTitleCase();

            // Assert
            Expect(result).To.Equal(expected);
        }
    }


    [TestFixture]
    [Parallelizable]
    public class ToCamelCase
    {
        [Test]
        [Parallelizable]
        public void OperatingOnNull_ShouldReturnNull()
        {
            // Arrange
            var input = null as string;

            // Pre-Assert

            // Act
            var result = input.ToCamelCase();

            // Assert
            Expect(result)
                .To.Be.Empty();
        }

        [TestCase("Moo", "moo")]
        [TestCase("MooCow", "mooCow")]
        [TestCase("i_am_snake", "iAmSnake")]
        [TestCase("is-already-kebabed", "isAlreadyKebabed")]
        [Parallelizable]
        public void ShouldConvert_(
            string from,
            string expected
        )
        {
            // Arrange

            // Pre-Assert

            // Act
            var result = from.ToCamelCase();

            // Assert
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ToWords
    {
        [Test]
        [Parallelizable]
        public void OperatingOnNull_ShouldReturnNull()
        {
            // Arrange
            var input = null as string;

            // Pre-Assert

            // Act
            var result = input.ToWords();

            // Assert
            Expect(result)
                .To.Be.Empty();
        }

        [TestCase("Moo", "moo")]
        [TestCase("MooCow", "moo cow")]
        [TestCase("i_am_snake", "i am snake")]
        [TestCase("i am already words", "i am already words")]
        [TestCase("is-already-kebabed", "is already kebabed")]
        [Parallelizable]
        public void ShouldConvert_(
            string from,
            string expected
        )
        {
            // Arrange

            // Pre-Assert

            // Act
            var result = from.ToWords();

            // Assert
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ToRandomCase
    {
        [Test]
        [Parallelizable]
        public void ShouldRandomiseCasingOfAlphaString()
        {
            // Arrange
            var src = GetRandomAlphaString(20, 40);
            var lowerSource = src.ToLowerInvariant();
            var runs = 512;
            // Pre-Assert
            // Act
            var collector = new List<string>();
            for (var i = 0; i < 512; i++)
            {
                var thisAttempt = src.ToRandomCase();
                Expect(thisAttempt.ToLowerInvariant()).To.Equal(lowerSource);
                collector.Add(thisAttempt);
            }

            // Assert
            Expect(runs - collector.Distinct().Count())
                .To.Be.Less.Than(runs / 2); // allow for some random collision
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnNumericStringImmediately()
        {
            // Arrange
            var src = GetRandomNumericString(10, 20);
            // Act
            var result = src.ToRandomCase();
            // Assert
            Expect(result).To.Equal(src);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [Parallelizable]
        public void ShouldReturnImmediatelyFor_(string input)
        {
            // Arrange
            // Act
            var result = input.ToRandomCase();
            // Assert
            Expect(result).To.Equal(input);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class OperatingOnCollections
    {
        [Test]
        [Parallelizable]
        public void ToUpper_ShouldUpperCaseAll()
        {
            // Arrange
            var src = GetRandomArray<string>(10);
            // Pre-assert
            Expect(src).To.Contain.At.Least(1)
                .Matched.By(s => s != s.ToUpper());
            // Act
            var result = src.ToUpper();
            // Assert
            Expect(result).To.Contain.All
                .Matched.By(s => s == s.ToUpper());
        }

        [Test]
        [Parallelizable]
        public void ToLower_ShouldLowerCaseAll()
        {
            // Arrange
            var src = GetRandomArray<string>(10).ToUpper();
            // Pre-assert
            Expect(src).To.Contain.All
                .Matched.By(s => s == s.ToUpper());
            // Act
            var result = src.ToLower();
            // Assert
            Expect(result).To.Contain.All
                .Matched.By(s => s == s.ToLower());
        }
    }

    [TestFixture]
    [Parallelizable]
    public class CharacterClasses
    {
        [TestFixture]
        [Parallelizable]
        public class IsNumeric
        {
            [Test]
            [Parallelizable]
            public void OperatingOn_Null_ShouldReturnFalse()
            {
                // Arrange
                var input = null as string;
                // Pre-assert
                // Act
                var result = input.IsNumeric();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_EmptyString_ShouldReturnFalse()
            {
                // Arrange
                var input = "";
                // Pre-assert
                // Act
                var result = input.IsNumeric();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_Whitespace_ShouldReturnFalse()
            {
                // Arrange
                var input = GetRandomFrom(
                    new[]
                    {
                        " ",
                        "\t",
                        "\r"
                    }
                );
                // Pre-assert
                // Act
                var result = input.IsNumeric();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_NumericString_ShouldReturnTrue()
            {
                // Arrange
                var input = GetRandomInt(1000, 200000).ToString();
                // Pre-assert
                // Act
                var result = input.IsNumeric();
                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_AlphaString_ShouldReturnFalse()
            {
                // Arrange
                var input = GetRandomAlphaString(1);
                // Pre-assert
                // Act
                var result = input.IsNumeric();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_AlphaNumericString_ShouldReturnFalse()
            {
                // Arrange
                var input = new[]
                {
                    GetRandomAlphaString(1),
                    GetRandomInt(1, 100).ToString()
                }.Randomize().JoinWith("");
                // Pre-assert
                // Act
                var result = input.IsNumeric();
                // Assert
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        [Parallelizable]
        public class IsAlpha
        {
            [Test]
            [Parallelizable]
            public void OperatingOn_Null_ShouldReturnFalse()
            {
                // Arrange
                var input = null as string;
                // Pre-assert
                // Act
                var result = input.IsAlpha();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_EmptyString_ShouldReturnFalse()
            {
                // Arrange
                var input = "";
                // Pre-assert
                // Act
                var result = input.IsAlpha();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_Whitespace_ShouldReturnFalse()
            {
                // Arrange
                var input = GetRandomFrom(
                    new[]
                    {
                        " ",
                        "\r",
                        "\t"
                    }
                );
                // Pre-assert
                // Act
                var result = input.IsAlpha();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_NumericString_ShouldReturnFalse()
            {
                // Arrange
                var input = GetRandomInt(1000, 2000).ToString();
                // Pre-assert
                // Act
                var result = input.IsAlpha();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_AlphaString_ShouldReturnTrue()
            {
                // Arrange
                var input = GetRandomAlphaString();
                // Pre-assert
                // Act
                var result = input.IsAlpha();
                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_AlphaNumericString_ShouldReturnFalse()
            {
                // Arrange
                var input = GetRandomAlphaNumericString();
                while (input.RegexReplace("[0-9]", "") == input)
                {
                    // ensure we have at least one number in there
                    input = GetRandomAlphaNumericString();
                }

                // Pre-assert
                // Act
                var result = input.IsAlpha();
                // Assert
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        [Parallelizable]
        public class IsAlphanumeric
        {
            [Test]
            [Parallelizable]
            public void OperatingOn_Null_ShouldReturnFalse()
            {
                // Arrange
                var input = null as string;
                // Pre-assert
                // Act
                var result = input.IsAlphanumeric();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_EmptyString_ShouldReturnFalse()
            {
                // Arrange
                var input = "";
                // Pre-assert
                // Act
                var result = input.IsAlphanumeric();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_Whitespace_ShouldReturnFalse()
            {
                // Arrange
                var input = GetRandomFrom(
                    new[]
                    {
                        " ",
                        "\r",
                        "\t"
                    }
                );
                // Pre-assert
                // Act
                var result = input.IsAlphanumeric();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_NumericString_ShouldReturnTrue()
            {
                // Arrange
                var input = GetRandomInt(1000, 2000).ToString();
                // Pre-assert
                // Act
                var result = input.IsAlphanumeric();
                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_AlphaString_ShouldReturnTrue()
            {
                // Arrange
                var input = GetRandomAlphaString(1);
                // Pre-assert
                // Act
                var result = input.IsAlphanumeric();
                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            [Parallelizable]
            public void OperatingOn_AlphaNumericString_ShouldReturnTrue()
            {
                // Arrange
                var numerics = new Regex("[0-9]");
                var alpha = new Regex("[a-zA-Z]");
                var input = GetRandomAlphaNumericString(50, 100);
                var attempts = 0;
                while (!numerics.IsMatch(input) || !alpha.IsMatch(input))
                {
                    if (++attempts > 50)
                    {
                        Assert.Fail("Can't find an alphanumeric string");
                    }

                    // random alphanumeric string may contain only alphas / numerics
                    // -> it is random, after all...
                    input += GetRandomAlphaString(5);
                }

                // Pre-assert
                // Act
                var result = input.IsAlphanumeric();

                // Assert
                Expect(result).To.Be.True(
                    $"{input.Stringify()} should be alpha-numeric"
                );
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class ToMemoryStream
    {
        [Test]
        [Parallelizable]
        public void ShouldConvertNonEmptyString()
        {
            // Arrange
            var input = GetRandomString(10);
            // Act
            var result = input.ToMemoryStream();
            // Assert
            Expect(result.ToArray().ToUtf8String()).To.Equal(input);
        }

        [Test]
        [Parallelizable]
        public void ShouldConvertEmptyByteArray()
        {
            // Arrange
            var input = "";
            // Act
            var result = input.ToMemoryStream();
            // Assert
            Expect(result.ToArray().ToUtf8String()).To.Be.Empty();
        }

        [Test]
        [Parallelizable]
        public void ShouldTreatNullAsEmpty()
        {
            // Arrange
            var input = null as byte[];
            // Act
            var result = input.ToMemoryStream();
            // Assert
            Expect(result.ToArray().ToUtf8String()).To.Be.Empty();
        }
    }

    [TestFixture]
    [Parallelizable]
    public class SplitCommandline
    {
        [Test]
        [Parallelizable]
        public void ShouldReturnEmptyCollectionForNull()
        {
            // Arrange
            var input = null as string;
            // Act
            var result = input.SplitCommandline();
            // Assert
            Expect(result)
                .To.Be.Empty();
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnSingleProgramNameWhenNoSpaces()
        {
            // Arrange
            var program = $"{GetRandomString(1)}.exe";
            // Act
            var result = program.SplitCommandline();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        program
                    }
                );
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnSingleQuotedProgramWithoutQuotes()
        {
            // Arrange
            var program = $"{GetRandomString(1)}.exe";
            var cli = $"\"{program}\"";
            // Act
            var result = cli.SplitCommandline();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        program
                    }
                );
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnSpacedProgramWithoutQuotes()
        {
            // Arrange
            var program = $"C:\\Program Files\\MyApp\\{GetRandomString(1)}.exe";
            var cli = $"\"{program}\"";
            // Act
            var result = cli.SplitCommandline();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        program
                    }
                );
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnSpacedProgramAndNonSpacedArguments()
        {
            // Arrange
            var program = $"C:\\Program Files\\MyApp\\{GetRandomString(1)}.exe";
            var cli = $"\"{program}\" arg1 arg2";
            // Act
            var result = cli.SplitCommandline();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        program,
                        "arg1",
                        "arg2"
                    }
                );
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnSpacedProgramAndSpacedArgumentsUnQuoted()
        {
            // Arrange
            var program = $"C:\\Program Files\\MyApp\\{GetRandomString(1)}.exe";
            var cli = $"\"{program}\" \"arg1 arg2\"";
            // Act
            var result = cli.SplitCommandline();
            // Assert
            Expect(result)
                .To.Equal(
                    new[]
                    {
                        program,
                        "arg1 arg2"
                    }
                );
        }
    }

    [TestFixture]
    [Parallelizable]
    public class DeQuote
    {
        [TestCase(" ")]
        [TestCase(null)]
        [TestCase("\t\r")]
        [Parallelizable]
        public void ShouldReturnNullOrWhitespace(string input)
        {
            // Arrange
            // Act
            var result = input.DeQuote();
            // Assert
            Expect(result).To.Equal(input);
        }

        [Test]
        [Parallelizable]
        public void ShouldNotInterfereWithNonQuotedString()
        {
            // Arrange
            var input = GetRandomString(1);
            // Act
            var result = input.DeQuote();
            // Assert
            Expect(result)
                .To.Equal(input);
        }

        [Test]
        [Parallelizable]
        public void ShouldNotDeQuoteLonelyQuote()
        {
            // Arrange
            var input = "\"";
            // Act
            var result = input.DeQuote();
            // Assert
            Expect(result)
                .To.Equal(input);
        }

        [TestCase("\"foo")]
        [TestCase("foo\"")]
        [Parallelizable]
        public void ShouldNotRemoveUnmatchedQuotes(string input)
        {
            // Arrange
            // Act
            var result = input.DeQuote();
            // Assert
            Expect(result).To.Equal(input);
        }

        [Test]
        [Parallelizable]
        public void ShouldRemoveBoundingQuotes()
        {
            // Arrange
            var expected = GetRandomString(1);
            var input = $"\"{expected}\"";
            // Act
            var result = input.DeQuote();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        [Parallelizable]
        public void ShouldNotRemoveInternalQuotes()
        {
            // Arrange
            var expected = $"\"{GetRandomString(1)}";
            var input = $"\"{expected}\"";
            // Act
            var result = input.DeQuote();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class Matches
    {
        [Test]
        [Parallelizable]
        public void ShouldMatchEmptyCollections()
        {
            // Arrange
            var src = new string[0];
            var dest = new string[0];
            // Act
            var result = src.Matches(dest);
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        [Parallelizable]
        public void ShouldMatchIdenticalCollections()
        {
            // Arrange
            var left = new[]
            {
                "a",
                "b",
                "c"
            };
            var right = new[]
            {
                "a",
                "b",
                "c"
            };
            // Act
            var result = left.Matches(right);
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        [Parallelizable]
        public void ShouldUseGivenStringComparison()
        {
            // Arrange
            var left = new[]
            {
                "a",
                "b",
                "c"
            };
            var right = new[]
            {
                "A",
                "B",
                "C"
            };
            // Act
            var result = left.Matches(right, StringComparison.OrdinalIgnoreCase);
            // Assert
            Expect(result)
                .To.Be.True();
        }
    }

    [TestFixture]
    [Parallelizable]
    public class Substr
    {
        [Test]
        [Parallelizable]
        public void ShouldReturnEmptyStringForNullAndAnyRange()
        {
            // Arrange
            var str = null as string;
            // Act
            var result1 = str.Substr(GetRandomInt());
            var result2 = str.Substr(GetRandomInt(), GetRandomInt());
            // Assert
            Expect(result1)
                .To.Equal("");
            Expect(result2)
                .To.Equal("");
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnPartialStringWhenGivenStartOnlyAndStartWithinString()
        {
            // Arrange
            var str = "foobar";
            // Act
            var result = str.Substr(3);
            // Assert
            Expect(result)
                .To.Equal("bar");
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnPartialStringWhenGivenStartAndLengthWithinString()
        {
            // Arrange
            var str = "quuxapple";
            // Act
            var result = str.Substr(2, 4);
            // Assert
            Expect(result)
                .To.Equal("uxap");
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnRemainderOfStringWhenLengthOutsideString()
        {
            // Arrange
            var str = "abc123";
            // Act
            var result = str.Substr(2, 10);
            // Assert
            Expect(result)
                .To.Equal("c123");
        }

        [Test]
        [Parallelizable]
        public void ShouldReturnEntireStringIfStartLessThanZero()
        {
            // Arrange
            var str = "aaa444";
            // Act
            var result = str.Substr(-10);
            // Assert
            Expect(result)
                .To.Equal(str);
        }

        [Test]
        [Parallelizable]
        public void ShouldTreatNegativeLengthAsOffsetFromEnd()
        {
            // Arrange
            var str = "123456";
            // Act
            var result = str.Substr(1, -1);
            // Assert
            Expect(result)
                .To.Equal("2345");
        }

        [TestFixture]
        public class GivenIndexesOutsideOfString
        {
            [Test]
            public void ShouldReturnTheEntireString()
            {
                // Arrange
                var str = "12345";
                // Act
                var result = str.Substr(-10, int.MaxValue);
                // Assert
                Expect(result)
                    .To.Equal(str);
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class UnBase64
    {
        [Test]
        [Parallelizable]
        public void ShouldBeAbleToUnBase64ByteData()
        {
            // Arrange
            var str = GetRandomString(20);
            var bytes = Encoding.UTF8.GetBytes(str);
            var base64 = Convert.ToBase64String(bytes);
            // Act
            var result = base64.UnBase64();
            // Assert
            Expect(result)
                .To.Equal(bytes);
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToUnBase64UnpaddedByteData()
        {
            // Arrange
            byte[] bytes;
            string base64;
            do
            {
                var str = GetRandomString(10, 32);
                bytes = Encoding.UTF8.GetBytes(str);
                base64 = Convert.ToBase64String(bytes);
            } while (!base64.Contains("="));

            // Act
            var result = base64.UnBase64();
            // Assert
            Expect(result)
                .To.Equal(bytes);
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToUnBase64AString()
        {
            // Arrange
            var str = GetRandomString(32);
            var base64 = str.ToBase64();
            // Act
            var result = base64.UnBase64(Encoding.UTF8);
            // Assert
            Expect(result)
                .To.Equal(str);
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToUnBase64AnUnPaddedString()
        {
            // Arrange
            var attempts = 0;
            var chars = 17;
            var str = GetRandomString(chars);
            var base64 = str.ToBase64();
            while (!base64.Contains("=") && ++attempts < 10)
            {
                chars++;
                str = GetRandomString(chars);
                base64 = str.ToBase64();
            }

            if (attempts >= 10)
            {
                Assert.Fail("Can't find a base64 input with padding");
            }

            base64 = base64.Replace("=", "");

            // Act
            var result = base64.UnBase64(Encoding.UTF8);
            // Assert
            Expect(result)
                .To.Equal(str);
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToUnBase64StringDataWithProvidedConverterToAnyType()
        {
            // Arrange
            var data = GetRandom<Poco>();
            var json = JsonConvert.SerializeObject(data);
            var base64 = json.ToBase64();
            // Act
            var result = base64.UnBase64(
                JsonConvert.DeserializeObject<Poco>
            );
            // Assert
            Expect(result)
                .To.Deep.Equal(data);
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToUnBase64StringUnPaddedBase64StringWithDeserializer()
        {
            // Arrange
            string base64;
            Poco data;
            do
            {
                data = GetRandom<Poco>();
                var json = JsonConvert.SerializeObject(data);
                base64 = json.ToBase64();
            } while (!base64.Contains("="));

            base64 = base64.Base64UnPadded();
            Expect(base64)
                .Not.To.Contain("=");
            // Act
            var result = base64.UnBase64(
                JsonConvert.DeserializeObject<Poco>
            );
            // Assert
            Expect(result)
                .To.Deep.Equal(data);
        }

        [Test]
        [Parallelizable]
        public void ShouldDefaultStringDecodeToBeUtf8String()
        {
            // Arrange
            var data = GetRandomString(20, 32);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            // Act
            var result = base64.UnBase64<string>();
            // Assert
            Expect(result)
                .To.Equal(data);
        }

        public class Poco
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class TrimBlock
    {
        [Test]
        [Parallelizable]
        public void ShouldReturnEmptyStringForNull()
        {
            // Arrange
            // Act
            var result = (null as string).Outdent(GetRandomInt());
            // Assert
            Expect(result)
                .To.Equal("");
        }

        [TestFixture]
        [Parallelizable]
        public class WhenIndentationIsBySpaces
        {
            [Test]
            [Parallelizable]
            public void ShouldOutdentASingleLine()
            {
                // Arrange
                var expected = "the line";
                var line = $"    {expected}";

                // Act
                var result = line.Outdent();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            [Parallelizable]
            public void ShouldOutdentLinesAllAtSameIndent()
            {
                // Arrange
                var lines = @"
                        line 1
                        line 2
";
                var expected = @"
line 1
line 2
";

                // Act
                var result = lines.Outdent();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            [Parallelizable]
            public void ShouldOutdentLinesToShallowestIndent()
            {
                // Arrange
                var lines = @"
        function foo() {
            if (true) {
                console.log('whee');
            }

        }
";
                var expected = @"
function foo() {
    if (true) {
        console.log('whee');
    }

}
";
                // Act
                var result = lines.Outdent();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            [Parallelizable]
            public void ShouldTrimEndByDefault()
            {
                // Arrange
                var lines = @"
        function foo() {
            if (true) {    
                console.log('whee');  
            }

        }
";
                var expected = @"
function foo() {
    if (true) {
        console.log('whee');
    }

}
";
                // Act
                var result = lines.Outdent();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            [Parallelizable]
            public void ShouldHandleTabsAutomatically()
            {
                // Arrange
                var lines = "\t\tline 1\n\t\tline 2";
                var expected = "line 1\nline 2";

                // Act
                var result = lines.Outdent();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            [Parallelizable]
            public void ShouldIndentToRequiredLevel()
            {
                // Arrange
                var lines = "\t\tline 1\n\t\tline 2";
                var expected = "\tline 1\n\tline 2";

                // Act
                var result = lines.Outdent(1);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class SplitOnce
    {
        [TestFixture]
        [Parallelizable]
        public class GivenNull
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnEmptyArray()
            {
                // Arrange
                var str = null as string;
                // Act
                var result = str.SplitOnce(";");
                // Assert
                Expect(result)
                    .To.Be.Empty();
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenEmptyString
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnSingleElementEmpty()
            {
                // Arrange
                var str = "";
                // Act
                var result = str.SplitOnce(";");
                // Assert
                Expect(result)
                    .To.Equal(
                        new[]
                        {
                            ""
                        }
                    );
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenNonEmptyStringWithoutDelimiter
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnSingleElement()
            {
                // Arrange
                var str = "moocakes";
                // Act
                var result = str.SplitOnce("::");
                // Assert
                Expect(result)
                    .To.Equal(
                        new[]
                        {
                            str
                        }
                    );
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenStringWithTwoElements
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnThoseTwoElements()
            {
                // Arrange
                var str = "foo;bar";
                var expected = new[]
                {
                    "foo",
                    "bar"
                };
                // Act
                var result = str.SplitOnce(";");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenStringWithThreeElements
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnOnlyTwoItemsInArray()
            {
                // Arrange
                var str = "foo::bar::qux";
                var expected = new[]
                {
                    "foo",
                    "bar::qux"
                };
                // Act
                var result = str.SplitOnce("::");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenNullSplitter
        {
            [Test]
            [Parallelizable]
            public void ShouldThrow()
            {
                // Arrange
                // Act
                Expect(() => "".SplitOnce(null))
                    .To.Throw<ArgumentNullException>()
                    .For("splitOn");
                // Assert
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class SplitPath
    {
        [Test]
        [Parallelizable]
        public void ShouldSplitWindowsPath()
        {
            // Arrange
            var expected = GetRandomArray<string>(3);
            var path = string.Join("\\", expected);
            // Act
            var result = path.SplitPath();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        [Parallelizable]
        public void ShouldSplitByUnixPath()
        {
            // Arrange
            var expected = GetRandomArray<string>(3);
            var path = string.Join("/", expected);
            // Act
            var result = path.SplitPath();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        [Parallelizable]
        public void ShouldSplitPathWithMixedDelimiters()
        {
            // Arrange
            var expected = GetRandomArray<string>(3, 3);
            var flag = GetRandomBoolean();
            var first = flag
                ? "/"
                : "\\";
            var second = flag
                ? "\\"
                : "/";
            var path = $"{expected[0]}{first}{expected[1]}{second}{expected[2]}";
            // Act
            var result = path.SplitPath();
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        [Parallelizable]
        public void ShouldSplitNullToEmptyArray()
        {
            // Arrange
            var input = null as string;
            // Act
            var result = input.SplitPath();
            // Assert
            Expect(result)
                .To.Be.Empty();
        }
    }

    [TestFixture]
    public class ResolvePath
    {
        [TestFixture]
        public class GivenPathWithNoDots
        {
            [Test]
            public void ShouldReturnPath()
            {
                // Arrange
                var input = "/foo/bar/qux";
                // Act
                var result = input.ResolvePath();
                // Assert
                Expect(result)
                    .To.Equal(input);
            }
        }

        [TestFixture]
        public class GivenPathWithSingleDots
        {
            [Test]
            public void ShouldDropTheDots()
            {
                // Arrange
                var input = "/foo/bar/./qux";
                // Act
                var result = input.ResolvePath();
                // Assert
                Expect(result)
                    .To.Equal("/foo/bar/qux");
            }
        }

        [TestFixture]
        public class GivenPathWithDoubleDots
        {
            [Test]
            public void ShouldGoUpOneFolderPerDoubleDot()
            {
                // Arrange
                var input = "/foo/bar/quux/../../moo/./cakes";
                // Act
                var result = input.ResolvePath();
                // Assert
                Expect(result)
                    .To.Equal("/foo/moo/cakes");
            }

            [Test]
            public void ShouldNotStepOutsideUnixDomain()
            {
                // Arrange
                var input = "/foo/../../bar";
                // Act
                var result = input.ResolvePath();
                // Assert
                Expect(result)
                    .To.Equal("/bar");
            }

            [Test]
            public void ShouldNotStepOutsideWindowsPath()
            {
                // Arrange
                var input = "C:/foo/../.././bar";
                // Act
                var result = input.ResolvePath();
                // Assert
                Expect(result)
                    .To.Equal("C:/bar");
            }

            [Test]
            public void ShouldNotStepOutsideWindowsPathBacksSlashed()
            {
                // Arrange
                var input = "C:\\foo\\..\\..\\.\\bar";
                // Act
                var result = input.ResolvePath();
                // Assert
                Expect(result)
                    .To.Equal("C:\\bar");
            }
        }

        [TestFixture]
        public class ResolvingWithAnotherPathPart
        {
            [Test]
            public void ShouldResolveJoinedPathsWithNoDots()
            {
                // Arrange
                var input = "/foo/bar";
                var extra = "moo/cakes";
                // Act
                var result = input.ResolvePath(extra);
                // Assert
                Expect(result)
                    .To.Equal("/foo/bar/moo/cakes");
            }

            [Test]
            public void ShouldResolveJoinedPathsWithSingleDot()
            {
                // Arrange
                var input = "/foo/bar";
                var extra = "./moo/cakes";
                // Act
                var result = input.ResolvePath(extra);
                // Assert
                Expect(result)
                    .To.Equal("/foo/bar/moo/cakes");
            }

            [Test]
            public void ShouldResolveJoinedPathsWithDoubleDot()
            {
                // Arrange
                var input = "/foo/bar";
                var extra = "../moo/cakes";
                // Act
                var result = input.ResolvePath(extra);
                // Assert
                Expect(result)
                    .To.Equal("/foo/moo/cakes");
            }

            [Test]
            public void ShouldResolveFromLastAbsolutePathInExtraList1()
            {
                // Arrange
                var input = "/foo/bar";
                var extra = "/moo/cakes";
                // Act
                var result = input.ResolvePath(extra);
                // Assert
                Expect(result)
                    .To.Equal(extra);
            }

            [Test]
            public void ShouldResolveFromLastAbsolutePathInExtraList1Win()
            {
                // Arrange
                var input = "D:/foo/bar";
                var extra = "C:/moo/cakes";
                // Act
                var result = input.ResolvePath(extra);
                // Assert
                Expect(result)
                    .To.Equal(extra);
            }

            [Test]
            public void ShouldResolveFromLastAbsolutePathInExtraList2()
            {
                // Arrange
                var input = "/foo/bar";
                var extra = "/moo/cakes";
                var extra2 = "beefs";
                // Act
                var result = input.ResolvePath(extra, extra2);
                // Assert
                Expect(result)
                    .To.Equal("/moo/cakes/beefs");
            }

            [Test]
            public void ShouldResolveFromLastAbsolutePathInExtraList2Win()
            {
                // Arrange
                var input = "D:/foo/bar";
                var extra = "C:/moo/cakes";
                var extra2 = "beefs";
                // Act
                var result = input.ResolvePath(extra, extra2);
                // Assert
                Expect(result)
                    .To.Equal("C:/moo/cakes/beefs");
            }
        }

        [TestCase("foo/bar", "../quux", "foo/quux")]
        [TestCase("foo", "bar", "foo/bar")]
        [TestCase("foo", "./bar", "foo/bar")]
        [TestCase("/path/to/resource", "../../other/resource", "/path/other/resource")]
        [TestCase("/path/to/resource", "../../other/../other2/resource", "/path/other2/resource")]
        public void ShouldResolve_(
            string original,
            string other,
            string expected
        )
        {
            // Arrange
            // Act
            var result = original.ResolvePath(other);
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class RelativeTo
    {
        [TestFixture]
        [Parallelizable]
        public class GivenTheSamePath
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnEmptyString()
            {
                // Arrange
                var path = GetRandomAbsolutePath();
                // Act
                var result = path.RelativeTo(path);
                // Assert
                Expect(result)
                    .To.Equal("");
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenAParentFolder
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnOnlyTheChildPart()
            {
                // Arrange
                var path = "/foo/bar/quux";
                var test = "/foo/bar";
                // Act
                var result = path.RelativeTo(test);
                // Assert
                Expect(result)
                    .To.Equal("quux");
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenAnAncestorFolder
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnTheFullRelativeChildPath()
            {
                // Arrange
                var path = "/foo/bar/quux";
                var test = "/foo";
                // Act
                var result = path.RelativeTo(
                    test,
                    PathType.Unix // just to make the test deterministic
                );
                // Assert
                Expect(result)
                    .To.Equal("bar/quux");
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenAChildPath
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnTheRelativeDottedPath()
            {
                // Arrange
                var path = "/foo";
                var test = "/foo/bar/quux";
                // Act
                var result = path.RelativeTo(
                    test,
                    PathType.Unix
                );
                // Assert
                Expect(result)
                    .To.Equal("../..");
            }
        }

        [TestFixture]
        [Parallelizable]
        public class GivenDivergentBranches
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnTheRelativeDottedPath()
            {
                // Arrange
                var path = "/foo/bar";
                var test = "/foo/quux";
                // Act
                var result = path.RelativeTo(test, PathType.Unix);
                // Assert
                Expect(result)
                    .To.Equal("../bar");
            }
        }

        [TestFixture]
        [Parallelizable]
        public class RealWorld
        {
            [Test]
            [Parallelizable]
            public void ShouldProduceRelativePathFromWwwRoot()
            {
                // Arrange
                var wwwroot = "C:\\projects\\webby-web\\mvc\\wwwroot";
                var scriptPath = $"{wwwroot}\\Content\\Scripts\\Moo.js";
                // Act
                var result = scriptPath.RelativeTo(wwwroot, PathType.Windows);
                // Assert
                Expect(result)
                    .To.Equal("Content\\Scripts\\Moo.js");
            }

            [Test]
            [Parallelizable]
            public void ShouldProduceRelativePathFromWwwRootMixedTypes()
            {
                // Arrange
                var wwwroot = "C:\\projects\\webby-web\\mvc\\wwwroot";
                var scriptPath = $"{wwwroot}\\Content/Scripts/Moo.js";
                // Act
                var result = scriptPath.RelativeTo(wwwroot, PathType.Windows);
                // Assert
                Expect(result)
                    .To.Equal("Content\\Scripts\\Moo.js");
            }
        }
    }

    [TestFixture]
    public class ContainsInOrder
    {
        [TestFixture]
        public class WhenOneNeedle
        {
            [TestFixture]
            public class AndIsContained
            {
                [TestFixture]
                public class AsExactMatch
                {
                    [Test]
                    public void ShouldReturnTrue()
                    {
                        // Arrange
                        var haystack = "foo bar quux";
                        // Act
                        var result = haystack.ContainsInOrder("bar");
                        // Assert
                        Expect(result)
                            .To.Be.True();
                    }
                }

                [TestFixture]
                public class AsInsensitiveMatch
                {
                    [Test]
                    public void ShouldReturnTrue()
                    {
                        // Arrange
                        var haystack = "foo bar quux";
                        // Act
                        var result = haystack.ContainsInOrder(
                            StringComparison.OrdinalIgnoreCase,
                            "BAR"
                        );
                        // Assert
                        Expect(result)
                            .To.Be.True();
                    }
                }
            }

            [TestFixture]
            public class AndIsNotContainedAtAll
            {
                [TestFixture]
                public class AsExactMismatch
                {
                    [Test]
                    public void ShouldReturnFalse()
                    {
                        // Arrange
                        var haystack = "foo bar quux";
                        // Act
                        var result = haystack.ContainsInOrder(
                            "bob"
                        );
                        // Assert
                        Expect(result)
                            .To.Be.False();
                    }
                }

                [TestFixture]
                public class AsInsensitiveMismatch
                {
                    [Test]
                    public void ShouldReturnFalse()
                    {
                        // Arrange
                        var haystack = "foo bar quux";
                        // Act
                        var result = haystack.ContainsInOrder(
                            StringComparison.OrdinalIgnoreCase,
                            "bob"
                        );
                        // Assert
                        Expect(result)
                            .To.Be.False();
                    }
                }
            }
        }

        [TestFixture]
        public class MoreComplexCases
        {
            [Test]
            public void ShouldFindThreeNeedlesInOrder()
            {
                // Arrange
                var haystack = "Bacon ipsum dolor amet pork loin ribeye tail, chislic pork pig spare ribs jowl";
                // Act
                var result = haystack.ContainsInOrder(
                    "ips",
                    "eye",
                    "isli"
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldFindThreeNeedlesInOrderInsensitive()
            {
                // Arrange
                var haystack = "Bacon ipsum dolor amet pork loin ribeye tail, chislic pork pig spare ribs jowl";
                // Act
                var result = haystack.ContainsInOrder(
                    StringComparison.OrdinalIgnoreCase,
                    "iPs",
                    "eYE",
                    "IsLi"
                );
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class GZip
        {
            [Test]
            public void ShouldGzipTheText()
            {
                // Arrange
                // Arrange
                var data = GetRandomWords();
                using var source = new MemoryStream(data.AsBytes());
                using var target = new MemoryStream();
                using var gzip = new GZipStream(target, CompressionLevel.Optimal, leaveOpen: true);
                source.CopyTo(gzip);
                gzip.Close();
                var expected = target.ToArray();
                // Act
                var result = data.GZip();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }
}