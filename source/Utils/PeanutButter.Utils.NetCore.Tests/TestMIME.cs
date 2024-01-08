using System;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestMIME
    {
        [TestFixture]
        public class GuessingMimeTypeForFileName
        {
            [Test]
            public void ShouldThrowForNull()
            {
                // Arrange
                // Act
                Expect(() => MIMEType.GuessForFileName(null))
                    .To.Throw<ArgumentNullException>();
                // Assert
            }

            [TestCase("application/octet-stream")]
            public void ShouldReturnForNoExtension_(string expected)
            {
                // Arrange
                // Act
                var result = MIMEType.GuessForFileName("some-file");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [TestCase("application/octet-stream")]
            public void ShouldReturnForUnknownExtension_(string expected)
            {
                // Arrange
                // Act
                var result = MIMEType.GuessForFileName("my-file.some-extension");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [TestCase("foo.xml", "text/xml")]
            [TestCase(".xml", "text/xml")]
            [TestCase("xml", "text/xml")]
            public void ShouldResolve_(string fileName, string expected)
            {
                // Arrange
                // Act
                var result = MIMEType.GuessForFileName(fileName);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class GuessingExtensionForMimeType
        {
            [TestCase("text/xml", ".xml")]
            [TestCase("image/png", ".png")]
            // special cases
            [TestCase("application/xml", ".xml")]
            [TestCase("image/x-png", ".png")]
            public void ShouldReturnTrueAndExtensionForKnownType(
                string mimeType,
                string expected
            )
            {
                // Arrange
                // Act
                var resolved = MIMEType.TryGuessExtensionForMimeType(
                    mimeType,
                    out var result
                );
                // Assert
                Expect(resolved)
                    .To.Be.True();
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }
}