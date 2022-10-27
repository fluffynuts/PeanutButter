using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

// ReSharper disable ExpressionIsAlwaysNull

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestStringExtensions
    {
        [TestCase("Hello World", "^Hello", "Goodbye", "Goodbye World")]
        [TestCase("Hello World", "Wor.*", "Goodbye", "Hello Goodbye")]
        [TestCase("Hello World", "Hello$", "Goodbye", "Hello World")]
        public void RegexReplace_ShouldReplaceAccordingToRegexWithSuppliedValue(
            string input,
            string re,
            string replaceWith,
            string expected)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.RegexReplace(re, replaceWith);


            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }

        [TestFixture]
        public class RegexReplaceAll
        {
            [Test]
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
        public class Or
        {
            [TestCase(null)]
            [TestCase("")]
            public void ShouldReturnOtherStringWhenPrimaryIs_(
                string value)
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
            [TestCase("", false)]
            [TestCase(" ", false)]
            [TestCase("\t", false)]
            [TestCase(null, false)]
            public void ShouldResolveToBooleanValue(
                string input,
                bool expected)
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
        public class AsString
        {
            [TestFixture]
            public class OperatingOnByteArray
            {
                [Test]
                public void GivenNull_ShouldReturnNull()
                {
                    // Arrange
                    byte[] src = null;
                    // Pre-Assert
                    // Act
                    var result = src.AsString();
                    // Assert
                    Expect(result).To.Be.Null();
                }

                [Test]
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
        public class AsBytes
        {
            [Test]
            public void WhenStringIsNull_ShouldReturnNull()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = ((string) null).AsBytes();

                //---------------Test Result -----------------------
                Expect(result as object).To.Be.Null();
            }

            [Test]
            public void OperatingOnEmptyString_ShouldReturnEmptyByteArray()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = string.Empty.AsBytes();

                //---------------Test Result -----------------------
                Expect(result as object).Not.To.Be.Null();
                Assert.That(result, Is.Empty);
            }

            [Test]
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
            {
                Encoding.UTF8,
                Encoding.ASCII,
                Encoding.UTF32,
                Encoding.UTF7
            };

            [TestCaseSource(nameof(Encodings))]
            public void
                OperatingOnNonEmptyString_WhenGivenEncoding_ShouldReturnStringEncodedAsBytesFromEncoding(
                    Encoding encoding)
            {
                //---------------Set up test pack-------------------
                var input = GetRandomString(50, 100);
                var expected = encoding.GetBytes(input);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = input.AsBytes(encoding);

                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(expected, result);
            }
        }

        [TestFixture]
        public class AsStream
        {
            [Test]
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
        public class IsInteger
        {
            [TestFixture]
            public class WhenStringIsInteger
            {
                [Test]
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
            public class WhenStringIsNotInteger
            {
                [Test]
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
        public class AsInteger
        {
            [TestFixture]
            public class WhenStringIsInteger
            {
                [Test]
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
            public class WhenStringIsFloatingPointWithPeriod
            {
                [Test]
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
            public class WhenStringIsFloatingPointWithComma
            {
                [Test]
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
            public class WhenStringContainsAlphaChars
            {
                [Test]
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
            public class WhenStringHasLeadingAlphaPart
            {
                [Test]
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
            public class WhenStringIsNotAnInteger
            {
                [TestCase("a")]
                [TestCase("")]
                [TestCase("\r\n")]
                [TestCase(null)]
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
        public class IsNullOrWhitespace
        {
            [TestFixture]
            public class WhenOperatingOnNullOrWhitespace
            {
                // ReSharper disable once MemberHidesStaticFromOuterClass
                public static readonly string[] NullOrWhitespaceStrings =
                {
                    null,
                    "\t",
                    "\r",
                    "\n"
                };

                [TestCaseSource(nameof(NullOrWhitespaceStrings))]
                public void ShouldReturnTrueFor_(
                    string src)
                {
                    //---------------Set up test pack-------------------

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = src.IsNullOrWhiteSpace();

                    //---------------Test Result -----------------------
                    Assert.IsTrue(result);
                }
            }

            [TestFixture]
            public class WhenOperatingOnNonWhitespaceString
            {
                [Test]
                public void ShouldReturnFalse()
                {
                    //---------------Set up test pack-------------------
                    var src = GetRandomString();

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = src.IsNullOrWhiteSpace();

                    //---------------Test Result -----------------------
                    Assert.IsFalse(result);
                }
            }

            private static readonly string[] NullOrWhitespaceStrings =
            {
                null,
                "\t",
                "\r",
                "\n"
            };
        }

        [TestFixture]
        public class IsNullOrEmpty
        {
            [TestCase(null)]
            [TestCase("")]
            public void ShouldReturnTrue_For_(
                string src)
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
            public void ShouldReturnFalse_For_(
                string src)
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
            public class WhenStringIsNotWhitespaceOrNull
            {
                [Test]
                public void ShouldReturnFalse()
                {
                    //---------------Set up test pack-------------------
                    var input = GetRandomString();

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = input.IsNullOrEmpty();

                    //---------------Test Result -----------------------
                    Assert.IsFalse(result);
                }
            }
        }

        [TestFixture]
        public class ContainsOneOf
        {
            [Test]
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
            public void OperatingOnStringContainingNoneOfTheNeedles_ShouldReturnFalse()
            {
                //--------------- Arrange -------------------
                var input = "foo";
                var search = new[] {"bar", "quuz", "wibbles"};

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = input.ContainsOneOf(search);

                //--------------- Assert -----------------------
                Expect(result).To.Be.False();
            }

            [Test]
            public void OperatingOnStringContainingOnneOfTheNeedles_ShouldReturnTrue()
            {
                //--------------- Arrange -------------------
                var input = "foo";
                var search = new[] {"bar", "quuz", "oo", "wibbles"}.Randomize().ToArray();

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = input.ContainsOneOf(search);

                //--------------- Assert -----------------------
                Expect(result).To.Be.True();
            }
        }

        [TestFixture]
        public class ContainsAllOf
        {
            [Test]
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
            public void WhenHaystackContainsAllConstituents_ShouldReturnTrue()
            {
                //--------------- Arrange -------------------
                var input = "hello, world";
                var search = new[] {"hello", ", ", "world"}.Randomize().ToArray();

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = input.ContainsAllOf(search);

                //--------------- Assert -----------------------
                Expect(result).To.Be.True();
            }

            [Test]
            public void WhenHaystackMissingNeedle_ShouldReturnFalse()
            {
                //--------------- Arrange -------------------
                var input = "hello, world";
                var search = new[] {"hello", ", ", "there"}.Randomize().ToArray();

                //--------------- Assume ----------------

                //--------------- Act ----------------------
                var result = input.ContainsAllOf(search);

                //--------------- Assert -----------------------
                Expect(result).To.Be.False();
            }
        }

        [TestFixture]
        public class IsWhiteSpace
        {
            [TestCase(" ")]
            [TestCase("\t")]
            [TestCase("\r")]
            [TestCase("\n")]
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
        public class IsEmptyOrWhiteSpace
        {
            [TestCase("")]
            [TestCase(" ")]
            [TestCase("\t")]
            [TestCase("\r")]
            [TestCase("\n")]
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
        public class ToBase64
        {
            [Test]
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
        public class ToKebabCase
        {
            [Test]
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
            public void ShouldConvert_(
                string from,
                string expected)
            {
                // Arrange

                // Pre-Assert

                // Act
                var result = from.ToKebabCase();

                // Assert
                Expect(result).To.Equal(expected);
            }

            [Test]
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
        public class ToSnakeCase
        {
            [Test]
            public void OperatingOnNull_ShouldReturnNull()
            {
                // Arrange
                var input = null as string;

                // Pre-Assert

                // Act
                var result = input.ToSnakeCase();

                // Assert
                Expect(result).To.Be.Null();
            }

            [TestCase("Moo", "moo")]
            [TestCase("MooCow", "moo_cow")]
            [TestCase("i_am_snake", "i_am_snake")]
            [TestCase("is-already-kebabed", "is_already_kebabed")]
            public void ShouldConvert_(
                string from,
                string expected)
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
        public class ToPascalCase
        {
            [Test]
            public void OperatingOnNull_ShouldReturnNull()
            {
                // Arrange
                var input = null as string;

                // Pre-Assert

                // Act
                var result = input.ToPascalCase();

                // Assert
                Expect(result).To.Be.Null();
            }

            public static IEnumerable<(string input, string expected)> PascalCaseTestCases()
            {
                yield return ("Moo", "Moo");
                yield return ("MooCow", "MooCow");
                yield return ("i_am_snake", "IAmSnake");
                yield return ("i am words", "I Am Words");
                yield return ("is-already-kebabed", "IsAlreadyKebabed");
            }

            [TestCaseSource(nameof(PascalCaseTestCases))]
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
        public class ToCamelCase
        {
            [Test]
            public void OperatingOnNull_ShouldReturnNull()
            {
                // Arrange
                var input = null as string;

                // Pre-Assert

                // Act
                var result = input.ToCamelCase();

                // Assert
                Expect(result).To.Be.Null();
            }

            [TestCase("Moo", "moo")]
            [TestCase("MooCow", "mooCow")]
            [TestCase("i_am_snake", "iAmSnake")]
            [TestCase("is-already-kebabed", "isAlreadyKebabed")]
            public void ShouldConvert_(
                string from,
                string expected)
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
        public class ToWords
        {
            [Test]
            public void OperatingOnNull_ShouldReturnNull()
            {
                // Arrange
                var input = null as string;

                // Pre-Assert

                // Act
                var result = input.ToWords();

                // Assert
                Expect(result).To.Be.Null();
            }

            [TestCase("Moo", "moo")]
            [TestCase("MooCow", "moo cow")]
            [TestCase("i_am_snake", "i am snake")]
            [TestCase("i am already words", "i am already words")]
            [TestCase("is-already-kebabed", "is already kebabed")]
            public void ShouldConvert_(
                string from,
                string expected)
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
        public class ToRandomCase
        {
            [Test]
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
        public class OperatingOnCollections
        {
            [Test]
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
            public void ToLower_ShouldOwerCaseAll()
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
        public class CharacterClasses
        {
            [TestFixture]
            public class IsNumeric
            {
                [Test]
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
                public void OperatingOn_Whitespace_ShouldReturnFalse()
                {
                    // Arrange
                    var input = GetRandomFrom(new[] {" ", "\t", "\r"});
                    // Pre-assert
                    // Act
                    var result = input.IsNumeric();
                    // Assert
                    Expect(result).To.Be.False();
                }

                [Test]
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
            public class IsAlpha
            {
                [Test]
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
                public void OperatingOn_Whitespace_ShouldReturnFalse()
                {
                    // Arrange
                    var input = GetRandomFrom(new[] {" ", "\r", "\t"});
                    // Pre-assert
                    // Act
                    var result = input.IsAlpha();
                    // Assert
                    Expect(result).To.Be.False();
                }

                [Test]
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
            public class IsAlphanumeric
            {
                [Test]
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
                public void OperatingOn_Whitespace_ShouldReturnFalse()
                {
                    // Arrange
                    var input = GetRandomFrom(new[] {" ", "\r", "\t"});
                    // Pre-assert
                    // Act
                    var result = input.IsAlphanumeric();
                    // Assert
                    Expect(result).To.Be.False();
                }

                [Test]
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
        public class ToMemoryStream
        {
            [Test]
            public void ShouldConvertNonEmptyString()
            {
                // Arrange
                var input = GetRandomString(10);
                // Act
                var result = input.ToMemoryStream();
                // Assert
                Expect(result.ToArray().ToUTF8String()).To.Equal(input);
            }

            [Test]
            public void ShouldConvertEmptyByteArray()
            {
                // Arrange
                var input = "";
                // Act
                var result = input.ToMemoryStream();
                // Assert
                Expect(result.ToArray().ToUTF8String()).To.Be.Empty();
            }

            [Test]
            public void ShouldTreatNullAsEmpty()
            {
                // Arrange
                var input = null as byte[];
                // Act
                var result = input.ToMemoryStream();
                // Assert
                Expect(result.ToArray().ToUTF8String()).To.Be.Empty();
            }
        }

        [TestFixture]
        public class SplitCommandline
        {
            [Test]
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
            public void ShouldReturnSingleProgramNameWhenNoSpaces()
            {
                // Arrange
                var program = $"{GetRandomString(1)}.exe";
                // Act
                var result = program.SplitCommandline();
                // Assert
                Expect(result)
                    .To.Equal(new[] {program});
            }

            [Test]
            public void ShouldReturnSingleQuotedProgramWithoutQuotes()
            {
                // Arrange
                var program = $"{GetRandomString(1)}.exe";
                var cli = $"\"{program}\"";
                // Act
                var result = cli.SplitCommandline();
                // Assert
                Expect(result)
                    .To.Equal(new[] {program});
            }

            [Test]
            public void ShouldReturnSpacedProgramWithoutQuotes()
            {
                // Arrange
                var program = $"C:\\Program Files\\MyApp\\{GetRandomString(1)}.exe";
                var cli = $"\"{program}\"";
                // Act
                var result = cli.SplitCommandline();
                // Assert
                Expect(result)
                    .To.Equal(new[] {program});
            }

            [Test]
            public void ShouldReturnSpacedProgramAndNonSpacedArguments()
            {
                // Arrange
                var program = $"C:\\Program Files\\MyApp\\{GetRandomString(1)}.exe";
                var cli = $"\"{program}\" arg1 arg2";
                // Act
                var result = cli.SplitCommandline();
                // Assert
                Expect(result)
                    .To.Equal(new[] {program, "arg1", "arg2"});
            }

            [Test]
            public void ShouldReturnSpacedProgramAndSpacedArgumentsUnQuoted()
            {
                // Arrange
                var program = $"C:\\Program Files\\MyApp\\{GetRandomString(1)}.exe";
                var cli = $"\"{program}\" \"arg1 arg2\"";
                // Act
                var result = cli.SplitCommandline();
                // Assert
                Expect(result)
                    .To.Equal(new[] {program, "arg1 arg2"});
            }
        }

        [TestFixture]
        public class DeQuote
        {
            [TestCase(" ")]
            [TestCase(null)]
            [TestCase("\t\r")]
            public void ShouldReturnNullOrWhitespace(string input)
            {
                // Arrange
                // Act
                var result = input.DeQuote();
                // Assert
                Expect(result).To.Equal(input);
            }

            [Test]
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
            public void ShouldNotRemoveUnmatchedQuotes(string input)
            {
                // Arrange
                // Act
                var result = input.DeQuote();
                // Assert
                Expect(result).To.Equal(input);
            }

            [Test]
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
        public class Matches
        {
            [Test]
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
            public void ShouldMatchIdenticalCollections()
            {
                // Arrange
                var left = new[] {"a", "b", "c"};
                var right = new[] {"a", "b", "c"};
                // Act
                var result = left.Matches(right);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldUseGivenStringComparison()
            {
                // Arrange
                var left = new[] {"a", "b", "c"};
                var right = new[] {"A", "B", "C"};
                // Act
                var result = left.Matches(right, StringComparison.OrdinalIgnoreCase);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class Substr
        {
            [Test]
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
        }

        [TestFixture]
        public class UnBase64
        {
            [Test]
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
            public void ShouldBeAbleToUnBase64UnpaddedByteData()
            {
                // Arrange
                string str;
                byte[] bytes;
                string base64;
                do
                {
                    str = GetRandomString(10, 32);
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
            public void ShouldBeAbleToUnBase64StringDataWithProvidedConverterToAnyType()
            {
                // Arrange
                var data = GetRandom<Poco>();
                var json = JsonConvert.SerializeObject(data);
                var base64 = json.ToBase64();
                // Act
                var result = base64.UnBase64<Poco>(
                    JsonConvert.DeserializeObject<Poco>
                );
                // Assert
                Expect(result)
                    .To.Deep.Equal(data);
            }

            [Test]
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
            public void ShouldDefaultStringDecodeToBeUTF8String()
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
        public class TrimBlock
        {
            [Test]
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
            public class WhenIndentationIsBySpaces
            {
                [Test]
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
        public class SplitOnce
        {
            [TestFixture]
            public class GivenNull
            {
                [Test]
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
            public class GivenEmptyString
            {
                [Test]
                public void ShouldReturnSingleElementEmpty()
                {
                    // Arrange
                    var str = "";
                    // Act
                    var result = str.SplitOnce(";");
                    // Assert
                    Expect(result)
                        .To.Equal(new[] { "" });
                }
            }

            [TestFixture]
            public class GivenNonEmptyStringWithoutDelimiter
            {
                [Test]
                public void ShouldReturnSingleElement()
                {
                    // Arrange
                    var str = "moocakes";
                    // Act
                    var result = str.SplitOnce("::");
                    // Assert
                    Expect(result)
                        .To.Equal(new[] { str });
                }
            }

            [TestFixture]
            public class GivenStringWithTwoElements
            {
                [Test]
                public void ShouldReturnThoseTwoElements()
                {
                    // Arrange
                    var str = "foo;bar";
                    var expected = new[] { "foo", "bar" };
                    // Act
                    var result = str.SplitOnce(";");
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class GivenStringWithThreeElements
            {
                [Test]
                public void ShouldReturnOnlyTwoItemsInArray()
                {
                    // Arrange
                    var str = "foo::bar::qux";
                    var expected = new[] { "foo", "bar::qux" };
                    // Act
                    var result = str.SplitOnce("::");
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class GivenNullSplitter
            {
                [Test]
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
        public class SplitPath
        {
            [Test]
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
            public void ShouldSplitPathWithMixedDelimiters()
            {
                // Arrange
                var expected = GetRandomArray<string>(3, 3);
                var flag = GetRandomBoolean();
                var first = flag ? "/" : "\\";
                var second = flag ? "\\": "/";
                var path = $"{expected[0]}{first}{expected[1]}{second}{expected[2]}";
                // Act
                var result = path.SplitPath();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }
}