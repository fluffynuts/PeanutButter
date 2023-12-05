using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PeanutButter.EasyArgs.Attributes;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;
using PeanutButter.Utils.Dictionaries;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
{
    /// <summary>
    /// Provides the extension methods to parse commandline arguments
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        static class ParserExtensions
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
        /// Parse to the provided target type T with provided parser options
        /// Unrecognised commandline arguments will be discarded, however,
        /// depending on the provided options, the process may exit, printing
        /// and error.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ParseTo<T>(
            this string[] arguments,
            ParserOptions options
        )
        {
            return arguments.ParseTo<T>(out _, options);
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
            var flags = new HashSet<string>();
            var collections = new HashSet<string>();
            foreach (var kvp in lookup)
            {
                if (kvp.Value.IsFlag)
                {
                    flags.Add(kvp.Key);
                }
                else if (kvp.Value.GetType().IsCollection())
                {
                    collections.Add(kvp.Key);
                }
            }

            var collected = Collect(
                arguments,
                flags,
                out var ignored
            );
            if (options.FallbackOnEnvironmentVariables)
            {
                collected = new MergeDictionary<string, IHasValue>(
                    collected,
                    GrabEnvVars<T>()
                );
            }

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

        private static readonly char[] FuzzyEnvVars =
        {
            '.',
            '-',
            '_',
            ':'
        };

        private static IDictionary<string, IHasValue> GrabEnvVars<T>()
        {
            var propertyNames = new HashSet<string>(
                typeof(T).GetProperties()
                    .Select(p => p.Name.ToLower())
                    .ToArray()
            );
            var result = new Dictionary<string, IHasValue>();
            foreach (DictionaryEntry e in Environment.GetEnvironmentVariables())
            {
                var key = $"{e.Key}";
                var k = $"{e.Key}".RemoveAll(
                    FuzzyEnvVars
                ).ToLower();
                if (propertyNames.Contains(k))
                {
                    result[key] = new StringCollection($"{e.Value}");
                }
            }

            return result;
        }

        private static T CreateTopMostCopyOf<T>(T asT)
        {
            // if given a POCO, the ducked type may have 'new'
            // props which will be cast away, so we re-generate
            // and copy top-most props to the clean result
            var cleanObj = Activator.CreateInstance<T>();
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ForEach(
                    pi =>
                    {
                        cleanObj.SetPropertyValue(
                            pi.Name,
                            asT.GetTopMostPropertyValue<object>(pi.Name)
                        );
                    }
                );
            return cleanObj;
        }

        /// <summary>
        /// Manually print the help for the provided parse target and options
        /// </summary>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        public static void PrintHelpFor<T>(ParserOptions options)
        {
            var lookup = GenerateSwitchLookupFor<T>();
            AddHelp(lookup);
            options.DisplayHelp<T>(
                lookup.Values.Distinct().ToArray()
            );
        }

        private static void AddImpliedOptionsTo(
            IDictionary<string, CommandlineArgument> lookup
        )
        {
            AddFlagNegations(lookup);
            AddHelp(lookup);
        }

        private static void AddHelp(
            IDictionary<string, CommandlineArgument> lookup
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

        private static void AddFlagNegations(IDictionary<string, CommandlineArgument> lookup)
        {
            var flags = lookup.Values
                .Distinct()
                .Where(o => o.IsFlag)
                .ToArray();
            flags.ForEach(
                f =>
                {
                    var negated = f.CloneNegated();
                    negated.IsImplicit = true;
                    if (!lookup.ContainsKey(negated.LongName))
                    {
                        lookup[negated.LongName] = negated;
                    }
                }
            );
        }

        private static bool TryFindOption(
            string key,
            IDictionary<string, CommandlineArgument> lookup,
            HashSet<string> errored,
            ParserOptions options,
            out CommandlineArgument result
        )
        {
            result = null;
            if (lookup.TryGetValueFuzzy(key, out result))
            {
                return true;
            }

            if (options.IgnoreUnknownSwitches)
            {
                return false;
            }

            if (!errored.Contains(key))
            {
                options.ReportUnknownSwitch(key);
                errored.Add(key);
            }

            return false;
        }

        private static IDictionary<string, object> TryMatch<T>(
            IDictionary<string, CommandlineArgument> lookup,
            IDictionary<string, IHasValue> collected,
            out string[] unmatched,
            ParserOptions options
        )
        {
            var uncollectedArgs = new List<string>();
            var errored = new HashSet<string>();
            var result = collected.Aggregate(
                new Dictionary<string, object>(),
                (acc, cur) =>
                {
                    if (cur.Key == "")
                    {
                        uncollectedArgs.AddRange(cur.Value.AllValues);
                        return acc;
                    }

                    if (!TryFindOption(cur.Key, lookup, errored, options, out var opt))
                    {
                        uncollectedArgs.Add(cur.Key);
                        uncollectedArgs.AddRange(cur.Value.AllValues);
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
                            StoreFlag(
                                options,
                                opt,
                                acc,
                                prop,
                                errored,
                                lookup,
                                collected
                            );
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
                }
            );

            VerifyRequiredOptions(result, lookup, options, errored);
            if (errored.Any())
            {
                if (options.ShowHelpOnArgumentError)
                {
                    PrintHelpFor<T>(options);
                }

                options.ExitIfRequired(ExitCodes.ARGUMENT_ERROR);
            }

            VerifyNumericRanges(result, lookup, options, errored);
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


            unmatched = uncollectedArgs.ToArray();
            return result;
        }

        private static void VerifyNumericRanges(
            Dictionary<string, object> result,
            IDictionary<string, CommandlineArgument> commandlineArguments,
            ParserOptions options,
            HashSet<string> errored
        )
        {
            var specified = new HashSet<string>(result.Keys);
            var hasMin = commandlineArguments.Values.Distinct()
                .Where(o => o.MinValue is not null && specified.Contains(o.Key))
                .ToArray();
            hasMin.ForEach(
                opt =>
                {
                    var stringValue = result[opt.Key] as string;
                    var dd = new DecimalDecorator(stringValue);
                    if (!dd.IsValidDecimal)
                    {
                        return;
                    }

                    var value = dd.ToDecimal();

                    if (value < opt.MinValue)
                    {
                        options.ReportMinimumViolation(
                            $"--{opt.LongName}",
                            opt.MinValue,
                            value
                        );
                        errored.Add(opt.Key);
                    }

                    if (value > opt.MaxValue)
                    {
                        options.ReportMaximumViolation(
                            $"--{opt.LongName}",
                            opt.MaxValue,
                            value
                        );
                        errored.Add(opt.Key);
                    }
                }
            );
        }

        private static void VerifyRequiredOptions(
            Dictionary<string, object> result,
            IDictionary<string, CommandlineArgument> commandlineArguments,
            ParserOptions options,
            HashSet<string> errored
        )
        {
            var specified = new HashSet<string>(result.Keys);
            var missing = commandlineArguments.Values
                .Distinct()
                .Where(o => o.IsRequired && !specified.Contains(o.Key))
                .ToArray();
            missing.ForEach(
                opt =>
                {
                    options.ReportMissingRequiredOption(opt);
                    errored.Add(opt.Key);
                }
            );
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
            uncollectedArgs.AddRange(
                input.AllValues.Except(
                    new[]
                    {
                        input.SingleValue
                    }
                )
            );
            if (store.ContainsKey(prop))
            {
                if (!errored.Contains(opt.Key))
                {
                    errored.Add(opt.Key);
                    options.ReportMultipleValuesForSingleValueArgument($"--{opt.LongName}");
                }
            }

            if (opt.VerifyFileExists &&
                // only error if the value was set; if it's required, it should be
                // marked [Required] too, and that validation should fail on its own
                !string.IsNullOrWhiteSpace(input.SingleValue)
               )
            {
                if (!File.Exists(input.SingleValue))
                {
                    errored.Add(opt.Key);
                    options.ReportMissingFile($"--{opt.LongName}", input.SingleValue);
                }
            }

            if (opt.VerifyFolderExists &&
                // only error if the value was set; if it's required, it should be
                // marked [Required] too, and that validation should fail on its own
                !string.IsNullOrWhiteSpace(input.SingleValue)
               )
            {
                if (!Directory.Exists(input.SingleValue))
                {
                    errored.Add(opt.Key);
                    options.ReportMissingFile($"--{opt.LongName}", input.SingleValue);
                }
            }

            store[prop] = input.SingleValue;
        }

        private static void StoreFlag(
            ParserOptions options,
            CommandlineArgument opt,
            Dictionary<string, object> acc,
            string prop,
            HashSet<string> errored,
            IDictionary<string, CommandlineArgument> lookup,
            IDictionary<string, IHasValue> collected
        )
        {
            var value = opt.Default ?? true;
            if (acc.TryGetValue(prop, out var existing) &&
                existing != value &&
                !errored.Contains(opt.Key))
            {
                errored.Add(opt.Key);
                var specifiedSwitches = collected.Keys
                    .Where(opt.HasSwitch)
                    .Distinct()
                    .ToArray();

                var negation = lookup.Values.FirstOrDefault(
                    arg => arg.Key == opt.Key && arg != opt
                );

                var negativeConflicts = negation is null
                    ? new string[0]
                    : collected.Keys
                        .Where(negation.HasSwitch)
                        .Distinct()
                        .ToArray();

                var allPossibleConflicts = lookup.Values.Where(
                        arg => arg.ConflictsWithKeys.Contains(opt.Key)
                    )
                    .Except(new[] { opt })
                    .Distinct()
                    .ToArray();

                var allSpecifiedDirectConflicts = allPossibleConflicts
                    .Select(a => new[] { a.LongSwitch, a.ShortSwitch })
                    .Flatten()
                    .Intersect(specifiedSwitches)
                    .ToArray();

                var allConflicts = negativeConflicts.Union(allSpecifiedDirectConflicts)
                    .Distinct()
                    .ToArray();

                allConflicts.ForEach(
                    conflict =>
                    {
                        specifiedSwitches.ForEach(
                            sw =>
                            {
                                options.ReportConflict(sw, conflict);
                            }
                        );
                    }
                );
            }
            else
            {
                acc[opt.Key] = !opt.IsNegatedFlag;
            }
        }

        private static void VerifyNoExplicitConflicts(
            Dictionary<string, object> result,
            CommandlineArgument[] options,
            ParserOptions parserOptions
        )
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
            canConflict.ForEach(
                o =>
                {
                    o.ConflictsWith.ForEach(
                        conflict =>
                        {
                            if (result.ContainsKey(o.Key) && result.ContainsKey(conflict))
                            {
                                errored = true;
                                var ordered = new[]
                                    {
                                        o.Key,
                                        conflict
                                    }.Select(n => options.FirstOrDefault(opt => opt.Key == n))
                                    .OrderBy(opt => opt?.LongName)
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
                        }
                    );
                }
            );
            if (errored)
            {
                parserOptions.ExitIfRequired(ExitCodes.ARGUMENT_ERROR);
            }
        }

        private static void AddMissingDefaults(
            Dictionary<string, object> result,
            CommandlineArgument[] options
        )
        {
            options.ForEach(
                opt =>
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
                }
            );
        }

        private static readonly ConcurrentDictionary<Type, IDictionary<string, CommandlineArgument>>
            SwitchCache = new();

        private static IDictionary<string, CommandlineArgument> GenerateSwitchLookupFor<T>()
        {
            if (SwitchCache.TryGetValue(typeof(T), out var result))
            {
                return result.Clone();
            }

            var options = GrokOptionsFor<T>();
            var shortNames = CollectShortNamesFrom(options);
            var longNames = CollectLongNamesFrom(options);
            result = new Dictionary<string, CommandlineArgument>();
            options.OrderByDescending(o => o.IsImplicit).ForEach(
                opt =>
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
                }
            );
            SwitchCache.TryAdd(typeof(T), result.Clone());
            return result;
        }

        private static void SetShortNameIfMissing(
            CommandlineArgument opt,
            HashSet<string> existing
        )
        {
            if (!string.IsNullOrWhiteSpace(opt.ShortName))
            {
                return;
            }

            if (opt.DisableShortNameGeneration)
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

        private static void SetLongNameIfMissing(
            CommandlineArgument opt,
            HashSet<string> existing
        )
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

        private static HashSet<string> CollectLongNamesFrom(
            List<CommandlineArgument> options
        )
        {
            return new(
                options
                    .Where(o => o.LongName is not null)
                    .Select(o => o.LongName)
            );
        }

        private static HashSet<string> CollectShortNamesFrom(
            List<CommandlineArgument> options
        )
        {
            return new(
                options
                    .Where(o => o.ShortName is not null)
                    .Select(o => o.ShortName)
            );
        }

        private static List<CommandlineArgument> GrokOptionsFor<T>()
        {
            return GetAllPropertiesOf<T>()
                .Where(pi => pi.GetCustomAttributes().OfType<IgnoreAttribute>().IsEmpty())
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
                            DisableShortNameGeneration = attribs
                                .OfType<DisableGeneratedShortNameAttribute>()
                                .Any(),
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
                            IsRequired = attribs.OfType<RequiredAttribute>().Any(),
                            MinValue = attribs.OfType<MinAttribute>()
                                .FirstOrDefault()?.Value,
                            MaxValue = attribs.OfType<MaxAttribute>()
                                .FirstOrDefault()?.Value,
                            VerifyFileExists = attribs.OfType<ExistingFileAttribute>()
                                .FirstOrDefault() is not null,
                            VerifyFolderExists = attribs.OfType<ExistingFolderAttribute>()
                                .FirstOrDefault() is not null
                        };

                        acc.Add(option);
                        return acc;
                    }
                );
        }

        private static PropertyInfo[] GetAllPropertiesOf<T>()
        {
            var type = typeof(T);
            if (!type.IsInterface)
            {
                return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            }

            return type.GetAllImplementedInterfaces()
                .SelectMany(o => o.GetProperties())
                .ToArray();
        }

        private static IDictionary<string, IHasValue> Collect(
            string[] args,
            HashSet<string> flags,
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

                    var thisArgIsNotASwitch = (!cur.StartsWith("-") || IsNumeric(cur));
                    var lastSwitchIsNotAFlag = !flags.Contains(lastSwitch.TrimStart('-'));
                    if (thisArgIsNotASwitch && lastSwitchIsNotAFlag)
                    {
                        return acc.Add(lastSwitch, cur);
                    }

                    lastSwitch = cur;
                    return acc.Add(lastSwitch);
                }
            );
            ignored = ignoredCollection.ToArray();
            return result;
        }

        private static bool IsNumeric(string value)
        {
            var dd = new DecimalDecorator(value);
            return dd.IsValidDecimal;
        }

        /// <summary>
        /// Generates an args array from the properties
        /// of the incoming object, preferring long names
        /// for options over short names
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string[] GenerateArgs(
            this object obj
        )
        {
            if (obj is null)
            {
                return new string[0];
            }

            return obj.GetType()
                .GetProperties()
                .Select(pi => GenerateArgsPairFor(pi, obj))
                .SelectMany(o => o)
                .ToArray();
        }

        private static string[] GenerateArgsPairFor(
            PropertyInfo pi,
            object o
        )
        {
            if (pi.PropertyType == typeof(bool))
            {
                var propValue = (bool) pi.GetValue(o);
                return new[]
                {
                    propValue
                        ? FindNameFor(pi)
                        : FindNameFor(pi).RegexReplace("^--", "--no-")
                };
            }

            var name = FindNameFor(pi);
            var rawValue = pi.GetValue(o);

            var value = Stringify(rawValue);
            return new[] { name }
                .Concat(value)
                .ToArray();
        }

        private static string[] Stringify(object o)
        {
            if (o is null)
            {
                return new[] { "" };
            }

            if (o is string str)
            {
                return new[] { str };
            }

            var enumerable = new EnumerableWrapper(o);
            if (!enumerable.IsValid)
            {
                return new[] { o.ToString() };
            }

            var result = new List<string>();
            foreach (var item in enumerable)
            {
                result.Add(item?.ToString());
            }

            return result
                .Where(s => s is not null)
                .ToArray();
        }

        private static string FindNameFor(PropertyInfo pi)
        {
            var attribs = pi.GetCustomAttributes()
                .ToArray();
            var longName = attribs.OfType<LongNameAttribute>()
                .FirstOrDefault()?.Value.PrependString("--");
            var shortName = attribs.OfType<ShortNameAttribute>()
                .FirstOrDefault()?.Value.PrependString("-");
            var fallback = pi.Name.ToKebabCase().PrependString("--");
            return longName ?? shortName ?? fallback;
        }

        private static string PrependString(
            this string str,
            string toPrepend
        )
        {
            return str is null
                ? null
                : $"{toPrepend}{str}";
        }
    }
}