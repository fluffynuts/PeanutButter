using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.NetCore.Tests
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
                "cmd",
                "/c",
                "echo",
                "moo"
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
            Expect(lines.Last() /* sometimes, pwsh has to break this test by outputting an upgrade nag :| */)
                .To.Equal(expected);
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

        [Test]
        public void ShouldBeAbleToWriteToStdInBashScript()
        {
            var bash = Find.InPath("bash");
            if (bash is null || Platform.IsWindows)
            {
                Assert.Ignore(
                    Platform.IsWindows
                        ? "This does not run reliably on Windows, even if you have bash available (eg from a git installation)"
                        : "Cannot find bash in your path? This is madness!"
                );
            }

            // Arrange
            var lines = GetRandomArray<string>(3, 5);
            using var f = new AutoTempFile(
                @"#!/bin/bash
while read line; do
    if test ""$line"" = ""quit""; then
        exit 0
    fi
    echo ""$line""
done < /dev/stdin
"
            );
            // Act
            using var io = ProcessIO.Start(
                bash,
                f.Path
            );
            foreach (var line in lines)
            {
                io.StandardInput.WriteLine(line);
            }

            io.StandardInput.WriteLine("quit");
            var collected = io.StandardOutput.ToArray();
            // Assert
            Expect(collected)
                .To.Equal(lines);
        }

        [Test]
        public void ShouldBeAbleToWriteToStdInNodeScript()
        {
            // Arrange
            var node = Find.InPath("node");
            if (node is null)
            {
                Assert.Ignore(
                    "This test requires node in your path to run"
                );
            }

            var lines = GetRandomArray<string>(3, 5);
            using var f = new AutoTempFile(
                @"var readline = require(""readline"");
var rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout,
    terminal: false
});

rl.on(""line"", function(line) {
    var trimmed = line.trim();
    if (trimmed === ""quit"") {
        process.exit(0);
    }
    console.log(line);
});
"
            );

            // Act
            using var io = ProcessIO.Start(
                node,
                f.Path
            );
            foreach (var line in lines)
            {
                io.StandardInput.WriteLine(line);
            }
            io.StandardInput.WriteLine("quit");
            var collected = io.StandardOutput.ToArray();
            // Assert
            Expect(collected)
                .To.Equal(lines);
        }
    }
}