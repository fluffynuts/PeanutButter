using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestStringExtensions
    {
        [TestCase("Hello World", "^Hello", "Goodbye", "Goodbye World")]
        [TestCase("Hello World", "Wor.*", "Goodbye", "Hello Goodbye")]
        [TestCase("Hello World", "Hello$", "Goodbye", "Hello World")]
        public void RegexReplace_ShouldReplaceAccordingToRegexWithSuppliedValue(string input, string re,
            string replaceWith, string expected)
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
        public void Or_ActingOnString_ShouldReturnOtherStringWhenPrimaryIs_(string value)
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
        public void AsBoolean_ShouldResolveToBooleanValue(string input, bool expected)
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
        public void AsInteger_WhenStringIsNotInteger_ShouldReturnZero_(string input)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsInteger();

            //---------------Test Result -----------------------
            Assert.AreEqual(0, result);
        }


        private static readonly string[] _nullOrWhitespaceStrings =
        {
            null,
            "\t",
            "\r",
            "\n"
        };

        [TestCaseSource(nameof(_nullOrWhitespaceStrings))]
        public void IsNullOrWhitespace_ShouldReturnTrueWhenActingOnNullOrWhitespaceString_(string src)
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
        public void IsNullOrEmpty_ShouldReturnTrue_WhenOperatingOnString_(string src)
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
        public void IsNullOrEmpty_ShouldReturnFalse_WhenOperatingOnWhitespaceString_(string src)
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
        public void ToKebabCase_ShouldConvert_(string from, string expected)
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
        public void ToSnakeCase_ShouldConvert_(string from, string expected)
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

        [TestCase("Moo", "Moo")]
        [TestCase("MooCow", "MooCow")]
        [TestCase("i_am_snake", "IAmSnake")]
        [TestCase("is-already-kebabed", "IsAlreadyKebabed")]
        public void ToPascalCase_ShouldConvert_(string from, string expected)
        {
            // Arrange

            // Pre-Assert

            // Act
            var result = from.ToPascalCase();

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
        public void ToCamelCase_ShouldConvert_(string from, string expected)
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
        public void ToWords_ShouldConvert_(string from, string expected)
        {
            // Arrange

            // Pre-Assert

            // Act
            var result = from.ToWords();

            // Assert
            Expect(result).To.Equal(expected);
        }
    }
}