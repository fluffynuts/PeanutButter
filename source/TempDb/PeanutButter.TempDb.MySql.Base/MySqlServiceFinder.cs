using System;
using System.IO;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql.Base
{
    /// <summary>
    /// Utility to try to find the path to the MySQL service binary on Windows
    /// </summary>
    public static class MySqlWindowsServiceFinder
    {
        /// <summary>
        /// Attempt to determine the path to mysqld.exe on windows, using the sc utility
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string FindPathToMySqlD()
        {
            var mysqlServiceName = FindFirstMySqlServiceName();
            return mysqlServiceName == null
                ? null
                : FindPathForService(mysqlServiceName);
        }

        public static string FindPathTo(string util)
        {
            if (string.IsNullOrWhiteSpace(util))
            {
                throw new ArgumentException(
                    "util not specified",
                    nameof(util)
                );
            }

            var mysqld = FindPathToMySqlD();
            if (mysqld is null)
            {
                return null;
            }

            var container = Path.GetDirectoryName(mysqld);
            if (container is null)
            {
                throw new Exception("Unable to determine the containing folder for mysqld");
            }

            if (!util.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                util = $"{util}.exe";
            }

            var seek = Path.Combine(container, util);
            return File.Exists(seek)
                ? seek
                : null;
        }

        private static string FindPathForService(string mysqlServiceName)
        {
            using var io = ProcessIO.Start("sc", "qc", mysqlServiceName);
            var commandLine = io.StandardOutput
                .FirstOrDefault(line => line.Trim().StartsWith("BINARY_PATH_NAME"))
                ?.Split(':')
                .Skip(1)
                .JoinWith(":").Trim();
            if (commandLine == null)
            {
                return null;
            }

            if (commandLine.StartsWith("\""))
            {
                var nextQuote = commandLine.IndexOf("\"", 2, StringComparison.InvariantCulture);
                return commandLine.Substring(1, nextQuote - 1);
            }

            return commandLine.Split(' ').First();
        }

        public static string FindFirstMySqlServiceName()
        {
            using var io = ProcessIO.Start("sc", "query", "state=", "all");
            var possibleServices = io.StandardOutput
                .Where(
                    l =>
                    {
                        var lower = l.ToLower();
                        return lower.StartsWith("service_name") &&
                            lower.Contains("mysql");
                    }
                ).Select(
                    s => s.Split(':').Last().Trim()
                )
                .OrderByDescending(s => s)
                .ToArray();
            if (!possibleServices.Any())
            {
                return null;
            }
            var preferred = Environment.GetEnvironmentVariable(EnvironmentVariables.PREFERRED_SERVICE);
            if (preferred is null)
            {
                return possibleServices[0];
            }
            var preferredMatch = possibleServices.FirstOrDefault(s => s.Equals(preferred, StringComparison.OrdinalIgnoreCase));
            return preferredMatch ?? possibleServices[0];
        }
    }
}