using System.IO;
using System.Linq;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestProcessIO
    {
        [Test]
        public void ShouldBeAbleToReadFromStdOut()
        {
            // Arrange
            // Act
            using (var io = new ProcessIO("echo", "moo"))
            {
                var lines = io.StandardOutput.ToArray().Select(l => l.Trim());
                // Assert
                Expect(lines).To.Equal(new[] { "moo" });
            }
        }
        [Test]
        public void ShouldBeAbleToReadFromStdErr()
        {
            // Arrange
            using (var tempFolder = new AutoTempFolder())
            {
                var fileName = Path.Combine(tempFolder.Path, "test.bat");
                File.WriteAllText(fileName, "echo moo 1>&2");
                // Act
                using (var io = new ProcessIO(fileName))
                {
                    var lines = io.StandardError.ToArray().Select(l => l.Trim());
                    // Assert
                    Expect(lines).To.Equal(new[] { "moo" });
                }
            }
        }
    }
}