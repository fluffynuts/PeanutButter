using System;
using System.IO;
using System.Reflection;
using System.Threading;
using PeanutButter.EasyArgs.Attributes;
using PeanutButter.INI;
using PeanutButter.ServiceShell;

namespace TestService
{
    public class TotallyNotInterestingService : Shell
    {
        public const string INIFILE = "config.ini";
        public const string SECTION_DELAY = "delay";

        public static ICliOptions Options { get; set; }

        public TotallyNotInterestingService()
        {
            var exePath = Assembly.GetEntryAssembly()?.CodeBase
                ?? "(unknown location)";
            DisplayName = Options?.DisplayName ?? "Totally Not Interesting Service at: " + exePath;
            ServiceName = Options?.Name ?? "Test Service";
            Interval = 1;
            Version.Major = 1;
        }

        protected override void RunOnce()
        {
            Log("Running once");
        }

        protected override void OnStart(string[] args)
        {
            if (!TryReadIniSetting<int>(
                    SECTION_DELAY,
                    nameof(Options.StartDelay),
                    out var delay))
            {
                return;
            }

            Thread.Sleep(delay);
        }

        private bool TryReadIniSetting<T>(string sectionName, string settingName, out T result)
        {
            var ini = new INIFile(INIFILE);
            result = default;
            if (!ini.HasSetting(sectionName, settingName))
            {
                return false;
            }

            try
            {
                result = (T) Convert.ChangeType(ini[sectionName][settingName], typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string LogFilePath = Path.Combine(
            Path.GetDirectoryName(
                new Uri(
                    typeof(TotallyNotInterestingService).Assembly.Location
                ).LocalPath
            ), "service.log");

        // void Log(string str)
        // {
        //     File.AppendAllLines(LogFilePath, new[]
        //     {
        //         $"[{DateTime.Now}] {str}"
        //     });
        // }

        protected override void OnPause()
        {
            SleepForEnvMs("PAUSE_DELAY");
        }

        private void SleepForEnvMs(string name)
        {
            var envVar = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrEmpty(envVar))
            {
                return;
            }

            if (!int.TryParse(envVar, out var ms))
            {
                return;
            }

            Thread.Sleep(ms);
        }
    }

    public interface IHackOptionsToShowAll
    {
        bool Install { get; set; }
        bool Uninstall { get; set; }
        bool RunOnce { get; set; }
        bool Debug { get; set; }
        int Wait { get; set; }
        bool ShowVersion { get; set; }
        bool StartService { get; set; }
        bool StopService { get; set; }
        bool ManualStart { get; set; }
        bool Disabled { get; set; }
        string Name { get; set; }
        string DisplayName { get; set; }
        int StartDelay { get; set; }
    }

    public class HackOptionsToShowAll : CliOptions,  IServiceCommandlineOptions, IHackOptionsToShowAll
    {
        // implemented from IServiceCommandLineOptions
        // just to get them in the help here
        [Description("Install this service")]
        public bool Install { get; set; }
        [Description("Uninstall this service")]
        public bool Uninstall { get; set; }
        [Description("Run one round of this service's code and exit")]
        public bool RunOnce { get; set; }
        [Description("Run continually, with log4net logging set to ALL")]
        public bool Debug { get; set; }
        [Description("Wait this many seconds before actually doing the round of work for run-once")]
        public int Wait { get; set; }
        [Description("Show the version of this service")]
        [ShortName('v')]
        [LongName("version")]
        public bool ShowVersion { get; set; }
        [Description("Start service")]
        [ShortName('s')]
        [LongName("start")]
        public bool StartService { get; set; }
        [Description("Stop service")]
        [ShortName('x')]
        [LongName("stop")]
        public bool StopService { get; set; }
        [Description("Install with manual startup")]
        public bool ManualStart { get; set; }
        public bool Disabled { get; set; }
    }

    public interface ICliOptions
    {
        string Name { get; set; }
        string DisplayName { get; set; }
        int StartDelay { get; set; }
    }

    public class CliOptions
        : ICliOptions
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int StartDelay { get; set; }
    }
}