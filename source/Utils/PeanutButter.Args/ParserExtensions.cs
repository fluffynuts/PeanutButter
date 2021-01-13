using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

namespace PeanutButter.Args
{
    public class ArgParserOptions
    {
        public Action<string> LineWriter { get; set; } = Console.WriteLine;
        public Action<int> ExitAction { get; set; } = Environment.Exit;
        public bool ExitWhenShowingHelp { get; set; } = true;
        public bool ShowedHelp { get; set; } = false;
        public bool ExitOnError { get; set; } = true;

        public virtual void ReportMultipleValuesForSingleValueArgument(string arg)
        {
            LineWriter(
                $"{arg} specified more than once but only accepts one value"
            );
        }

        public virtual void ReportConflict(string left, string right)
        {
            LineWriter(
                $"{left} conflicts with {right}"
            );
        }

        public virtual void ReportUnknownArg(string arg)
        {
            LineWriter(
                $"unknown option: {arg}"
            );
        }

        protected virtual string NegateMessage =>
            "Negate any flag argument with --no-{option}";

        public virtual void DisplayHelp<T>(Option[] options)
        {
            if (ShowedHelp)
            {
                return;
            }

            ShowedHelp = true;

            var head = GenerateHelpHead<T>();
            var headSpacer = head.Any()
                ? OneLine
                : NoLines;
            var body = GenerateArgumentHelp<T>(options.Where(
                o => !o.IsImplicit || o.Key == Option.HELP_FLAG_KEY
            ).ToArray());
            var footer = GenerateHelpFooter<T>();
            var footerSpacer = footer.Any()
                ? OneLine
                : NoLines;
            var negateMessage = options.Any(o => o.IsFlag)
                ? new[] { "", NegateMessage }
                : NoLines;
            head
                .And(headSpacer)
                .And(body)
                .And(negateMessage)
                .And(footerSpacer)
                .And(footer)
                .ForEach(s => LineWriter(s.TrimEnd()));

            if (ExitOnError)
            {
                ExitAction?.Invoke(ExitCodes.SHOWED_HELP);
            }

            ShowedHelp = true;
        }

        private static readonly string[] OneLine = { "" };
        private static readonly string[] NoLines = new string[0];

        protected virtual string[] GenerateHelpHead<T>()
        {
            return ReadTextAttributes<T, DescriptionAttribute>();
        }

        private const int SHORT_NAME_LENGTH = 1;
        private static readonly int DASH_LENGTH = "-".Length;
        private const string LEFT_BRACKET = "[";
        private const string RIGHT_BRACKET = "]";
        private static readonly int LEFT_BRACKET_LENGTH = LEFT_BRACKET.Length;
        private static readonly int RIGHT_BRACKET_LENGTH = RIGHT_BRACKET.Length;
        private static readonly int COMMA_AND_SPACE_LENGTH = ", ".Length;
        private static readonly int SINGLE_SPACE_LENGTH = " ".Length;
        private static readonly int COLUMN_PADDING_LENGTH = 1;

        protected virtual string[] GenerateArgumentHelp<T>(Option[] options)
        {
            var result = new List<string>();
            var longestLeftCol = options.Select(o =>
                DASH_LENGTH +
                SHORT_NAME_LENGTH +
                COMMA_AND_SPACE_LENGTH +
                DASH_LENGTH + DASH_LENGTH +
                o.LongName.Length +
                SINGLE_SPACE_LENGTH +
                LEFT_BRACKET_LENGTH +
                o.Type.Length +
                RIGHT_BRACKET_LENGTH +
                COLUMN_PADDING_LENGTH
            ).Max();
            options.ForEach(opt =>
            {
                result.Add(FormatOptionHelp(
                        opt.ShortName,
                        opt.LongName,
                        opt.Type,
                        opt.IsFlag,
                        FormatDescriptionText(opt.Description, opt.IsFlag, opt.Default),
                        longestLeftCol,
                        ConsoleWidth
                    )
                );
            });
            return result
                .ToArray();
        }

        protected virtual string FormatDescriptionText(
            string description,
            bool isFlag,
            object defaultValue
        )
        {
            if (isFlag)
            {
                return description;
            }

            var space = string.IsNullOrWhiteSpace(description)
                ? ""
                : " ";
            return defaultValue is null
                ? description?.Trim() ?? ""
                : $"{description?.Trim()}{space}({defaultValue})";
        }

        protected virtual string TypeAnnotationFor(string type, bool isFlag)
        {
            if (string.IsNullOrWhiteSpace(type) || isFlag)
            {
                return "";
            }

            return $" {LEFT_BRACKET}{type}{RIGHT_BRACKET}";
        }

