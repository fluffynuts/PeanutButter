using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using PeanutButter.Utils;
using NExpect;
using PeanutButter.FileSystem;
using static NExpect.Expectations;

// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.WindowsServiceManagement.Core.Tests
{
    [TestFixture]
    [Explicit("Relies on a locally-installed RabbitMQ")]
    public class TestWindowsServiceUtil
    {
        private static readonly Guid TestServiceIdentifier = Guid.NewGuid();
        private static readonly string TestServiceName = $"test-service-{TestServiceIdentifier}";
        private static readonly string TestServiceDisplayName = $"test service {TestServiceIdentifier}";

        private static string _testServicePath;

        private static string TestServicePath =>
            _testServicePath ??= FindTestService();

        [Test]
        public void CanFindTestServiceFind()
        {
            // Arrange
            // Act
            Expect(TestServicePath)
                .To.Exist();
            // Assert
        }


        [TestFixture]
        public class Query
        {
            [OneTimeSetUp]
            public void OneTimeSetup()
            {
                EnsureTestServiceIsNotInstalled();
                InstallTestService();
            }

            [OneTimeTearDown]
            public void OneTimeTeardown()
            {
                EnsureTestServiceIsNotInstalled();
            }

            [Test]
            public void ShouldReflectTheServiceName()
            {
                // Arrange
                var sut = Create();
                // Act
                var result = sut.ServiceName;
                // Assert
                Expect(result)
                    .To.Equal(TestServiceName);
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
                    .To.Equal(TestServiceDisplayName);
            }

            [Test]
            public void ShouldReflectStoppedServiceState()
            {
                // Arrange
                EnsureTestServiceIsNotRunning();
                var sut = Create();
                // Act
                var result = sut.State;
                // Assert
                Expect(result)
                    .To.Equal(ServiceState.Stopped);
            }

            [Test]
            public void ShouldReflectStartedServiceState()
            {
                // Arrange
                EnsureTestServiceIsNotRunning();
                StartTestService();
                WaitFor(() => ReadCurrentTestServiceState() == ServiceState.Running);
                var sut = Create();
                // Act
                var result = sut.State;
                // Assert
                Expect(result)
                    .To.Equal(ServiceState.Running);
            }

            [Test]
            public void ShouldReflectPausedServiceState()
            {
                // Arrange
                EnsureTestServiceIsNotRunning();
                StartTestService();
                WaitFor(() => ReadCurrentTestServiceState() == ServiceState.Running);
                Run("sc", "pause", TestServiceName);
                WaitFor(() => ReadCurrentTestServiceState() == ServiceState.Paused);
                var sut = Create();
                // Act
                var result = sut.State;
                // Assert
                Expect(result)
                    .To.Equal(ServiceState.Paused);
            }

            [Test]
            public void ShouldSetAllowedTransitionForStoppedStateOfEnabledService()
            {
                // Arrange
                EnsureTestServiceIsNotRunning();
                var sut = Create();
                // Act
                var result = sut.AllowedTransitions;
                // Assert
                Expect(result)
                    .To.Equal(new[] { ServiceState.Running });
            }
        }

        [TestFixture]
        public class Control
        {
            [OneTimeSetUp]
            public void OneTimeSetup()
            {
                EnsureTestServiceIsNotInstalled();
                InstallTestService();
            }

            [OneTimeTearDown]
            public void OneTimeTeardown()
            {
                EnsureTestServiceIsNotInstalled();
            }

            [Test]
            public void ShouldStartTheStoppedService()
            {
                // Arrange
                EnsureTestServiceIsNotRunning();
                var sut = Create(TestServiceName);
                // Act
                sut.Start();
                // Assert
                Expect(sut.State)
                    .To.Equal(ServiceState.Running);
                Expect(ReadCurrentTestServiceState())
                    .To.Equal(ServiceState.Running);
            }

            [Test]
            public void ShouldNotThrowWhenAttemptingToStartServicePendingStart()
            {
                // Arrange
                EnsureTestServiceIsNotRunning();
                var sut = Create(TestServiceName);
                // Act
                sut.Start(wait: false);
                Expect(() => sut.Start())
                    .Not.To.Throw();
                Expect(() => sut.Start())
                    .Not.To.Throw();
                // Assert
                Expect(sut.State)
                    .To.Equal(ServiceState.Running);
            }

            [Test]
            public void ShouldStopTheStartedService()
            {
                // Arrange
                EnsureTestServiceIsNotRunning();
                var sut = Create(TestServiceName);
                sut.Start();
                
                // Act
                sut.Stop();
                
                // Assert
                Expect(sut.State)
                    .To.Equal(ServiceState.Stopped);
                Expect(ReadCurrentTestServiceState())
                    .To.Equal(ServiceState.Stopped);
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


        private static string FindTestService()
        {
            var myDir = Path.GetDirectoryName(
                new Uri(typeof(TestWindowsServiceUtil).Assembly.Location).LocalPath
            );
            var current = Path.GetDirectoryName(myDir);
            while (true)
            {
                var dirs = Directory.GetDirectories(current)
                    .Select(Path.GetFileName)
                    .ToArray();
                if (dirs.Contains("source"))
                {
                    throw new InvalidOperationException(
                        $"Can't find TestService source folder, traversing up from {myDir}"
                    );
                }

                if (dirs.Contains("TestService"))
                {
                    current = Path.Join(current, "TestService");
                    var fs = new LocalFileSystem(current);
                    var relative = fs.ListFilesRecursive("TestService.exe")
                        .FirstOrDefault() ?? throw new InvalidOperationException(
                        $"Can't find TestService.exe under {current}"
                    );
                    return Path.Combine(current, relative);
                }

                current = Path.GetDirectoryName(current) ?? throw new InvalidOperationException(
                    $"Unable to find 'source' or 'TestService' folder, travelling up from {myDir}"
                );
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

        private static void EnsureTestServiceIsNotRunning()
        {
            TryDo(() =>
            {
                Run("sc", "stop", TestServiceName);
                WaitFor(() => ReadCurrentTestServiceState() == ServiceState.Stopped);
            });
        }

        private static void StartTestService()
        {
            Run("sc", "start", TestServiceName);
            WaitFor(() => ReadCurrentTestServiceState() == ServiceState.Running);
        }

        private static void InstallTestService()
        {
            Run(TestServicePath,
                "-i",
                "--name", TestServiceName,
                "--display-name", TestServiceDisplayName,
                "--start-delay", "1000"
            );
        }

        private static ServiceState ReadCurrentTestServiceState()
        {
            using var io = ProcessIO.Start("sc", "query", TestServiceName);
            foreach (var line in io.StandardOutput)
            {
                var parts = line.Trim().Split(':')
                    .Select(p => p.Trim())
                    .ToArray();
                if (parts[0] == "STATE")
                {
                    var stateId = parts[1].Split(' ')
                        .First()
                        .AsInteger();
                    return (ServiceState) stateId;
                }
            }

            throw new InvalidOperationException(
                $"Can't determine the state of the test service"
            );
        }

        private static void WaitFor(
            Expression<Func<bool>> expr,
            int maxWaitSeconds = 10
        )
        {
            var start = DateTime.Now;
            var timeout = TimeSpan.FromSeconds(maxWaitSeconds);
            var fn = expr.Compile();
            while (DateTime.Now - start < timeout)
            {
                if (fn())
                {
                    return;
                }

                Thread.Sleep(100);
            }

            throw new TimeoutException($"Timed out waiting for condition: {expr}");
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