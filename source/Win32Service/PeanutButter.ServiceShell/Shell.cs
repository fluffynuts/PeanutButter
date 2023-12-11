using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Local

namespace PeanutButter.ServiceShell
{
    /// <summary>
    /// Describes the contract for a very simple logging service
    /// </summary>
    public interface ISimpleLogger
    {
        /// <summary>
        /// Log a debug message
        /// </summary>
        /// <param name="message"></param>
        void LogDebug(string message);

        /// <summary>
        /// Log an info message
        /// </summary>
        /// <param name="message"></param>
        void LogInfo(string message);

        /// <summary>
        /// Log a warning message
        /// </summary>
        /// <param name="message"></param>
        void LogWarning(string message);

        /// <summary>
        /// Log a fatal message - service should probably quit
        /// </summary>
        /// <param name="message"></param>
        void LogFatal(string message);
    }

    /// <inheritdoc cref="System.ServiceProcess.ServiceBase" />
    public abstract class Shell : ServiceBase, ISimpleLogger
    {
        /// <summary>
        /// Sets the *max* interval, in seconds, between calls to your implementation of RunOnce
        ///   Note that this means that if your RunOnce takes longer than this to run, the service
        ///   will essentially loop your RunOnce non-stop. Only one RunOnce iteration is done at
        ///   at a time (ie no concurrency)
        /// </summary>
        public int Interval { get; protected set; }

        /// <summary>
        /// The current version of the service
        /// </summary>
        public VersionInfo Version { get; private set; }

        /// <summary>
        /// Whether or not the service was invoked from the commandline (as
        /// opposed to being invoked via the SCM)
        /// </summary>
        public bool RunningOnceFromCLI { get; set; }

        /// <summary>
        /// The name displayed for this service in the SCM MMI snap-in (services.msc)
        /// </summary>
        public string DisplayName
        {
            get { return GetDisplayName(); }
            protected set { _displayName = value; }
        }

        private string GetDisplayName()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
            {
                throw new ServiceUnconfiguredException("DisplayName");
            }

            return _displayName;
        }

        private string _copyright;

        /// <summary>
        /// Copyright information for this service
        /// </summary>
        public string CopyrightInformation
        {
            get { return _copyright ?? string.Empty; }
            set { _copyright = value ?? string.Empty; }
        }

