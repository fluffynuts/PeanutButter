using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
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
        public class CommandLine
        {
            [TestFixture]
            public class WhenServiceIsNotInstalled
            {
                private string ServiceName;
                [SetUp]
                public void OneTimeSetup()
                {
                    ServiceName = $"test-service-{Guid.NewGuid()}";
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
                    var util = MakeTestServiceUtil();
                    Expect(util.State)
                        .To.Equal(ServiceState.Running);
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
                    var util = MakeTestServiceUtil();
                    Expect(util.State)
                        .To.Equal(ServiceState.Stopped);
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
            
            private static readonly string TestServiceName = $"test-service-${Guid.NewGuid()}";

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
                using var io = new ProcessIO(
                    program, 
                    args.And("-n", TestServiceName));
                io.StandardOutput.ForEach(line => Console.WriteLine($"stdout: {line}"));
                io.StandardError.ForEach(line => Console.WriteLine($"stderr: {line}"));
                return io.ExitCode;
            }

            private static WindowsServiceUtil MakeTestServiceUtil()
            {
                return new WindowsServiceUtil(TestServiceName);
            }

            private static bool ServiceAlreadyInstalled(string name)
            {
                var util = new WindowsServiceUtil(name);
                return util.IsInstalled;
            }

            private static string MyFolder = Path.GetDirectoryName(
                new Uri(typeof(TestShell).GetAssembly().Location).LocalPath
            );

            private static string TestService = Path.Combine(MyFolder, "TestService.exe");

        }
    }
}