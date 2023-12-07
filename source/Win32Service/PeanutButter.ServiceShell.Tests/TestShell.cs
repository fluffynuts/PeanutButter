using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using NSubstitute;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement;
using static NExpect.Expectations;

namespace PeanutButter.ServiceShell.Tests
{
    [TestFixture]
    public class TestShell
    {
        [TestFixture]
        public class TestingThatArgsArePassedInCorrectly
        {
            [Test]
            public void WhenRunMainCalledWithMatchingTypeAndParameters_ShouldNotThrow()
            {
                //---------------Set up test pack-------------------
                Shell.StartTesting();
                var args = GetRandomCollection<string>(2, 3).ToArray();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Program.Main(args);
                //---------------Test Result -----------------------
                Expect(() => Shell.ShouldHaveRunMainFor<SomeService>(args))
                    .Not.To.Throw();
            }

            [Test]
            public void WhenRunMainCalledWithMisMatchingTypeAndMatchingParameters_ShouldThrow()
            {
                //---------------Set up test pack-------------------
                Shell.StartTesting();
                var args = GetRandomCollection<string>(2, 3).ToArray();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Program.Main(args);
                //---------------Test Result -----------------------
                Expect(() => Shell.ShouldHaveRunMainFor<AnotherService>(args))
                    .To.Throw<ShellTestFailureException>();
            }

            [Test]
            public void WhenRunMainCalledWithMatchingTypeAndDifferentParameters_ShouldThrow()
            {
                //---------------Set up test pack-------------------
                Shell.StartTesting();
                var args = GetRandomCollection<string>(2, 3).ToArray();
                var otherArgs = GetRandomCollection<string>(args.Length, args.Length).ToArray();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Program.Main(args);
                //---------------Test Result -----------------------
                Expect(() => Shell.ShouldHaveRunMainFor<SomeService>(otherArgs))
                    .To.Throw<ShellTestFailureException>();
            }

            [Test]
            public void WhenNotInTestMode_ShouldThrowException()
            {
                //---------------Set up test pack-------------------
                var args = new[] { "-v" };

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Program.Main(args);

                //---------------Test Result -----------------------
                Expect(() => Shell.ShouldHaveRunMainFor<SomeService>(args))
                    .To.Throw<ShellTestFailureException>();
            }
        }

        [TestFixture]
        public class WhenPaused
        {
            [Test]
            public void ShouldNotCallRunOnceFromRun()
            {
                // Arrange
                using var sut = new SomeService();
                var method = sut.GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(mi => mi.Name == "Run" && mi.GetParameters().Length == 0);
                Expect(method)
                    .Not.To.Be.Null();
                Expect(sut.Get<bool>("Paused"))
                    .To.Be.False();
                // fake that we're running
                sut.Set("Running", true);
                Expect(sut.RunCount)
                    .To.Equal(0);
                var barrier = new Barrier(2);
                Task.Run(
                    () =>
                    {
                        barrier.SignalAndWait();
                        sut.InvokeMethodWithResult("Run");
                    }
                );
                // Act
                barrier.SignalAndWait();
                while (sut.RunCount < 1)
                {
                    Thread.Sleep(1);
                }

                sut.InvokeMethodWithResult("OnPause");
                var before = sut.RunCount;
                Expect(before)
                    .To.Be.Greater.Than(0);
                Expect(sut.Get<bool>("Paused"))
                    .To.Be.True();
                Task.Run(() => sut.InvokeMethodWithResult("Run"));
                
                // Assert
                Expect(sut.RunCount)
                    .To.Equal(before);
                
            }

            public class SomeService: Shell
            {
                public int RunCount { get; private set; }
                private readonly ILog _logger;

                public SomeService()
                {
                    DisplayName = nameof(SomeService);
                    ServiceName = nameof(SomeService);
                    _logger = Substitute.For<ILog>();
                }

                protected override void RunOnce()
                {
                    RunCount++;
                }

                protected override void Dispose(bool disposing)
                {
                    OnStop();
                    if (Platform.IsWindows)
                    {
                        base.Dispose(disposing);
                    }
                }

                protected override ILog GetLogger()
                {
                    return _logger;
                }
            }
        }

        private const string SCM_FLAKY = "Flaky when run from cli because SCM sucks";
        [TestFixture]
        [Explicit(SCM_FLAKY)]
        public class CommandLine
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
                    Expect(exitCode).Not.To.Equal(0);
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
            [Explicit(SCM_FLAKY)]
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
                    var exitCode = RunTestService(arg);
                    // Assert
                    Expect(exitCode).To.Equal(0);
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
                int maxMilliseconds)
            {
                var endAt = DateTime.Now.AddMilliseconds(maxMilliseconds);
                while (DateTime.Now > endAt && condition())
                {
                }
            }

            private static int RunTestService(params string[] args)
            {
                return Run(TestService, args);
            }

            private static int Run(
                string program,
                params string[] args)
            {
                using var io = ProcessIO.Start(
                    program,
                    args.And("-n", TestServiceName));
                io.StandardOutput.ForEach(line =>
                    Console.WriteLine($"stdout: {line}")
                );
                io.StandardError.ForEach(
                    line => Console.WriteLine($"stderr: {line}")
                );
                return io.ExitCode;
            }

            private static WindowsServiceUtil MakeTestServiceUtil()
            {
                return new WindowsServiceUtil(TestServiceName);
            }

            private static string MyFolder = Path.GetDirectoryName(
                new Uri(typeof(TestShell).GetAssembly().Location).LocalPath
            );

            private static string TestService = Path.Combine(
                MyFolder,
                "TestService.exe"
            );

            private static string TestServiceName = $"test-service-{Guid.NewGuid()}";
            [SetUp]
            public void Setup()
            {
                TestServiceName = $"test-service-{Guid.NewGuid()}";
            }
        }
    }
}