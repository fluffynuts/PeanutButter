using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Helper class to find all instances of running LocalDb services
    /// </summary>
    public class LocalDbInstanceEnumerator
    {
        private static readonly string[] _availableDefaultInstances;

        static LocalDbInstanceEnumerator()
        {
            _availableDefaultInstances = FindInstancesWithUtil();
        }

        /// <summary>
        /// Finds the list of found LocalDb instance names
        /// </summary>
        /// <returns>String array of found LocalDb instance names</returns>
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

        /// <summary>
        /// Attempts to find the highest-versioned LocalDb instance
        /// </summary>
        /// <returns>Instance name of the highest-versioned LocalDb instance, typically something like "v11.0"</returns>
        public string FindHighestDefaultInstance()
        {
            const string fallback = "v11.0";
            try
            {
                return FindInstances().First(); 
            }
            catch (Exception)
            {
                Debug.WriteLine($"WARNING: Unable to determine actual LocalDb instances; falling back on '{fallback}'");
                return fallback;
            }
        }

        private static string[] FindDefaultInstancesFromOutputOf(Process process)
        {
            var lines = process.StandardOutput
                .ReadToEnd()
                .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            var versionLines = lines.Where(v => v.StartsWith("v"))
                .OrderByDescending(v => v)
                .ToArray();

            // prefer MSSQLLocalDB, which is what SQL Express 2014 and later set up
            var newInstanceName = LOCALDB_INSTANCE_2014_AND_LATER.ToLower(CultureInfo.InvariantCulture);
            if (lines.Any(l => l.ToLower(CultureInfo.InvariantCulture) == newInstanceName))
                return new[] { LOCALDB_INSTANCE_2014_AND_LATER }.And(versionLines);
            return versionLines;
        }

        private const string LOCALDB_INSTANCE_2014_AND_LATER = "MSSQLLocalDB";


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