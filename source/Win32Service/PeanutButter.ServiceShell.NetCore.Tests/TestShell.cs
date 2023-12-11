using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement;

namespace PeanutButter.ServiceShell.NetCore.Tests;

[TestFixture]
public class TestShell
{
    [TestFixture]
    public class NetCore
    {
        [TestFixture]
        [Explicit(SCM_FLAKY)]
        public class WhenServiceIsNotInstalled
        {
            [SetUp]
            public void OneTimeSetup()
            {
                EnsureTestServiceIsNotInstalled();
            }

            [TestCase("--uninstall")]
            [TestCase("-u")]
            [TestCase("--stop")]
            [TestCase("-x")]
            [TestCase("--start")]
            [TestCase("-s")]
            public void ShouldErrorWhenPassed_(string arg)
            {
                // Arrange
                // Act
                var exitCode = RunTestService(arg);
                // Assert
                Expect(exitCode)
                    .Not.To.Equal(0);

                var util = MakeTestServiceUtil();
                Expect(util.IsInstalled)
                    .To.Be.False();
            }

            [TestCase("--install")]
            [TestCase("-i")]
            public void ShouldInstallWhenPassed_(string arg)
            {
                // Arrange
                // Act
                var exitCode = RunTestService(arg);
                // Assert
                Expect(exitCode).To.Equal(0);
                var util = MakeTestServiceUtil();
                Expect(util.IsInstalled)
                    .To.Be.True();
            }
        }

        [TestFixture]
        // [Explicit(SCM_FLAKY)]
        public class WhenServiceInstalled
        {
            [SetUp]
            public void OneTimeSetup()
            {
                EnsureTestServiceIsInstalled();
            }

            [TearDown]
            public void OneTimeTeardown()
            {
                EnsureTestServiceIsNotInstalled();
            }


            [TestCase("--start")]
            [TestCase("-s")]
            public void ShouldStartTheServiceWhenPassed_(string arg)
            {
                // Arrange
                EnsureTestServiceIsStopped();
                // Act
                Console.Error.WriteLine("--- start ---");
                var exitCode = RunTestService(arg);
                // Assert
                Expect(exitCode)
                    .To.Equal(0);
                var util = MakeTestServiceUtil();
                Expect(util.State)
                    .To.Equal(ServiceState.Running);
            }

            [TestCase("--start")]
            [TestCase("-s")]
            public void ShouldErrorWhenServiceRunningAndWhenPassed_(string arg)
            {
                // Arrange
                EnsureTestServiceIsRunning();
                // Act
                var exitCode = RunTestService(arg);
                // Assert
                Expect(exitCode).Not.To.Equal(0);
            }

            [TestCase("--stop")]
            [TestCase("-x")]
            public void ShouldStopTheRunningServiceWhenPassed_(string arg)
            {
                // Arrange
                EnsureTestServiceIsRunning();
                // Act
                var exitCode = RunTestService(arg);
                // Assert
                Expect(exitCode).To.Equal(0);
                var util = MakeTestServiceUtil();
                Expect(util.State)
                    .To.Equal(ServiceState.Stopped);
            }

            [TestCase("--stop")]
            [TestCase("-x")]
            public void ShouldErrorWhenServiceNotRunningAndPassed_(string arg)
            {
                // Arrange
                EnsureTestServiceIsStopped();
                // Act
                var exitCode = RunTestService(arg);
                // Assert
                Expect(exitCode).Not.To.Equal(0);
            }
        }

        private static void EnsureTestServiceIsStopped()
        {
            var util = MakeTestServiceUtil();
            if (util.State == ServiceState.Running)
            {
                util.Stop(true);
            }
        }

        private static void EnsureTestServiceIsRunning()
        {
            var util = MakeTestServiceUtil();
            if (util.State != ServiceState.Running)
            {
                util.Start(true);
            }
        }


        private static void EnsureTestServiceIsInstalled()
        {
            var util = MakeTestServiceUtil();
            if (util.IsInstalled)
            {
                return;
            }

            if (RunTestService("--install") != 0)
            {
                Assert.Fail("Can't install test service");
            }

            WaitFor(() => util.IsInstalled, 2000);
            var state = util.State;
            var markedForDelete = util.IsMarkedForDelete;

            if (!util.IsInstalled)
            {
                Assert.Fail($"Unable to install service {TestServiceName}");
            }
        }

        private static void EnsureTestServiceIsNotInstalled()
        {
            var util = MakeTestServiceUtil();
            if (!util.IsInstalled)
            {
                return;
            }

            if (RunTestService("--uninstall") != 0)
            {
                Assert.Fail($"Can't uninstall test service: '{util.ServiceName}'");
            }

            WaitFor(() => !util.IsInstalled, 2000);

            if (util.IsInstalled)
            {
                Assert.Fail($"Test service NOT uninstalled: '{util.ServiceName}'");
            }
        }

        private static void WaitFor(
            Func<bool> condition,
            int maxMilliseconds
        )
        {
            var endAt = DateTime.Now.AddMilliseconds(maxMilliseconds);
            while (DateTime.Now > endAt && condition())
            {
            }
        }

        private static int RunTestService(params string[] args)
        {
            return Run(ServicePath, args);
        }

        private static int Run(
            string program,
            params string[] args
        )
        {
            using var io = ProcessIO.Start(
                program,
                args.And("--name", TestServiceName)
            );
            io.StandardOutput.ForEach(
                line =>
                    Console.WriteLine($"stdout: {line}")
            );
            io.StandardError.ForEach(
                line => Console.WriteLine($"stderr: {line}")
            );
            if (io.ExitCode != 0)
            {
                Console.Error.WriteLine(
                    string.Join(
                        "\n",
                        io.StandardOutputAndErrorInterleavedSnapshot
                    )
                );
            }

            return io.ExitCode;
        }

        private static WindowsServiceUtil MakeTestServiceUtil()
        {
            return new WindowsServiceUtil(TestServiceName);
        }

        private static string ServiceContainerFolder = Path.GetDirectoryName(
            new Uri(typeof(TestShell).GetAssembly().Location).LocalPath
        );

        private static string ServicePath = Path.Combine(
            ServiceContainerFolder,
            "DotNetService.exe"
        );

        [SetUp]
        public void Setup()
        {
            TestServiceName = $"dotnet-test-service-{Guid.NewGuid()}";
        }

        private static string TestServiceName = $"dotnet-test-service-{Guid.NewGuid()}";
    }

    private const string SCM_FLAKY = "Flaky when run from cli because SCM sucks";
}