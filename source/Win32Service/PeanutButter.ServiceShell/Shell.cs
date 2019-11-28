using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using PeanutButter.WindowsServiceManagement;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Local

namespace PeanutButter.ServiceShell
{
    public interface ISimpleLogger
    {
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogFatal(string message);
    }

    public abstract class Shell : ServiceBase, ISimpleLogger
    {
        /// <summary>
        /// Sets the *max* interval, in seconds, between calls to your implementation of RunOnce
        ///   Note that this means that if your RunOnce takes longer than this to run, the service
        ///   will essentially loop your RunOnce non-stop. Only one RunOnce iteration is done at
        ///   at a time (ie no concurrency)
        /// </summary>
        public int Interval { get; protected set; }

        public VersionInfo Version { get; private set; }
        public bool RunningOnceFromCLI { get; set; }

        public string DisplayName
        {
            get { return GetDisplayName(); }
            protected set { _displayName = value; }
        }

        private string GetDisplayName()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
                throw new ServiceUnconfiguredException("DisplayName");
            return _displayName;
        }

        private string _copyright;

        public string CopyrightInformation
        {
            get { return _copyright ?? string.Empty; }
            set { _copyright = value ?? string.Empty; }
        }

        public new string ServiceName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_serviceName))
                    throw new ServiceUnconfiguredException("ServiceName");
                return _serviceName;
            }
            set { _serviceName = value; }
        }

        private string _serviceName;
        private string _displayName;

        public class VersionInfo
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Build { get; set; }

            public override string ToString()
            {
                return string.Join(".", new[] { Major, Minor, Build });
            }
        }

        private bool Running;
        private bool Paused;

        protected Shell()
        {
            Version = new VersionInfo();
            Running = false;
            Paused = false;
            CanPauseAndContinue = true;
        }

        public static int RunMain<T>(string[] args) where T : Shell, new()
        {
            if (InTestModeFor<T>(args))
                return -1;
            var instance = new T();
            var cliRunResult = CLIRunResultFor(args, instance);
            if (cliRunResult.HasValue)
                return cliRunResult.Value;
            DisableConsoleLogging();

            var servicesToRun = new ServiceBase[] { instance };
            ServiceBase.Run(servicesToRun);
            return 0;
        }

        private static int? CLIRunResultFor<T>(string[] args, T instance) where T : Shell, new()
        {
            var cli = new CommandlineOptions(args, instance.DisplayName, instance.CopyrightInformation);
            var runResult = PerformServiceShellTasksFor(instance, cli);
            if (runResult.HasValue)
                return runResult.Value;
            if (cli.Debug)
            {
                ConfigureLogLevel();
                EnsureHaveConsoleLogger();
            }

            var lastRun = DateTime.Now;
            var lastResult = RunOnceResultFor(instance, cli);
            if (cli.Debug)
            {
                instance.Running = true;
                do
                {
                    Console.CancelKeyPress += Stop(instance);
                    instance.WaitForIntervalFrom(lastRun);

                    lastRun = DateTime.Now;
                    lastResult = RunOnceResultFor(instance, cli);
                } while (instance.Running);
            }

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
            EnsureHaveConfigured();

            var repository = LogManager.GetRepository() as Hierarchy;
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
            return (sender, eventArgs) =>
            {
                if (instance.Running)
                {
                    Console.WriteLine("== Exiting now... ==");
                }

                instance.Running = false;
            };
        }

        private static int? RunOnceResultFor<T>(T instance, CommandlineOptions cli) where T : Shell, new()
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
                    return (int) CommandlineOptions.ExitCodes.Success;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error running main routine:");
                    Console.WriteLine(ex.Message);
                    return (int) CommandlineOptions.ExitCodes.Failure;
                }
            }

            return null;
        }

        private static int? PerformServiceShellTasksFor<T>(T instance, CommandlineOptions cli) where T : Shell, new()
        {
            if (cli.ExitCode == CommandlineOptions.ExitCodes.ShowedHelp)
                return (int) cli.ExitCode;
            if (cli.ShowVersion)
                return instance.ShowVersion<T>();
            if (cli.Uninstall)
            {
                instance.StopMe(true);
                return instance.UninstallMe();
            }

            if (cli.Install)
            {
                var result = instance.InstallMe();
                return cli.StartService
                    ? instance.StartMe()
                    : result;
            }

            return null;
        }

        protected virtual void RunOnce()
        {
            throw new NotImplementedException("You must override RunOnce in your deriving service class");
        }

        private int StartMe()
        {
            var existingServiceUtil = new WindowsServiceUtil(ServiceName);
            if (!existingServiceUtil.IsInstalled)
            {
                Console.WriteLine("Unable to start service: not installed");
                return (int) CommandlineOptions.ExitCodes.Failure;
            }

            if (!IsStartable(existingServiceUtil))
            {
                Console.WriteLine("Service cannot be started");
                return (int) CommandlineOptions.ExitCodes.Failure;
            }

            try
            {
                existingServiceUtil.Start();
                return (int) CommandlineOptions.ExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to start service: " + ex.Message);
                return (int) CommandlineOptions.ExitCodes.Failure;
            }
        }

        private bool IsStartable(WindowsServiceUtil service)
        {
            switch (service.State)
            {
                case ServiceState.Stopped:
                case ServiceState.PausePending:
                case ServiceState.Paused:
                case ServiceState.Running:
                case ServiceState.StartPending:
                    return true;
                default:
                    return false;
            }
        }

        private int FailWith(string message, bool silent)
        {
            if (silent)
                return (int) CommandlineOptions.ExitCodes.Success;
            Console.WriteLine(message);
            return (int) CommandlineOptions.ExitCodes.Failure;
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

            if (!IsStoppable(existingServiceUtil))
            {
                return FailWith("Service already stopped", silentFail);
            }

            try
            {
                existingServiceUtil.Start();
                return (int) CommandlineOptions.ExitCodes.Success;
            }
            catch (Exception ex)
            {
                return FailWith("Unable to start service: " + ex.Message, silentFail);
            }
        }

        private bool IsStoppable(WindowsServiceUtil service)
        {
            switch (service.State)
            {
                case ServiceState.ContinuePending:
                case ServiceState.PausePending:
                case ServiceState.Paused:
                case ServiceState.Running:
                case ServiceState.StartPending:
                    return true;
                default:
                    return false;
            }
        }

        private int InstallMe()
        {
            var myExePath = new FileInfo(Environment.GetCommandLineArgs()[0]).FullName;
            var existingSvcUtil = new WindowsServiceUtil(ServiceName);
            if (existingSvcUtil.ServiceExe == myExePath)
            {
                Console.WriteLine("Service already installed correctly");
                return (int) CommandlineOptions.ExitCodes.Success;
            }

            try
            {
                if (existingSvcUtil.IsInstalled)
                    existingSvcUtil.Uninstall(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Service already installed at: " + existingSvcUtil.ServiceExe +
                    " and I can't uninstall it: " + ex.Message);
                return (int) CommandlineOptions.ExitCodes.InstallFailed;
            }

            var svcUtil = new WindowsServiceUtil(ServiceName, DisplayName, myExePath);
            try
            {
                svcUtil.Install();
                Console.WriteLine("Installed!");
                return (int) CommandlineOptions.ExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to install: " + ex.Message);
                return (int) CommandlineOptions.ExitCodes.InstallFailed;
            }
        }

        private int UninstallMe()
        {
            var svcUtil = new WindowsServiceUtil(ServiceName);
            if (!svcUtil.IsInstalled)
            {
                Console.WriteLine("Not installed!");
                return (int) CommandlineOptions.ExitCodes.UninstallFailed;
            }

            try
            {
                svcUtil.Uninstall();
                Console.WriteLine("Uninstalled!");
                return (int) CommandlineOptions.ExitCodes.Success;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to install: " + ex.Message);
                return (int) CommandlineOptions.ExitCodes.UninstallFailed;
            }
        }

        private int ShowVersion<T>() where T : Shell, new()
        {
            var svc = new T();
            Console.WriteLine(string.Join(" ",
                new[]
                {
                    Path.GetFileName(Environment.GetCommandLineArgs()[0]), "version:", svc.Version.ToString()
                }));

            return (int) CommandlineOptions.ExitCodes.Success;
        }

        private static void DisableConsoleLogging()
        {
            foreach (var appender in LogManager.GetRepository().GetAppenders())
            {
                var a = appender as log4net.Appender.AppenderSkeleton;
                if (a == null)
                    continue;
                if ((a is log4net.Appender.ColoredConsoleAppender) ||
                    (a is log4net.Appender.ConsoleAppender) ||
                    (a is log4net.Appender.AnsiColorTerminalAppender))
                    a.Threshold = log4net.Core.Level.Off;
            }
        }

        public virtual void Log(string status)
        {
            GetLogger().Info(status);
        }

        protected override void OnStart(string[] args)
        {
            LogState("Starting up");
            var thread = new Thread(Run);
            thread.Start();
        }

        protected override void OnStop()
        {
            LogState("Stopping");
            Running = false;
            Paused = false;
        }

        protected override void OnPause()
        {
            LogState("Pausing");
            Paused = true;
        }

        protected override void OnShutdown()
        {
            LogState("Stopping due to system shutdown");
            Running = false;
            Paused = false;
        }

        protected override void OnContinue()
        {
            LogState("Continuing");
            Paused = false;
        }

        private void LogState(string state)
        {
            Log(string.Join(" ", new[] { DisplayName, "::", state }));
        }

        public virtual void LogDebug(string message)
        {
            GetLogger().Debug(message);
        }

        public virtual void LogInfo(string message)
        {
            GetLogger().Info(message);
        }

        public virtual void LogWarning(string message)
        {
            GetLogger().Warn(message);
        }

        public virtual void LogFatal(string message)
        {
            GetLogger().Fatal(message);
        }

        protected void Run()
        {
            Running = true;
            LogState("Running");
            while (Running)
            {
                if (PausedThenStopped())
                    break;
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

            GetLogger().Info(ServiceName + ": Exiting");
        }

        protected bool PausedThenStopped()
        {
            while (Paused && Running)
            {
                Thread.Sleep(500);
            }

            return !Running;
        }

        public void WaitForIntervalFrom(DateTime lastRun)
        {
            var delta = DateTime.Now - lastRun;
            while (delta.TotalSeconds < Interval)
            {
                var granularity = 500;
                Thread.Sleep(granularity);
                if (!Running)
                    break;
                delta = DateTime.Now - lastRun;
            }
        }

        private bool _haveConfiguredLogging;

        protected ILog GetLogger()
        {
            if (!_haveConfiguredLogging)
            {
                EnsureHaveConfigured();
                _haveConfiguredLogging = true;
            }

            return LogManager.GetLogger(ServiceName);
        }

        private static void EnsureHaveConfigured()
        {
            try
            {
                var configuredExternally = LogManager.GetRepository().Configured;
                if (!configuredExternally)
                {
                    XmlConfigurator.Configure();
                }
            }
            catch
            {
                /* suppress */
            }
        }

        // test enablers
        private static bool _testModeEnabled;
        private static Type _testClass;
        private static string[] _testArgs;

        public static void StartTesting()
        {
            _testModeEnabled = true;
        }

        private static bool InTestModeFor<T>(string[] args)
        {
            if (!_testModeEnabled)
                return false;
            if (_testClass != null)
                throw new Exception($"Already have run for {_testClass.Name}");
            _testClass = typeof(T);
            _testArgs = args;
            return true;
        }

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

        public static void ResetTestMode()
        {
            _testModeEnabled = false;
            _testClass = null;
            _testArgs = null;
        }

        private static bool AllMatch(string[] testArgs, string[] args)
        {
            if (testArgs.Length != args.Length)
                return false;
            return !testArgs.Where((t, i) => t != args[i]).Any();
        }
    }
}