using System.IO;
using System.Reflection;
using PeanutButter.ServiceShell;

namespace TestService
{
    public class TotallyNotInterestingService: Shell
    {
        public TotallyNotInterestingService()
        {
            var exePath = Assembly.GetEntryAssembly()?.CodeBase
                ?? "(unknown location)";
            DisplayName = "Totally Not Interesting Service at: " + exePath;
            ServiceName = "Test Service";
            Interval = 1;
            Version.Major = 1;
        }

        protected override void RunOnce()
        {
            Log("Running once");
        }
    }
}