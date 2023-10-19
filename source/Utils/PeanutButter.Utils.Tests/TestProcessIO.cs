using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

    [Test]
    public void ShouldBeAbleToReadInterleavedIo()
    {
        // Arrange
        // Act
        using var io = ProcessIO
            .Start(
                "node",
                "-e",
                "console.log('stdout 1');console.error('stderr 1');console.error('stderr 2');console.log('stdout 2');"
            );
        io.WaitForExit();
        // Assert
        Expect(io.StandardOutputAndErrorInterleaved)
            .To.Equal(
                new[]
                {
                    "stdout 1",
                    "stderr 1",
                    "stderr 2",
                    "stdout 2",
                }
            );
    }

    [TestFixture]
    public class Snapshots
    {
        [Test]
        public void ShouldBeAbleToSnapshotStdOutThusFar()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    console.log('stdout 1');
    console.error('stderr 1');
    console.error('stderr 2');
    console.log('stdout 2');
    await sleep(1000);
    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            Thread.Sleep(500);
            var snapshot = io.StandardOutputSnapshot.ToArray();
            // Assert
            Expect(snapshot)
                .To.Equal(
                    new[]
                    {
                        "stdout 1",
                        "stdout 2"
                    }
                );
        }

        [Test]
        public void ShouldBeAbleToSnapshotStdErrThusFar()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    console.log('stdout 1');
    console.error('stderr 1');
    console.error('stderr 2');
    console.log('stdout 2');
    await sleep(1000);
    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            Thread.Sleep(500);
            var snapshot = io.StandardErrorSnapshot.ToArray();
            // Assert
            Expect(snapshot)
                .To.Equal(
                    new[]
                    {
                        "stderr 1",
                        "stderr 2"
                    }
                );
        }

        [Test]
        public void ShouldBeAbleToSnapshotInterleavedIoThusFar()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(1000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            Thread.Sleep(500);
            var snapshot = io.StandardOutputAndErrorInterleavedSnapshot.ToArray();
            // Assert
            Expect(snapshot)
                .To.Equal(
                    new[]
                    {
                        "stdout 1",
                        "stderr 1",
                        "stderr 2",
                        "stdout 2"
                    }
                );
        }
    }

    [TestFixture]
    public class WaitingForSpecificOutput
    {
        [Test]
        public void ShouldBeAbleToWaitForSpecificStandardOutput()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(1000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            io.WaitForOutput(StandardIo.StdOut, s => s.Trim() == "stdout 2");
            var snapshot = io.StandardOutputAndErrorInterleavedSnapshot.ToArray();
            // Assert
            Expect(snapshot)
                .To.Equal(
                    new[]
                    {
                        "stdout 1",
                        "stderr 1",
                        "stderr 2",
                        "stdout 2"
                    }
                );
        }

        [Test]
        public void ShouldBeAbleToWaitForSpecificStandardError()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(1000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            io.WaitForOutput(StandardIo.StdErr, s => s.Trim() == "stderr 2");
            var snapshot = io.StandardOutputAndErrorInterleavedSnapshot.ToArray();
            // Assert
            Expect(snapshot)
                .To.Equal(
                    new[]
                    {
                        "stdout 1",
                        "stderr 1",
                        "stderr 2",
                    }
                );
        }

        [Test]
        public void ShouldBeAbleToWaitForSpecificOutputOnAnyPipe()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(1000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            io.WaitForOutput(StandardIo.StdOutOrStdErr, s => s.Trim() == "stdout 2");
            var snapshot = io.StandardOutputAndErrorInterleavedSnapshot.ToArray();
            // Assert
            Expect(snapshot)
                .To.Equal(
                    new[]
                    {
                        "stdout 1",
                        "stderr 1",
                        "stderr 2",
                        "stdout 2"
                    }
                );
        }

        [Test]
        public void ShouldBeAbleToWaitForSpecificOutputOnStdOutWithTimeout()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(4000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            var shouldBeTrue = io.WaitForOutput(
                StandardIo.StdOut,
                s => s.Trim() == "stdout 2",
                100
            );
            var shouldBeFalse = io.WaitForOutput(
                StandardIo.StdOut,
                s => s.Trim() == "stdoput 3",
                500
            );
            var snapshot = io.StandardOutputAndErrorInterleavedSnapshot.ToArray();
            // Assert
            Expect(shouldBeTrue)
                .To.Be.True();
            Expect(shouldBeFalse)
                .To.Be.False();
            Expect(snapshot)
                .Not.To.Contain("stdout 3");
        }

        [Test]
        public void ShouldBeAbleToWaitForSpecificOutputOnStdErrWithTimeout()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(4000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            var shouldBeTrue = io.WaitForOutput(
                StandardIo.StdErr,
                s => s.Trim() == "stderr 2",
                100
            );
            var shouldBeFalse = io.WaitForOutput(
                StandardIo.StdErr,
                s => s.Trim() == "stderr 3",
                500
            );
            var snapshot = io.StandardOutputAndErrorInterleavedSnapshot.ToArray();
            // Assert
            Expect(shouldBeTrue)
                .To.Be.True();
            Expect(shouldBeFalse)
                .To.Be.False();
            Expect(snapshot)
                .Not.To.Contain("stdout 3");
        }

        [Test]
        public void ShouldBeAbleToWaitForSpecificOutputOnAnyPipeWithTimeout()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(4000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            var shouldBeTrue = io.WaitForOutput(
                StandardIo.StdOutOrStdErr,
                s => s.Trim() == "stdout 2",
                100
            );
            var shouldBeFalse = io.WaitForOutput(
                StandardIo.StdOutOrStdErr,
                s => s.Trim() == "stdout 3",
                500
            );
            var snapshot = io.StandardOutputAndErrorInterleavedSnapshot.ToArray();
            // Assert
            Expect(shouldBeTrue)
                .To.Be.True();
            Expect(shouldBeFalse)
                .To.Be.False();
            Expect(snapshot)
                .Not.To.Contain("stdout 3");
        }

        [Test]
        [Timeout(10000)]
        public void ShouldReturnFalseWhenProcessExitsWithoutTheRequiredMatcherBeingHit()
        {
            // Arrange
            using var tmpFile = new AutoTempFile(
                @"
(async function() {
    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    async function giveIoAChanceToGetOutThere() {
        // because the io handlers are async, without a minor
        // wait, they may end up (slightly) out of order - which
        // probably doesn't matter for consumers, but consistently
        // breaks this test; even a sleep(0) works around this
        await sleep(0);
    }

    console.log('stdout 1');
    await giveIoAChanceToGetOutThere()
    console.error('stderr 1');
    await giveIoAChanceToGetOutThere()

    console.error('stderr 2');
    await giveIoAChanceToGetOutThere()
    console.log('stdout 2');

    await sleep(2000);

    console.log('stdout 3');
    console.error('stderr 4');
})();
".TrimStart()
            );
            // Act
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            using var io = ProcessIO
                .Start(
                    "node",
                    tmpFile.Path
                );
            var shouldBeFalse = io.WaitForOutput(
                StandardIo.StdOutOrStdErr,
                s => s.Trim() == "wibbles"
            );
            stopwatch.Stop();
            // Assert
            Expect(shouldBeFalse)
                .To.Be.False();
            Expect(stopwatch.Elapsed)
                .To.Be.Greater.Than(
                    TimeSpan.FromSeconds(2)
                )
                .And
                .To.Be.Less.Than(
                    // allow 2 seconds for node overhead
                    TimeSpan.FromSeconds(4)
                );
        }
    }

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
        public void ShouldDrainReaderWhenReceivingIoEvents()
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