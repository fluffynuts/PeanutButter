using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestCommandline
    {
        [TestFixture]
        public class RenderingAsAString
        {
            [Test]
            public void ShouldHandleSimpleCommandNoArgs()
            {
                // Arrange
                var expected = GetRandomString(10);
                var sut = Create(expected);
                // Act
                var result = sut.ToString();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldHandleSpacedCommandNoArgs()
            {
                // Arrange
                var app = GetRandomWords(2);
                var expected = $"\"{app}\"";
                var sut = Create(app);
                // Act
                var result = sut.ToString();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldHandleQuotedSpacedCommandNoArgs()
            {
                // Arrange
                var app = GetRandomWords(2);
                var expected = $"\"{app}\"";
                var sut = Create(expected);
                // Act
                var result = sut.ToString();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldHandleSpacesInArgs()
            {
                // Arrange
                var app = GetRandomWords(2);
                var arg1 = GetRandomWords(2);
                var arg2 = GetRandomString(10);
                var expected = $"\"{app}\" \"{arg1}\" {arg2}";
                var sut = Create(app, arg1, arg2);
                // Act
                var result = sut.ToString();
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class Parsing
        {
            [Test]
            public void ShouldParseSimpleNoSpacesCommand()
            {
                // Arrange
                var app = GetRandomString(10);

                // Act
                var result = Commandline.Parse(app);
                // Assert
                Expect(result.Command)
                    .To.Equal(app);
                Expect(result.Args)
                    .To.Be.Empty();
            }

            [Test]
            public void ShouldParseSimpleNoSpacesCommandWithUnnecessaryQuotes()
            {
                // Arrange
                var app = GetRandomString(10);

                // Act
                var result = Commandline.Parse($"\"{app}\"");
                // Assert
                Expect(result.Command)
                    .To.Equal(app);
                Expect(result.Args)
                    .To.Be.Empty();
            }

            [Test]
            public void ShouldParseSimpleCommandWithSpaces()
            {
                // Arrange
                var app = GetRandomWords(2);
                var input = $"\"{app}\"";
                // Act
                var result = Commandline.Parse(input);
                // Assert
                Expect(result.Command)
                    .To.Equal(app);
                Expect(result.Args)
                    .To.Be.Empty();
            }

            [Test]
            public void ShouldParseCommandWithArgs()
            {
                // Arrange
                var app = GetRandomWords(2);
                var arg1 = GetRandomWords(2);
                var arg2 = GetRandomString();
                var input = $"\"{app}\" \"{arg1}\" {arg2}";
                // Act
                var result = Commandline.Parse(input);
                // Assert
                Expect(result.Command)
                    .To.Equal(app);
                Expect(result.Args)
                    .To.Equal(new[] { arg1, arg2 });
            }

            [Test]
            public void ShouldParseCommandWithUnnecessarilyQuotedArgs()
            {
                // Arrange
                var app = GetRandomWords(2);
                var arg1 = GetRandomWords(2);
                var arg2 = GetRandomString();
                var input = $"\"{app}\" \"{arg1}\" \"{arg2}\"";
                // Act
                var result = Commandline.Parse(input);
                // Assert
                Expect(result.Command)
                    .To.Equal(app);
                Expect(result.Args)
                    .To.Equal(new[] { arg1, arg2 });
            }
        }

        [TestFixture]
        public class ImplicitConversions
        {
            [Test]
            public void ShouldConvertToString()
            {
                // Arrange
                var app = GetRandomWords(2);
                var arg1 = GetRandomWords(2);
                var arg2 = GetRandomString();
                var expected = $"\"{app}\" \"{arg1}\" {arg2}";
                var sut = Create(app, arg1, arg2);
                // Act
                var result = (string)sut;
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldConvertFromString()
            {
                // Arrange
                var app = GetRandomWords(2);
                var arg1 = GetRandomWords(2);
                var arg2 = GetRandomString();
                var cli = $"\"{app}\" \"{arg1}\" {arg2}";
                // Act
                var result = (Commandline)cli;
                // Assert
                Expect(result.ToString())
                    .To.Equal(cli);
            }
        }

        private static Commandline Create(
            string command,
            params string[] args
        )
        {
            return new(command, args);
        }
    }
}