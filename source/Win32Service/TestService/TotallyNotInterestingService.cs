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

        private string IniFilePath =>
            Path.Combine(
                Path.GetDirectoryName(
                    new Uri(
                        typeof(TotallyNotInterestingService).Assembly.Location
                    ).LocalPath
                ) ?? ".",
                INIFILE
            );

        public static IServiceOptions Options { get; set; }

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
            Log("on start - test if should delay");
            if (!TryReadIniSetting<int>(
                    SECTION_DELAY,
                    nameof(Options.StartDelay),
                    out var delay))
            {
                Log(" -> no delay");
                return;
            }

            Log($" -> delay: {delay}ms");

            Thread.Sleep(delay);
        }

        private bool TryReadIniSetting<T>(
            string sectionName,
            string settingName,
            out T result
        )
        {
            Log($"read ini at {IniFilePath}");
            var ini = new INIFile(IniFilePath);
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

    public interface IServiceOptions : IServiceCommandlineOptions
    {
        [Description("Set the short name for this service")]
        string Name { get; set; }

        [Description("Set the long name for this service")]
        string DisplayName { get; set; }

        [Description("Delay, in ms, when starting up")]
        int StartDelay { get; set; }
    }
}