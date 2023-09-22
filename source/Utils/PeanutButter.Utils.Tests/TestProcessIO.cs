using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests;

[TestFixture]
// ReSharper disable once InconsistentNaming
public class TestProcessIO
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var node = Find.InPath("node");
        if (node is null)
        {
            Assert.Ignore(
                "These tess require node to be in your path (required to build & test from the CLI too)"
            );
        }
    }

    [Test]
    public void ShouldBeAbleToWaitForExit()
    {
        // Arrange
        using var folder = new AutoTempFolder();
        var expected = GetRandomWords(300);
        using var sourceFile = new AutoTempFile(
            folder.Path,
            Encoding.UTF8.GetBytes(
                expected
            )
        );
        using var targetFile = new AutoTempFile(
            folder.Path,
            new byte[0]
        );
        var sourceFileName = Path.GetFileName(sourceFile.Path);
        var targetFileName = Path.GetFileName(targetFile.Path);
        var script = GenerateCopyFileInlineJsFor(
            sourceFileName,
            targetFileName
        );

        // Act
        int exitCode;
        string[] stderr;
        string[] stdout;
        using (var io = ProcessIO.In(folder.Path)
                   .StartNode(
                       script
                   ))
        {
            io.WaitForExit();
            stderr = io.StandardError.ToArray();
            stdout = io.StandardOutput.ToArray();
            exitCode = io.ExitCode;
        }

        // Assert
        Expect(exitCode)
            .To.Equal(0, stderr.JoinWith("\n"));
        var written = File.ReadAllText(targetFile.Path);
        Expect(written)
            .To.Equal(expected);
    }

    string GenerateCopyFileInlineJsFor(
        string sourceFileName,
        string targetFileName
    )
    {
        return
            $"\"const fs = require(`fs`);const data = fs.readFileSync(`{sourceFileName}`);fs.writeFileSync(`{targetFileName}`, data);\"";
    }

    [Test]
    public void ShouldBeAbleToWaitForExitLongerRunning()
    {
        // Arrange
        using var targetFile = new AutoTempFile();
        var container = Path.GetDirectoryName(targetFile.Path);
        var expected = GetRandomWords();
        int exitCode;
        string[] stderr;
        string[] stdout;
        var script = @$"
(async function() {{
    const fs = require(`fs`);
    function sleep(ms) {{
        return new Promise(resolve => setTimeout(resolve, 500));
    }}
    await sleep(500);
    const data = Buffer.from(`{expected}`);
    fs.writeFileSync(`{targetFile.Path.Replace("\\", "/")}`, data);
}})()";
        // Act
        using (var io = ProcessIO.In(container)
                   .StartNode(
                       script
                   ))
        {
            exitCode = io.WaitForExit();
            stderr = io.StandardError.ToArray();
            stdout = io.StandardOutput.ToArray();
        }

        // Assert
        Expect(exitCode)
            .To.Equal(0, string.Join("\n", stderr));
        var written = File.ReadAllText(targetFile.Path);
        Expect(written)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToWaitForExitWhenLotsOfOutput()
    {
        // Arrange
        using var folder = new AutoTempFolder();
        var expected = PyLike.Range(0, 100).Select(_ => GetRandomWords(300))
            .ToArray()
            .JoinWith("\n");
        using var sourceFile = new AutoTempFile(
            folder.Path,
            Encoding.UTF8.GetBytes(
                expected
            )
        );
        using var targetFile = new AutoTempFile(
            folder.Path,
            new byte[0]
        );
        var sourceFileName = Path.GetFileName(sourceFile.Path);
        var targetFileName = Path.GetFileName(targetFile.Path);
        var script = GenerateCopyFileInlineJsFor(sourceFileName, targetFileName);

        // Act
        using (var io = ProcessIO.In(folder.Path)
                   .StartNode(
                       script
                   )
              )
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
        // Arrange
        // Act
        using var io = ProcessIO.Start(
            "node",
            "-e",
            "\"console.log('moo');\""
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
        // Act
        using var io = ProcessIO.In(Environment.CurrentDirectory)
            .StartNode("\"console.error('moo');\"");
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
        var script = $@"
const fs = require(`fs`);
const data = fs.readFileSync(`data.txt`);
console.log(data.toString());
";
        using var io = ProcessIO.In(tempFolder.Path)
            .StartNode(script);
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
        var expected = GetRandomAlphaString(4);
        var envVar = GetRandomAlphaString(4);
        var script = $@"
console.log(process.env[`{envVar}`]);
";
        // Act
        using var io = ProcessIO
            .WithEnvironmentVariable(envVar, expected)
            .StartNode(script);
        // Assert
        var lines = io.StandardOutput
            .Trim()
            .ToArray();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    expected
                }
            );
    }

    [Test]
    public void ShouldBeAbleToInjectEnvironmentVariablesAndCustomWorkingFolder()
    {
        // Arrange
        using var folder = new AutoTempFolder();
        var expected = GetRandomAlphaString(4);
        var envVar = GetRandomAlphaString(4);
        var script = $@"
console.log(process.env[`{envVar}`]);
";
        // Act
        using var io = ProcessIO
            .In(folder.Path)
            .WithEnvironmentVariable(envVar, expected)
            .StartNode(script);
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    expected
                }
            );
    }

    [Test]
    public void ShouldBeAbleToInjectEnvironmentVariablesAndCustomWorkingFolder2()
    {
        // Arrange
        using var folder = new AutoTempFolder();
        var expected = GetRandomAlphaString(4);
        var envVar = GetRandomAlphaString(4);
        var script = $@"
console.log(process.env[`{envVar}`]);
";
        // Act
        using var io = ProcessIO
            .WithEnvironmentVariable(envVar, expected)
            .In(folder.Path)
            .StartNode(script);
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    expected
                }
            );
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
        var script = $@"
