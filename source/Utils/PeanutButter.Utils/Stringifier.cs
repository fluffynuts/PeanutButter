using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IntroduceOptionalParameters.Global
#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
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
    public static string StringifyCollection<T>(
        this IEnumerable<T> objs
    )
    {
        if (typeof(T) == typeof(char))
        {
            return objs == null
                ? DEFAULT_NULL_PLACEHOLDER
                : $"\"{objs as string}\"";
        }

        return StringifyCollectionInternal(
            objs,
            new StringifyContext(DEFAULT_NULL_PLACEHOLDER),
            0,
            new HashSet<object>()
        );
    }

    private static string StringifyCollectionInternal<T>(
        IEnumerable<T> objs,
        StringifyContext context,
        int level,
        HashSet<object> seenObjects
    )
    {
        if (objs is null)
        {
            return DEFAULT_NULL_PLACEHOLDER;
        }

        var result = new List<string>(["[ "]);
        var idx = 0;
        foreach (var item in objs)
        {
            if (idx > 0)
            {
                result.Add(", ");
            }

            if (context.HasTimedOut())
            {
                result.Add(context.Truncated(MAX_TIME_TRUNCATION));
                break;
            }

            if (++idx >= MAX_OBJECTS || seenObjects.Count >= MAX_OBJECTS)
            {
                result.Add(context.Truncated(MAX_OBJECTS_TRUNCATION));
                break;
            }

            result.Add(
                SafeStringifier(
                    item,
                    level,
                    context,
                    seenObjects
                )
            );
        }

        result.Add(" ]");

        return string.Join(
            "",
            result
        );
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
        return Stringify(
            obj,
            DEFAULT_NULL_PLACEHOLDER
        );
    }

    /// <summary>
    /// make-shift drop-in for LINQPad's .Dump(), to
    /// use, eg, from csharprepl
    /// </summary>
    /// <param name="obj"></param>
    public static void Dump(
        this object obj
    )
    {
        Console.WriteLine(
            obj.Stringify()
        );
    }

    /// <summary>
    /// The default value put into a stringified result when null is encountered
    /// </summary>
    public const string DEFAULT_NULL_PLACEHOLDER = "null";

    /// <summary>
    /// The placeholder put into a stringified result when a circular reference is
    /// encountered
    /// </summary>
    public const string SEEN_OBJECT_PLACEHOLDER = "(at:";

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
        return SafeStringifier(
            obj,
            0,
            new StringifyContext(nullRepresentation),
            new HashSet<object>()
        );
    }

    /// <summary>
    /// your output will contain "...truncated"
    /// The max depth the stringifier will go to; after this,
    /// </summary>
    public const int MAX_STRINGIFY_DEPTH = 10;

    /// <summary>
    /// Limits breadth and path length, not the total: a collection shows at
    /// most this many items (the rest are truncated), and rendering does not
    /// descend once this many objects are on the current path. Because each
    /// branch counts for itself, many branches can together render far more
    /// than this: see MAX_TOTAL_OBJECTS for the limit across the whole call.
    /// </summary>
    public const int MAX_OBJECTS = 32;

    /// <summary>
    /// Abort stringifying after rendering this many non-primitive
    /// objects in total, across the whole call
    /// </summary>
    public const int MAX_TOTAL_OBJECTS = 200;

    /// <summary>
    /// Strings longer than this are truncated in the output
    /// </summary>
    public const int MAX_STRING_LENGTH = 128;

    /// <summary>
    /// Stringifying is abandoned (with what has been rendered so far
    /// kept) once it has run for this long
    /// </summary>
    public const int MAX_STRINGIFY_MILLISECONDS = 1000;

    internal static string StringifyWithClock(
        object obj,
        Func<long> elapsedMilliseconds
    )
    {
        return SafeStringifier(
            obj,
            0,
            new StringifyContext(
                DEFAULT_NULL_PLACEHOLDER,
                elapsedMilliseconds
            ),
            new HashSet<object>()
        );
    }

    private const int INDENT_SIZE = 2;

    private static readonly Dictionary<Type, Func<object, string>> PrimitiveStringifiers
        = new Dictionary<Type, Func<object, string>>()
        {
            [typeof(string)] = o => StringifyString((string) o),
            [typeof(bool)] = o => o.ToString().ToLower()
        };

    private static readonly Tuple<Func<object, int, bool>, Func<object, int, StringifyContext, HashSet<object>, string>>[]
        Strategies =
        {
            MakeStrategy(
                IsNull,
                PrintNull
            ),
            MakeStrategy(
                IsDateTime,
                StringifyDateTime
            ),
            MakeStrategy(
                IsPrimitive,
                StringifyPrimitive
            ),
            MakeStrategy(
                IsEnum,
                StringifyEnum
            ),
            MakeStrategy(
                IsType,
                StringifyType
            ),
            MakeStrategy(
                IsEnumerable,
                StringifyCollection
            ),
            MakeStrategy(
                IsXDocument,
                StringifyXDocument
            ),
            MakeStrategy(
                IsXElement,
                StringifyXElement
            ),
            MakeStrategy(
                Default,
                StringifyJsonLike
            ),
            MakeStrategy(
                LastPass,
                JustToStringIt
            )
        };

    private static string StringifyXElement(
        object arg1,
        int arg2,
        StringifyContext arg3,
        HashSet<object> seen
    )
    {
        return ((XElement)arg1).ToString();
    }

    private static bool IsXElement(
        object arg1,
        int arg2
    )
    {
        return arg1 is XElement;
    }

    private static string StringifyXDocument(
        object arg1,
        int arg2,
        StringifyContext arg3,
        HashSet<object> seen
    )
    {
        return ((XDocument)arg1).ToString();
    }

    private static bool IsXDocument(
        object obj,
        int level
    )
    {
        return obj is XDocument;
    }

    private static string StringifyType(
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seen
    )
    {
        return (obj as Type).PrettyName();
    }

    private static bool IsType(
        object obj,
        int level
    )
    {
        return obj is Type;
    }

    private static string StringifyDateTime(
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seen
    )
    {
        var dt = (DateTime)obj;
        return $"{dt.ToString(CultureInfo.InvariantCulture)} ({dt.Kind})";
    }

    private static bool IsDateTime(
        object obj,
        int level
    )
    {
        return obj is DateTime;
    }

    private static string StringifyCollection(
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seen
    )
    {
        var itemType = obj.GetType().TryGetEnumerableItemType() ??
                       throw new Exception($"{obj.GetType()} is not IEnumerable<T>");
        var method = typeof(Stringifier)
            .GetMethod(
                nameof(StringifyCollectionInternal),
                BindingFlags.NonPublic | BindingFlags.Static
            );
        if (method == null)
            throw new InvalidOperationException(
                $"No non-public, static '{nameof(StringifyCollectionInternal)}' method found on {typeof(Stringifier).PrettyName()}"
            );
        var specific = method.MakeGenericMethod(itemType);
        return (string)(specific.Invoke(
            null,
            new[]
            {
                obj,
                context,
                level,
                seen
            }
        ));
    }

    private static bool IsEnumerable(
        object obj,
        int level
    )
    {
        return obj.GetType().ImplementsEnumerableGenericType();
    }

    private static string StringifyEnum(
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seen
    )
    {
        return obj.ToString();
    }

    private static bool IsEnum(
        object obj,
        int level
    )
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
        StringifyContext context,
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
        int arg2
    )
    {
        return true;
    }

    private static string PrintNull(
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seenObjects
    )
    {
        return context.NullRepresentation;
    }

    private static bool IsNull(
        object obj,
        int level
    )
    {
        return obj == null;
    }

    private static Tuple<Func<object, int, bool>, Func<object, int, StringifyContext, HashSet<object>, string>> MakeStrategy(
        Func<object, int, bool> matcher,
        Func<object, int, StringifyContext, HashSet<object>, string> writer
    )
    {
        return Tuple.Create(
            matcher,
            writer
        );
    }

    private static bool IsPrimitive(
        object obj,
        int level
    )
    {
        return level >= MAX_STRINGIFY_DEPTH ||
               Types.PrimitivesAndImmutables.Contains(obj.GetType());
    }

    private static bool Default(
        object obj,
        int level
    )
    {
        return true;
    }
    
    private static readonly string MAX_DEPTH_TRUNCATION = $"<truncated: max depth of {MAX_STRINGIFY_DEPTH} reached>";
    private static readonly string MAX_OBJECTS_TRUNCATION = $"<truncated: more than {MAX_OBJECTS} objects>";
    private const string SHORT_TRUNCATION = "<truncated>";
    private static readonly string MAX_TOTAL_OBJECTS_TRUNCATION = $"<truncated: more than {MAX_TOTAL_OBJECTS} objects in total>";
    private static readonly string MAX_TIME_TRUNCATION = $"<truncated: stringification exceeded {(MAX_STRINGIFY_MILLISECONDS / 1000d).ToString(CultureInfo.InvariantCulture)}s>";

    private static string SafeStringifier(
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seen
    )
    {
        if (context.HasTimedOut())
        {
            return context.Truncated(MAX_TIME_TRUNCATION);
        }

        var objType = obj?.GetType();
        var isComposite = objType is not null && !objType.IsPrimitiveOrImmutable();
        if (isComposite)
        {
            if (seen.Contains(obj))
            {
                var handle = GCHandle.Alloc(obj);
                var seenResult =
                    $"{{ {objType.Name} {SEEN_OBJECT_PLACEHOLDER} {(long)GCHandle.ToIntPtr(handle):x8}) }}";
                handle.Free();
                return seenResult;
            }

            seen.Add(obj);
        }

        if (level >= MAX_STRINGIFY_DEPTH)
        {
            return context.Truncated(MAX_DEPTH_TRUNCATION);
        }

        if (seen.Count >= MAX_OBJECTS)
        {
            return context.Truncated(MAX_OBJECTS_TRUNCATION);
        }

        if (isComposite && !context.TryCountObject())
        {
            return context.Truncated(MAX_TOTAL_OBJECTS_TRUNCATION);
        }

        var result = Strategies.Aggregate(
            null as string,
            (
                    acc,
                    cur
                ) => acc ??
                     ApplyStrategy(
                         cur.Item1,
                         cur.Item2,
                         obj,
                         level,
                         context,
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
                .Any(
                    mi => mi.Name == nameof(ToString) &&
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
        Func<object, int, StringifyContext, HashSet<object>, string> strategy,
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seen
    )
    {
        try
        {
            var isMatched = matcher(
                obj,
                level
            );
            return isMatched
                ? strategy(
                    obj,
                    level,
                    context,
                    seen
                )
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
        StringifyContext context,
        HashSet<object> seen
    )
    {
        var props = obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(pi => pi.GetCustomAttribute<SkipStringifyAttribute>() is null)
            .OrderBy(pi => pi.Name)
            .ToArray();
        var indentMinus1 = new string(
            ' ',
            level * INDENT_SIZE
        );
        var indent = indentMinus1 + new string(
            ' ',
            INDENT_SIZE
        );
        var rendered = new List<string>();
        foreach (var prop in props)
        {
            if (context.HasTimedOut())
            {
                rendered.Add(context.Truncated(MAX_TIME_TRUNCATION));
                break;
            }

            var propValue = prop.GetValue(obj);
            rendered.Add(
                string.Join(
                    "",
                    prop.Name,
                    ": ",
                    SafeStringifier(
                        propValue,
                        level + 1,
                        context,
                        new HashSet<object>(seen)
                    )
                )
            );
        }

        var joinWith = rendered.JoinWith($"\n{indent}");
        return ("{\n" +
                string.Join(
                    $"\n{indent}",
                    $"{indent}{joinWith}"
                ) +
                $"\n{indentMinus1}}}").Compact();
    }

    private static string StringifyString(
        string str
    )
    {
        if (str.Length <= MAX_STRING_LENGTH)
        {
            return $"\"{str}\"";
        }

        var shown = MAX_STRING_LENGTH;
        if (char.IsHighSurrogate(str[shown - 1]))
        {
            // don't cut a surrogate pair in half
            shown--;
        }

        return $"\"{str.Substring(0, shown)}\"<truncated: showing {shown} of {str.Length} chars>";
    }

    /// <summary>
    /// State shared by every step of a single Stringify call, so that
    /// the limits which apply to the whole call (time, total objects)
    /// are enforced across all of the stringification strategies
    /// </summary>
    private sealed class StringifyContext
    {
        public string NullRepresentation { get; }

        private readonly Func<long> _elapsedMilliseconds;
        private int _objectsCounted;
        private bool _timedOut;
        private readonly HashSet<string> _truncationReasonsGiven = new();

        public StringifyContext(
            string nullRepresentation,
            Func<long> elapsedMilliseconds = null
        )
        {
            NullRepresentation = nullRepresentation ?? DEFAULT_NULL_PLACEHOLDER;
            if (elapsedMilliseconds is not null)
            {
                _elapsedMilliseconds = elapsedMilliseconds;
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            _elapsedMilliseconds = () => stopwatch.ElapsedMilliseconds;
        }

        // once the time limit has been exceeded, it stays exceeded: every
        // remaining step returns immediately, so the render unwinds quickly
        public bool HasTimedOut()
        {
            if (_timedOut)
            {
                return true;
            }

            _timedOut = _elapsedMilliseconds() > MAX_STRINGIFY_MILLISECONDS;
            return _timedOut;
        }

        // the first time a reason is hit, it is spelled out; every later
        // cut for the same reason is just marked, so that it's clear how much
        // was skipped without repeating the explanation
        public string Truncated(
            string reason
        )
        {
            return _truncationReasonsGiven.Add(reason)
                ? reason
                : SHORT_TRUNCATION;
        }

        public bool TryCountObject()
        {
            return ++_objectsCounted <= MAX_TOTAL_OBJECTS;
        }
    }

    private static string StringifyPrimitive(
        object obj,
        int level,
        StringifyContext context,
        HashSet<object> seen
    )
    {
        if (obj == null)
            return context.NullRepresentation;
        return PrimitiveStringifiers.TryGetValue(obj.GetType(), out var strategy)
            ? strategy(obj)
            : obj.ToString();
    }
}

internal static class StringifierStringExtensions
{
    internal static string Compact(
        this string str
    )
    {
        return new[]
            {
                "\r\n",
                "\n"
            }.Aggregate(
                str,
                (
                    acc,
                    cur
                ) =>
                {
                    var twice = $"{cur}{cur}";
                    while (acc.Contains(twice))
                        acc = acc.Replace(twice, "");
                    return acc;
                }
            )
            .SquashEmptyObjects();
    }

    private static string SquashEmptyObjects(
        this string str
    )
    {
        return str.RegexReplace("{\\s*}", "{}");
    }
}

/// <summary>
/// Signals to the .Stringify() extension that this property
/// or field should be ignored in the output.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class SkipStringifyAttribute : Attribute
{
}