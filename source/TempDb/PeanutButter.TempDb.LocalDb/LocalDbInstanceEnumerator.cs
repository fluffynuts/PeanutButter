using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.LocalDb
{
    /// <summary>
    /// Helper class to find all instances of running LocalDb services
    /// </summary>
    public class LocalDbInstanceEnumerator
    {
        private static string[] AvailableDefaultInstances;

        /// <summary>
        /// Finds the list of found LocalDb instance names
        /// </summary>
        /// <returns>String array of found LocalDb instance names</returns>
        public string[] FindInstances()
        {
            return FindInstancesWithUtil();
        }

        private static readonly object Lock = new object();

        private static string[] FindInstancesWithUtil()
        {
            lock (Lock)
            {
                if (AvailableDefaultInstances != null)
                {
                    return AvailableDefaultInstances;
                }

                var toRun = FindLocalDbUtility();
                if (toRun == null)
                {
                    throw new UnableToFindLocalDbUtilityException();
                }

                var process = RunToCompletion(toRun);
                return FindInstancesFromOutputOf(process);
            }
        }

        /// <summary>
        /// Attempts to find the highest-versioned LocalDb instance
        /// </summary>
        /// <returns>Instance name of the highest-versioned LocalDb instance, typically something like "v11.0"</returns>
        public string FindFirstAvailableInstance()
        {
            return _availableInstance ?? (_availableInstance = InterrogateInstances());
        }

        private string InterrogateInstances()
        {
            const string fallback = "v11.0";
            try
            {
                return FindInstances().First(CanConnect);
            }
            catch (UnableToFindLocalDbUtilityException)
            {
                throw;
            }
            catch (Exception)
            {
                Debug.WriteLine($"WARNING: Unable to determine actual LocalDb instances; falling back on '{fallback}'");
                return fallback;
            }
        }

        private static string _availableInstance;

        private bool CanConnect(string instanceName)
        {
            using (var conn =
                new SqlConnection(
                    $"Data Source=(localdb)\\{instanceName};Initial Catalog=master;Integrated Security=True"))
            {
                try
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
                catch
                {
                    Debug.WriteLine($"WARNING: cannot connect to localdb instance {instanceName}");
                    return false;
                }
            }
        }

        private static string[] FindInstancesFromOutputOf(Process process)
        {
            var lines = process.StandardOutput
                .ReadToEnd()
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .OrderBy(IsNewLocalDbDefaultInstanceName)
                .ThenByDescending(VersionedName)
                .ThenBy(l => l)
                .ToArray();
            return lines;
        }

        private static bool IsNewLocalDbDefaultInstanceName(string arg)
        {
            return arg != LOCALDB_INSTANCE_2014_AND_LATER;
        }

        private static string VersionedName(string arg)
        {
            return arg.StartsWith("v") && decimal.TryParse(arg.Substring(1), out decimal _)
                ? arg.Substring(1)
                : "99999999999999999999";
        }

        private const string LOCALDB_INSTANCE_2014_AND_LATER = "MSSQLLocalDB";


        private static Process RunToCompletion(string toRun)
        {
            var process = new Process
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
                }
            };
            process.Start();
            process.WaitForExit();
            return process;
        }

        private static string FindLocalDbUtility()
        {
            return SafeWalk.EnumerateFiles("C:\\Program Files\\Microsoft SQL Server", "SqlLocalDb.exe",
                    SearchOption.AllDirectories).OrderBy(p => p)
                .LastOrDefault();
        }
    }
}