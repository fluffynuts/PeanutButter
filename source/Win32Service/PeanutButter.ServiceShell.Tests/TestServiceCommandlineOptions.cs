using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.ServiceShell.Tests
{
    [TestFixture]
    public class TestServiceCommandlineOptions
    {
        [TestCase('i', "install", "install this service")]
        [TestCase('u', "uninstall", "uninstall this service")]
        [TestCase('r', "run-once", "run one round of this service's code and exit")]
        [TestCase('w', "wait",
            "wait this many seconds before actually doing.*")] // this text is wrapped for console purposes
        [TestCase('h', "help", "shows this help")]
        [TestCase('v', "version", "show the version of this service")]
        [TestCase('s', "start", "start service")]
        [TestCase('x', "stop", "stop service")]
        public void Given_HelpArgument_ShouldPrintHelp(
            char shortName,
            string longName,
            string helpText)
        {
            //---------------Set up test pack-------------------
            var heading = GetRandomString();
            var copyRight = GetRandomString();
            var args = new[] { "-h" };
            var result = new List<string>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Create(args, heading, copyRight, s => result.Add(s));

            //---------------Test Result -----------------------
            var finalResult = string.Join("\n", result);
            var lines = finalResult.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var re = new Regex("-" + shortName + ",\\s.*--" + longName + "\\s.*" + helpText);
            Expect(lines)
                .To.Contain.Exactly(1)
                .Matched.By(s => re.IsMatch(s),
                    () => $@"no match for help line containing ""-{
                        shortName
                    }"", ""--{
                        longName
                    }"" and ""{
                        helpText
                    }""\n\nFull text was:{
                        finalResult
                    }"
                );
        }

        [TestCase('i', "install", "install this service")]
        [TestCase('u', "uninstall", "uninstall this service")]
        [TestCase('r', "run-once", "run one round of this service's code and exit")]
        [TestCase('w', "wait",
            "wait this many seconds before actually doing.*")] // this text is wrapped for console purposes
        [TestCase('h', "help", "shows this help")]
        [TestCase('v', "version", "show the version of this service")]
        [TestCase('s', "start", "start service")]
        [TestCase('x', "stop", "stop service")]
        public void Given_InvalidArgument_ShouldPrintHelp(
            char shortName,
            string longName,
            string helpText)
        {
            //---------------Set up test pack-------------------
            var heading = GetRandomString();
            var copyRight = GetRandomString();
            var args = new[] { "-a" };
            var result = new List<string>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Create(args, heading, copyRight, s => result.Add(s));

            //---------------Test Result -----------------------
            var finalResult = string.Join("\n", result);
            var lines = finalResult.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            var re = new Regex("-" + shortName + ",\\s.*--" + longName + "\\s.*" + helpText);
            Expect(lines)
                .To.Contain.Exactly(1)
                .Matched.By(s => re.IsMatch(s),
                    () => $@"no match for help line containing ""-{
                        shortName
                    }"", ""--{
                        longName
                    }"" and ""{
                        helpText
                    }""\n\nFull text was:{
                        finalResult
                    }"
                );
        }

        [Test]
        public void ShouldSetNoFlagsByDefault()
        {
            // Arrange
            // Act
            var sut = Create(new string[0], null, null);
            // Assert
            Expect(sut.StartService)
                .To.Be.False();
            Expect(sut.StopService)
                .To.Be.False();
            Expect(sut.Install)
                .To.Be.False();
            Expect(sut.Uninstall)
                .To.Be.False();
            Expect(sut.RunOnce)
                .To.Be.False();
            Expect(sut.ShowVersion)
                .To.Be.False();
            Expect(sut.Wait)
                .To.Equal(0);
        }
        
        [TestCase("-w")]
        [TestCase("--wait")]
        public void ShouldSetWait(string flag)
        {
            // Arrange
            var expected = GetRandomInt(2, 10);
            var args = new[] { flag, expected.ToString() };
            var sut = Create(args, "", "");
            // Act
            var result = sut.Wait;
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [TestCase("-s")]
        [TestCase("--start")]
        public void ShouldSetStartFlag(string flag)
        {
            // Arrange
            var args = new[] { flag };
            var sut = Create(args, "", "");
            // Act
            var result = sut.StartService;
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [TestCase("-v")]
        [TestCase("--version")]
        public void ShouldSetVersionFlag(string flag)
        {
            // Arrange
            var args = new[] { flag };
            var sut = Create(args, "", "");
            // Act
            var result = sut.ShowVersion;
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [TestCase("-x")]
        [TestCase("--stop")]
        public void ShouldSetStopFlag(string flag)
        {
            // Arrange
            var args = new[] { flag };
            var sut = Create(args, "", "");
            // Act
            var result = sut.StopService;
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [TestCase("-r")]
        [TestCase("--run-once")]
        public void ShouldSetRunOnceFlag(string flag)
        {
            // Arrange
            var args = new[] { flag };
            var sut = Create(args, "", "");
            // Act
            var result = sut.RunOnce;
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [TestCase("-i")]
        [TestCase("--install")]
        public void ShouldSetInstallFlag(string flag)
        {
            // Arrange
            var args = new[] { flag };
            var sut = Create(args, "", "");
            // Act
            var result = sut.Install;
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [TestCase("-u")]
        [TestCase("--uninstall")]
        public void ShouldSetUninstallFlag(string flag)
        {
            // Arrange
            var args = new[] { flag };
            var sut = Create(args, "", "");
            // Act
            var result = sut.Uninstall;
            // Assert
            Expect(result)
                .To.Be.True();
        }

        private ServiceCommandlineOptions Create(
            string[] args,
            string helpHeading,
            string copyRightInformation,
            Action<string> helpWriter = null
        )
        {
            return new ServiceCommandlineOptions(
                args,
                helpHeading,
                copyRightInformation,
                helpWriter ?? Console.WriteLine
            );
        }
    }
}