using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using NUnit.Framework;
using NExpect;
using PeanutButter.FileSystem;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement.Exceptions;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using TimeoutException = System.TimeoutException;

// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.WindowsServiceManagement.Tests
{
    // TODO: delayed startup for native windows service util?
    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class TestNativeWindowsServiceUtil : TestWindowsServiceUtilBase
    {
        public TestNativeWindowsServiceUtil()
            : base(typeof(NativeWindowsServiceUtil))
        {
        }
        [Test]
        [Explicit("Requires windows environment and knowledge of a running service pid")]
        public void ShouldBeAbleToLoadServiceByPid()
        {
            // Arrange
            var pid = 5932;
            var expected = "RabbitMQ";
            // Act
            var result = NativeWindowsServiceUtil.GetServiceByPid(pid);
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.ServiceName)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    [Parallelizable(ParallelScope.None)]
    public class TestWindowsServiceUtil : TestWindowsServiceUtilBase
    {
        public TestWindowsServiceUtil()
            : base(typeof(WindowsServiceUtil))
        {
        }
        
        [Test]
        [Explicit("Requires windows environment and knowledge of a running service pid")]
        public void ShouldBeAbleToLoadServiceByPid()
        {
            // Arrange
            var pid = 5932;
            var expected = "RabbitMQ";
            // Act
            var result = WindowsServiceUtil.GetServiceByPid(pid);
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.ServiceName)
                .To.Equal(expected);
        }

    }

    public abstract class TestWindowsServiceUtilBase
    {
        public TestWindowsServiceUtilBase(Type implementationType)
        {
            _implementationType = implementationType;
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var scm = Win32Api.OpenSCManager(null, null, ScmAccessRights.Connect);
            if (scm == IntPtr.Zero)
            {
                if (Platform.IsWindows)
                {
                    Assert.Ignore("Cannot open SCM (these tests must be run elevated)");
                }
                else
                {
                    Assert.Ignore("These tests are windows-specific");
                }

                return;
            }

            Win32Api.CloseServiceHandle(scm);
        }

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
            var svc = new WindowsServiceUtil(GetRandomString(20, 30));
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
        [Explicit("Requires a windows world")]
        public void DelayedAutoStart()
        {
            // Arrange
            var sut = Create(TestServiceName, "test service", TestServicePath);
            // Act
            if (sut.IsInstalled)
            {
                sut.Uninstall();
            }

            try
            {
                sut.Install(ServiceStartupTypes.DelayedAutomatic);
                var check = new WindowsServiceUtil(TestServiceName);
                Expect(check.StartupType)
                    .To.Equal(ServiceStartupTypes.DelayedAutomatic);
            }
            finally
            {
                sut.Uninstall();
            }
            // Assert
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

        [TestCase("-m")]
        [TestCase("--manual-start")]
        public void ShouldBeAbleToInstallServiceAsManualStart(string arg)
        {
            // Arrange
            var serviceExe = TestServicePath;
            Expect(serviceExe)
                .To.Exist($"Expected to find test service at {serviceExe}");
            EnsureTestServiceIsNotInstalled();
            // Act
            Do("Install via cli: manual start",
                () => Run(serviceExe, "--install", arg, "--name", TestServiceName)
            );
            var util = new WindowsServiceUtil(TestServiceName);
            // Assert
            Expect(util.StartupType)
                .To.Equal(ServiceStartupTypes.Manual);
            Expect(util.State)
                .To.Equal(ServiceState.Stopped);
        }

        [Test]
        public void ShouldBeAbleToInstallServiceAsManualStartViaApi()
        {
            // Arrange
            var serviceExe = TestServicePath;
            Expect(serviceExe)
                .To.Exist($"Expected to find test service at {serviceExe}");
            EnsureTestServiceIsNotInstalled();
            var sut = Create(TestServiceName, TestServiceName, serviceExe);
            // Act
            // sut.Install(ServiceBootFlag.ManualStart);
            sut.Install(ServiceStartupTypes.Manual);
            // Assert
            var util = Create(TestServiceName);
            Expect(util.StartupType)
                .To.Equal(ServiceStartupTypes.Manual);
            Expect(util.State)
                .To.Equal(ServiceState.Stopped);
        }

        [TestCase("-z")]
        [TestCase("--disabled")]
        public void ShouldBeAbleToInstallServiceAsDisabled(string arg)
        {
            // Arrange
            var serviceExe = TestServicePath;
            Expect(serviceExe)
                .To.Exist($"Expected to find test service at {serviceExe}");
            EnsureTestServiceIsNotInstalled();
            // Act
            Do("Install via cli: manual start",
                () => Run(serviceExe, "--install", arg, "--name", TestServiceName)
            );
            var util = new WindowsServiceUtil(TestServiceName);
            // Assert
            Expect(util.StartupType)
                .To.Equal(ServiceStartupTypes.Disabled);
        }

        [Test]
        // may fail when running in parallel
        [Retry(3)]
        public void BigHairyIntegrationTest()
        {
            if (Platform.Is32Bit)
            {
                Assert.Ignore(
                    "Running 32-bit: test will fail: 32-bit process cannot access 64-bit process info"
                );
            }

            // Assert.That(() =>
            {
                // Arrange
                var serviceExe = TestServicePath;
                Expect(serviceExe).To.Exist($"Expected to find test service at {serviceExe}");
                EnsureTestServiceIsNotInstalled();
                // Act
                Do("Install via cli", () => Run(serviceExe, "--install", "--name", TestServiceName));
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
                        var processes = Process.GetProcesses();
                        var process = processes.FirstOrDefault(p =>
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
                        Expect(process).Not.To.Be.Null(
                            () =>
                                $"@Service should be running: {serviceExe}\nrunning:\n{DumpProcesses()}"
                        );
                        Expect(process.Id)
                            .To.Equal(util.ServicePID);

                        string DumpProcesses()
                        {
                            return processes.Select(p =>
                            {
                                try
                                {
                                    return $"{p.MainModule.FileName}";
                                }
                                catch (Exception e)
                                {
                                    return $"(unknown) ({e.Message})";
                                }
                            }).OrderBy(s => s).JoinWith("\n");
                            ;
                        }
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
                        Run(serviceExe, "--uninstall", "--name", TestServiceName);

                        if (util is WindowsServiceUtil u)
                        {
                            u.Refresh();
                        }

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
                Do("Re-install and start (api)",
                    () =>
                    {
                        util = new WindowsServiceUtil(
                            TestServiceName,
                            TestServiceName,
                            serviceExe);
                        util.Uninstall();
                        util.InstallAndStart();
                        Expect(util.IsInstalled)
                            .To.Be.True();
                        Expect(util.State)
                            .To.Equal(ServiceState.Running);
                    });
                Do("Uninstall (api)", () => util.Uninstall());
                // Assert
            }
            //, Throws.Nothing);
        }

        [Test]
        // [Explicit("Slow and flaky")]
        public void KillService_ShouldKillSingleWithoutArgs()
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
                .To.Equal(KillServiceResult.Killed);
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
        // [Explicit("Slow and flaky")]
        public void KillService_ShouldKillTheCorrectService()
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

        [Test]
        public void StubbornService_ShouldBeAbleToSuccessfullyUninstallWithForceOption()
        {
            // by definition, if this fails, it may require manual cleanup
            // -> stubborn-service will basically lock for a minute
            //    when requested to stop
            // Arrange
            var serviceExe = StubbornServiceExe;
            var serviceName = $"stubborn-service-{Guid.NewGuid()}";
            var util = Create(
                serviceName,
                serviceName,
                serviceExe);
            // Act
            util.InstallAndStart();
            var servicePid = util.ServicePID;
            util.Uninstall(
                ControlOptions.Wait |
                ControlOptions.Force
            );
            // Assert
            Expect(() => Process.GetProcessById(servicePid))
                .To.Throw<ArgumentException>(); // shouldn't be running
            var anotherUtil = Create(serviceName);
            Expect(anotherUtil.IsInstalled)
                .To.Be.False();
            Expect(anotherUtil.IsMarkedForDelete)
                .To.Be.False();
        }

        [Test]
        public void StubbornService_ShouldNotSuccessfullyUninstallWithDefaultUninstallMethod()
        {
            // by default, Uninstall _should_ wait for the stop / uninstall
            // but _not_ kill (in case of loss of data)
            // Arrange
            var serviceExe = StubbornServiceExe;
            var serviceName = $"stubborn-service{Guid.NewGuid()}";
            var sut = Create(
                serviceName,
                serviceName,
                serviceExe
            );
            using var _ = new AutoResetter(Noop, () => sut.Uninstall(
                ControlOptions.Force | ControlOptions.Wait
            ));
            sut.InstallAndStart();
            // Act
            if (sut is NativeWindowsServiceUtil u)
            {
                u.ServiceStateExtraWaitSeconds = 0;
                Expect(() => sut.Uninstall())
                    .To.Throw<ServiceOperationException>()
                    .With.Message.Containing("Unable to perform Stop");
            }
            else if (sut is WindowsServiceUtil u2)
            {
                u2.ServiceControlTimeoutSeconds = 5;
                Expect(() => sut.Uninstall())
                    .To.Throw<TimeoutException>()
                    .With.Message.Containing("state: Stopped");
            }

            // Assert
            Expect(sut.IsInstalled)
                .To.Be.True();
            Process process = null;
            Expect(() => process = Process.GetProcessById(sut.ServicePID))
                .Not.To.Throw();
            Expect(process.HasExited)
                .To.Be.False();
        }

        [Test]
        // [Explicit("Slow and flaky")]
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
            EnsureTestServiceIsNotInstalled();
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

        [Test]
        public void ShouldFailToOverwriteExistingService()
        {
            // Arrange
            var serviceName = GetRandomString(10);
            using var _ = new AutoResetter(() =>
            {
                var util = new WindowsServiceUtil(serviceName);
                util.Uninstall();
            });
            var displayName1 = "PB unit test service #1";
            var cli1 = $"{TestServicePath} a b c";
            var sut1 = new WindowsServiceUtil(
                serviceName,
                displayName1,
                cli1
            );
            var displayName2 = "PB unit test service #2";
            var cli2 = $"{TestServicePath} 1 2 3";
            sut1.Install();
            var check1 = new WindowsServiceUtil(serviceName);
            Expect(check1.IsInstalled)
                .To.Be.True();
            Expect(check1.DisplayName)
                .To.Equal(displayName1);
            Expect(check1.ServiceExe)
                .To.Equal(TestServicePath);
            Expect(check1.Arguments)
                .To.Equal(new[] { "a", "b", "c" });

            // Act
            var sut2 = new WindowsServiceUtil(
                serviceName,
                displayName2,
                cli2
            );
            Expect(() => sut2.Install())
                .To.Throw();

            // Assert
        }

        [Test]
        public void ShouldNotThrowForUnknownService()
        {
            // Arrange
            // Act
            var sut = Create(GetRandomString(20));
            // Assert
            Expect(sut.IsInstalled)
                .To.Be.False();
        }

        private static string SpacedServiceExe =>
            _spacedServiceExe ??= FindPath(
                "SpacedService",
                "SpacedService.exe",
                p => !p.Contains("obj")
            );

        private static string StubbornServiceExe =>
            _stubbornServiceExe ??= FindPath(
                "StubbornService",
                "StubbornService.exe",
                p => !p.Contains("obj")
            );

        private static string _stubbornServiceExe;

        private static string _spacedServiceExe;

        private Guid SpacedServiceIdentifier =>
            _spacedServiceIdentifier ??= Guid.NewGuid();

        private Guid? _spacedServiceIdentifier;

        private string SpacedServiceName =>
            _spacedServiceName ??= $"SpacedService-{SpacedServiceIdentifier}";

        private string SpacedServiceDisplayName => $"Spaced Service {SpacedServiceIdentifier}";

        private string _spacedServiceName;

        [Test]
        // [Explicit("Slow and flaky")]
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
            if (sut is NativeWindowsServiceUtil native)
            {
                native.ServiceStateExtraWaitSeconds = 30;
            }
            else if (sut is WindowsServiceUtil xplat)
            {
                xplat.ServiceControlTimeoutSeconds = 60;
            }

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

        [Test]
        public void WhenServiceNotFound()
        {
            // Arrange
            var sut = Create(Guid.NewGuid().ToString());
            // Act
            Expect(sut.State)
                .To.Equal(ServiceState.NotFound);
            // Assert
        }

        private static void Noop()
        {
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

        private static Type _implementationType;

        private static IWindowsServiceUtil Create(string serviceName)
        {
            if (_implementationType is null)
            {
                throw new InvalidOperationException("No implementation set");
            }

            return (IWindowsServiceUtil) (Activator.CreateInstance(_implementationType, serviceName));
        }

        private static IWindowsServiceUtil Create(
            string serviceName,
            string displayName,
            string commandline)
        {
            if (_implementationType is null)
            {
                throw new InvalidOperationException("No implementation set");
            }

            return (IWindowsServiceUtil) (Activator.CreateInstance(_implementationType, serviceName, displayName,
                commandline));
        }

        private static string FindPath(
            string travelUpToFindFolder,
            string searchForThisFile,
            Func<string, bool> extraMatcher)
        {
            var current = Path.GetDirectoryName(
                new Uri(
                    typeof(TestWindowsServiceUtilBase)
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
            // using var proc = new Process();
            // var si = proc.StartInfo;
            // si.FileName = program;
            // si.Arguments = args.Select(QuoteIfNecessary).JoinWith(" ");
            // si.RedirectStandardError = true;
            // si.RedirectStandardInput = true;
            // si.RedirectStandardOutput = true;
            // si.CreateNoWindow = true;
            // si.UseShellExecute = false;
            //
            // if (!proc.Start())
            // {
            //     throw new ApplicationException($"Can't start {program} {args.JoinWith(" ")}");
            // }
            //
            // proc.WaitForExit();
            // if (proc.ExitCode != 0)
            // {
            //     throw new Exception($"{program} {args.JoinWith(" ")}\nexited with code {proc.ExitCode}");
            // }
            
            using var io = ProcessIO.Start(program, args);
            io.WaitForExit();
            if (io.ExitCode != 0)
            {
                var stderr = io.StandardError.ToArray();
                var stdout = io.StandardError.ToArray();
                var output = new List<string>();
                if (stderr.Any())
                {
                    output.Add("stderr:");
                    output.AddRange(stderr);
                }

                if (stdout.Any())
                {
                    output.Add("stdout:");
                    output.AddRange(stdout);
                }

                throw new Exception($"{program} {args.JoinWith(" ")}\nexited with code {io.ExitCode}\n{output.JoinWith("\n")}");
            }
        }

        private static string TestServicePath =>
            Path.Combine(
                Path.GetDirectoryName(
                    new Uri(typeof(TestWindowsServiceUtilBase).Assembly.Location).LocalPath
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

    // TODO: this might be useful in PB.Utils; however this very variant is
    // super-windows-specific, so one needs to come up with a better cross-platform
    // variant; perhaps 'ps' for *nix?
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