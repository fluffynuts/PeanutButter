using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NExpect;
using PeanutButter.FileSystem;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement.Exceptions;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static PeanutButter.WindowsServiceManagement.ServiceControlKeys;
// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.WindowsServiceManagement.Core.Tests
{
    [TestFixture]
    [Explicit($"Requires running on windows, with {ServiceName} installed")]
    public class TestServiceControlInterface
    {
        private const string ServiceName = "rabbitmq";

        [TestFixture]
        [Explicit($"Requires running on windows, with {ServiceName} installed")]
        public class QueryEx
        {
            [Test]
            public void ShouldReturnServiceName()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryEx(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.SERVICE_NAME)
                    .With.Value("rabbitmq");
            }

            [Test]
            public void ShouldReturnType()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryEx(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.TYPE)
                    .With.Value("10  WIN32_OWN_PROCESS");
            }

            [Test]
            public void ShouldReturnServiceState()
            {
                // Arrange
                var sut = Create();
                
                // Act
                var result = sut.QueryEx(ServiceName);
                
                // Assert
                Expect(result)
                    .To.Contain.Key(ServiceControlKeys.STATE)
                    .With.Value("4  RUNNING (STOPPABLE, NOT_PAUSABLE, ACCEPTS_SHUTDOWN)");
            }

            [TestCase(WIN32_EXIT_CODE)]
            [TestCase(SERVICE_EXIT_CODE)]
            [TestCase(WAIT_HINT)]
            [TestCase(PROCESS_ID)]
            [TestCase(FLAGS)]
            public void ShouldContainKey_(string expected)
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.QueryEx(ServiceName);
                // Assert
                Expect(result)
                    .To.Contain.Key(expected);
            }
        }

        private static IServiceControlInterface Create()
        {
            return new ServiceControlInterface();
        }
    }

    [TestFixture]
    [Explicit("WIP")]
    public class TestWindowsServiceUtil
    {
        private void TryRun(
            Action toRun,
            TimeSpan maxWaitForSuccess)
        {
            var giveUp = DateTime.Now + maxWaitForSuccess;
            Exception last = null;
            while (DateTime.Now <= giveUp)
            {
                try
                {
                    toRun();
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }

            throw last ?? new InvalidOperationException("No last exception captured");
        }

        [TearDown]
        public void Teardown()
        {
            EnsureTestServiceIsNotInstalled();
        }

        [SetUp]
        public void Setup()
        {
            EnsureTestServiceIsNotInstalled();
        }

        private void Run(
            string program,
            params string[] args)
        {
            using var proc = new Process();
            var si = proc.StartInfo;
            si.FileName = program;
            si.Arguments = args.Select(QuoteIfNecessary).JoinWith(" ");
            si.RedirectStandardError = true;
            si.RedirectStandardInput = true;
            si.RedirectStandardOutput = true;
            si.CreateNoWindow = true;
            si.UseShellExecute = false;

            if (!proc.Start())
            {
                throw new ApplicationException($"Can't start {program} {args.JoinWith(" ")}");
            }

            proc.WaitForExit();
            if (proc.ExitCode != 0)
            {
                throw new Exception($"{program} {args.JoinWith(" ")}\nexited with code {proc.ExitCode}");
            }
        }

        private static string TestServicePath =>
            Path.Combine(
                Path.GetDirectoryName(
                    new Uri(typeof(TestWindowsServiceUtil).Assembly.Location).LocalPath
                ), "TestService.exe"
            );

        private string QuoteIfNecessary(string arg)
        {
            return arg?.Contains(" ") ?? false
                ? $"\"{arg}\""
                : arg;
        }

        private static void TryDo(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        private void EnsureTestServiceIsNotInstalled()
        {
            // attempts to stop and uninstall all found test service instances
            var serviceNames = new List<string>();
            using var io = ProcessIO.Start("sc", "query", "type=", "service", "state=", "all");
            serviceNames.AddRange(
                io.StandardOutput
                    .Select(line => line.Trim())
                    .Where(line => line.StartsWith("SERVICE_NAME"))
                    .Select(line => string.Join(":", line.Split(':').Skip(1)).Trim())
                    .Where(serviceName => serviceName.StartsWith("test-service-"))
                    .ToArray()
            );

            serviceNames.ForEach(serviceName =>
            {
                TryDo(() => Run("sc", "stop", serviceName));
                TryDo(() => Run("sc", "delete", serviceName));
            });
        }
    }
}