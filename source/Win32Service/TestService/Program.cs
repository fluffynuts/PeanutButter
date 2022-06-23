using System.Collections.Generic;
using System.Linq;
using PeanutButter.EasyArgs;
using PeanutButter.ServiceShell;

namespace TestService
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static int Main(string[] args)
        {
            TotallyNotInterestingService.Options =
                args.ParseTo<TotallyNotInterestingService.CliOptions>(
                    out var uncollected,
                    new ParserOptions()
                    {
                        IgnoreUnknownSwitches = true
                    });
            return Shell.RunMain<TotallyNotInterestingService>(
                uncollected
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