        /// <summary>
        /// The short name for the service, typically used to control
        /// it via the commandline. This is _not_ the name displayed
        /// in services.msc.
        /// </summary>
        /// <exception cref="ServiceUnconfiguredException"></exception>
        public new string ServiceName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_serviceName))
                {
                    throw new ServiceUnconfiguredException("ServiceName");
                }

                return _serviceName;
            }
            set { _serviceName = value; }
        }

        private string _serviceName;
        private string _displayName;

        /// <summary>
        /// Provides basic version information
        /// </summary>
        public class VersionInfo
        {
            /// <summary>
            /// Major version number
            /// </summary>
            public int Major { get; set; }

            /// <summary>
            /// Minor version number
            /// </summary>
            public int Minor { get; set; }

            /// <summary>
            /// Patch version number
            /// </summary>
            public int Build { get; set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return string.Join(
                    ".",
                    new[]
                    {
                        Major,
                        Minor,
                        Build
                    }
                );
            }
        }

        private bool Running;
        private bool Paused;

        /// <inheritdoc />
        protected Shell()
        {
            Version = new VersionInfo();
            Running = false;
            Paused = false;
            CanPauseAndContinue = true;
        }

        /// <summary>
        /// Runs the service via the given arguments, ie either
        /// to affect a required service control operation, or
        /// with no arguments, as per the SCM.
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int RunMain<T>(string[] args) where T : Shell, new()
        {
            if (InTestModeFor<T>(args))
            {
                return -1;
            }

            var instance = new T();
            var cliRunResult = CLIRunResultFor(args, instance);
            if (cliRunResult.HasValue)
            {
                return cliRunResult.Value;
            }

            DisableConsoleLogging();
            var servicesToRun = new ServiceBase[]
            {
                instance
            };
            ServiceBase.Run(servicesToRun);
            return 0;
        }

        private static int? CLIRunResultFor<T>(string[] args, T instance) where T : Shell, new()
        {
            var cli = new ServiceCommandlineOptions(args, instance.DisplayName, instance.CopyrightInformation);
            var runResult = PerformServiceShellTasksFor(instance, cli);
            if (runResult.HasValue)
            {
                return runResult.Value;
            }

            if (cli.Debug || cli.RunOnce)
            {
                ConfigureLogLevel();
                EnsureHaveConsoleLogger();
            }

            var lastRun = DateTime.Now;
            var lastResult = RunOnceResultFor(instance, cli);
            if (!cli.Debug)
            {
                return lastResult;
            }

            instance.Running = true;
            Console.CancelKeyPress += Stop(instance);
            do
            {
                instance.WaitForIntervalFrom(lastRun);
                lastRun = DateTime.Now;
                lastResult = RunOnceResultFor(instance, cli);
            } while (instance.Running);

            return lastResult;
        }

        private static void ConfigureLogLevel()
        {
            var repository = LogManager.GetRepository() as Hierarchy;
            if (repository == null)
            {
                return;
            }

            var root = repository.Root;
            root.Level = DetermineDebugLogLevel();
            repository.RaiseConfigurationChanged(EventArgs.Empty);
        }

        private static void EnsureHaveConsoleLogger()
        {
            ConfigureLog4NetIfRequired();

            var repository = LogManager.GetRepository() as Hierarchy;
            if (repository == null)
            {
                throw new ApplicationException(
                    "Unable to retrieve log4net repository as Hierarchy"
                );
            }

            var root = repository.Root;

            var haveConsoleAppender = root.Appenders.ToArray().Any(a => a is ConsoleAppender);
            if (haveConsoleAppender)
            {
                return;
            }

            var consoleAppender = new ConsoleAppender
            {
                Layout = new PatternLayout("%date %-5level: %message%newline"),
                Target = "Console.Out",
                Threshold = DetermineDebugLogLevel()
            };
            root.AddAppender(consoleAppender);
            repository.RaiseConfigurationChanged(EventArgs.Empty);
        }

        private static Level DetermineDebugLogLevel()
        {
            var env = Environment.GetEnvironmentVariable("LOG_LEVEL");
            return LogLevels.TryGetValue(env ?? "debug", out var level)
                ? level
                : Level.All;
        }

        private static readonly Dictionary<string, Level> LogLevels
            = new Dictionary<string, Level>(StringComparer.OrdinalIgnoreCase)
            {
                ["debug"] = Level.Debug,
                ["info"] = Level.Info,
                ["warn"] = Level.Warn
            };


        private static ConsoleCancelEventHandler Stop(Shell instance)
        {
            return (_, _) =>
            {
                if (instance.Running)
                {
                    Console.WriteLine("== Exiting now... ==");
                }

                instance.Running = false;
            };
        }

        private static int? RunOnceResultFor<T>(T instance, IServiceCommandlineOptions cli) where T : Shell, new()
        {
            if (cli.RunOnce || cli.Debug)
            {
                if (cli.Wait > 0)
                {
                    Thread.Sleep(cli.Wait);
                }

                try
                {
                    instance.RunningOnceFromCLI = true;
                    instance.RunOnce();
                    return (int)ServiceCommandlineOptions.ExitCodes.Success;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error running main routine:");
                    Console.WriteLine(ex.Message);
                    return (int)ServiceCommandlineOptions.ExitCodes.Failure;
                }
            }

            return null;
        }

        private static int? PerformServiceShellTasksFor<T>(T instance, ServiceCommandlineOptions cli)
            where T : Shell, new()
        {
            var task = ShellTasks.FirstOrDefault(t => t.Selector(cli));
            return task?.Logic.Invoke(cli, instance);
        }

        private static readonly ShellTaskStrategy[] ShellTasks =
        {
            ShellTask(o => o.ShowVersion, ShowVersionFor),
            ShellTask(o => o.Uninstall, StopAndUninstall),
            ShellTask(o => o.Install, InstallAndPerhapsStart),
            ShellTask(o => o.StartService, StartIfPossible),
            ShellTask(o => o.StopService, StopIfPossible)
        };

        private static int StopIfPossible(
            ServiceCommandlineOptions cli,
            Shell instance
        )
        {
            if (!instance.ServiceUtil.IsStoppable)
            {
                return Fail($"Service '{instance.ServiceName}' cannot be stopped in stated '{instance.State}'");
            }

            return TryRunShellTask(
                () => instance.ServiceUtil.Stop(true),
                ex => $"Unable to stop '{instance.ServiceName}': {ex.Message}"
            );
        }

        private static int TryRunShellTask(
            Action toRun,
            Func<Exception, string> errorMessageGenerator
        )
        {
            try
            {
                toRun();
                return Success();
            }
            catch (Exception ex)
            {
                return Fail(errorMessageGenerator(ex));
            }
        }

        /// <summary>
        /// Reflects the current state of the service
        /// </summary>
        public ServiceState State => ServiceUtil.State;

        private WindowsServiceUtil ServiceUtil =>
            _serviceUtil ??= new WindowsServiceUtil(ServiceName);

        private static int StartIfPossible(
            ServiceCommandlineOptions arg1,
            Shell arg2
        )
        {
            var svc = new WindowsServiceUtil(arg2.ServiceName);
            if (svc.State == ServiceState.Unknown ||
                svc.State == ServiceState.NotFound)
            {
                return Fail($"{svc.ServiceName} not installed or not queryable");
            }

            var entryExe = ResolveEntryExecutable(arg2.GetType());
            if (!entryExe.Equals(svc.Commandline, StringComparison.InvariantCultureIgnoreCase))
            {
                return Fail(
                    $"{svc.ServiceName} is installed at {svc.Commandline}.",
                    "Issuing start command here will probably not do what you expect."
                );
            }

            return Starters.TryGetValue(svc.State, out var handler)
                ? handler(svc)
                : Fail($"No handler found for service state {svc.State}");
        }

        private static Dictionary<ServiceState, Func<WindowsServiceUtil, int>> Starters
            = new()
            {
                [ServiceState.Running] = ServiceIs("already running"),
                [ServiceState.StartPending] = ServiceIs("already starting up"),
                [ServiceState.Paused] = ServiceIs("paused"),
                [ServiceState.Unknown] = ServiceIs("unknown or not queryable"),
                [ServiceState.NotFound] = ServiceIs("Unknown or not queryable"),
                [ServiceState.ContinuePending] = ServiceIs("continuing from pause"),
                [ServiceState.PausePending] = ServiceIs("pausing"),
                [ServiceState.StopPending] = ServiceIs("stopping"),
                [ServiceState.Stopped] = StartService
            };

        private static Func<WindowsServiceUtil, int> ServiceIs(string state)
        {
            return (util) => Fail($"{util.ServiceName} is {state}");
        }

        private static int StartService(WindowsServiceUtil arg)
        {
            try
            {
                arg.Start(true);
                return Success();
            }
            catch (Exception ex)
            {
                return Fail($"Unable to start {arg.ServiceName}: {ex.Message}");
            }
        }

        private static int Success()
        {
            return (int)ServiceCommandlineOptions.ExitCodes.Success;
        }

        private static int Fail(params string[] messages)
        {
            foreach (var line in messages)
            {
                Console.WriteLine(line);
            }

            return (int)ServiceCommandlineOptions.ExitCodes.Failure;
        }

        private static int ShowVersionFor(
            ServiceCommandlineOptions cli,
            Shell shell
        )
        {
            return shell.ShowVersion();
        }

        private static int InstallAndPerhapsStart(
            ServiceCommandlineOptions cli,
            Shell instance
        )
        {
            var result = instance.InstallMe(cli);
            return cli.StartService
                ? instance.StartMe()
                : result;
        }

        private static int StopAndUninstall(
            ServiceCommandlineOptions options,
            Shell shell
        )
        {
            shell.StopMe(true);
            return shell.UninstallMe();
        }

        private static ShellTaskStrategy ShellTask(
            Func<ServiceCommandlineOptions, bool> selector,
            Func<ServiceCommandlineOptions, Shell, int> logic
        )
        {
            return new ShellTaskStrategy(selector, logic);
        }

        private class ShellTaskStrategy
        {
            public Func<ServiceCommandlineOptions, bool> Selector { get; }
            public Func<ServiceCommandlineOptions, Shell, int> Logic { get; }

            public ShellTaskStrategy(
                Func<ServiceCommandlineOptions, bool> selector,
                Func<ServiceCommandlineOptions, Shell, int> logic
            )
            {
                Selector = selector;
                Logic = logic;
            }
        }

        /// <summary>
        /// Runs the main logic once. You MUST override this in your
        /// derived service.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected abstract void RunOnce();

        private int StartMe()
        {
            if (!ServiceUtil.IsInstalled)
            {
                return Fail("Unable to start service: not installed");
            }

            if (!ServiceUtil.IsStartable) // IsStartable(existingServiceUtil))
            {
                return Fail("Service cannot be started");
            }

            try
            {
                ServiceUtil.Start();
                return Success();
            }
            catch (Exception ex)
            {
                return Fail($"Unable to start '{ServiceUtil.ServiceName}': {ex.Message}");
            }
        }

        private int FailWith(string message, bool silent)
        {
            if (silent)
            {
                return (int)ServiceCommandlineOptions.ExitCodes.Success;
            }

            Console.WriteLine(message);
            return (int)ServiceCommandlineOptions.ExitCodes.Failure;
        }

