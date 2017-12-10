using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

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
        public static string Stringify<T>(this IEnumerable<T> objs)
        {
            return StringifyCollectionInternal(objs, "null", 0);
        }

        private static string StringifyCollectionInternal<T>(
            IEnumerable<T> objs,
            string nullRepresentation,
            int level
        )
        {
            return objs == null
                ? "(null collection)"
                : $"[ {string.Join(", ", objs.Select(o => Stringify(o, nullRepresentation, level)))} ]";
        }

        /// <summary>
        /// Provides a reasonable human-readable string representation of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Human-readable representation of object</returns>
        public static string Stringify(this object obj)
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
            return Stringify(obj, nullRepresentation, 0);
        }

        private static string Stringify(
            object obj,
            string nullRepresentation,
            int level)
        {
            return SafeStringifier(obj, level, nullRepresentation ?? "null");
        }

        private const int MAX_STRINGIFY_DEPTH = 10;
        private const int INDENT_SIZE = 2;

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

        private static readonly Tuple<Func<object, int, bool>, Func<object, int, string, string>>[]
            _strategies =
            {
                MakeStrategy(IsNull, PrintNull),
                MakeStrategy(IsDateTime, StringifyDateTime),
                MakeStrategy(IsPrimitive, StringifyPrimitive),
                MakeStrategy(IsEnum, StringifyEnum),
                MakeStrategy(IsType, StringifyType),
                MakeStrategy(IsEnumerable, StringifyCollection),
                MakeStrategy(Default, StringifyJsonLike),
                MakeStrategy(LastPass, JustToStringIt)
            };

        private static string StringifyType(object obj, int level, string nullRep)
        {
            return (obj as Type).PrettyName();
        }

        private static bool IsType(object obj, int level)
        {
            return obj is Type;
        }

        private static string StringifyDateTime(object obj, int level, string nullRep)
        {
            var dt = (DateTime)obj;
            return $"{dt.ToString(CultureInfo.InvariantCulture)} ({dt.Kind})";
        }

        private static bool IsDateTime(object obj, int level)
        {
            return obj is DateTime;
        }

        private static string StringifyCollection(object obj, int level, string nullRep)
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
            return (string) (specific.Invoke(null, new[] {obj, nullRep, level}));
        }

        private static bool IsEnumerable(object obj, int level)
        {
            return obj.GetType().ImplementsEnumerableGenericType();
        }

        private static string StringifyEnum(object obj, int level, string nullRepresentation)
        {
            return obj.ToString();
        }

        private static bool IsEnum(object obj, int level)
        {
#if NETSTANDARD
            return obj.GetType().GetTypeInfo().IsEnum;
#else
            return obj.GetType().IsEnum;
#endif
        }

        private static string JustToStringIt(object obj, int level, string nullRepresentation)
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

        private static bool LastPass(object arg1, int arg2)
        {
            return true;
        }

        private static string PrintNull(object obj, int level, string nullRepresentation)
        {
            return nullRepresentation;
        }

        private static bool IsNull(object obj, int level)
        {
            return obj == null;
        }

        private static Tuple<Func<object, int, bool>, Func<object, int, string, string>> MakeStrategy(
            Func<object, int, bool> matcher, Func<object, int, string, string> writer
        )
        {
            return Tuple.Create(matcher, writer);
        }

        private static bool IsPrimitive(object obj, int level)
        {
            return level >= MAX_STRINGIFY_DEPTH ||
                   Types.PrimitivesAndImmutables.Contains(obj.GetType());
        }

        private static bool Default(object obj, int level)
        {
            return true;
        }

        private static string SafeStringifier(object obj, int level, string nullRepresentation)
        {
            if (level >= MAX_STRINGIFY_DEPTH)
            {
                return StringifyPrimitive(obj, level, nullRepresentation);
            }
            return _strategies.Aggregate(null as string,
                (acc, cur) => acc ??
                              ApplyStrategy(
                                  cur.Item1,
                                  cur.Item2,
                                  obj,
                                  level,
                                  nullRepresentation
                              )
            );
        }

        private static string ApplyStrategy(
            Func<object, int, bool> matcher,
            Func<object, int, string, string> strategy,
            object obj,
            int level,
            string nullRepresentation)
        {
            try
            {
                return matcher(obj, level)
                    ? strategy(obj, level, nullRepresentation)
                    : null;
            }
            catch
            {
                return null;
            }
        }


        private static string StringifyJsonLike(object obj, int level, string nullRepresentation)
        {
            var props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var indentMinus1 = new string(' ', level * INDENT_SIZE);
            var indent = indentMinus1 + new string(' ', INDENT_SIZE);
            var joinWith = props.Aggregate(new List<string>(), (acc, cur) =>
                {
                    var propValue = cur.GetValue(obj);
                    if (_ignoreAssembliesByName.Contains(
#if NETSTANDARD
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

        private static string StringifyPrimitive(object obj, int level, string nullRep)
        {
            if (obj == null)
                return nullRep;
            return _primitiveStringifiers.TryGetValue(obj.GetType(), out var strategy)
                ? strategy(obj)
                : obj.ToString();
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
                })
                .SquashEmptyObjects();
        }

        private static string SquashEmptyObjects(this string str)
        {
            return str.RegexReplace("{\\s*}", "{}");
        }
    }
}