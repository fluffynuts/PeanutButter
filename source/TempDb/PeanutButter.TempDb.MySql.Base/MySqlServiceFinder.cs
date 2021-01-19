using System;
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
        public static string FindPathToMySql()
        {
            var mysqlServiceName = FindFirstMySqlServiceName();
            return mysqlServiceName == null 
                ? null 
                : FindPathForService(mysqlServiceName);
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

        private static string FindFirstMySqlServiceName()
        {
            using var io = ProcessIO.Start("sc", "query", "state=", "all");
            return io.StandardOutput
                .FirstOrDefault(
                    l =>
                    {
                        var lower = l.ToLower();
                        return lower.StartsWith("service_name") &&
                            lower.Contains("mysql");
                    })?.Split(':').Last().Trim();
        }
    }
}