using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using NExpect;
using PeanutButter.Utils;
using TestService;
using static NExpect.Expectations;

// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.WindowsServiceManagement.Tests
{
    [TestFixture]
    public class TestWindowsServiceManagement
    {
        [Test]
        [Explicit("Run manually on known services")]
        public void ACCEPT_WhenTestingStartupStateOfKnownServices_ShouldReturnExpectedState()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var svc1 = new WindowsServiceUtil("mssqlserver");
            Expect(svc1.StartupType)
                .To.Equal(ServiceStartupTypes.Automatic);
            var svc2 = new WindowsServiceUtil("gupdatem");
            Expect(svc2.StartupType)
                .To.Equal(ServiceStartupTypes.Manual);

            //---------------Test Result -----------------------
        }

        [Test]
        public void ShouldNotBorkWhenNoService()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------
            var svc = new WindowsServiceUtil(RandomValueGen.GetRandomString(20, 30));
            //---------------Execute Test ----------------------
            Expect(svc.IsInstalled)
                .To.Be.False();

            //---------------Test Result -----------------------
        }

        [Test]
        [Explicit("manually testing some stuff")]
        public void InstallThing()
        {
            //---------------Set up test pack-------------------
            var localPath = TestServicePath;

            //---------------Assert Precondition----------------
            Expect(localPath)
                .To.Exist($"Expected to find test service at {localPath}");

            //---------------Execute Test ----------------------
            Run(localPath, "-i");
            var util = new WindowsServiceUtil("test service");
            Expect(util.IsInstalled).To.Be.True();

            //---------------Test Result -----------------------
        }


        [Test]
        [Explicit("Use to manually unregister test-service, if you need to (:")]
        public void ReinstallThing()
        {
            //---------------Set up test pack-------------------
            var util = new WindowsServiceUtil("test-service");

            //---------------Assert Precondition----------------
            Expect(util.IsInstalled)
                .To.Be.True();

            //---------------Execute Test ----------------------
            util.Uninstall(true);

            //---------------Test Result -----------------------
        }

        [Test]
        [Explicit("Run manually, may be system-specific")]
        public void ServiceExe_ShouldReturnPathTo()
        {
            //---------------Set up test pack-------------------
            var util = new WindowsServiceUtil("Themes");

            //---------------Assert Precondition----------------
            Expect(util.IsInstalled)
                .To.Be.True();

            //---------------Execute Test ----------------------
            var path = util.ServiceExe;

            //---------------Test Result -----------------------
            Expect(
                path?.ToLowerInvariant()
            ).To.Equal("c:\\windows\\system32\\svchost.exe -k netsvcs");
        }

        [Test]
        public void BigHairyIntegrationTest()
        {
            // Arrange
            var serviceExe = TestServicePath;
            Expect(serviceExe).To.Exist($"Expected to find test service at {serviceExe}");
            EnsureTestServiceIsNotInstalled();
            // Act
            Do("Install via cli", () => Run(serviceExe, "-i", "-n", TestServiceName));
            var util = new WindowsServiceUtil(TestServiceName);
            Do("Test is installed",
                () => Expect(util.IsInstalled)
                    .To.Be.True()
            );
            Do("Test service executable path",
                () =>
                    Expect(util.ServiceExe)
                        .To.Equal(serviceExe)
            );
            Do("Test default startup type",
                () =>
                    Expect(util.StartupType)
                        .To.Equal(ServiceStartupTypes.Automatic)
            );

            Do("Disabled service",
                () =>
                {
                    util.Disable();
                    Expect(util.StartupType)
                        .To.Equal(ServiceStartupTypes.Disabled);
                });
            Do("Re-enable service",
                () =>
                {
                    util.SetAutomaticStart();
                    Expect(util.StartupType)
                        .To.Equal(ServiceStartupTypes.Automatic);
                });

            Process process = null;
            Do("Start service",
                () =>
                {
                    util.Start();
                    Expect(util.State)
                        .To.Equal(ServiceState.Running);
                    process = Process.GetProcesses().FirstOrDefault(p =>
                    {
                        try
                        {
                            return p?.MainModule?.FileName.Equals(serviceExe, StringComparison.OrdinalIgnoreCase) ??
                                false;
                        }
                        catch
                        {
                            return false;
                        }
                    });
                    Expect(process).Not.To.Be.Null($"Service should be running: {serviceExe}");
                    Expect(process.Id)
                        .To.Equal(util.ServicePID);
                });

            var byPath = WindowsServiceUtil.GetServiceByPath(serviceExe);
            Expect(byPath)
                .Not.To.Be.Null($"Should be able to query service by path {serviceExe}");

            Do("Pause service",
                () =>
                {
                    byPath.Pause();
                    Expect(byPath.State)
                        .To.Equal(ServiceState.Paused);
                });

            Do("Resume service",
                () =>
                {
                    byPath.Continue();
                    Expect(byPath.State)
                        .To.Equal(ServiceState.Running);
                });

            Do("Stop service",
                () =>
                {
                    util.Stop();
                    Expect(util.State)
                        .To.Equal(ServiceState.Stopped);
                });

            Do("Uninstall via cli",
                () =>
                {
                    Run(serviceExe, "-u", "-n", TestServiceName);

                    Expect(util.IsInstalled)
                        .To.Be.False();
                });

            Do("Install and start (api)",
                () =>
                {
                    util.InstallAndStart();
                    Expect(util.IsInstalled)
                        .To.Be.True();
                    Expect(util.State)
                        .To.Equal(ServiceState.Running);
                });
            // Do("Uninstall (api)", () => util.Uninstall());
            // Assert
        }

        private static readonly string TestServiceName = $"test-service-{Guid.NewGuid()}";

        private void Do(
            string message,
            Action toRun,
            TimeSpan? maxWait = null)
        {
            maxWait = maxWait ?? TimeSpan.FromSeconds(5);

            var pad = 32 - message.Length - 7;
            var padString = "";
            if (pad > 0)
            {
                padString = new String(' ', pad);
            }

            Console.Out.Write($"{message}{padString}");
            Console.Out.Flush();
            try
            {
                TryRun(toRun, maxWait.Value);
                Console.Out.WriteLine(" [ OK ]");
            }
            catch
            {
                Console.Out.WriteLine(" [FAIL]");
                throw;
            }
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

            throw last ?? new InvalidOperationException($"No last exception captured");
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
            using (var proc = new Process())
            {
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
        }

        private static string TestServicePath =>
            Path.Combine(
                Path.GetDirectoryName(
                    new Uri(typeof(TestWindowsServiceManagement).Assembly.Location).LocalPath
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
            using (var io = new ProcessIO("sc", "query", "type=", "service"))
            {
                serviceNames.AddRange(
                    io.StandardOutput
                        .Select(line => line.Trim())
                        .Where(line => line.StartsWith("SERVICE_NAME"))
                        .Select(line => string.Join(":", line.Split(':').Skip(1)).Trim())
                        .Where(serviceName => serviceName.StartsWith("test-service-"))
                        .ToArray()
                );
            }

            serviceNames.ForEach(serviceName =>
            {
                TryDo(() => Run("sc", "stop", serviceName));
                TryDo(() => Run("sc", "delete", serviceName));
            });
        }
    }
}