using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.Args.Attributes;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

namespace PeanutButter.Args
{
    public static class ParserExtensions
    {
        public static T ParseTo<T>(
            this string[] arguments
        )
        {
            return arguments.ParseTo<T>(
                out _
            );
        }

        public static T ParseTo<T>(
            this string[] arguments,
            out string[] uncollected
        )
        {
            return arguments.ParseTo<T>(
                out uncollected,
                new ParserOptions()
            );
        }

        public static T ParseTo<T>(
            this string[] arguments,
            out string[] uncollected,
            ParserOptions options
        )
        {
            var lookup = GenerateSwitchLookupFor<T>();
            AddImpliedOptionsTo(lookup);
            var collected = Collect(arguments);
            var matched = TryMatch<T>(
                lookup,
                collected,
                out uncollected,
                options
            );
            var ducked = matched.ForceFuzzyDuckAs<T>(true);
            return typeof(T).IsConcrete()
                ? CreateTopMostCopyOf(ducked)
                : ducked;
        }

        private static T CreateTopMostCopyOf<T>(T asT)
        {
            // if given a POCO, the ducked type may have 'new'
            // props which will be cast away, so we re-generate
            // and copy top-most props to the clean result
            var cleanObj = Activator.CreateInstance<T>();
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ForEach(pi =>
                {
                    cleanObj.SetPropertyValue(
                        pi.Name,
                        asT.GetTopMostPropertyValue<object>(pi.Name)
                    );
                });
            return cleanObj;
        }

        private static void AddImpliedOptionsTo(
            Dictionary<string, CommandlineArgument> lookup
        )
        {
            AddFlagNegations(lookup);
            AddHelp(lookup);
        }

        private static void AddHelp(
            Dictionary<string, CommandlineArgument> lookup
        )
        {
            var opt = new CommandlineArgument()
            {
                Key = CommandlineArgument.HELP_FLAG_KEY,
                Default = false,
                IsFlag = true,
                Description = "shows this help",
                IsImplicit = true,
                LongName = "help",
                ShortName = "h"
            };
            if (lookup.ContainsKey(opt.ShortName))
            {
                opt.ShortName = "";
            }
            else
            {
                lookup[opt.ShortName] = opt;
            }

            if (lookup.ContainsKey(opt.LongName))
            {
                opt.LongName = "";
            }
            else
            {
                lookup[opt.LongName] = opt;
            }
        }

        private static void AddFlagNegations(Dictionary<string, CommandlineArgument> lookup)
        {
            var flags = lookup.Values
                .Distinct()
                .Where(o => o.IsFlag)
                .ToArray();
            flags.ForEach(f =>
            {
                var negated = f.Negate();
                negated.IsImplicit = true;
                if (!lookup.ContainsKey(negated.LongName))
                {
                    lookup[negated.LongName] = negated;
                }
            });
        }

        private static IDictionary<string, object> TryMatch<T>(
            Dictionary<string, CommandlineArgument> lookup,
            IDictionary<string, IHasValue> collected,
            out string[] uncollected,
            ParserOptions options)
        {
            var uncollectedArgs = new List<string>();
            var errored = new HashSet<string>();
            var result = collected.Aggregate(
                new Dictionary<string, object>(),
                (acc, cur) =>
                {
                    if (!lookup.TryGetValueFuzzy(cur.Key, out var opt))
                    {
                        if (!errored.Contains(cur.Key))
                        {
                            options.ReportUnknownArg(
                                cur.Key
                            );
                            errored.Add(cur.Key);
                        }

                        return acc;
                    }

                    var input = cur.Value;
                    var prop = opt.Key;
                    if (opt.AllowMultipleValues)
                    {
                        acc[prop] = input.AllValues;
                    }
                    else if (opt.IsFlag)
                    {
                        if (opt.IsHelpFlag)
                        {
                            options.DisplayHelp<T>(
                                lookup.Values
                                    .Distinct()
                                    .OrderBy(o => o.LongName)
                                    .ToArray()
                            );
                        }
                        else
                        {
                            StoreFlag(options, opt, acc, prop, errored);
                        }
                    }
                    else
                    {
                        StoreSingleValue(
                            opt,
                            input,
                            options,
                            uncollectedArgs,
                            acc,
                            errored
                        );
                    }

                    return acc;
                });

            if (errored.Any())
            {
                options.ExitIfRequired(ExitCodes.ARGUMENT_ERROR);
            }

            VerifyNoExplicitConflicts(
                result,
                lookup.Values.Distinct().ToArray(),
                options
            );

            AddMissingDefaults(
                result,
                lookup.Values.Distinct().ToArray()
            );

            uncollected = uncollectedArgs.ToArray();
            return result;
        }

        private static void StoreSingleValue(
            CommandlineArgument opt,
            IHasValue input,
            ParserOptions options,
            List<string> uncollectedArgs,
            Dictionary<string, object> store,
            HashSet<string> errored
        )
        {
            var prop = opt.Key;
            uncollectedArgs.AddRange(input.AllValues.Except(
                new[]
                {
                    input.SingleValue
                })
            );
            if (store.ContainsKey(prop))
            {
                if (!errored.Contains(opt.Key))
                {
                    errored.Add(opt.Key);
                    options.ReportMultipleValuesForSingleValueArgument($"--{opt.LongName}");
                }
            }

            store[prop] = input.SingleValue;
        }

        private static void StoreFlag(
            ParserOptions options,
            CommandlineArgument opt,
            Dictionary<string, object> acc,
            string prop,
            HashSet<string> errored)
        {
            var value = opt.Default ?? true;
            if (acc.TryGetValue(prop, out var existing) &&
                existing != value &&
                !errored.Contains(opt.Key))
            {
                errored.Add(opt.Key);
                options.ReportConflict($"--{opt.LongName}", $"--no-{opt.LongName}");
            }
            else
            {
                acc[opt.Key] = opt.Default ?? true;
            }
        }

