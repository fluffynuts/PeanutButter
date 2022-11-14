using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Imported.PeanutButter.Utils;
using PeanutButter.WindowsServiceManagement.Exceptions;

[assembly: InternalsVisibleTo("PeanutButter.WindowsServiceManagement.Core.Tests")]

namespace PeanutButter.WindowsServiceManagement
{
    public interface IServiceControlInterface
    {
        /// <summary>
        /// Query all information (queryex and config) about a service by name
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        IDictionary<string, string> QueryAll(string serviceName);
        /// <summary>
        /// Perform a queryex for a service by name
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        IDictionary<string, string> QueryEx(string serviceName);
        /// <summary>
        /// Query service configuration by service name
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        IDictionary<string, string> QueryConfiguration(string serviceName);

        /// <summary>
        /// Run a raw sc.exe command with the provided arguments and get back
        /// the raw-ish key-value pairs of information
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        IDictionary<string, string> RunServiceControl(params string[] args);
        /// <summary>
        /// List all known services on the local machine
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> ListAllServices();
        /// <summary>
        /// Attempt to identify the service associated with the provided
        /// process id. If the process id is not for a service, the return
        /// value will be null.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        string FindServiceByPid(int pid);
    }

    public class ServiceControlInterface : IServiceControlInterface
    {
        public string FindServiceByPid(int pid)
        {
            using var io = ProcessIO.Start(
                "sc", "queryex"
            );
            var lastServiceName = "";
            foreach (var line in io.StandardOutput)
            {
                if (!TryParseKeyAndValueFrom(line, out var key, out var value))
                {
                    continue;
                }

                if (key == ServiceControlKeys.SERVICE_NAME)
                {
                    lastServiceName = value;
                    continue;
                }

                if (key == ServiceControlKeys.PROCESS_ID)
                {
                    if (!int.TryParse(value, out var thisPid))
                    {
                        continue;
                    }

                    if (thisPid == pid)
                    {
                        return lastServiceName;
                    }
                }
            }
            return null;
        }

        public IEnumerable<string> ListAllServices()
        {
            using var io = ProcessIO.Start(
                "sc", "query", "state=", "all"
            );
            foreach (var line in io.StandardOutput)
            {
                if (!TryParseKeyAndValueFrom(line, out var key, out var value))
                {
                    continue;
                }

                if (key == ServiceControlKeys.SERVICE_NAME)
                {
                    yield return value;
                }
            }
        }

        public IDictionary<string, string> QueryAll(string serviceName)
        {
            return QueryEx(serviceName)
                .MergedWith(QueryConfiguration(serviceName));
        }

        public IDictionary<string, string> QueryEx(string serviceName)
        {
            return RunServiceControl("queryex", serviceName);
        }

        public IDictionary<string, string> QueryConfiguration(string serviceName)
        {
            return RunServiceControl(
                (key, value) => key == ServiceControlKeys.BINARY_PATH_NAME
                    ? value.Trim('"')
                    : value,
                "qc",
                serviceName
            );
        }

        public IDictionary<string, string> RunServiceControl(
            params string[] args
        )
        {
            return RunServiceControl(
                (_, value) => value,
                args
            );
        }

        private static readonly Dictionary<int, Action<string[], string>>
            ServiceControlErrorHandlers = new()
            {
                [1060] = HandleServiceNotFound,
                [1639] = HandleBadServiceControlCommandline
            };

        private static void HandleBadServiceControlCommandline(
            string[] args,
            string output)
        {
            throw new InvalidOperationException(
                $@"The following sc.exe commandline was invalid:\n{
                    new Commandline("sc.exe", args)
                }\nThis is an error in PeanutButter. Please report it.\nSC output:\n{
                    output
                }"
            );
        }

        private static void HandleServiceNotFound(
            string[] args,
            string output)
        {
            throw new ServiceNotInstalledException(
                args.Last(),
                output
            );
        }

        private IDictionary<string, string> RunServiceControl(
            Func<string, string, string> mutator,
            params string[] args
        )
        {
            using var io = ProcessIO.Start(
                "sc", EscapeQuotesIn(args)
            );
            io.Process.WaitForExit();
            if (io.ExitCode != 0)
            {
                var lines = io.StandardOutput
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray()
                    .JoinWith(Environment.NewLine);
                if (!ServiceControlErrorHandlers.TryGetValue(io.ExitCode, out var handler))
                {
                    var startInfo = io.Process.StartInfo;
                    throw new ServiceControlException(
                        lines,
                        $"{startInfo.FileName.QuoteIfSpaced()} {startInfo.Arguments}"
                    );
                }

                handler(args, lines);
            }

            var result = new Dictionary<string, string>();
            var lastKey = null as string;
            foreach (var line in io.StandardOutput)
            {
                if (TryParseKeyAndValueFrom(line, out var key, out var value))
                {
                    result[key] = mutator(key, value);
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

        private string[] EscapeQuotesIn(string[] args)
        {
            var result = new List<string>();
            foreach (var arg in args)
            {
                result.Add(arg.Replace("\"", "\\\""));
            }

            return result.ToArray();
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
    }
}