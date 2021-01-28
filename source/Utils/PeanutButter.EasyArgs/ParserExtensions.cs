using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.EasyArgs.Attributes;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

namespace PeanutButter.EasyArgs
{
    /// <summary>
    /// Provides the extension methods to parse commandline arguments
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Simplest use-case: parse to provided type, ignoring extraneous
        /// command-line argument values; will, however, error on unknown switches
        /// </summary>
        /// <param name="arguments"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ParseTo<T>(
            this string[] arguments
        )
        {
            return arguments.ParseTo<T>(
                out _
            );
        }

        /// <summary>
        /// Parse to provided type and output all uncollected arguments. Useful
        /// if your app, for example, has some switched arguments and then accepts,
        /// eg, a collection of file paths
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="uncollected"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Full-control parsing:
        /// - collect stray arguments
        /// - override behavior
        ///   - LineWriter (default is Console.WriteLine)
        ///   - ExitOnError (default is true)
        ///   - ExitAction (default is Environment.Exit)
        ///   - ExitWhenShowingHelp (default is true)
        ///   - Message formatting (make your own messages for the following)
        ///     - ReportMultipleValuesForSingleValueArgument
        ///     - ReportConflict
        ///     - ReportUnknownArg
        ///     - ReportMissingRequiredOption
        ///     - NegateMessage
        ///     - DisplayHelp
        ///       - GenerateHelpHead
        ///       - GenerateArgumentHelp
        ///         - FormatOptionHelp
        ///           - FormatArg
        ///           - TypeAnnotationFor
        ///         - ConsoleWidth
        ///       - GenerateFooter
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="uncollected"></param>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ParseTo<T>(
            this string[] arguments,
            out string[] uncollected,
            ParserOptions options
        )
        {
            var lookup = GenerateSwitchLookupFor<T>();
            AddImpliedOptionsTo(lookup);
            var collected = Collect(arguments, out var ignored);
            var matched = TryMatch<T>(
                lookup,
                collected,
                out var unmatched,
                options
            );
            uncollected = unmatched.And(ignored);
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
                            options.ReportUnknownSwitch(
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
                        StoreSingleValue(opt, input, options, uncollectedArgs, acc, errored);
                    }

                    return acc;
                });

            VerifyRequiredOptions(result, lookup, options, errored);
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

        private static void VerifyRequiredOptions(
            Dictionary<string, object> result,
            Dictionary<string, CommandlineArgument> commandlineArguments,
            ParserOptions options,
            HashSet<string> errored)
        {
            var specified = new HashSet<string>(result.Keys);
            var missing = commandlineArguments.Values
                .Distinct()
                .Where(o => o.IsRequired && !specified.Contains(o.Key))
                .ToArray();
            missing.ForEach(opt =>
            {
                options.ReportMissingRequiredOption($"--{opt.LongName}");
                errored.Add(opt.Key);
            });
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
                .Where(o => !o.IsImplicit && o.ConflictsWithKeys.Any())
                .Select(o => new { o.Key, ConflictsWith = o.ConflictsWithKeys })
                .ToArray();
            if (!canConflict.Any())
            {
                return;
            }

            var errored = false;
            var reported = new HashSet<StringPair>();
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
                        var thisConflict = new StringPair(left, right);
                        var alreadyReported = reported.Contains(thisConflict);
                        if (alreadyReported)
                        {
                            return;
                        }

                        reported.Add(thisConflict);
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
                    .Where(o => o.LongName is not null)
                    .Select(o => o.LongName)
            );
        }

        private static HashSet<string> CollectShortNamesFrom<T>(List<CommandlineArgument> options)
        {
            return new(
                options
                    .Where(o => o.ShortName is not null)
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
                            ConflictsWithKeys = attribs.OfType<ConflictsWithAttribute>()
                                .Select(a => a.Value)
                                .ToArray(),
                            IsImplicit = false,
                            IsRequired = attribs.OfType<RequiredAttribute>().Any()
                        };

                        acc.Add(option);
                        return acc;
                    });
        }

        private static IDictionary<string, IHasValue> Collect(
            string[] args,
            out string[] ignored
        )
        {
            var lastSwitch = "";
            var afterDoubleDash = false;
            var ignoredCollection = new List<string>();
            var result = args.Aggregate(
                new Dictionary<string, IHasValue>(),
                (acc, cur) =>
                {
                    if (afterDoubleDash)
                    {
                        ignoredCollection.Add(cur);
                        return acc;
                    }

                    if (cur == "--")
                    {
                        afterDoubleDash = true;
                        return acc;
                    }

                    if (!cur.StartsWith("-"))
                    {
                        return acc.Add(lastSwitch, cur);
                    }

                    lastSwitch = cur;
                    return acc.Add(lastSwitch);
                });
            ignored = ignoredCollection.ToArray();
            return result;
        }
    }
}