        private static void VerifyNoExplicitConflicts(
            Dictionary<string, object> result,
            CommandlineArgument[] options,
            ParserOptions parserOptions)
        {
            var canConflict = options
                .Where(o => !o.IsImplicit && o.ConflictsWith.Any())
                .Select(o => new { o.Key, o.ConflictsWith })
                .ToArray();
            if (!canConflict.Any())
            {
                return;
            }

            var errored = false;
            var reported = new HashSet<(string left, string right)>();
            canConflict.ForEach(o =>
            {
                o.ConflictsWith.ForEach(conflict =>
                {
                    if (result.ContainsKey(o.Key) && result.ContainsKey(conflict))
                    {
                        errored = true;
                        var ordered = new[]
                            {
                                o.Key,
                                conflict
                            }.Select(n => options.FirstOrDefault(o => o.Key == n))
                            .OrderBy(o => o.LongName)
                            .ToArray();
                        var left = ordered[0].LongName;
                        var right = ordered[1].LongName;
                        var alreadyReported = reported.Contains((left, right)) ||
                            reported.Contains((right, left));
                        if (alreadyReported)
                        {
                            return;
                        }

                        reported.Add((left, right));
                        parserOptions.ReportConflict($"--{left}", $"--{right}");
                    }
                });
            });
            if (errored)
            {
                parserOptions.ExitIfRequired(ExitCodes.ARGUMENT_ERROR);
            }
        }

        private static void AddMissingDefaults(
            Dictionary<string, object> result,
            CommandlineArgument[] options)
        {
            options.ForEach(opt =>
            {
                if (result.ContainsKey(opt.Key))
                {
                    return;
                }

                if (opt.Default is null)
                {
                    return;
                }

                result[opt.Key] = opt.Default;
            });
        }

        private static Dictionary<string, CommandlineArgument> GenerateSwitchLookupFor<T>()
        {
            var options = GrokOptionsFor<T>();
            var shortNames = CollectShortNamesFrom<T>(options);
            var longNames = CollectLongNamesFrom<T>(options);
            var result = new Dictionary<string, CommandlineArgument>();
            options.OrderByDescending(o => o.IsImplicit).ForEach(opt =>
            {
                SetShortNameIfMissing(opt, shortNames);
                SetLongNameIfMissing(opt, longNames);
                if (!string.IsNullOrWhiteSpace(opt.ShortName))
                {
                    result[opt.ShortName] = opt;
                }

                if (!string.IsNullOrWhiteSpace(opt.LongName))
                {
                    result[opt.LongName] = opt;
                }
            });
            return result;
        }

        private static void SetShortNameIfMissing(CommandlineArgument opt, HashSet<string> existing)
        {
            if (!string.IsNullOrWhiteSpace(opt.ShortName))
            {
                return;
            }

            var firstChar = opt.Key[0].ToString();
            var potentials = new[] { firstChar.ToLowerInvariant(), firstChar.ToUpperInvariant() };
            var potential = potentials.FirstOrDefault(p => !existing.Contains(p));
            if (potential is null)
            {
                return;
            }

            existing.Add(potential);
            opt.ShortName = potential;
        }

        private static void SetLongNameIfMissing(CommandlineArgument opt, HashSet<string> existing)
        {
            if (!string.IsNullOrWhiteSpace(opt.LongName))
            {
                return;
            }

            var potential = opt.Key
                .ToKebabCase()
                .ToLowerInvariant();
            if (existing.Contains(potential))
            {
                return;
            }

            existing.Add(potential);
            opt.LongName = potential;
        }

        private static HashSet<string> CollectLongNamesFrom<T>(List<CommandlineArgument> options)
        {
            return new(
                options
                    .Where(o => !(o.LongName is null))
                    .Select(o => o.LongName)
            );
        }

        private static HashSet<string> CollectShortNamesFrom<T>(List<CommandlineArgument> options)
        {
            return new(
                options
                    .Where(o => !(o.ShortName is null))
                    .Select(o => o.ShortName)
            );
        }

        private static List<CommandlineArgument> GrokOptionsFor<T>()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Aggregate(
                    new List<CommandlineArgument>(),
                    (acc, cur) =>
                    {
                        var attribs = cur.GetCustomAttributes().ToArray();
                        var option = new CommandlineArgument()
                        {
                            ShortName = attribs
                                .OfType<ShortNameAttribute>()
                                .FirstOrDefault()
                                ?.Value,
                            LongName = attribs
                                .OfType<LongNameAttribute>()
                                .FirstOrDefault()
                                ?.Value,
                            Description = attribs
                                .OfType<DescriptionAttribute>()
                                .FirstOrDefault()
                                ?.Value,
                            Default = attribs
                                .OfType<DefaultAttribute>()
                                .FirstOrDefault()
                                ?.Value,
                            Property = cur,
                            ConflictsWith = attribs.OfType<ConflictsWithAttribute>()
                                .Select(a => a.Value)
                                .ToArray(),
                            IsImplicit = false
                        };

                        acc.Add(option);
                        return acc;
                    });
        }

        private static IDictionary<string, IHasValue> Collect(string[] args)
        {
            var lastSwitch = "";
            return args.Aggregate(
                new Dictionary<string, IHasValue>(),
                (acc, cur) =>
                {
                    if (!cur.StartsWith("-"))
                    {
                        return acc.Add(lastSwitch, cur);
                    }

                    lastSwitch = cur;
                    return acc.Add(lastSwitch);
                });
        }
    }
}