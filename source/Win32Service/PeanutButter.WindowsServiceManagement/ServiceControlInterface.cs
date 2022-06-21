#if NETSTANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Imported.PeanutButter.Utils;

[assembly: InternalsVisibleTo("PeanutButter.WindowsServiceManagement.Core.Tests")]

namespace PeanutButter.WindowsServiceManagement
{
    internal static class ServiceControlKeys
    {
        public const string SERVICE_NAME = "SERVICE_NAME";
        public const string TYPE = "TYPE";
        public const string STATE = "STATE";
        public const string WIN32_EXIT_CODE = "WIN32_EXIT_CODE";
        public const string SERVICE_EXIT_CODE = "SERVICE_EXIT_CODE";
        public const string WAIT_HINT = "WAIT_HINT";
        public const string PROCESS_ID = "PID";
        public const string FLAGS = "FLAGS";
    }

    internal interface IServiceControlInterface
    {
        IDictionary<string, string> QueryEx(string serviceName);
        IDictionary<string, string> QueryConfiguration(string serviceName);
    }

    internal class ServiceControlInterface : IServiceControlInterface
    {
        public IDictionary<string, string> QueryEx(string serviceName)
        {
            using var io = ProcessIO.Start(
                "sc", "queryex", serviceName
            );
            var result = new Dictionary<string, string>();
            var lastKey = null as string;
            foreach (var line in io.StandardOutput)
            {
                if (TryParseKeyAndValueFrom(line, out var key, out var value))
                {
                    result[key] = value;
                    lastKey = key;
                }
                else
                {
                    if (lastKey is not null)
                    {
                        result[lastKey] += $" {line.Trim()}";
                    }
                }
            }

            return result;
        }

        private bool TryParseKeyAndValueFrom(string line, out string key, out string value)
        {
            var parts = line.Split(':');
            if (parts.Length < 2)
            {
                key = default;
                value = default;
                return false;
            }

            key = parts.First().Trim();
            value = parts.Skip(1).JoinWith(":").Trim();
            return true;
        }

        public IDictionary<string, string> QueryConfiguration(string serviceName)
        {
            throw new NotImplementedException();
        }
    }

    public interface IWindowsServiceUtil
    {
    }

    public class WindowsServiceUtil : IWindowsServiceUtil
    {
    }
}
#endif