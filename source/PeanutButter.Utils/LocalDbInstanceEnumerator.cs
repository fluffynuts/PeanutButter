using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PeanutButter.Utils
{
    public class LocalDbInstanceEnumerator
    {
        private static string[] _availableDefaultInstances;

        static LocalDbInstanceEnumerator()
        {
            _availableDefaultInstances = FindInstancesWithUtil();
        }

        public string[] FindInstances()
        {
            return _availableDefaultInstances;
        }

        private static string[] FindInstancesWithUtil()
        {
            var toRun = FindLocalDbUtility();
            if (toRun == null)
                throw new UnableToFindLocalDbUtilityException();
            var process = RunToCompletion(toRun);
            return FindDefaultInstancesFromOutputOf(process);
        }

        public string FindHighestDefaultInstance()
        {
            const string fallback = "v11.0";
            try
            {
                return FindInstances().First(); 
            }
            catch (Exception)
            {
                Debug.WriteLine($"WARNING: Unable to determin actual LocalDb instances; falling back on '{fallback}'");
                return fallback;
            }
        }

        private static string[] FindDefaultInstancesFromOutputOf(Process process)
        {
            var output = process.StandardOutput
                .ReadToEnd()
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Where(v => v.StartsWith("v"))
                .OrderByDescending(v => v)
                .ToArray();
            return output;
        }

        private static Process RunToCompletion(string toRun)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(toRun)
                {
                    Arguments = "info",
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                },
            };
            process.Start();
            process.WaitForExit();
            return process;
        }

        private static string FindLocalDbUtility()
        {
            return SafeWalk.EnumerateFiles("C:\\Program Files\\Microsoft SQL Server", "SqlLocalDb.exe", SearchOption.AllDirectories)
                .OrderBy(p => p)
                .LastOrDefault();
        }
    }
}