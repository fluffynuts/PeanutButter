using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using NExpect;
using PeanutButter.FileSystem;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement.Exceptions;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.WindowsServiceManagement.Tests
{
    [TestFixture]
    public class TestWindowsServiceUtil
    {
        [Test]
        [Explicit("Relies on local installation of mysql 5.7")]
        public void ServiceExeShouldReturnServiceExe()
        {
            // Arrange
            var service = new WindowsServiceUtil("MYSQL57");
            // Act
            var result = service.Commandline;
            // Assert
        }

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

            // check that we can uninstall
            util.Uninstall(true);
            Expect(util.IsInstalled)
                .To.Be.False();
            // check that attempting again throws
            Expect(() => util.Uninstall(true))
                .To.Throw<ServiceNotInstalledException>();

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
            var path = util.Commandline;

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
            Do("Test service commandline",
                () => Expect(util.Commandline)
                    .To.Equal(serviceExe)
            );
            Do("Test default startup type",
                () => Expect(util.StartupType)
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

            Do("Start service",
                () =>
                {
                    util.Start();
                    Expect(util.State)
                        .To.Equal(ServiceState.Running);
                    var process = Process.GetProcesses().FirstOrDefault(p =>
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
                    util = new WindowsServiceUtil(
                        TestServiceName,
                        TestServiceName,
                        serviceExe);
                    util.InstallAndStart();
                    Expect(util.IsInstalled)
                        .To.Be.True();
                    Expect(util.State)
                        .To.Equal(ServiceState.Running);
                });
            Do("Uninstall (api)", () => util.Uninstall());
            // Assert
        }

        [TestFixture]
        public class KillService : TestWindowsServiceUtil
        {
            [Test]
            public void ShouldKillSingleWithoutArgs()
            {
                // Arrange
                var util = Create(
                    TestServiceName,
                    TestServiceName,
                    TestServicePath
                );
                util.InstallAndStart();
                // Act
                var allProcesses = Process.GetProcesses();
                var proc = allProcesses
                    .Single(p => HasMainModule(p, TestServicePath));
                var result = util.KillService();
                // Assert
                Expect(proc.HasExited)
                    .To.Be.True();
                Expect(result)
                    .To.Be.True();
            }

            private static bool HasMainModule(
                Process p,
                string path)
            {
                try
                {
                    return p.MainModule.FileName == path;
                }
                catch // easy to get access denied!
                {
                    return false;
                }
            }

            // Spaced Service doesn't attempt to do anything with arguments, so it's
            //    nice for testing this out

            [Test]
            public void ShouldKillTheCorrectService()
            {
                // Arrange
                var service1Name = $"service-1-{Guid.NewGuid()}";
                var service2Name = $"service-2-{Guid.NewGuid()}";
                var args1 = GetRandomArray<string>(2);
                var args2 = GetRandomArray<string>(2);
                var cli1 = $"\"{SpacedServiceExe}\" {string.Join(" ", args1)}";
                var cli2 = $"\"{SpacedServiceExe}\" {string.Join(" ", args2)}";
                var util1 = Create(service1Name, service1Name, cli1);
                var util2 = Create(service2Name, service2Name, cli2);
                util1.InstallAndStart();
                util2.InstallAndStart();

                using var _ = new AutoResetter(Noop, () =>
                {
                    TryDo(() => util1.Uninstall());
                    TryDo(() => util2.Uninstall());
                });

                var processes = Process.GetProcesses()
                    .Where(p => HasMainModule(p, SpacedServiceExe))
                    .ToArray();
                var p1 = processes.FirstOrDefault(
                    p =>
                    {
                        var testArgs = p.QueryCommandline()
                            .SplitCommandline()
                            .Skip(1)
                            .ToArray();
                        return testArgs.Matches(args1);
                    }
                );
                var p2 = processes.FirstOrDefault(
                    p => p.QueryCommandline()
                        .SplitCommandline()
                        .Skip(1)
                        .Matches(args2)
                );

                Expect(p1)
                    .Not.To.Be.Null(() => $"Can't find process for {cli1}");
                Expect(p2)
                    .Not.To.Be.Null(() => $"Can't find process for {cli2}");

                // Act
                util1.KillService();

                // Assert
                Expect(p1.HasExited)
                    .To.Be.True();
                Expect(p2.HasExited)
                    .To.Be.False();
            }
        }

        [Test]
        [Explicit("Slow; requires SCM")]
        public void ServiceWithArgs()
        {
            // Arrange
            var myPath = new Uri(GetType().Assembly.Location).LocalPath;
            var myDir = Path.GetDirectoryName(myPath);
            var serviceExe = Path.Combine(myDir, "ServiceWithArgs.exe");
            var logFile = Path.Combine(myDir, "service.log");
            var arg1 = GetRandomString(3);
            var arg2 = GetRandomString(3);
            var args = new[] { logFile, arg1, arg2 }.Select(p => p.QuoteIfSpaced());
            var serviceName = "test-service-foo-bar";
            var displayName = "Test Service - Foo,Bar";
            var commandline = string.Join(
                " ",
                new[] { serviceExe }
                    .Concat(args)
            );
            var util = new WindowsServiceUtil(
                serviceName,
                displayName,
                commandline
            );
            if (util.IsInstalled)
            {
                util.Uninstall();
            }

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }

            // Act
            using var resetter = new AutoResetter(
                () => util.Install(),
                () => util.Uninstall(true));
            util.Install();
            // Assert
            Expect(util.IsInstalled)
                .To.Be.True();
            util.Start();
            Expect(util.State)
                .To.Equal(ServiceState.Running);

            Expect(logFile)
                .To.Exist();
            var logData = TryReadAllLinesFrom(logFile);
            Expect(logData)
                .Not.To.Be.Empty();
            Expect(logData[0])
                .To.Contain(arg1)
                .Then(arg2);

            util.Stop();

            Expect(util.State)
                .To.Equal(ServiceState.Stopped);

            var anotherUtil = Create(serviceName);
            Expect(anotherUtil.Commandline)
                .To.Equal(commandline);
            Expect(anotherUtil.ServiceExe)
                .To.Equal(serviceExe);
            Expect(anotherUtil.DisplayName)
                .To.Equal(displayName);
        }

        private string SpacedServiceExe =>
            _spacedServiceExe ??= FindPath(
                "SpacedService",
                "SpacedService.exe",
                p => !p.Contains("obj")
            );

        private string _spacedServiceExe;

        private Guid SpacedServiceIdentifier =>
            _spacedServiceIdentifier ??= Guid.NewGuid();

        private Guid? _spacedServiceIdentifier;

        private string SpacedServiceName =>
            _spacedServiceName ??= $"SpacedService-{SpacedServiceIdentifier}";

        private string SpacedServiceDisplayName => $"Spaced Service {SpacedServiceIdentifier}";

        private string _spacedServiceName;

        [Test]
        public void InstallingServiceWithSpacedPath()
        {
            // Arrange
            Expect(SpacedServiceExe)
                .Not.To.Be.Null();
            var sut = Create(
                SpacedServiceName,
                SpacedServiceDisplayName,
                $"\"{SpacedServiceExe}\" --arg --another-arg"
            );
            sut.ServiceStateExtraWaitSeconds = 30;
            // Act
            var completed = false;
            using var _ = new AutoResetter(Noop, () =>
            {
                if (completed)
                {
                    return;
                }

                EnsureServiceNotRunning(
                    SpacedServiceName,
                    SpacedServiceExe
                );
            });
            Expect(sut.Install)
                .Not.To.Throw();
            // Assert
            Expect(sut.Start)
                .Not.To.Throw();
            Expect(sut.State)
                .To.Equal(ServiceState.Running);
            sut.Stop();
            Expect(sut.State)
                .To.Equal(ServiceState.Stopped);
            sut.Uninstall();
            // signal the safety-net that it doesn't have to do anything
            completed = true;
        }

        private static void Noop()
        {
        }

        private void EnsureServiceNotRunning(string name, string exe)
        {
            // big hammer to enforce service non-existence at exit

            // attempt stop
            using var stop = new ProcessIO("sc", "stop", name);
            stop.StandardOutput.ForEach(Log);
            stop.StandardError.ForEach(Log);

            // delete service
            using var delete = new ProcessIO("sc", "delete", name);
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

        private WindowsServiceUtil Create(string serviceName)
        {
            return new WindowsServiceUtil(serviceName);
        }

        private WindowsServiceUtil Create(
            string serviceName,
            string displayName,
            string commandline)
        {
            return new WindowsServiceUtil(
                serviceName,
                displayName,
                commandline
            );
        }

        private string FindPath(
            string travelUpToFindFolder,
            string searchForThisFile,
            Func<string, bool> extraMatcher)
        {
            var current = Path.GetDirectoryName(
                new Uri(
                    GetType()
                        .Assembly
                        .Location
                ).LocalPath
            );
            var start = current;
            string test;
            do
            {
                current = Path.GetDirectoryName(current);
                if (current is null)
                {
                    throw new InvalidOperationException(
                        $"Can't find {travelUpToFindFolder} traversing up from {start}"
                    );
                }

                test = Path.Combine(current, travelUpToFindFolder);
            } while (!Directory.Exists(test));

            var fs = new LocalFileSystem(test);
            var matches = fs.ListRecursive(searchForThisFile)
                .Select(p => Path.Combine(test, p))
                .ToArray();
            return matches.FirstOrDefault(extraMatcher);
        }


        private string[] TryReadAllLinesFrom(string path)
        {
            var attempts = 0;
            do
            {
                try
                {
                    return File.ReadAllLines(path);
                }
                catch
                {
                    Thread.Sleep(100);
                }
            } while (++attempts < 10);

            throw new Exception($"Can't read from file: {path}");
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
            using (var io = new ProcessIO("sc", "query", "type=", "service", "state=", "all"))
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

    public static class ProcessExtensions
    {
        public static string QueryCommandline(
            this Process proc)
        {
            if (proc == null || proc.HasExited || proc.Id < 1)
            {
                return ""; // nothing we can do
            }

            var query = $"select CommandLine from Win32_Process where ProcessId={proc.Id}";
            var searcher = new ManagementObjectSearcher(query);
            var collection = searcher.Get();
            foreach (var o in collection)
            {
                return o["CommandLine"]?.ToString() ?? "";
            }

            return "";
        }
    }
}