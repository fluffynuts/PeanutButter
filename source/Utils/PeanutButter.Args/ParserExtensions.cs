using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Args
{
    public static class ParserExtensions
    {
        public static T Parse<T>(
            this string[] arguments
        )
        {
            return arguments.Parse<T>(out _);
        }

        public static T Parse<T>(
            this string[] arguments,
            out string[] unparsed
        )
        {
            var collected = Collect(arguments);
            var matched = TryMatch<T>(collected);
            throw new NotImplementedException();
        }

        private static IDictionary<string, IHasValue> TryMatch<T>(
            IDictionary<string, IHasValue> collected
        )
        {
            var lookup = GenerateSwitchLookupFor<T>();
            return collected.Aggregate(
                new Dictionary<string, IHasValue>(),
                (acc, cur) =>
                {
                    // try match by explicit matches
                    // otherwise let fuzzy ducking take over
                    if (lookup.TryGetValue(cur.Key, out var opt))
                    {
                        acc[opt.Property.Name] = cur.Value;
                    }
                    else
                    {
                        
                    }

                    return acc;
                });
        }

        private static Dictionary<string, Option> GenerateSwitchLookupFor<T>()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Aggregate(
                    new Dictionary<string, Option>(),
                    (acc, cur) =>
                    {
                        var attribs = cur.GetCustomAttributes().ToArray();
                        var opt = new Option()
                        {
                            ShortName = (attribs
                                        .OfType<ShortNameAttribute>()
                                        .FirstOrDefault()
                                    ?? new ShortNameAttribute(cur.Name[0])
                                ).Value,
                            LongName = (attribs
                                    .OfType<LongNameAttribute>()
                                    .FirstOrDefault()
                                ?? new LongNameAttribute(cur.Name)).Value,
                            Property = cur
                        };
                        // TODO: help, type, etc
                        if (!acc.ContainsKey(opt.ShortName))
                        {
                            acc[opt.ShortName] = opt;
                        }

                        if (!acc.ContainsKey(opt.LongName))
                        {
                            acc[opt.LongName] = opt;
                        }

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
                    return acc;
                });
        }
    }

    public class Option
    {
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public PropertyInfo Property { get; set; }
    }

    internal static class GatheringExtensions
    {
        public static Dictionary<string, IHasValue> Add(
            this Dictionary<string, IHasValue> dict,
            string lastSwitch,
            string value
        )
        {
            if (!dict.TryGetValue(lastSwitch, out var collection))
            {
                collection = new StringCollection();
                dict[lastSwitch] = collection;
            }

            collection.Add(value);

            return dict;
        }
    }

    internal interface IHasValue
    {
        object Value { get; }
        void Add(string value);
    }

    internal class StringCollection : IHasValue
    {
        public object Value => (object) SingleValue ?? AllValues;

        public string SingleValue => _values.Count == 1
            ? _values.Single()
            : null;

        public string[] AllValues => _values.Count > 1
            ? _values.ToArray()
            : null;

        private List<string> _values = new();

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

    public abstract class StringAttribute: Attribute
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

    public static class DictionaryExtensions
    {
        public static bool TryMatchKey<T>(
            this IDictionary<string, T> dict,
            string key,
            out string matched
        )
        {
            if (dict.ContainsKey(key))
            {
                matched = key;
                return true;
            }
            var fuzzyMatch = dict.Keys.Aggregate(
                null as string,
                (acc, cur) =>
                {
                    return acc ?? (
                });
        }
    }
}