        protected string FormatOptionHelp(
            string shortName,
            string longName,
            string type,
            bool isFlag,
            string helpText,
            int leftColWidth,
            int maxLineWidth
        )
        {
            var left = $@"{
                    FormatArg(shortName, 1, true)
                } {
                    FormatArg(longName, 2, false)
                }{
                    TypeAnnotationFor(type, isFlag)
                }".PadRight(leftColWidth);
            var rightWords = (helpText ?? "").Trim().Split(' ').ToList();
            if (!rightWords.Any())
            {
                return left;
            }

            var result = new List<string>();
            while (rightWords.Any())
            {
                if (result.Count == 1)
                {
                    left = new String(' ', left.Length);
                }

                result.Add(GrabWords(left, rightWords, maxLineWidth));
            }

            return result.JoinWith("\n");
        }

        protected string FormatArg(string name, int dashes, bool isFirst)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new String(' ', dashes + 1 + (isFirst
                        ? 1
                        : 0)
                );
            }

            return $"{new String('-', dashes)}{name}{(isFirst ? "," : "")}";
        }

        protected virtual string NegatedFlagLeft(
            string flagName,
            int columnWidth
        )
        {
            return $"    --no-{flagName}".PadRight(columnWidth);
        }

        private string GrabWords(
            string left,
            List<string> words,
            int maxLineWidth
        )
        {
            if (!words.Any())
            {
                return left;
            }

            var result = new List<string>()
            {
                left
            };
            var haveAddedWord = false;
            var totalLength = result.JoinWith(" ").Length;
            while (words.Any() && totalLength + words.First().Length + 1 < maxLineWidth)
            {
                haveAddedWord = true;
                result.Add(words[0]);
                words.RemoveAt(0);
                totalLength = result.JoinWith(" ").Length;
            }

            if (!haveAddedWord)
            {
                result.Add(words.Shift());
            }

            return result.JoinWith(" ");
        }

        public virtual int ConsoleWidth => TryReadConsoleWidth();

        private int TryReadConsoleWidth()
        {
            try
            {
                return Console.WindowWidth;
            }
            catch
            {
                return 80;
            }
        }

        protected virtual string[] GenerateHelpFooter<T>()
        {
            return ReadTextAttributes<T, MoreInfo>();
        }

        protected string[] ReadTextAttributes<THost, TAttribute>()
            where TAttribute : StringAttribute
        {
            return typeof(THost)
                .GetCustomAttributes()
                .OfType<TAttribute>()
                .Select(a => a.Value?.Trim() ?? "")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToArray();
        }
    }

    public static class ExitCodes
    {
        public const int ARGUMENT_ERROR = 1;
        public const int SHOWED_HELP = 2;
    }

    public static class ParserExtensions
    {
        public static T Parse<T>(
            this string[] arguments
        )
        {
            return arguments.Parse<T>(
                out _
            );
        }

        public static T Parse<T>(
            this string[] arguments,
            out string[] uncollected
        )
        {
            return arguments.Parse<T>(
                out uncollected,
                new ArgParserOptions()
            );
        }

        public static T Parse<T>(
            this string[] arguments,
            out string[] uncollected,
            ArgParserOptions options
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
            return matched.ForceFuzzyDuckAs<T>();
        }

        private static void AddImpliedOptionsTo(
            Dictionary<string, Option> lookup
        )
        {
            AddFlagNegations(lookup);
            AddHelp(lookup);
        }

        private static void AddHelp(
            Dictionary<string, Option> lookup
        )
        {
            var opt = new Option()
            {
                Key = Option.HELP_FLAG_KEY,
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

        private static void AddFlagNegations(Dictionary<string, Option> lookup)
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
            Dictionary<string, Option> lookup,
            IDictionary<string, IHasValue> collected,
            out string[] uncollected,
            ArgParserOptions options)
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
            Option opt,
            IHasValue input,
            ArgParserOptions options,
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
            ArgParserOptions options,
            Option opt,
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
            Option[] options,
            ArgParserOptions parserOptions)
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
            Option[] options)
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

        private static Dictionary<string, Option> GenerateSwitchLookupFor<T>()
        {
            var options = GrokOptionsFor<T>();
            var shortNames = CollectShortNamesFrom<T>(options);
            var longNames = CollectLongNamesFrom<T>(options);
            var result = new Dictionary<string, Option>();
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

        private static void SetShortNameIfMissing(Option opt, HashSet<string> existing)
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

        private static void SetLongNameIfMissing(Option opt, HashSet<string> existing)
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

        private static HashSet<string> CollectLongNamesFrom<T>(List<Option> options)
        {
            return new(
                options
                    .Where(o => !(o.LongName is null))
                    .Select(o => o.LongName)
            );
        }

        private static HashSet<string> CollectShortNamesFrom<T>(List<Option> options)
        {
            return new(
                options
                    .Where(o => !(o.ShortName is null))
                    .Select(o => o.ShortName)
            );
        }

        private static List<Option> GrokOptionsFor<T>()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Aggregate(
                    new List<Option>(),
                    (acc, cur) =>
                    {
                        var attribs = cur.GetCustomAttributes().ToArray();
                        var option = new Option()
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

    public class Option
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }

        public string Type =>
            _type ??= GrokType();

        private string GrokType()
        {
            if (Property is null)
            {
                return "";
            }

            if (Property.PropertyType == typeof(bool) ||
                Property.PropertyType == typeof(bool?))
            {
                return "flag";
            }

            return Property.PropertyType.IsNumericType()
                ? "number"
                : "text";
        }

        private string _type;
        public PropertyInfo Property { get; set; }

        public string Key
        {
            get => _explicitKey ?? Property.Name;
            set => _explicitKey = value;
        }

        private string _explicitKey;
        public object Default { get; set; }
        public bool IsImplicit { get; set; }
        public string[] ConflictsWith { get; set; }

        public bool AllowMultipleValues
            => _allowMultipleValues ??= Property?.PropertyType?.IsCollection() ?? false;

        public bool IsFlag
        {
            get => _explicitFlag ?? (
                Property.PropertyType == typeof(bool) ||
                Property.PropertyType == typeof(bool?)
            );
            set => _explicitFlag = true;
        }

        public const string HELP_FLAG_KEY = "$help$";
        public bool IsHelpFlag => Key == HELP_FLAG_KEY;

        private bool? _explicitFlag;

        private bool? _allowMultipleValues;

        public Option Negate()
        {
            var result = new Option();
            this.CopyPropertiesTo(result, deep: false);
            result.LongName = $"no-{result.LongName}";
            result.ConflictsWith = new[] { Key };
            try
            {
                result.Default = !(bool) Default;
            }
            catch
            {
                result.Default = false;
            }

            return result;
        }
    }

    internal static class GatheringExtensions
    {
        internal static Dictionary<string, IHasValue> Add(
            this Dictionary<string, IHasValue> dict,
            string lastSwitch,
            string value
        )
        {
            var collection = dict.FindOrAdd(lastSwitch);
            collection.Add(value);
            return dict;
        }

        internal static Dictionary<string, IHasValue> Add(
            this Dictionary<string, IHasValue> dict,
            string sw
        )
        {
            dict.FindOrAdd(sw);
            return dict;
        }

        internal static IHasValue FindOrAdd(
            this Dictionary<string, IHasValue> dict,
            string sw
        )
        {
            if (dict.TryGetValue(sw, out var result))
            {
                return result;
            }

            result = new StringCollection();
            dict[sw] = result;
            return result;
        }
    }

    internal interface IHasValue
    {
        string SingleValue { get; }
        string[] AllValues { get; }
        void Add(string value);
    }

    internal class StringCollection : IHasValue
    {
        public string SingleValue => _values.FirstOrDefault();
        public string[] AllValues => _values.ToArray();

        private readonly List<string> _values = new();

        public void Add(string value)
        {
            _values.Add(value);
        }
    }

    public class ShortNameAttribute : StringAttribute
    {
        public ShortNameAttribute(char name) : base(name.ToString())
        {
        }
    }

    public abstract class StringAttribute : Attribute
    {
        public string Value { get; }

        public StringAttribute(string value)
        {
            Value = value;
        }
    }

    public class LongNameAttribute : StringAttribute
    {
        public LongNameAttribute(string value) : base(value)
        {
        }
    }

    public class DescriptionAttribute : StringAttribute
    {
        public DescriptionAttribute(string value) : base(value)
        {
        }
    }

    public class MoreInfo : StringAttribute
    {
        public MoreInfo(string value) : base(value)
        {
        }
    }

    public class ObjectAttribute : Attribute
    {
        public object Value { get; }

        public ObjectAttribute(object value)
        {
            Value = value;
        }
    }

    public class DefaultAttribute : ObjectAttribute
    {
        public DefaultAttribute(object value) : base(value)
        {
        }
    }

    public class ConflictsWithAttribute : StringAttribute
    {
        public ConflictsWithAttribute(string value) : base(value)
        {
        }
    }

    public static class DictionaryExtensions
    {
        public static bool TryGetValueFuzzy<T>(
            this IDictionary<string, T> dict,
            string key,
            out T value
        )
        {
            var matchedKey = dict.FuzzyFindKeyFor(key);
            if (matchedKey is null)
            {
                value = default;
                return false;
            }

            value = dict[matchedKey];
            return true;
        }
    }

    internal static class ArgParserOptionsExtensions
    {
        internal static void ExitIfRequired(
            this ArgParserOptions opts,
            int exitCode
        )
        {
            if (opts.ExitOnError)
            {
                opts.ExitAction?.Invoke(exitCode);
            }
        }
    }
}