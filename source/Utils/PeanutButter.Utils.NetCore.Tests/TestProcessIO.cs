using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.NetCore.Tests;

[TestFixture]
// ReSharper disable once InconsistentNaming
public class TestProcessIO
{
    [Test]
    public void ShouldBeAbleToReadFromStdOut()
    {
        // Arrange
        var node = FindNode();
        // Act
        using var io = ProcessIO.Start(
            node,
            "-e",
            "console.log('moo');"
        );
        Expect(io.StartException)
            .To.Be.Null();
        var lines = io.StandardOutput.ToArray().Select(l => l.Trim());
        // Assert
        Expect(lines).To.Equal(
            new[]
            {
                "moo"
            }
        );
    }

    [Test]
    public void ShouldBeAbleToReadFromStdErr()
    {
        // Arrange
        var node = FindNode();
        // Act
        using var io = ProcessIO.Start(
            node,
            "-e",
            "console.error('moo');"
        );
        Expect(io.StartException)
            .To.Be.Null();
        var lines = io.StandardError.ToArray().Select(l => l.Trim());
        // Assert
        Expect(lines).To.Equal(
            new[]
            {
                "moo"
            }
        );
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
            .To.Equal(
                new[]
                {
                    expected
                }
            );
    }

    [Test]
    public void ShouldBeAbleToInjectEnvironmentVariables()
    {
        // Arrange
        var node = FindNode();
        var expected = GetRandomAlphaString(4);
        var envVar = GetRandomAlphaString(4);
        // Act
        using var io = ProcessIO
            .WithEnvironmentVariable(envVar, expected)
            .Start(
                node,
                "-e",
                $"console.log(process.env['{envVar}']);"
            );
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines.Last() /* sometimes, pwsh has to break this test by outputting an upgrade nag :| */)
            .To.Equal(expected, () => lines.Stringify());
    }

    [Test]
    public void ShouldBeAbleToInjectEnvironmentVariablesByNameAndValueAndCustomWorkingFolder()
    {
        // Arrange
        var node = FindNode();
        using var folder = new AutoTempFolder();
        var expected = GetRandomAlphaString(4);
        var envVar = GetRandomAlphaString(4);
        // Act
        using var io = ProcessIO
            .In(folder.Path)
            .WithEnvironmentVariable(envVar, expected)
            .Start(
                node,
                "-e",
                $"console.log(process.cwd()); console.log(process.env['{envVar}']);"
            );
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    folder.Path,
                    expected
                }
            );
    }

    [Test]
    public void ShouldBeAbleToInjectEnvironmentVariablesDictAndCustomWorkingFolder()
    {
        // Arrange
        var node = FindNode();
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
            .Start(
                node,
                "-e",
                $"console.log(process.cwd()); console.log(process.env['{envVar}']);"
            );
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    folder.Path,
                    expected
                }
            );
    }

    [Test]
    public void ShouldNotBreakGivenNullEnvironment()
    {
        // Arrange
        var node = FindNode();
        using var folder = new AutoTempFolder();
        var expected = GetRandomAlphaString(4);
        var envVar = GetRandomAlphaString(4);
        // Act
        using var io = ProcessIO
            .In(folder.Path)
            .WithEnvironment(null)
            .WithEnvironmentVariable(envVar, expected)
            .Start(
                node,
                "-e",
                $"console.log(process.cwd()); console.log(process.env['{envVar}']);"
            );
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    folder.Path,
                    expected
                }
            );
    }

    [Test]
    public void ShouldBeAbleToWriteToStdInBashScript()
    {
        if (Platform.IsWindows)
        {
            Assert.Ignore(
                "This does not run reliably on Windows, even if you have bash available (eg from a git installation)"
            );
        }

        var bash = FindBash();
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
        var node = FindNode();
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

    [Test]
    public void ShouldBeAbleToEnforceMaxBufferSize()
    {
        // Arrange
        var node = FindNode();
        using var f = new AutoTempFile(
            @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    for (var i = 0; i < 5; i++) {
        await sleep(100);
        console.log(`line: ${i}`);
    }
})();
"
        );
        // Act
        using var io = ProcessIO.Start(
            node,
            f.Path
        );
        io.MaxBufferLines = 1;
        io.WaitForExit();
        var result1 = io.StandardOutput.ToArray();
        
        // Assert
        Expect(result1)
            .To.Equal(new[] { "line: 4" });
    }

    private static string TryFindInPath(string name)
    {
        var result = Find.InPath(name);
        if (result is null)
        {
            Assert.Ignore(
                $"Unable to run test: no '{name}' found in path"
            );
        }

        return result;
    }

    private static string FindNode()
    {
        return TryFindInPath("node");
    }

    private static string FindBash()
    {
        return TryFindInPath("bash");
    }
}