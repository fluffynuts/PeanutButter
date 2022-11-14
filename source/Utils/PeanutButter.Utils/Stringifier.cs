using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IntroduceOptionalParameters.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
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
        public static string Stringify<T>(
            this IEnumerable<T> objs)
        {
            if (typeof(T) == typeof(char))
            {
                return objs == null
                    ? DEFAULT_NULL_PLACEHOLDER
                    : $"\"{objs as string}\"";
            }

            return StringifyCollectionInternal(objs, "null", 0, new HashSet<object>());
        }

        private static string StringifyCollectionInternal<T>(
            IEnumerable<T> objs,
            string nullRepresentation,
            int level,
            HashSet<object> seenObjects
        )
        {
            return objs == null
                ? DEFAULT_NULL_PLACEHOLDER
                : $"[ {string.Join(", ", objs.Select(o => Stringify(o, nullRepresentation, level, seenObjects)))} ]";
        }

        /// <summary>
        /// Provides a reasonable human-readable string representation of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Human-readable representation of object</returns>
        public static string Stringify(
            this object obj
        )
        {
            return Stringify(obj, DEFAULT_NULL_PLACEHOLDER);
        }

        /// <summary>
        /// The default value put into a stringified result when null is encountered
        /// </summary>
        public const string DEFAULT_NULL_PLACEHOLDER = "null";
        /// <summary>
        /// The placeholder put into a stringified result when a circular reference is
        /// encountered
        /// </summary>
        public const string SEEN_OBJECT_PLACEHOLDER = "🔁";

        /// <summary>
        /// Provides a reasonable human-readable string representation of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nullRepresentation">How to represent null values - defaults to the string "null"</param>
        /// <returns>Human-readable representation of object</returns>
        public static string Stringify(
            object obj,
            string nullRepresentation
        )
        {
            return Stringify(obj, nullRepresentation, 0, new HashSet<object>());
        }

        private static string Stringify(
            object obj,
            string nullRepresentation,
            int level,
            HashSet<object> seenObjects
        )
        {
            return SafeStringifier(
                obj,
                level,
                nullRepresentation ?? "null",
                seenObjects
            );
        }

        private const int MAX_STRINGIFY_DEPTH = 10;
        private const int INDENT_SIZE = 2;

        private static readonly Dictionary<Type, Func<object, string>> PrimitiveStringifiers
            = new Dictionary<Type, Func<object, string>>()
            {
                [typeof(string)] = o => $"\"{o}\"",
                [typeof(bool)] = o => o.ToString().ToLower()
            };

        private static readonly string[] IgnoreAssembliesByName =
        {
            "mscorlib"
        };

        private static readonly Tuple<Func<object, int, bool>, Func<object, int, string, HashSet<object>, string>>[]
            Strategies =
            {
                MakeStrategy(IsNull, PrintNull),
                MakeStrategy(IsDateTime, StringifyDateTime),
                MakeStrategy(IsPrimitive, StringifyPrimitive),
                MakeStrategy(IsEnum, StringifyEnum),
                MakeStrategy(IsType, StringifyType),
                MakeStrategy(IsEnumerable, StringifyCollection),
                MakeStrategy(IsXDocument, StringifyXDocument),
                MakeStrategy(IsXElement, StringifyXElement),
                MakeStrategy(Default, StringifyJsonLike),
                MakeStrategy(LastPass, JustToStringIt)
            };

        private static string StringifyXElement(
            object arg1,
            int arg2,
            string arg3,
            HashSet<object> seen
        )
        {
            return ((XElement) arg1).ToString();
        }

        private static bool IsXElement(
            object arg1,
            int arg2)
        {
            return arg1 is XElement;
        }

        private static string StringifyXDocument(
            object arg1,
            int arg2,
            string arg3,
            HashSet<object> seen
        )
        {
            return ((XDocument) arg1).ToString();
        }

        private static bool IsXDocument(
            object obj,
            int level)
        {
            return obj is XDocument;
        }

        private static string StringifyType(
            object obj,
            int level,
            string nullRep,
            HashSet<object> seen
        )
        {
            return (obj as Type).PrettyName();
        }

        private static bool IsType(
            object obj,
            int level)
        {
            return obj is Type;
        }

        private static string StringifyDateTime(
            object obj,
            int level,
            string nullRep,
            HashSet<object> seen
        )
        {
            var dt = (DateTime) obj;
            return $"{dt.ToString(CultureInfo.InvariantCulture)} ({dt.Kind})";
        }

        private static bool IsDateTime(
            object obj,
            int level)
        {
            return obj is DateTime;
        }

        private static string StringifyCollection(
            object obj,
            int level,
            string nullRep,
            HashSet<object> seen
        )
        {
            var itemType = obj.GetType().TryGetEnumerableItemType() ??
                throw new Exception($"{obj.GetType()} is not IEnumerable<T>");
            var method = typeof(Stringifier)
                .GetMethod(nameof(StringifyCollectionInternal), BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException(
                    $"No non-public, static '{nameof(StringifyCollectionInternal)}' method found on {typeof(Stringifier).PrettyName()}"
                );
            var specific = method.MakeGenericMethod(itemType);
            return (string) (specific.Invoke(null, new[] { obj, nullRep, level, seen }));
        }

        private static bool IsEnumerable(
            object obj,
            int level)
        {
            return obj.GetType().ImplementsEnumerableGenericType();
        }

        private static string StringifyEnum(
            object obj,
            int level,
            string nullRepresentation,
            HashSet<object> seen
        )
        {
            return obj.ToString();
        }

        private static bool IsEnum(
            object obj,
            int level)
        {
#if NETSTANDARD
            return obj.GetType().GetTypeInfo().IsEnum;
#else
            return obj.GetType().IsEnum;
#endif
        }

        private static string JustToStringIt(
            object obj,
            int level,
            string nullRepresentation,
            HashSet<object> seen
        )
        {
            try
            {
                return obj.ToString();
            }
            catch
            {
                return $"{{{obj.GetType()}}}";
            }
        }

        private static bool LastPass(
            object arg1,
            int arg2)
        {
            return true;
        }

        private static string PrintNull(
            object obj,
            int level,
            string nullRepresentation,
            HashSet<object> seenObjects
        )
        {
            return nullRepresentation;
        }

        private static bool IsNull(
            object obj,
            int level)
        {
            return obj == null;
        }

        private static Tuple<Func<object, int, bool>, Func<object, int, string, HashSet<object>, string>> MakeStrategy(
            Func<object, int, bool> matcher,
            Func<object, int, string, HashSet<object>, string> writer
        )
        {
            return Tuple.Create(matcher, writer);
        }

        private static bool IsPrimitive(
            object obj,
            int level)
        {
            return level >= MAX_STRINGIFY_DEPTH ||
                Types.PrimitivesAndImmutables.Contains(obj.GetType());
        }

        private static bool Default(
            object obj,
            int level)
        {
            return true;
        }

        private static string SafeStringifier(
            object obj,
            int level,
            string nullRepresentation,
            HashSet<object> seen
        )
        {
            if (!obj?.GetType().IsPrimitiveOrImmutable() ?? false)
            {
                if (seen.Contains(obj))
                {
                    return SEEN_OBJECT_PLACEHOLDER;
                }

                seen.Add(obj);
            }

            if (level >= MAX_STRINGIFY_DEPTH)
            {
                return StringifyPrimitive(obj, level, nullRepresentation, seen);
            }

            var result = Strategies.Aggregate(
                null as string,
                (
                        acc,
                        cur) => acc ??
                    ApplyStrategy(
                        cur.Item1,
                        cur.Item2,
                        obj,
                        level,
                        nullRepresentation,
                        // create a copy of the seen collection
                        // so that collision detection is for circular
                        // references, not just repeated references
                        new HashSet<object>(seen)
                    )
            );
            return result == EMPTY_OBJECT && HasCustomToString(obj)
                ? $"<< {obj} >>"
                : result;
        }

        private static bool HasCustomToString(object o)
        {
            return CustomToStringCache.FindOrAdd(
                o?.GetType(),
                () => o?.GetType().GetMethods()
                    .Any(mi => mi.Name == nameof(ToString) &&
                        mi.GetParameters().Length == 0 &&
                        !BaseTypes.Contains(mi.DeclaringType)
                    ) ?? false
            );
        }

        private static readonly Dictionary<Type, bool> CustomToStringCache = new();

        private static readonly HashSet<Type> BaseTypes = new(
            new[]
            {
#if NETSTANDARD
                typeof(ValueType),
#endif
                typeof(object)
            }
        );

        private const string EMPTY_OBJECT = "{}";

        private static string ApplyStrategy(
            Func<object, int, bool> matcher,
            Func<object, int, string, HashSet<object>, string> strategy,
            object obj,
            int level,
            string nullRepresentation,
            HashSet<object> seen)
        {
            try
            {
                var isMatched = matcher(obj, level);
                return isMatched
                    ? strategy(obj, level, nullRepresentation, seen)
                    : null;
            }
            catch
            {
                return null;
            }
        }


        private static string StringifyJsonLike(
            object obj,
            int level,
            string nullRepresentation,
            HashSet<object> seen
        )
        {
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var indentMinus1 = new string(' ', level * INDENT_SIZE);
            var indent = indentMinus1 + new string(' ', INDENT_SIZE);
            var joinWith = props.Aggregate(
                    new List<string>(),
                    (
                        acc,
                        cur) =>
                    {
                        var propValue = cur.GetValue(obj);

                        if (IgnoreAssembliesByName.Contains(
#if NETSTANDARD
                                cur.DeclaringType?.AssemblyQualifiedName?.Split(
                                    new[] { "," },
                                    StringSplitOptions.RemoveEmptyEntries
                                ).Skip(1).FirstOrDefault()
#else
                            cur.DeclaringType?.Assembly.GetName().Name
#endif
                            ))
                        {
                            acc.Add(string.Join("",
                                    cur.Name,
                                    ": ",
                                    SafeStringifier(propValue, level + 1, nullRepresentation, new HashSet<object>(seen))
                                )
                            );
                        }
                        else
                        {
                            acc.Add(
                                string.Join(
                                    "",
                                    cur.Name,
                                    ": ",
                                    SafeStringifier(propValue, level + 1, nullRepresentation, new HashSet<object>(seen))));
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

        private static string StringifyPrimitive(
            object obj,
            int level,
            string nullRep,
            HashSet<object> seen
        )
        {
            if (obj == null)
                return nullRep;
            return PrimitiveStringifiers.TryGetValue(obj.GetType(), out var strategy)
                ? strategy(obj)
                : obj.ToString();
        }
    }

    internal static class StringifierStringExtensions
    {
        internal static string Compact(
            this string str)
        {
            return new[]
                {
                    "\r\n",
                    "\n"
                }.Aggregate(
                    str,
                    (
                        acc,
                        cur) =>
                    {
                        var twice = $"{cur}{cur}";
                        while (acc.Contains(twice))
                            acc = acc.Replace(twice, "");
                        return acc;
                    })
                .SquashEmptyObjects();
        }

        private static string SquashEmptyObjects(
            this string str)
        {
            return str.RegexReplace("{\\s*}", "{}");
        }
    }
}