console.log(process.env[`{envVar}`]);
";
        // Act
        using var io = ProcessIO
            .WithEnvironment(dict)
            .In(folder.Path)
            .StartNode(script);
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    expected
                }
            );
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
        var script = $@"
console.log(process.env[`{envVar}`]);
";
        // Act
        using var io = ProcessIO
            .In(folder.Path)
            .WithEnvironment(dict)
            .StartNode(script);
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    expected
                }
            );
    }

    [Test]
    public void ShouldNotBreakGivenNullEnvironment()
    {
        // Arrange
        using var folder = new AutoTempFolder();
        var expected = GetRandomAlphaString(4);
        var envVar = GetRandomAlphaString(4);
        var script = $@"
console.log(process.env[`{envVar}`]);
";
        // Act
        using var io = ProcessIO
            .In(folder.Path)
            .WithEnvironment(null)
            .WithEnvironmentVariable(envVar, expected)
            .StartNode(script);
        // Assert
        var lines = io.StandardOutput.ToArray().Trim();
        Expect(lines)
            .To.Equal(
                new[]
                {
                    expected
                }
            );
    }

    [Test]
    [Timeout(10000)]
    public void ShouldNotHangWhenProcessOutputsALotOfDataOnStartup()
    {
        // Arrange
        var line1 = "aaaa aaaa aaaa aaaa aaaa aaaa!";
        var line2 = "bbbb bbbb bbbb bbbb bbbb bbbb!";
        var count = 100;
        var expected = string.Join(
            "\n",
            PyLike.Range(0, 100)
                .Select(i => $"{i}{line1}\n{i}{line2}")
        );
        // Act
        using var io = ProcessIO
            .Start(
                "node",
                "-e",
                $"for (var i = 0; i < {count}; i++) {{ console.log(`${{i}}{line1}`); console.log(`${{i}}{line2}`); }}"
            );
        io.WaitForExit();
        var result = string.Join("\n", io.StandardOutput);
        // Assert
        Expect(result.Length)
            .To.Equal(expected.Length);
        Expect(result)
            .To.Equal(expected);
    }


    private static bool ExeIsAvailable(string name)
    {
        return Find.InPath(name) is not null;
    }

    private const string REQUIRES_CMD = "This test uses a win32 cmd.exe";

    private const string REQUIRES_PWSH = "This test uses pwsh which is not found in your path";

    private const string REQUIRES_CAT =
        "This test uses the output from `cat`, which is not available on this system";

    [TestFixture]
    [Explicit("Discovery")]
    public class DiscoveryTests
    {
        [Test]
        public void WhatHappensIfYouKillTwice()
        {
            // answer: Win32Exception with "Access is denied"
            // Arrange
            var proc = new Process()
            {
                StartInfo =
                {
                    FileName = "node",
                    Arguments =
                        "-e \"(async function() { while (true) { await new Promise(resolve => setTimeout(resolve, 500)); } })();\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };
            proc.Start();
            // Act
            proc.Kill();
            proc.Kill();
            // Assert
        }

        [Test]
        public void DoesReceivingIoEventsDrainTheAssociatedReader()
        {
            // Answer: Yes!
            // Arrange
            var proc = new Process()
            {
                StartInfo =
                {
                    FileName = "node",
                    Arguments = "-e \"console.log('foo'); console.log('bar');\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                }
            };
            var captured = new List<string>();
            proc.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    return;
                }

                captured.Add(e.Data);
            };

            // Act
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            proc.WaitForExit();
            // Assert
            Expect(captured)
                .To.Equal(
                    new[]
                    {
                        "foo",
                        "bar"
                    }
                );
        }
    }
}

public static class ProcessIOExtensions
{
    public static IProcessIO StartNode(
        this IUnstartedProcessIO unstarted,
        string script
    )
    {
        return unstarted.Start("node", "-e", script);
    }
}