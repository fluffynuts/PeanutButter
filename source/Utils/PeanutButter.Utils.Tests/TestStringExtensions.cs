using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        [TestCase(null)]
        [TestCase("")]
        public void Or_ActingOnString_ShouldReturnOtherStringWhenPrimaryIs_(
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
        public void Or_CanChainUntilFirstValidValue()
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
        public void AsBoolean_ShouldResolveToBooleanValue(
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

        [Test]
        public void IsInteger_WhenStringIsInteger_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomInt().ToString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.IsInteger();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsInteger_WhenStringIsNotInteger_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomAlphaString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.IsInteger();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void AsInteger_WhenStringIsInteger_ShouldReturnThatIntegerValue()
        {
            //---------------Set up test pack-------------------
            var expected = GetRandomInt(10, 100);
            var input = expected.ToString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsInteger();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AsInteger_WhenStringIsFloatingPointWithPeriod_ShouldReturnTruncatedIntPart()
        {
            //---------------Set up test pack-------------------
            var input = "1.2";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsInteger();

            //---------------Test Result -----------------------
            Assert.AreEqual(1, result);
        }

        [Test]
        public void AsInteger_WhenStringIsFloatingPointWithComma_ShouldReturnTruncatedIntPart()
        {
            //---------------Set up test pack-------------------
            var input = "1,2";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsInteger();

            //---------------Test Result -----------------------
            Assert.AreEqual(1, result);
        }

        [Test]
        public void AsInteger_WhenStringHasAlphaPart_ShouldReturnIntPartOfBeginning()
        {
            //---------------Set up test pack-------------------
            var input = "2ab4";
            var expected = 2;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsInteger();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void AsInteger_WhenStringHasLeadingAlphaPart_ShouldReturnIntPartOfBeginning()
        {
            //---------------Set up test pack-------------------
            var input = "woof42meow4";
            var expected = 42;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsInteger();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [TestCase("a")]
        [TestCase("")]
        [TestCase("\r\n")]
        [TestCase(null)]
        public void AsInteger_WhenStringIsNotInteger_ShouldReturnZero_(
            string input)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsInteger();

            //---------------Test Result -----------------------
            Assert.AreEqual(0, result);
        }


        private static readonly string[] NullOrWhitespaceStrings =
        {
            null,
            "\t",
            "\r",
            "\n"
        };

        [TestCaseSource(nameof(NullOrWhitespaceStrings))]
        public void IsNullOrWhitespace_ShouldReturnTrueWhenActingOnNullOrWhitespaceString_(
            string src)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsNullOrWhiteSpace();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsNullOrWhitespace_ShouldReturnFalse_WhenOperatingOnNonWhitespaceString()
        {
            //---------------Set up test pack-------------------
            var src = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsNullOrWhiteSpace();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [TestCase(null)]
        [TestCase("")]
        public void IsNullOrEmpty_ShouldReturnTrue_WhenOperatingOnString_(
            string src)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsNullOrEmpty();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }


        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r")]
        public void IsNullOrEmpty_ShouldReturnFalse_WhenOperatingOnWhitespaceString_(
            string src)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.IsNullOrEmpty();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }


        [Test]
        public void IsNullOrEmpty_ShouldReturnFalse_WhenOperatingOnNonWhitespaceString()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.IsNullOrEmpty();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsOneOf_GivenNoNeedles_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
//            Expect(() => "foo".ContainsOneOf())
//                .Throws<ArgumentException>();
            Assert.That(() => "foo".ContainsOneOf(), Throws.Exception.InstanceOf<ArgumentException>());
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsOneOf_GivenNullNeedle_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
//            Expect(() => "foo".ContainsOneOf(null, "foo"))
//                .Throws<ArgumentException>();
            Assert.That(() => "foo".ContainsOneOf(null, "foo"), Throws.Exception.InstanceOf<ArgumentException>());
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsOneOf_OperatingOnNull_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = (null as string).ContainsOneOf("foo");
            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void ContainsOneOf_OperatingOnStringContainingNoneOfTheNeedles_ShouldReturnFalse()
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
        public void ContainsOneOf_OperatingOnStringContainingOnneOfTheNeedles_ShouldReturnTrue()
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


        [Test]
        public void ContainsAllOf_GivenNoNeedles_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Assert.That(() => "foo".ContainsAllOf(), Throws.Exception.InstanceOf<ArgumentException>());
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsAllOf_GivenNullNeedle_ShouldThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            // NExpect FIXME: why does this fail with an esoteric NSubstitute error?
//            Expect(() => "foo".ContainsAllOf(null, "foo"))
//                .Throws<ArgumentException>();
            Assert.That(() => "foo".ContainsAllOf(null, "foo"), Throws.Exception.InstanceOf<ArgumentException>());
            //--------------- Assert -----------------------
        }

        [Test]
        public void ContainsAllOf_OperatingOnNull_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = (null as string).ContainsAllOf("foo");
            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void ContainsAllOf_WhenHaystackContainsAllConstituents_ShouldReturnTrue()
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
        public void ContainsAllOf_WhenHaystackMissingNeedle_ShouldReturnFalse()
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

        [Test]
        public void ToBase64_OperatingOnNullString_ShouldReturnNull()
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
        public void ToBase64_OperatingOnEmptyString()
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
        public void ToBase64_OperatingOnNonEmptyString()
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

        [Test]
        public void ToKebabCase_OperatingOnNull_ShouldReturnNull()
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
        public void ToKebabCase_ShouldConvert_(
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
        public void ToSnakeCase_OperatingOnNull_ShouldReturnNull()
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
        public void ToSnakeCase_ShouldConvert_(
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

        [Test]
        public void ToPascalCase_OperatingOnNull_ShouldReturnNull()
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
        public void ToPascalCase_ShouldConvert_((string input, string expected) testCase)
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
        public void ToTitleCase_ShouldConvert_((string input, string expected) testCase)
        {
            // Arrange
            var (from, expected) = testCase;

            // Pre-Assert

            // Act
            var result = from.ToTitleCase();

            // Assert
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void ToCamelCase_OperatingOnNull_ShouldReturnNull()
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
        public void ToCamelCase_ShouldConvert_(
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

        [Test]
        public void ToWords_OperatingOnNull_ShouldReturnNull()
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
        public void ToWords_ShouldConvert_(
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
                Expect(result).To.Contain.All()
                    .Matched.By(s => s == s.ToUpper());
            }

            [Test]
            public void ToLower_ShouldOwerCaseAll()
            {
                // Arrange
                var src = GetRandomArray<string>(10).ToUpper();
                // Pre-assert
                Expect(src).To.Contain.All()
                    .Matched.By(s => s == s.ToUpper());
                // Act
                var result = src.ToLower();
                // Assert
                Expect(result).To.Contain.All()
                    .Matched.By(s => s == s.ToLower());
            }
        }

        [TestFixture]
        public class CharacterClasses
        {
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
                    var input = GetRandomAlphaNumericString(10);
                    while (!numerics.IsMatch(input) || !alpha.IsMatch(input))
                    {
                        // random alphanumeric string may contain only alphas / numerics
                        // -> it is random, after all...
                        input = GetRandomNumericString(10);
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
                    .To.Equal(new[] { program });
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
                    .To.Equal(new[] { program });
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
                    .To.Equal(new[] { program });
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
                    .To.Equal(new[] { program, "arg1", "arg2" });
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
                    .To.Equal(new[] { program, "arg1 arg2" });
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
                var left = new[] { "a", "b", "c" };
                var right = new[] { "a", "b", "c" };
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
                var left = new[] { "a", "b", "c" };
                var right = new[] { "A", "B", "C" };
                // Act
                var result = left.Matches(right, StringComparison.OrdinalIgnoreCase);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }
    }
}