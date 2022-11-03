using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class TestProcessIO
    {
        [Test]
        public void ShouldBeAbleToWaitForExit()
        {
            // Arrange
            if (!ExeIsAvailable("cmd"))
            {
                Assert.Ignore(REQUIRES_CMD);
                return;
            }

            if (!ExeIsAvailable("cat"))
            {
                Assert.Ignore(REQUIRES_CAT);
                return;
            }
            using var folder = new AutoTempFolder();
            var expected = GetRandomWords(300);
            using var sourceFile = new AutoTempFile(
                folder.Path,
                Encoding.UTF8.GetBytes(
                    expected
                )
            );
            using var targetFile = new AutoTempFile(
                folder.Path, new byte[0]
            );
            var sourceFileName = Path.GetFileName(sourceFile.Path);
            var targetFileName = Path.GetFileName(targetFile.Path);
            using var batFile = new AutoTempFile(
                folder.Path,
                "test.bat",
                Encoding.UTF8.GetBytes(
                    $"cat {sourceFileName} > {targetFileName}"
                )
            );

            // Act
            using (var io = ProcessIO.In(folder.Path)
                .Start(batFile.Path))
            {
                io.WaitForExit();
            }
            // Assert
            var written = File.ReadAllText(targetFile.Path);
            Expect(written)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToReadFromStdOut()
        {
            if (!CmdIsAvailable())
            {
                Assert.Ignore("This test uses a win32 cmd.exe");
                return;
            }

            // Arrange
            // Act
            using var io = ProcessIO.Start(
                "cmd", "/c", "echo", "moo"
            );
            Expect(io.StartException)
                .To.Be.Null();
            var lines = io.StandardOutput.ToArray().Select(l => l.Trim());
            // Assert
            Expect(lines).To.Equal(new[] { "moo" });
        }

        [Test]
        public void ShouldBeAbleToReadFromStdErr()
        {
            if (!CmdIsAvailable())
            {
                Assert.Ignore(REQUIRES_CMD);
                return;
            }

            // Arrange
            using var tempFolder = new AutoTempFolder();
            var fileName = Path.Combine(tempFolder.Path, "test.bat");
            File.WriteAllText(fileName, "echo moo 1>&2");
            // Act
            using var io = ProcessIO.Start(fileName);
            Expect(io.StartException)
                .To.Be.Null();
            var lines = io.StandardError.ToArray().Select(l => l.Trim());
            // Assert
            Expect(lines).To.Equal(new[] { "moo" });
        }

        [Test]
        public void ShouldBeAbleToRunInDifferentDirectory()
        {
            // Arrange
            if (!ExeIsAvailable("cat"))
            {
                Assert.Ignore(REQUIRES_CAT);
                return;
            }

            using var tempFolder = new AutoTempFolder();
            var tempFilePath = Path.Combine(tempFolder.Path, "data.txt");
            var expected = GetRandomString(32);
            File.WriteAllText(tempFilePath, expected);
            // Act
            using var io = ProcessIO.In(tempFolder.Path).Start("cat", "data.txt");
            // Assert
            var lines = io.StandardOutput.ToArray().Select(l => l.Trim());

            Expect(lines)
                .To.Equal(new[] { expected });
        }

        [Test]
        public void ShouldBeAbleToInjectEnvironmentVariables()
        {
            // Arrange
            if (!PwshIsAvailable())
            {
                Assert.Ignore(REQUIRES_PWSH);
                return;
            }

            var expected = GetRandomAlphaString(4);
            var envVar = GetRandomAlphaString(4);
            // Act
            using var io = ProcessIO
                .WithEnvironmentVariable(envVar, expected)
                .Start("pwsh", "-Command", $"Write-Host $env:{envVar}");
            // Assert
            var lines = io.StandardOutput.ToArray().Trim();
            Expect(lines)
                .To.Equal(new[] { expected });
        }

        [Test]
        public void ShouldBeAbleToInjectEnvironmentVariablesAndCustomWorkingFolder()
        {
            // Arrange
            if (!PwshIsAvailable())
            {
                Assert.Ignore(REQUIRES_PWSH);
                return;
            }

            using var folder = new AutoTempFolder();
            var expected = GetRandomAlphaString(4);
            var envVar = GetRandomAlphaString(4);
            // Act
            using var io = ProcessIO
                .In(folder.Path)
                .WithEnvironmentVariable(envVar, expected)
                .Start("pwsh", "-Command", $"Write-Host $env:{envVar}");
            // Assert
            var lines = io.StandardOutput.ToArray().Trim();
            Expect(lines)
                .To.Equal(new[] { expected });
        }

        [Test]
        public void ShouldBeAbleToInjectEnvironmentVariablesAndCustomWorkingFolder2()
        {
            // Arrange
            if (!PwshIsAvailable())
            {
                Assert.Ignore(REQUIRES_PWSH);
                return;
            }

            using var folder = new AutoTempFolder();
            var expected = GetRandomAlphaString(4);
            var envVar = GetRandomAlphaString(4);
            // Act
            using var io = ProcessIO
                .WithEnvironmentVariable(envVar, expected)
                .In(folder.Path)
                .Start("pwsh", "-Command", $"Write-Host $env:{envVar}");
            // Assert
            var lines = io.StandardOutput.ToArray().Trim();
            Expect(lines)
                .To.Equal(new[] { expected });
        }

        [Test]
        public void ShouldBeAbleToInjectEnvironmentVariablesAndCustomWorkingFolder3()
        {
            // Arrange
            if (!PwshIsAvailable())
            {
                Assert.Ignore(REQUIRES_PWSH);
                return;
            }

            using var folder = new AutoTempFolder();
            var expected = GetRandomAlphaString(4);
            var envVar = GetRandomAlphaString(4);
            var dict = new Dictionary<string, string>()
            {
                [envVar] = expected
            };
            // Act
            using var io = ProcessIO
                .WithEnvironment(dict)
                .In(folder.Path)
                .Start("pwsh", "-Command", $"Write-Host $env:{envVar}");
            // Assert
            var lines = io.StandardOutput.ToArray().Trim();
            Expect(lines)
                .To.Equal(new[] { expected });
        }

        [Test]
        public void ShouldBeAbleToInjectEnvironmentVariablesAndCustomWorkingFolder4()
        {
            // Arrange
            if (!PwshIsAvailable())
            {
                Assert.Ignore(REQUIRES_PWSH);
                return;
            }

            using var folder = new AutoTempFolder();
            var expected = GetRandomAlphaString(4);
            var envVar = GetRandomAlphaString(4);
            var dict = new Dictionary<string, string>()
            {
                [envVar] = expected
            };
            // Act
            using var io = ProcessIO
                .In(folder.Path)
                .WithEnvironment(dict)
                .Start("pwsh", "-Command", $"Write-Host $env:{envVar}");
            // Assert
            var lines = io.StandardOutput.ToArray().Trim();
            Expect(lines)
                .To.Equal(new[] { expected });
        }

        [Test]
        public void ShouldNotBreakGivenNullEnvironment()
        {
            // Arrange
            if (!PwshIsAvailable())
            {
                Assert.Ignore(REQUIRES_PWSH);
                return;
            }

            using var folder = new AutoTempFolder();
            var expected = GetRandomAlphaString(4);
            var envVar = GetRandomAlphaString(4);
            // Act
            using var io = ProcessIO
                .In(folder.Path)
                .WithEnvironment(null)
                .WithEnvironmentVariable(envVar, expected)
                .Start("pwsh", "-Command", $"Write-Host $env:{envVar}");
            // Assert
            var lines = io.StandardOutput.ToArray().Trim();
            Expect(lines)
                .To.Equal(new[] { expected });
        }

        private static bool ExeIsAvailable(string name)
        {
            return Find.InPath(name) is not null;
        }

        private static bool CmdIsAvailable()
        {
            return Platform.IsWindows && ExeIsAvailable("cmd");
        }

        private static bool PwshIsAvailable()
        {
            return ExeIsAvailable("pwsh");
        }

        private const string REQUIRES_CMD = "This test uses a win32 cmd.exe";

        private const string REQUIRES_PWSH = "This test uses pwsh which is not found in your path";

        private const string REQUIRES_CAT =
            "This test uses the output from `cat`, which is not available on this system";
    }
}