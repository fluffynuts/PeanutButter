using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public void ShouldBeAbleToReadFromStdOut()
        {
            if (!Platform.IsWindows)
            {
                Assert.Ignore("This test uses a win32 commandline for testing");
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
    }
}