using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable IntroduceOptionalParameters.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides convenience functions to get reasonable string representations of objects and collections
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public 
#endif
    static class Stringifier
    {
        /// <summary>
        /// Provides a reasonable human-readable string representation of a collection
        /// </summary>
        /// <param name="objs"></param>
        /// <returns>Human-readable representation of collection</returns>
        public static string Stringify<T>(IEnumerable<T> objs)
        {
            return objs == null
                ? "(null collection)"
                : $"[ {string.Join(", ", objs.Select(o => Stringify(o)))} ]";
        }

        /// <summary>
        /// Provides a reasonable human-readable string representation of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Human-readable representation of object</returns>
        public static string Stringify(object obj)
        {
            return Stringify(obj, "null");
        }

        /// <summary>
        /// Provides a reasonable human-readable string representation of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nullRepresentation">How to represent null values - defaults to the string "null"</param>
        /// <returns>Human-readable representation of object</returns>
        public static string Stringify(object obj, string nullRepresentation)
        {
            return SafeStringifier(obj, 0, nullRepresentation ?? "null");
        }

        private const int MaxStringifyDepth = 10;
        private const int IndentSize = 2;

        private static readonly Dictionary<Type, Func<object, string>> _primitiveStringifiers
            = new Dictionary<Type, Func<object, string>>()
            {
                [typeof(string)] = o => $"\"{o}\"",
                [typeof(bool)] = o => o.ToString().ToLower()
            };

        private static readonly string[] _ignoreAssembliesByName =
        {
            "mscorlib"
        };

        private static string SafeStringifier(object obj, int level, string nullRepresentation)
        {
            if (obj == null)
                return nullRepresentation;
            var objType = obj.GetType();
            if (level >= MaxStringifyDepth || Types.PrimitivesAndImmutables.Contains(objType))
            {
                Func<object, string> strategy;
                return _primitiveStringifiers.TryGetValue(objType, out strategy)
                    ? strategy(obj)
                    : obj.ToString();
            }
            try
            {
                var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var indentMinus1 = new string(' ', level * IndentSize);
                var indent = indentMinus1 + new string(' ', IndentSize);
                var joinWith = props.Aggregate(new List<string>(), (acc, cur) =>
                    {
                        var propValue = cur.GetValue(obj);
                    if (_ignoreAssembliesByName.Contains(
#if NETSTANDARD1_6
                            cur.DeclaringType?.AssemblyQualifiedName.Split(
                            new[] { "," }, StringSplitOptions.RemoveEmptyEntries
                        ).Skip(1).FirstOrDefault()
#else
                            cur.DeclaringType?.Assembly.GetName().Name
#endif
                        ))
                        {
                            acc.Add(string.Join("", cur.Name, ": ", propValue?.ToString()));
                        }
                        else
                        {
                            acc.Add(string.Join(
                                "",
                                cur.Name,
                                ": ",
                                SafeStringifier(propValue, level + 1, nullRepresentation)));
                        }

                        return acc;
                    })
                    .JoinWith($"\n{indent}");
                return ("{\n" +
                       string.Join(
                           "\n{indent}",
                           $"{indent}{joinWith}"
                       ) +
                       $"\n{indentMinus1}}}").Compact();
            }
            catch
            {
                return obj.ToString();
            }
        }
    }

    internal static class StringifierStringExtensions
    {
        internal static string Compact(this string str)
        {
            return new[]
            {
                "\r\n",
                "\n"
            }.Aggregate(str, (acc, cur) =>
            {
                var twice = $"{cur}{cur}";
                while (acc.Contains(twice))
                    acc = acc.Replace(twice, "");
                return acc;
            }).SquashEmptyObjects();
        }

        private static string SquashEmptyObjects(this string str)
        {
            return str.RegexReplace("{\\s*}", "{}");
        }
    }

}