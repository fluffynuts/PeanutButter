using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.ServiceShell.Tests
{
    [TestFixture]
    public class TestCommandlineOptions
    {
        [TestCase("Install", typeof(bool), 'i', "install", "Install this service")]
        [TestCase("Uninstall", typeof(bool), 'u', "uninstall", "Uninstall this service")]
        [TestCase("RunOnce", typeof(bool), 'r', "run-once", "Run one round of this service's code and exit")]
        [TestCase("Wait", typeof(int), 'w', "wait", "Wait this many seconds before actually doing the round of work")]
        [TestCase("ShowHelp", typeof(bool), 'h', "help", "Show this help")]
        [TestCase("ShowVersion", typeof(bool), 'v', "version", "Show the version of this service")]
        [TestCase("StartService", typeof(bool), 's', "start", "Start service")]
        [TestCase("StopService", typeof(bool), 'x', "stop", "Stop service")]
        public void Type_ShouldHaveOptionPropertyWith_(string propertyName,
            Type propertyType,
            char shortName,
            string longName,
            string helpText)
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ServiceCommandlineOptions);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var propInfo = sut.FindPropertyInfoForPath(propertyName, s => Assert.Fail(s));

            //---------------Test Result -----------------------
            Expect(propInfo.PropertyType)
                .To.Be(propertyType, () => $"unexpected property type for '{propertyName}'");
            var optionAttribute = propInfo.GetCustomAttributes<OptionAttribute>().Single();
            Expect(optionAttribute.ShortName)
                .To.Equal(shortName);
            Expect(optionAttribute.LongName)
                .To.Equal(longName);
            Expect(optionAttribute.HelpText)
                .To.Equal(helpText);
        }

        [TestCase('i', "install", "Install this service")]
        [TestCase('u', "uninstall", "Uninstall this service")]
        [TestCase('r', "run-once", "Run one round of this service's code and exit")]
        [TestCase('w', "wait",
            "Wait this many seconds before actually doing.*")] // this text is wrapped for console purposes
        [TestCase('h', "help", "Show this help")]
        [TestCase('v', "version", "Show the version of this service")]
        [TestCase('s', "start", "Start service")]
        [TestCase('x', "stop", "Stop service")]
        public void Given_HelpArgument_ShouldPrintHelp(
            char shortName,
            string longName,
            string helpText)
        {
            //---------------Set up test pack-------------------
            var heading = RandomValueGen.GetRandomString();
            var copyRight = RandomValueGen.GetRandomString();
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

        [TestCase('i', "install", "Install this service")]
        [TestCase('u', "uninstall", "Uninstall this service")]
        [TestCase('r', "run-once", "Run one round of this service's code and exit")]
        [TestCase('w', "wait",
            "Wait this many seconds before actually doing.*")] // this text is wrapped for console purposes
        [TestCase('h', "help", "Show this help")]
        [TestCase('v', "version", "Show the version of this service")]
        [TestCase('s', "start", "Start service")]
        [TestCase('x', "stop", "Stop service")]
        public void Given_InvalidArgument_ShouldPrintHelp(
            char shortName,
            string longName,
            string helpText)
        {
            //---------------Set up test pack-------------------
            var heading = RandomValueGen.GetRandomString();
            var copyRight = RandomValueGen.GetRandomString();
            var args = new[] { "-a" };
            var result = new List<string>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(args, heading, copyRight, s => result.Add(s));

            //---------------Test Result -----------------------
            Assert.AreEqual(ServiceCommandlineOptions.ExitCodes.ShowedHelp, sut.ExitCode);
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
        public void Construct_ShouldUseParserToParse()
        {
            // test that the parser is used and trust that it does what it's supposed to
            //---------------Set up test pack-------------------
            var parser = Substitute.For<IParser>();
            var heading = RandomValueGen.GetRandomString();
            var copyRight = RandomValueGen.GetRandomString();
            var args = new[] { "-a" };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(args, heading, copyRight, s =>
            {
            }, parser);

            //---------------Test Result -----------------------
            parser.Received().ParseArguments(args, sut);
        }

        [Test]
        public void PublicConstructor_ShouldSetUpInternalReferenceToDefaultParser()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new ServiceCommandlineOptions(new[] { "-h" }, RandomValueGen.GetRandomString(),
                RandomValueGen.GetRandomString());

            //---------------Test Result -----------------------
            var actual = sut.OptionsParser.GetPropertyValue("Actual");
            Assert.AreEqual(actual, Parser.Default);
        }

        private ServiceCommandlineOptions Create(string[] args,
            string helpHeading,
            string copyRightInformation,
            Action<string> helpWriter = null,
            IParser parser = null)
        {
            return new ServiceCommandlineOptions(args, helpHeading, copyRightInformation,
                helpWriter ?? Console.WriteLine,
                parser ?? new ParserFacade(Parser.Default));
        }
    }
}