#pragma warning disable S3241 // Methods should not return values that are never used
        private int StopMe(bool silentFail = false)
#pragma warning restore S3241 // Methods should not return values that are never used
        {
            var existingServiceUtil = new WindowsServiceUtil(ServiceName);
            if (!existingServiceUtil.IsInstalled)
            {
                return FailWith("Unable to stop service: not installed", silentFail);
            }

            if (!existingServiceUtil.IsStoppable)
            {
                return FailWith("Service already stopped", silentFail);
            }

            try
            {
                existingServiceUtil.Start();
                return (int)ServiceCommandlineOptions.ExitCodes.Success;
            }
            catch (Exception ex)
            {
                return FailWith("Unable to start service: " + ex.Message, silentFail);
            }
        }

        private static string ResolveEntryExecutable(
            Type forType
        )
        {
            var asmLocation = new Uri(forType.Assembly.Location).LocalPath;
            return asmLocation.RegexReplace("\\.dll$", ".exe");
        }

        private int InstallMe(ServiceCommandlineOptions cli)
        {
            var exe = ResolveEntryExecutable(GetType());
            var existingSvcUtil = new WindowsServiceUtil(ServiceName);
            // bug: services with arguments will never match here & args can't be known (as-is)
            if (existingSvcUtil.Commandline == exe)
            {
                Console.WriteLine("Service already installed correctly");
                return (int)ServiceCommandlineOptions.ExitCodes.Success;
            }

            try
            {
                if (existingSvcUtil.IsInstalled)
                {
                    existingSvcUtil.Uninstall(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Service already installed at: {existingSvcUtil.Commandline} and I can't uninstall it:\n{ex}"
                );
                return (int)ServiceCommandlineOptions.ExitCodes.InstallFailed;
            }

            Console.WriteLine(
                $"Attempt to install with:\n  ServiceName: {ServiceName}\n  DisplayName: {DisplayName}\n  Executable: {exe}"
            );
            var svcUtil = new WindowsServiceUtil(ServiceName, DisplayName, exe);
            try
            {
                var bootFlag = ResolveStartupTypeFor(cli);
                svcUtil.Install(bootFlag);
                Console.WriteLine("Installed!");
                return (int)ServiceCommandlineOptions.ExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to install: " + ex.Message);
                return (int)ServiceCommandlineOptions.ExitCodes.InstallFailed;
            }
        }

        private ServiceStartupTypes ResolveStartupTypeFor(ServiceCommandlineOptions cli)
        {
            if (!cli.ManualStart && !cli.Disabled)
            {
                return ServiceStartupTypes.Automatic;
            }

            if (cli.ManualStart && cli.Disabled)
            {
                throw new ArgumentException(
                    "Cannot specify that the service be both disabled and manual start",
                    nameof(cli)
                );
            }

            return cli.ManualStart
                ? ServiceStartupTypes.Manual
                : ServiceStartupTypes.Disabled;
        }

        private int UninstallMe()
        {
            var svcUtil = new WindowsServiceUtil(ServiceName);
            if (!svcUtil.IsInstalled)
            {
                Console.WriteLine("Not installed!");
                return (int)ServiceCommandlineOptions.ExitCodes.UninstallFailed;
            }

            try
            {
                svcUtil.Uninstall();
                Console.WriteLine("Uninstalled!");
                return (int)ServiceCommandlineOptions.ExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to install: " + ex.Message);
                return (int)ServiceCommandlineOptions.ExitCodes.UninstallFailed;
            }
        }

        private int ShowVersion()
        {
            Console.WriteLine(
                string.Join(
                    " ",
                    new[]
                    {
                        Path.GetFileName(
                            Environment.GetCommandLineArgs()[0]
                        ),
                        "version:",
                        Version.ToString()
                    }
                )
            );

            return (int)ServiceCommandlineOptions.ExitCodes.Success;
        }

        private static void DisableConsoleLogging()
        {
            foreach (var appender in LogManager.GetRepository().GetAppenders())
            {
                if (!(appender is AppenderSkeleton a))
                {
                    return;
                }

                if ((a is ColoredConsoleAppender) ||
                    (a is ConsoleAppender) ||
                    (a is AnsiColorTerminalAppender))
                {
                    a.Threshold = Level.Off;
                }
            }
        }

        /// <summary>
        /// Log a string, as INFO
        /// </summary>
        /// <param name="status"></param>
        public virtual void Log(string status)
        {
            GetLogger().Info(status);
        }

        /// <inheritdoc />
        protected override void OnStart(string[] args)
        {
            LogState("Starting up");
            var thread = new Thread(Run);
            thread.Start();
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            LogState("Stopping");
            Running = false;
            Paused = false;
        }

        /// <inheritdoc />
        protected override void OnPause()
        {
            LogState("Pausing");
            Paused = true;
        }

        /// <inheritdoc />
        protected override void OnShutdown()
        {
            LogState("Stopping due to system shutdown");
            Running = false;
            Paused = false;
        }

        /// <inheritdoc />
        protected override void OnContinue()
        {
            LogState("Continuing");
            Paused = false;
        }

        private void LogState(string state)
        {
            Log(
                string.Join(
                    " ",
                    new[]
                    {
                        DisplayName,
                        "::",
                        state
                    }
                )
            );
        }

        /// <inheritdoc />
        public virtual void LogDebug(string message)
        {
            GetLogger().Debug(message);
        }

        /// <inheritdoc />
        public virtual void LogInfo(string message)
        {
            GetLogger().Info(message);
        }

        /// <inheritdoc />
        public virtual void LogWarning(string message)
        {
            GetLogger().Warn(message);
        }

        /// <inheritdoc />
        public virtual void LogFatal(string message)
        {
            GetLogger().Fatal(message);
        }

        /// <summary>
        /// Runs the main loop once
        /// </summary>
        protected void Run()
        {
            Running = true;
            LogState("Running");
            while (Running)
            {
                if (Paused)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                if (Stopped())
                {
                    break;
                }

                var lastRun = DateTime.Now;
                try
                {
                    RunOnce();
                }
                catch (Exception ex)
                {
                    LogWarning("Exception running " + GetType().Name + ".RunOnce: " + ex.Message);
                }

                WaitForIntervalFrom(lastRun);
            }

            LogInfo(ServiceName + ": Exiting");
        }

        /// <summary>
        /// Test if the service has been stopped. Will block whilst the service is paused.
        /// </summary>
        /// <returns></returns>
        protected bool Stopped()
        {
            while (Paused && Running)
            {
                Thread.Sleep(500);
            }

            return !Running;
        }

        /// <summary>
        /// Waits until the next Interval should be fired, taking into account
        /// the run-time of the last round.
        /// </summary>
        /// <param name="lastRun"></param>
        public void WaitForIntervalFrom(DateTime lastRun)
        {
            var delta = DateTime.Now - lastRun;
            while (delta.TotalSeconds < Interval)
            {
                var granularity = 500;
                Thread.Sleep(granularity);
                if (!Running)
                {
                    break;
                }

                delta = DateTime.Now - lastRun;
            }
        }

        private bool _haveConfiguredLogging;

        /// <summary>
        /// Provide the ILog logger
        /// </summary>
        /// <returns></returns>
        protected virtual ILog GetLogger()
        {
            if (!_haveConfiguredLogging)
            {
                ConfigureLog4NetIfRequired();
                _haveConfiguredLogging = true;
            }

            return LogManager.GetLogger(ServiceName);
        }

        private static void ConfigureLog4NetIfRequired()
        {
            try
            {
                var configuredExternally = LogManager.GetRepository().Configured;
                if (!configuredExternally && ConfiguredViaAppConfig())
                {
                    XmlConfigurator.Configure();
                }
            }
            catch
            {
                /* suppress */
            }
        }

        private static bool ConfiguredViaAppConfig()
        {
            var entryPoint = new FileInfo(Environment.GetCommandLineArgs()[0]).FullName;
            if (!File.Exists(entryPoint))
            {
                return false;
            }

            var config = $"{entryPoint}.config";
            if (!File.Exists(config))
            {
                return false;
            }

            try
            {
                var doc = XDocument.Parse(config);
                var typedConfigSections = doc.XPathSelectElements(
                    "/configurations/configSections[@type]"
                );
                var seek = typeof(Log4NetConfigurationSectionHandler);
                if (string.IsNullOrWhiteSpace(seek.FullName))
                {
                    return false; // can't check; give up
                }

                return typedConfigSections
                    .Select(el => el.Attribute("type")?.Value)
                    .Where(t => t is not null)
                    .Any(
                        typeName => typeName.IndexOf(
                            seek.FullName,
                            StringComparison.OrdinalIgnoreCase
                        ) > -1
                    );
            }
            catch
            {
                return false;
            }
        }

        // test enablers
        private static bool _testModeEnabled;
        private static Type _testClass;
        private static string[] _testArgs;
        private WindowsServiceUtil _serviceUtil;

        /// <summary>
        /// For testability, this sets the service into "test mode"
        /// </summary>
        public static void StartTesting()
        {
            _testModeEnabled = true;
        }

        private static bool InTestModeFor<T>(string[] args)
        {
            if (!_testModeEnabled)
            {
                return false;
            }

            if (_testClass != null)
            {
                throw new Exception($"Already have run for {_testClass.Name}");
            }

            _testClass = typeof(T);
            _testArgs = args;
            return true;
        }

        /// <summary>
        /// Tests if the main entry point was run
        /// </summary>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <exception cref="ShellTestFailureException"></exception>
        public static void ShouldHaveRunMainFor<T>(string[] args)
        {
            try
            {
                if (_testModeEnabled &&
                    _testClass == typeof(T) &&
                    AllMatch(_testArgs, args))
                {
                    return;
                }

                var allArgs = string.Join(" ", args);
                throw new ShellTestFailureException($"Expected to run for type {typeof(T).Name} with args {allArgs}");
            }
            finally
            {
                ResetTestMode();
            }
        }

        /// <summary>
        /// Resets the test mode (this is code to enable testing)
        /// </summary>
        public static void ResetTestMode()
        {
            _testModeEnabled = false;
            _testClass = null;
            _testArgs = null;
        }

        private static bool AllMatch(string[] testArgs, string[] args)
        {
            if (testArgs.Length != args.Length)
            {
                return false;
            }

            return !testArgs.Where((t, i) => t != args[i]).Any();
        }
    }
}