using System.Collections.Generic;
using System.Linq;
using PeanutButter.ServiceShell;

namespace TestService
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static int Main(string[] args)
        {
            var argsList = new List<string>(args);
            var providedName = argsList.FindParameters("-n", "--name")
                .FirstOrDefault();
            TotallyNotInterestingService.CliServiceName = providedName;
            return Shell.RunMain<TotallyNotInterestingService>(
                argsList.ToArray()
            );
        }
    }

    public static class Args
    {
        public static bool FindFlag(
            this IList<string> args,
            params string[] switches)
        {
            return args.TryFindFlag(switches)
                ?? false;
        }

        public static bool? TryFindFlag(
            this IList<string> args,
            params string[] switches)
        {
            return switches.Aggregate(
                null as bool?,
                (acc, cur) =>
                {
                    int idx;
                    while ((idx = args.IndexOf(cur)) > -1)
                    {
                        args.RemoveAt(idx);
                        acc = true;
                    }

                    return acc;
                });
        }

        public static string[] FindParameters(
            this IList<string> args,
            params string[] switches)
        {
            var result = new List<string>();
            var toRemove = new List<int>();
            var inSwitch = false;
            var idx = -1;
            foreach (var arg in args)
            {
                idx++;
                if (switches.Contains(arg))
                {
                    inSwitch = true;
                    toRemove.Add(idx);
                    continue;
                }

                if (!inSwitch)
                {
                    continue;
                }

                inSwitch = false;
                result.Add(arg);
                toRemove.Add(idx);
            }

            idx = -1;
            foreach (var removeIndex in toRemove)
            {
                idx++;
                args.RemoveAt(removeIndex - idx);
            }

            return result.ToArray();
        }
    }
}