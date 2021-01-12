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
            var flags = lookup.Values
                .Distinct()
                .Where(o => o.IsFlag)
                .ToArray();
            flags.ForEach(f =>
            {
                var negated = f.Negate();
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
            ArgParserOptions argParserOptions)
        {
            var uncollectedArgs = new List<string>();
            var result = collected.Aggregate(
                new Dictionary<string, object>(),
                (acc, cur) =>
                {
                    if (lookup.TryGetValueFuzzy(cur.Key, out var opt))
                    {
                        var input = cur.Value;
                        if (opt.AllowMultipleValues)
                        {
                            acc[opt.Property.Name] = input.AllValues;
                        }
                        else if (opt.IsFlag)
                        {
                            acc[opt.Property.Name] = opt.Default ?? true;
                        }
                        else
                        {
                            uncollectedArgs.AddRange(input.AllValues.Except(
                                new[]
                                {
                                    input.SingleValue
                                })
                            );
                            acc[opt.Property.Name] = cur.Value.SingleValue;
                        }
                    }

                    return acc;
                });

            VerifyNoConflicts(
                result,
                lookup.Values.Distinct().ToArray(),
                argParserOptions
            );

            AddMissingDefaults(
                result,
                lookup.Values.Distinct().ToArray()
            );

            uncollected = uncollectedArgs.ToArray();
            return result;
        }

        private static void VerifyNoConflicts(
            Dictionary<string, object> result,
            Option[] options,
            ArgParserOptions argParserOptions)
        {
            var canConflict = options
                .Where(o => o.ConflictsWith.Any())
                .Select(o => new { o.Property.Name, o.ConflictsWith })
                .ToArray();
            if (!canConflict.Any())
            {
                return;
            }

            var errored = false;
            canConflict.ForEach(o =>
            {
                o.ConflictsWith.ForEach(conflict =>
                {
                    if (result.ContainsKey(o.Name) && result.ContainsKey(conflict))
                    {
                        errored = true;
                        var ordered = new[]
                            {
                                o.Name,
                                conflict
                            }.Select(n => options.FirstOrDefault(o => o.Property.Name == n))
                            .OrderBy(o => o.LongName)
                            .ToArray();
                        argParserOptions.LineWriter(
                            $"--{ordered[0].LongName} conflicts with --{ordered[1].LongName}"
                        );
                    }
                });
            });
            if (errored && argParserOptions.ExitOnError)
            {
                argParserOptions.ExitAction?.Invoke(1);
            }
        }

        private static void AddMissingDefaults(
            Dictionary<string, object> result,
            Option[] options)
        {
            options.ForEach(opt =>
            {
                if (result.ContainsKey(opt.Property.Name))
                {
                    return;
                }

                if (opt.Default is null)
                {
                    return;
                }

                result[opt.Property.Name] = opt.Default;
            });
        }

        private static Dictionary<string, Option> GenerateSwitchLookupFor<T>()
        {
            var options = GrokOptionsFor<T>();
            var shortNames = CollectShortNamesFrom<T>(options);
            var longNames = CollectLongNamesFrom<T>(options);
            var result = new Dictionary<string, Option>();
            options.ForEach(opt =>
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

            var potential = opt.Property.Name.ToLowerInvariant()[0].ToString();
            if (existing.Contains(potential))
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

            var potential = opt.Property.Name
                .ToLowerInvariant()
                .ToKebabCase();
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

    internal class Option
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }

        public string Type =>
            _type ??= GrokType();

        private string GrokType()
        {
            return Property.PropertyType.IsNumericType()
                ? "number"
                : "text";
        }

        private string _type;
        public PropertyInfo Property { get; set; }
        public object Default { get; set; }
        public bool IsImplicit { get; set; }
        public string[] ConflictsWith { get; set; }

        public bool AllowMultipleValues
            => _allowMultipleValues ??= Property.PropertyType.IsCollection();

        public bool IsFlag
            => Property.PropertyType == typeof(bool) ||
                Property.PropertyType == typeof(bool?);


        private bool? _allowMultipleValues;

        public Option Negate()
        {
            var result = new Option();
            this.CopyPropertiesTo(result, deep: false);
            result.LongName = $"no-{result.LongName}";
            result.ConflictsWith = new[] { Property.Name };
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
}