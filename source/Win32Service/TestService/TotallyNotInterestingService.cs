using System.IO;
using System.Reflection;
using PeanutButter.ServiceShell;

namespace TestService
{
    public class TotallyNotInterestingService : Shell
    {
        public class CliOptions
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
        }

        public static CliOptions Options { get; set; }

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
    }
}