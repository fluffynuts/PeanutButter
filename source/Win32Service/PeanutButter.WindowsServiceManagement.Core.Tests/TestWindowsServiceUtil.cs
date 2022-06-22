using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PeanutButter.Utils;
using NExpect;
using static NExpect.Expectations;

// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.WindowsServiceManagement.Core.Tests
{
    [TestFixture]
    [Explicit("Relies on a locally-installed RabbitMQ")]
    public class TestWindowsServiceUtil
    {
        private static readonly string TestServiceName = $"test-service-{Guid.NewGuid()}";

        private static string TestServicePath =>
            Path.Combine(
                Path.GetDirectoryName(
                    new Uri(typeof(TestWindowsServiceUtil).Assembly.Location).LocalPath
                ), "TestService.exe"
            );

        [TestFixture]
        [Explicit("WIP")]
        public class Query
        {
            [SetUp]
            public void Setup()
            {
                EnsureTestServiceIsNotInstalled();
            }

            [TearDown]
            public void Teardown()
            {
                EnsureTestServiceIsNotInstalled();
            }

            [Test]
            public void ShouldReflectTheServiceName()
            {
                // Arrange
                InstallTestService();
                var sut = Create();
                // Act
                var result = sut.ServiceName;
                // Assert
                Expect(result)
                    .To.Equal("fix me: use test service");
            }

            [Test]
            public void ShouldReflectServiceDisplayName()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.DisplayName;
                // Assert
                Expect(result)
                    .To.Equal("fix me: set and use test service display name");
            }
        }

        private static void InstallTestService(
            string displayName = null
        )
        {
            var args = new List<string>();
            if (displayName is not null)
            {
                args.AddRange(
                    new[] { "--display-name", QuoteIfNecessary(displayName) }
                );
                
            }
        }

        private static IWindowsServiceUtil Create(
            string serviceName = null
        )
        {
            return new WindowsServiceUtil(
                serviceName ?? TestServiceName
            );
        }


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

        private static void Run(
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


        private static string QuoteIfNecessary(string arg)
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

        private static void EnsureTestServiceIsNotInstalled()
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

        private void EnsureServiceNotRunning(string name, string exe)
        {
            // big hammer to enforce service non-existence at exit

            // attempt stop
            using var stop = ProcessIO.Start("sc", "stop", name);
            stop.StandardOutput.ForEach(Log);
            stop.StandardError.ForEach(Log);

            // delete service
            using var delete = ProcessIO.Start("sc", "delete", name);
            delete.StandardOutput.ForEach(Log);
            delete.StandardError.ForEach(Log);

            // kill remaining process
            var proc = Process.GetProcesses()
                .Where(p =>
                {
                    try
                    {
                        return p.StartInfo.FileName.Equals(exe, StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        return false;
                    }
                })
                .ToArray();
            proc.ForEach(p =>
            {
                try
                {
                    p.Kill();
                }
                catch
                {
                    // may already be dead
                }
            });

            void Log(string s)
            {
                Console.WriteLine(s);
            }
        }
    }
}