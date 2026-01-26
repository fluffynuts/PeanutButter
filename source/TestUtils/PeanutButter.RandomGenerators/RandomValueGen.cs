using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
// ReSharper disable ConstantNullCoalescingCondition

// ReSharper disable UnusedAutoPropertyAccessor.Local

// SwapViaDeconstruction will require a reference to System.Tuple for netfx hosts
// ReSharper disable SwapViaDeconstruction
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;
using static Imported.PeanutButter.Utils.PyLike;

namespace Imported.PeanutButter.RandomGenerators;
#else
using PeanutButter.Utils;
using static PeanutButter.Utils.PyLike;

namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Context to use when getting a random timespan
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    enum TimeSpanContexts
{
#pragma warning disable 1591
    Ticks,
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days
#pragma warning restore 1591
}

/// <summary>
/// Class which provides a number of static methods to produce random values.
/// Suggestion: make use of "using static" to bring these methods into your class
/// as if they were local members, ie:
/// using static PeanutButter.RandomGenerators.RandomValueGen;
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class RandomValueGen
{
    private static readonly Dictionary<Type, Func<object>> PrimitiveGenerators =
        new()
        {
            [typeof(int)] = () => GetRandomInt(),
            [typeof(byte)] = () => Convert.ToByte(GetRandomInt(0, 255)),
            [typeof(char)] = () => Convert.ToChar(GetRandomInt(0, 255)),
            [typeof(long)] = () => GetRandomLong(),
            [typeof(float)] = () => Convert.ToSingle(GetRandomDecimal(decimal.MinValue, decimal.MaxValue)),
            [typeof(double)] = () => Convert.ToDouble(GetRandomDecimal(decimal.MinValue, decimal.MaxValue)),
            [typeof(decimal)] = () => GetRandomDecimal(decimal.MinValue, decimal.MaxValue),
            [typeof(DateTime)] = () => GetRandomDate(),
            [typeof(string)] = () => GetRandomString(),
            [typeof(bool)] = () => GetRandomBoolean(),
            [typeof(Type)] = GetRandomType,
            [typeof(DateTimeOffset)] = () => GetRandomDateTimeOffset(),
        };

    /// <summary>
    /// Gets a random value of the specified type by attempting to find the correct
    /// random generator method and invoking it. Works on primitives (eg int, string),
    /// enums and complex objects. When invoked on a complex object, will attempt to fine
    /// (or, if necessary, create) a GenericBuilder to produce the result
    /// </summary>
    /// <typeparam name="T">Type to generate a random value of</typeparam>
    /// <returns>New instance of the specified type. Should be different every time, when possible.</returns>
    public static T GetRandom<T>()
    {
        SatelliteAssemblyInitializer.InitializeSatelliteAssemblies<T>();
        var type = typeof(T);
        if (RandomGenerators.TryGetValue(type, out var handler))
        {
            return (T)handler();
        }

        if (type.IsEnum())
        {
            return (T)GetRandomEnum(type);
        }

        var dictionaryInterfaceType = type
            .GetAllImplementedInterfaces()
            .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        if (dictionaryInterfaceType is not null)
        {
            var genericArgs = dictionaryInterfaceType.GetGenericArguments();
            var keyType = genericArgs[0];
            var valueType = genericArgs[1];
            var method = GenericGetRandomDictionary.MakeGenericMethod(keyType, valueType);
            return (T)method.Invoke(null, new object[0]);
        }

        return (T)GetRandom(type);
    }

    private static readonly MethodInfo GenericGetRandomDictionary = typeof(RandomValueGen)
        .GetMethods()
        .FirstOrDefault(mi => mi.Name == nameof(GetRandomDictionary) && mi.GetParameters().Length == 0);

    /// <summary>
    /// Generates a random dictionary of the provided key / value types
    /// with at least one item in it
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> GetRandomDictionary<TKey, TValue>()
    {
        return GetRandomDictionary<TKey, TValue>(DefaultRanges.MIN_ITEMS + 1);
    }

    /// <summary>
    /// Generates a random dictionary of the provided key / value types
    /// with at least {minItems} in it
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> GetRandomDictionary<TKey, TValue>(
        int minItems
    )
    {
        return GetRandomDictionary<TKey, TValue>(minItems, DefaultRanges.MAX_ITEMS + minItems);
    }

    /// <summary>
    /// Generates a random dictionary of the provided key / value types
    /// with at least {minItems} and at most {maxItems} in it
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> GetRandomDictionary<TKey, TValue>(
        int minItems,
        int maxItems
    )
    {
        var result = new Dictionary<TKey, TValue>();
        var howMany = GetRandomInt(minItems, maxItems);
        for (var i = 0; i < howMany; i++)
        {
            var key = GetRandom<TKey>();
            var value = GetRandom<TValue>();
            result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// Generates a random NameValueCollection
    /// with at least one item in it
    /// </summary>
    /// <returns></returns>
    public static NameValueCollection GetRandomNameValueCollection()
    {
        return GetRandomNameValueCollection(DefaultRanges.MIN_ITEMS + 1);
    }

    /// <summary>
    /// Generates a random NameValueCollection
    /// with at least {minItems} in it
    /// </summary>
    /// <returns></returns>
    public static NameValueCollection GetRandomNameValueCollection(
        int minItems
    )
    {
        return GetRandomNameValueCollection(minItems, DefaultRanges.MAX_ITEMS + minItems);
    }

    /// <summary>
    /// Generates a random NameValueCollection
    /// with at least {minItems} and at most {maxItems} in it
    /// </summary>
    /// <returns></returns>
    public static NameValueCollection GetRandomNameValueCollection(
        int minItems,
        int maxItems
    )
    {
        var result = new NameValueCollection();
        var howMany = GetRandomInt(minItems, maxItems);
        for (var i = 0; i < howMany; i++)
        {
            var key = GetRandomString();
            var value = GetRandomString();
            result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// Add a special case for generating random values, eg for
    /// types which have no parameterless constructor, but, eg,
    /// may have a .Parse method
    /// NB: this method is NOT thread-safe when used concurrently
    /// with GetRandom&lt;T&gt;()
    /// </summary>
    /// <param name="generator"></param>
    public static void InstallRandomGenerator<T>(
        Func<T> generator
    )
    {
        RandomGenerators[typeof(T)] = () => generator();
    }

    private static readonly ConcurrentDictionary<Type, Func<object>> RandomGenerators = new()
    {
        [typeof(IPAddress)] = () => IPAddress.Parse(GetRandomIPv4Address()),
        [typeof(NameValueCollection)] = GetRandomNameValueCollection
    };

    /// <summary>
    /// Returns a random loaded type in the current app domain
    /// </summary>
    /// <returns></returns>
    public static Type GetRandomType()
    {
        return GetRandomFrom(LoadedTypes);
    }

    private static readonly Type[] EmptyTypes = new Type[0];

    private static readonly Type[] LoadedTypes = AppDomain.CurrentDomain.GetAssemblies()
        .Select(asm =>
            {
                try
                {
                    return asm.GetExportedTypes();
                }
                catch
                {
                    return EmptyTypes;
                }
            }
        )
        .SelectMany(o => o)
        .ToArray();

    /// <summary>
    /// Gets a random value of the specified type by attempting to find the correct
    /// random generator method and invoking it. Works on primitives (eg int, string),
    /// enums and complex objects. When invoked on a complex object, will attempt to fine
    /// (or, if necessary, create) a GenericBuilder to produce the result
    /// </summary>
    /// <param name="type">Type to generate a random value of</param>
    /// <returns>New instance of the specified type. Should be different every time, when possible.</returns>
    public static object GetRandom(
        Type type
    )
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"A generic type definition can't be generated: {type.Name}",
                nameof(type)
            );
        }

        return PrimitiveGenerators.TryGetValue(
            type ?? throw new ArgumentNullException(nameof(type)),
            out var randomGenerator
        )
            ? randomGenerator()
            : GetRandomValueForType(type);
    }

    private static object GetRandomValueForType(
        Type type
    )
    {
        var builder = GetBuilderFor(type);
        if (builder == null)
        {
            throw new Exception(
                "Can't get random value for type: '" + type.Name +
                "': either too complex or I missed a simple type?"
            );
        }

        return builder.GenericWithRandomProps().GenericBuild();
    }

    private static IGenericBuilder GetBuilderFor(
        Type type
    )
    {
        var builderType = GenericBuilderLocator.TryFindExistingBuilderFor(type)
            ?? GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(type);
        return builderType == null
            ? null
            : Activator.CreateInstance(builderType) as IGenericBuilder;
    }

    /// <summary>
    /// Exposes the default range values used within RandomValueGen when
    /// the ranges are omitted by calling code
    /// </summary>
    public static class DefaultRanges
    {
        /// <summary>
        /// Default minimum number of words to generate
        /// </summary>
        public const int MIN_WORDS = 10;

        /// <summary>
        /// Default maximum number of words to generate
        /// </summary>
        public const int MAX_WORDS = 50;

        /// <summary>
        /// Default range of the number of words to generate
        /// </summary>
        public const int DEFAULT_WORD_RANGE = MAX_WORDS - MIN_WORDS;

        /// <summary>
        /// Default minimum length of random strings
        /// </summary>
        public const int MINLENGTH_STRING = 8;

        /// <summary>
        /// Default minimum number of bytes to return
        /// </summary>
        public const int MINLENGTH_BYTES = 0;

        /// <summary>
        /// Default maximum number of bytes to return
        /// </summary>
        public const int MAXLENGTH_BYTES = 1024;

        /// <summary>
        ///  Defines the range of default bytes
        /// </summary>
        public const int DEFAULT_BYTES_RANGE = MAXLENGTH_BYTES - MINLENGTH_BYTES;

        /// <summary>
        /// Default minimum integer value returned
        /// </summary>
        public const int MIN_INT_VALUE = 0;

        /// <summary>
        /// Default maximum integer value returned
        /// </summary>
        public const int MAX_INT_VALUE = 10;

        /// <summary>
        /// Defines the range of default integer max / min
        /// </summary>
        public const int DEFAULT_INT_RANGE = MAX_INT_VALUE - MIN_INT_VALUE;

        /// <summary>
        /// Defines the default minimum money value returned
        /// </summary>
        public const int MIN_MONEY_VALUE = 10;

        /// <summary>
        /// Defines the default maximum money value returned
        /// </summary>
        public const int MAX_MONEY_VALUE = 100;

        /// <summary>
        /// Defines the range of the default money max / min
        /// </summary>
        public const int DEFAULT_MONEY_RANGE = MAX_MONEY_VALUE - MIN_MONEY_VALUE;

        /// <summary>
        /// Defines the default minimum TAX value returned
        /// </summary>
        public const int MIN_TAX_VALUE = 3;

        /// <summary>
        /// Defines the default maximum TAX value returned
        /// </summary>
        public const int MAX_TAX_VALUE = 20;

        /// <summary>
        /// Defines the range of the default TAX max / min
        /// </summary>
        public const int DEFAULT_TAX_RANGE = MAX_TAX_VALUE - MIN_TAX_VALUE;

        /// <summary>
        /// Default minimum long value returned
        /// </summary>
        public const int MIN_LONG_VALUE = 0;

        /// <summary>
        /// Default maximum long value returned
        /// </summary>
        public const int MAX_LONG_VALUE = 1000;

        /// <summary>
        /// Defines the range of default long min / max
        /// </summary>
        public const int DEFAULT_LONG_RANGE = MAX_LONG_VALUE - MIN_LONG_VALUE;

        /// <summary>
        /// Default minimum number of items in a random collection
        /// </summary>
        public const int MIN_ITEMS = 0;

        /// <summary>
        /// Default Maximum number of items in a random collection
        /// </summary>
        public const int MAX_ITEMS = 10;

        /// <summary>
        /// Number of seconds in a day
        /// </summary>
        public const int ONE_DAY_IN_SECONDS = 86400;

        /// <summary>
        /// The default number of items to return via GetSubSetOf
        /// </summary>
        public const int DEFAULT_MIN_SUBSET_ITEMS = 1;

        /// <summary>
        /// The max number of items to return via GetSubSetOf
        /// - negative numbers are inferred to mean (collection size - N)
        /// </summary>
        public const int DEFAULT_MAX_SUBSET_ITEMS = -1;
    }

    private static readonly ThreadSafeRandom RandomGenerator = new();
    private const string DEFAULT_RANDOM_STRING_CHARS = "abcdefghijklmnopqrstuvwxyz1234567890";
    private const string IPV6_STRING_CHARS = "0123456789abcdef";

    /// <summary>
    /// Produces a random integer between 0 and 10 inclusive
    /// </summary>
    /// <returns></returns>
    public static int GetRandomInt()
    {
        return GetRandomInt(
            DefaultRanges.MIN_INT_VALUE,
            DefaultRanges.MAX_INT_VALUE
        );
    }

    /// <summary>
    /// Produces an integer between the provided value and
    /// that value + 10, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static int GetRandomInt(
        int minValue
    )
    {
        return GetRandomInt(
            minValue,
            minValue + DefaultRanges.DEFAULT_INT_RANGE
        );
    }

    /// <summary>
    /// Returns a random integer within the specified range
    /// </summary>
    /// <param name="minValue">Minimum value to consider</param>
    /// <param name="maxValue">Maximum value to consider</param>
    /// <returns>Random integer between minValue and maxValue (inclusive)</returns>
    public static int GetRandomInt(
        int minValue,
        int maxValue
    )
    {
        return (int)GetRandomLong(minValue, maxValue);
    }

    /// <summary>
    /// Returns a random integer from 1 to 1000, as might
    /// be seen on an integer id field in a database.
    /// </summary>
    /// <returns></returns>
    public static int GetRandomIntKey()
    {
        return GetRandomIntKey(1000);
    }

    /// <summary>
    /// Returns a random integer from 1 to {maxValue}, as might
    /// be seen on an integer id field in a database.
    /// </summary>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown if maxValue is provided &lt; 2</exception>
    public static int GetRandomIntKey(int maxValue)
    {
        if (maxValue < 2)
        {
            throw new ArgumentException(
                $"maxValue for {nameof(GetRandomIntKey)} must be at least 2"
            );
        }

        return GetRandomInt(1, maxValue);
    }

    /// <summary>
    /// Returns a random boolean value
    /// </summary>
    /// <returns>True or False</returns>
    public static bool GetRandomBoolean()
    {
        return GetRandomInt(1, 100) < 50;
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Gets a pseudo-random mimetype (picks from a short list of known mime types)
    /// </summary>
    /// <returns>String which is a valid mime type</returns>
    [Obsolete("This has been renamed to GetRandomMimeType and will be removed in a future release")]
    public static string GetRandomMIMEType()
    {
        return GetRandomMimeType();
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Gets a pseudo-random mimetype (picks from a short list of known mime types)
    /// </summary>
    /// <returns>String which is a valid mime type</returns>
    public static string GetRandomMimeType()
    {
        return GetRandomFrom(
            MimeTypes.KnownMimeTypes
        );
    }

    /// <summary>
    /// Produces a random long between 0 and 1000, inclusive
    /// </summary>
    /// <returns></returns>
    public static long GetRandomLong()
    {
        return GetRandomLong(
            DefaultRanges.MIN_LONG_VALUE
        );
    }

    /// <summary>
    /// Returns a random long between the provided min value and
    /// that value + 1000, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static long GetRandomLong(
        long minValue
    )
    {
        return GetRandomLong(
            minValue,
            minValue + DefaultRanges.DEFAULT_LONG_RANGE
        );
    }

    /// <summary>
    /// Returns a random long within the specified range
    /// </summary>
    /// <param name="minValue">Minimum value to consider</param>
    /// <param name="maxValue">Maximum value to consider</param>
    /// <returns>Random integer between minValue and maxValue (inclusive)</returns>
    public static long GetRandomLong(
        long minValue,
        long maxValue
    )
    {
        if (minValue > maxValue)
        {
            var swap = minValue;
            minValue = maxValue;
            maxValue = swap;
        }

        var dec = RandomGenerator.NextDouble();
        var range = maxValue - minValue + 1;
        return minValue + (long)(range * dec);
    }

    /// <summary>
    /// Gets a random string
    /// </summary>
    /// <param name="minLength">Minimum length required</param>
    /// <param name="maxLength">Maximum length required. When left null, will be the minimum length plus DefaultRanges.MINLENGTH_STRING</param>
    /// <param name="charSet">Character set to use, as required</param>
    /// <returns>
    /// A new string which is between the minimum and maximum lengths (inclusive)
    /// and which is made up of the provided (or default, when not provided) character set
    /// </returns>
    public static string GetRandomString(
        int minLength = DefaultRanges.MINLENGTH_STRING,
        int? maxLength = null,
        string charSet = null
    )
    {
        var actualMaxLength = maxLength ?? minLength + DefaultRanges.MINLENGTH_STRING;
        var actualLength = GetRandomInt(minLength, actualMaxLength);
        var chars = new List<char>();
        charSet ??= DEFAULT_RANDOM_STRING_CHARS;

        var charSetLength = charSet.Length;
        for (var i = 0; i < actualLength; i++)
        {
            var pos = GetRandomInt(0, charSetLength - 1);
            chars.Add(charSet[pos]);
        }

        return string.Join(string.Empty, chars.Select(c => c.ToString()).ToArray());
    }

    /// <summary>
    /// Gets a random Local DateTime value, by default within SQL-safe range
    /// </summary>
    /// <param name="minDate">Minimum date to consider</param>
    /// <param name="maxDate">Maximum date to consider</param>
    /// <param name="dateOnly">Flag to determine if times should be truncated</param>
    /// <param name="minTime">Minimum time to consider (default all)</param>
    /// <param name="maxTime">Maximum time to consider (default all)</param>
    /// <returns>Random Local DateTime within the specified range</returns>
    public static DateTime GetRandomDate(
        DateTime? minDate,
        DateTime? maxDate,
        bool dateOnly,
        DateTime? minTime,
        DateTime? maxTime
    )
    {
        var kind = ResolveRequiredDateTimeKind(minDate, maxDate);
        return GetRandomDate(
            kind,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
    }


    /// <summary>
    /// Returns a local random datetime
    /// </summary>
    /// <returns></returns>
    public static DateTime GetRandomDate()
    {
        return GetRandomDate(DateTimeKind.Local);
    }

    /// <summary>
    /// Returns a random date-time with the required kind
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static DateTime GetRandomDate(
        DateTimeKind kind
    )
    {
        return GetRandomDate(
            kind,
            DefaultMinimumDate
        );
    }

    private static readonly DateTime DefaultMinimumDate = new(1990, 1, 1);
    private const int RANDOM_DATE_AUTOMATIC_RANGE_YEARS = 30;

    /// <summary>
    /// Gets a random DateTime value of the specified kind, by default within SQL-safe range
    /// </summary>
    /// <param name="kind">DateTimeKind required for this value</param>
    /// <param name="minDate">Minimum date to consider</param>
    /// <param name="maxDate">Maximum date to consider</param>
    /// <param name="dateOnly">Flag to determine if times should be truncated</param>
    /// <param name="minTime">Minimum time to consider (default all)</param>
    /// <param name="maxTime">Maximum time to consider (default all)</param>
    /// <returns>Random DateTime value of the specified kind, within the specified range</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static DateTime GetRandomDate(
        DateTimeKind kind,
        DateTime? minDate,
        DateTime? maxDate,
        bool dateOnly, // = false,
        DateTime? minTime, // = null,
        DateTime? maxTime // = null
    )
    {
        if (dateOnly)
        {
            minDate = minDate?.AddTicks(-1).AddDays(1).StartOfDay();
            maxDate = maxDate?.StartOfDay().AddDays(1).AddTicks(-1);
            if (minDate > maxDate)
            {
                minDate = minDate.Value.AddDays(-1);
            }
        }

        var minTicks = (minDate ?? maxDate?.AddYears(-RANDOM_DATE_AUTOMATIC_RANGE_YEARS) ?? DefaultMinimumDate).Ticks;
        var maxTicks = (maxDate ?? new DateTime(minTicks).AddYears(RANDOM_DATE_AUTOMATIC_RANGE_YEARS)).Ticks;
        var actualTicks = GetRandomLong(minTicks, maxTicks);
        var rawDateTime = new DateTime(actualTicks);
        var sanitised = new DateTime(
            rawDateTime.Year,
            rawDateTime.Month,
            rawDateTime.Day,
            rawDateTime.Hour,
            rawDateTime.Minute,
            rawDateTime.Second,
            rawDateTime.Millisecond,
            kind
        );
        return dateOnly
            ? sanitised.StartOfDay()
            : RangeCheckTimeOnRandomDate(minTime, maxTime, sanitised);
    }

    /// <summary>
    /// Gets a random DateTime value of the specified kind, by default within SQL-safe range
    /// </summary>
    /// <param name="minDate">Minimum date to consider</param>
    /// <param name="maxDate">Maximum date to consider</param>
    /// <param name="dateOnly">Flag to determine if times should be truncated</param>
    /// <param name="minTime">Minimum time to consider (default all)</param>
    /// <param name="maxTime">Maximum time to consider (default all)</param>
    /// <returns>Random DateTime value of the specified kind, within the specified range</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static DateTime GetRandomDate(
        // ReSharper disable once MethodOverloadWithOptionalParameter
        // if I use ReSharper alt-enter to pull out an overload,
        // then code referencing this method doesn't get it any more,
        // so we're going to not believe ReSharper for a little bit here
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool dateOnly = false,
        TimeSpan? minTime = null,
        TimeSpan? maxTime = null
    )
    {
        var kind = ResolveRequiredDateTimeKind(minDate, maxDate);
        return GetRandomDate(
            kind,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
    }

    private static DateTimeKind ResolveRequiredDateTimeKind(
        DateTime? minDate,
        DateTime? maxDate
    )
    {
        var kind = minDate?.Kind ?? maxDate?.Kind ?? DateTimeKind.Local;
        return kind == DateTimeKind.Unspecified
            ? DateTimeKind.Local
            : kind;
    }

    /// <summary>
    /// Gets a random DateTime value of the specified kind, by default within SQL-safe range
    /// </summary>
    /// <param name="kind">DateTimeKind required for this value</param>
    /// <param name="minDate">Minimum date to consider</param>
    /// <param name="maxDate">Maximum date to consider</param>
    /// <param name="dateOnly">Flag to determine if times should be truncated</param>
    /// <param name="minTime">Minimum time to consider (default all)</param>
    /// <param name="maxTime">Maximum time to consider (default all)</param>
    /// <returns>Random DateTime value of the specified kind, within the specified range</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static DateTime GetRandomDate(
        DateTimeKind kind,
        DateTime? minDate,
        DateTime? maxDate = null,
        bool dateOnly = false,
        TimeSpan? minTime = null,
        TimeSpan? maxTime = null
    )
    {
        var dateRangeLower = new DateTime(1990, 1, 1);
        const int dateRangeYears = 30;

        if (minDate is not null &&
            maxDate is not null &&
            minDate.Value > maxDate.Value)
        {
            var swap = minDate;
            minDate = maxDate;
            maxDate = swap;
        }

        if (dateOnly)
        {
            minDate = minDate?.AddTicks(-1).AddDays(1).StartOfDay();
            maxDate = maxDate?.StartOfDay().AddDays(1).AddTicks(-1);
            if (minDate > maxDate)
            {
                minDate = minDate.Value.AddDays(-1);
            }
        }

        var minTicks = (minDate ?? maxDate?.AddYears(-dateRangeYears) ?? dateRangeLower).Ticks;
        var maxTicks = (maxDate ?? new DateTime(minTicks).AddYears(dateRangeYears)).Ticks;
        var actualTicks = GetRandomLong(minTicks, maxTicks);
        var rawDateTime = new DateTime(actualTicks);
        var sanitised = new DateTime(
            rawDateTime.Year,
            rawDateTime.Month,
            rawDateTime.Day,
            rawDateTime.Hour,
            rawDateTime.Minute,
            rawDateTime.Second,
            rawDateTime.Millisecond,
            kind
        );
        return dateOnly
            ? sanitised.StartOfDay()
            : RangeCheckTimeOnRandomDate(minTime, maxTime, sanitised);
    }

    /// <summary>
    /// Like GetRandomDate, but for DateTimeOffset values
    /// </summary>
    /// <param name="minDate"></param>
    /// <param name="maxDate"></param>
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
    /// <param name="dateOnly"></param>
    /// <returns></returns>
    public static DateTimeOffset GetRandomDateTimeOffset(
        DateTimeOffset? minDate = null,
        DateTimeOffset? maxDate = null,
        TimeSpan? minTime = null,
        TimeSpan? maxTime = null,
        bool dateOnly = false
    )
    {
        var dateRangeLower = new DateTimeOffset(DefaultMinimumDate);
        const int dateRangeYears = 30;
        minDate ??= dateRangeLower;
        maxDate ??= new DateTimeOffset(DateTime.Now.AddYears(dateRangeYears));

        if (maxDate < minDate)
        {
            var swap = minDate;
            minDate = maxDate;
            maxDate = swap;
        }

        return new DateTimeOffset(
            GetRandomDate(
                minDate.Value.DateTime,
                maxDate.Value.DateTime,
                dateOnly,
                minTime,
                maxTime
            )
        );
    }

    /// <summary>
    /// Returns a random UTC date within the specified range
    /// </summary>
    /// <param name="minDate"></param>
    /// <param name="maxDate"></param>
    /// <param name="dateOnly"></param>
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public static DateTime GetRandomUtcDate(
        DateTime? minDate,
        DateTime? maxDate,
        bool dateOnly,
        DateTime? minTime,
        DateTime? maxTime
    )
    {
        return GetRandomDate(
            DateTimeKind.Utc,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
    }

    /// <summary>
    /// Returns a random UTC date within the specified range
    /// </summary>
    /// <param name="minDate"></param>
    /// <param name="maxDate"></param>
    /// <param name="dateOnly"></param>
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public static DateTime GetRandomUtcDate(
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool dateOnly = false,
        TimeSpan? minTime = null,
        TimeSpan? maxTime = null
    )
    {
        return GetRandomDate(
            DateTimeKind.Utc,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
    }

    /// <summary>
    /// Gets a random Local DateTime range, by default within SQL-safe range
    /// </summary>
    /// <param name="minDate">Minimum date to consider</param>
    /// <param name="maxDate">Maximum date to consider</param>
    /// <param name="dateOnly">Flag to determine if times should be truncated</param>
    /// <param name="minTime">Minimum time to consider (default all)</param>
    /// <param name="maxTime">Maximum time to consider (default all)</param>
    /// <returns>Random Local DateTime value</returns>
    public static DateRange GetRandomDateRange(
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool dateOnly = false,
        DateTime? minTime = null,
        DateTime? maxTime = null
    )
    {
        var kind = ResolveRequiredDateTimeKind(minDate, maxDate);
        return GetRandomDateRange(
            kind,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
    }

    /// <summary>
    /// Gets a random Local DateTime range, by default within SQL-safe range
    /// </summary>
    /// <param name="minDate">Minimum date to consider</param>
    /// <param name="maxDate">Maximum date to consider</param>
    /// <param name="dateOnly">Flag to determine if times should be truncated</param>
    /// <param name="minTime">Minimum time to consider (default all)</param>
    /// <param name="maxTime">Maximum time to consider (default all)</param>
    /// <returns>Random Local DateTime value</returns>
    public static DateRange GetRandomUtcDateRange(
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool dateOnly = false,
        DateTime? minTime = null,
        DateTime? maxTime = null
    )
    {
        return GetRandomDateRange(
            DateTimeKind.Utc,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
    }

    /// <summary>
    /// Gets a random timespan
    /// </summary>
    /// <param name="min">Minimum length</param>
    /// <param name="max">Maximum length</param>
    /// <param name="context">Context for the min/max length</param>
    /// <returns></returns>
    public static TimeSpan GetRandomTimeSpan(
        double min,
        double max = DefaultRanges.MAX_INT_VALUE,
        TimeSpanContexts context = TimeSpanContexts.Minutes
    )
    {
        var howMany = GetRandomDouble(min, max);
        return TimespanGenerators[context](howMany);
    }

    /// <summary>
    /// Returns a TimeSpan between min and max
    /// </summary>
    /// <param name="min">min timespan -- defaults to TimeSpan.Zero</param>
    /// <param name="max">max timespan -- defaults to TimeSpan.MaxValue</param>
    /// <returns></returns>
    public static TimeSpan GetRandomTimeSpan(
        TimeSpan min,
        TimeSpan? max = null
    )
    {
        max ??= TimeSpan.MaxValue;
        var ticksDelta = GetRandomLong(0, (max - min).Value.Ticks);
        return TimeSpan.FromTicks(min.Ticks + ticksDelta);
    }

    /// <summary>
    /// Returns a random TimeSpan between TimeSpan.Z
    /// </summary>
    /// <returns></returns>
    public static TimeSpan GetRandomTimeSpan()
    {
        return GetRandomTimeSpan(
            TimeSpan.FromDays(-7),
            TimeSpan.FromDays(7)
        );
    }

    private static readonly Dictionary<TimeSpanContexts, Func<double, TimeSpan>>
        TimespanGenerators = new Dictionary<TimeSpanContexts, Func<double, TimeSpan>>()
        {
            [TimeSpanContexts.Ticks] = i => TimeSpan.FromTicks((long)i),
            [TimeSpanContexts.Milliseconds] = TimeSpan.FromMilliseconds,
            [TimeSpanContexts.Seconds] = TimeSpan.FromSeconds,
            [TimeSpanContexts.Minutes] = TimeSpan.FromMinutes,
            [TimeSpanContexts.Hours] = TimeSpan.FromHours,
            [TimeSpanContexts.Days] = TimeSpan.FromDays
        };

    /// <summary>
    /// Gets a random DateTime range of the specified kind, by default within SQL-safe range
    /// </summary>
    /// <param name="kind">DateTimeKind required for this value</param>
    /// <param name="minDate">Minimum date to consider</param>
    /// <param name="maxDate">Maximum date to consider</param>
    /// <param name="dateOnly">Flag to determine if times should be truncated</param>
    /// <param name="minTime">Minimum time to consider (default all)</param>
    /// <param name="maxTime">Maximum time to consider (default all)</param>
    /// <returns>DateRange object with From and To within the specified range</returns>
    public static DateRange GetRandomDateRange(
        DateTimeKind kind,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool dateOnly = false,
        DateTime? minTime = null,
        DateTime? maxTime = null
    )
    {
        var fromDate = GetRandomDate(
            kind,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
        var toDate = GetRandomDate(
            kind,
            minDate,
            maxDate,
            dateOnly,
            minTime,
            maxTime
        );
        return new DateRange(fromDate, toDate);
    }

    internal static DateTime RangeCheckTimeOnRandomDate(
        TimeSpan? minTime,
        TimeSpan? maxTime,
        DateTime value
    )
    {
        var baseDate = new DateTime(value.Year, value.Month, value.Day);
        var minDate = baseDate.Add(minTime ?? TimeSpan.FromSeconds(0));
        var maxDate = baseDate.Add(maxTime ?? TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1)));

        if (minDate > maxDate)
        {
            var swap = minDate;
            minDate = maxDate;
            maxDate = swap;
        }

        return value > maxDate || value < minDate
            ? GetRandomDate(minDate, maxDate)
            : value;
    }

    internal static DateTime RangeCheckTimeOnRandomDate(
        DateTime? minTime,
        DateTime? maxTime,
        DateTime value
    )
    {
        var baseDate = new DateTime(value.Year, value.Month, value.Day);
        minTime = baseDate.Add(minTime?.TimeOfDay ?? TimeSpan.FromSeconds(0));
        maxTime = baseDate.Add(maxTime?.TimeOfDay ?? TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1)));

        if (minTime > maxTime)
        {
            // ReSharper disable once SwapViaDeconstruction
            var swap = minTime;
            minTime = maxTime;
            maxTime = swap;
        }

        return value > maxTime || value < minTime
            ? GetRandomDate(minTime, maxTime)
            : value;
    }

    /// <summary>
    /// Produces a random double value between 0 and 10 inclusive
    /// </summary>
    /// <returns></returns>
    public static double GetRandomDouble()
    {
        return GetRandomDouble(DefaultRanges.MIN_INT_VALUE);
    }

    /// <summary>
    /// Produces a random double value between the provides
    /// double value and that value + 10, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static double GetRandomDouble(
        double minValue
    )
    {
        return GetRandomDouble(
            minValue,
            minValue + DefaultRanges.DEFAULT_INT_RANGE
        );
    }

    /// <summary>
    /// Gets a random double value within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Double value within the specified range</returns>
    public static double GetRandomDouble(
        double min,
        double max
    )
    {
        return (RandomGenerator.NextDouble() * (max - min)) + min;
    }

    /// <summary>
    /// Gets a random decimal value within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Decimal value within the specified range</returns>
    public static decimal GetRandomDecimal(
        double min,
        double max
    )
    {
        return GetRandomDecimal((decimal)min, (decimal)max);
    }

    /// <summary>
    /// Gets a random decimal value within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Decimal value within the specified range</returns>
    public static decimal GetRandomDecimal(
        long min,
        long max
    )
    {
        return GetRandomDecimal((decimal)min, max);
    }

    /// <summary>
    /// Gets a random decimal value within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Decimal value within the specified range</returns>
    public static decimal GetRandomDecimal(
        decimal min,
        decimal max
    )
    {
        return (decimal)GetRandomDouble((double)min, (double)max);
    }

    /// <summary>
    /// Produces a random decimal between 0 and 10 inclusive
    /// </summary>
    /// <returns></returns>
    public static decimal GetRandomDecimal()
    {
        return GetRandomDecimal(
            DefaultRanges.MIN_INT_VALUE
        );
    }

    /// <summary>
    /// Produces a random decimal between the provided
    /// minValue and that value + 10, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static decimal GetRandomDecimal(
        decimal minValue
    )
    {
        return GetRandomDecimal(
            minValue,
            minValue + DefaultRanges.DEFAULT_INT_RANGE
        );
    }

    /// <summary>
    /// Gets a random money value (decimal with max 2 places)
    /// within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Decimal value within the specified range</returns>
    public static decimal GetRandomMoney(
        decimal min,
        decimal max
    )
    {
        return (decimal)GetRandomDouble((double)min, (double)max)
            .ToFixed(2);
    }

    /// <summary>
    /// Produces a money value (decimal with max 2 places)
    /// between 10 and 100 inclusive
    /// </summary>
    /// <returns></returns>
    public static decimal GetRandomMoney()
    {
        return GetRandomMoney(
            DefaultRanges.MIN_MONEY_VALUE
        );
    }

    /// <summary>
    /// Produces a random decimal between the provided
    /// minValue and that value + 10, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static decimal GetRandomMoney(
        decimal minValue
    )
    {
        return GetRandomMoney(
            minValue,
            minValue + DefaultRanges.DEFAULT_MONEY_RANGE
        );
    }

    /// <summary>
    /// Gets a random tax rate value (decimal with max 2 places)
    /// within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Decimal value within the specified range</returns>
    public static decimal GetRandomTaxRate(
        decimal min,
        decimal max
    )
    {
        return (decimal)GetRandomDouble((double)min, (double)max)
            .ToFixed(2);
    }

    /// <summary>
    /// Produces a tax rate value (decimal with max 2 places)
    /// between 3 and 20 inclusive
    /// </summary>
    /// <returns></returns>
    public static decimal GetRandomTaxRate()
    {
        return GetRandomTaxRate(
            DefaultRanges.MIN_TAX_VALUE
        );
    }

    /// <summary>
    /// Produces a tax rate value (decimal with max 2 places)
    /// between of the provided minimum value up to that value
    /// + 17
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static decimal GetRandomTaxRate(
        decimal minValue
    )
    {
        return GetRandomTaxRate(
            minValue,
            minValue + DefaultRanges.DEFAULT_TAX_RANGE
        );
    }

    /// <summary>
    /// Gets a random interest rate value (decimal with max 2 places)
    /// within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Decimal value within the specified range</returns>
    public static decimal GetRandomInterestRate(
        decimal min,
        decimal max
    )
    {
        return (decimal)GetRandomDouble((double)min, (double)max)
            .ToFixed(2);
    }

    /// <summary>
    /// Produces a interest rate value (decimal with max 2 places)
    /// between 3 and 20 inclusive
    /// </summary>
    /// <returns></returns>
    public static decimal GetRandomInterestRate()
    {
        return GetRandomInterestRate(
            DefaultRanges.MIN_TAX_VALUE
        );
    }

    /// <summary>
    /// Produces a interest rate value (decimal with max 2 places)
    /// between of the provided minimum value up to that value
    /// + 17
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static decimal GetRandomInterestRate(
        decimal minValue
    )
    {
        return GetRandomInterestRate(
            minValue,
            minValue + DefaultRanges.DEFAULT_TAX_RANGE
        );
    }

    /// <summary>
    /// Produces a random decimal between the provided
    /// minValue and that value + 10, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static decimal GetRandomDecimal(long minValue)
    {
        return GetRandomDecimal(minValue, DefaultRanges.MAX_INT_VALUE);
    }

    /// <summary>
    /// Produces a random decimal between the provided
    /// minValue and that value + 10, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static decimal GetRandomDecimal(double minValue)
    {
        return GetRandomDecimal(minValue, DefaultRanges.MAX_INT_VALUE);
    }

    /// <summary>
    /// Produces a random float between 0 and 10 inclusive
    /// </summary>
    /// <returns></returns>
    public static float GetRandomFloat()
    {
        return GetRandomFloat(
            DefaultRanges.MIN_INT_VALUE
        );
    }

    /// <summary>
    /// Produces a random float between the provided
    /// minValue and that value + 10, inclusive
    /// </summary>
    /// <param name="minValue"></param>
    /// <returns></returns>
    public static float GetRandomFloat(
        float minValue
    )
    {
        return GetRandomFloat(
            minValue,
            minValue + DefaultRanges.MIN_INT_VALUE
        );
    }

    /// <summary>
    /// Gets a random float value within the specified range
    /// </summary>
    /// <param name="min">Minimum value to consider</param>
    /// <param name="max">Maximum value to consider</param>
    /// <returns>Float value within the specified range</returns>
    public static float GetRandomFloat(
        float min,
        float max
    )
    {
        return (float)GetRandomDouble(min, max);
    }

    /// <summary>
    /// Produces a random time of day
    /// </summary>
    /// <returns></returns>
    public static TimeSpan GetRandomTimeOfDay()
    {
        return GetRandomTimeOfDay(
            TimeSpan.FromSeconds(0)
        );
    }

    /// <summary>
    /// Produces a random time of day from the provided
    /// minimum, inclusive
    /// </summary>
    /// <param name="min"></param>
    /// <returns></returns>
    public static TimeSpan GetRandomTimeOfDay(
        TimeSpan min
    )
    {
        return GetRandomTimeOfDay(
            min,
            TimeSpan.FromSeconds(DefaultRanges.ONE_DAY_IN_SECONDS)
        );
    }

    /// <summary>
    /// Returns a random time of day
    /// </summary>
    /// <param name="min">Minimum time to consider</param>
    /// <param name="max">Maximum time to consider</param>
    /// <returns>Timespan representing a time on a day, clamped to within 24 hours</returns>
    public static TimeSpan GetRandomTimeOfDay(
        TimeSpan min,
        TimeSpan max
    )
    {
        var minSeconds = min.TotalSeconds;
        var maxSeconds = max.TotalSeconds;
        if (minSeconds < 0)
        {
            minSeconds = 0;
        }

        if (maxSeconds > DefaultRanges.ONE_DAY_IN_SECONDS)
        {
            maxSeconds = DefaultRanges.ONE_DAY_IN_SECONDS;
        }

        return TimeSpan.FromSeconds(
            GetRandomInt((int)minSeconds, (int)maxSeconds)
        );
    }

    /// <summary>
    /// Produces an array of random bytes between 0 and 1024
    /// in length, inclusive
    /// </summary>
    /// <returns></returns>
    public static byte[] GetRandomBytes()
    {
        return GetRandomBytes(DefaultRanges.MINLENGTH_BYTES);
    }

    /// <summary>
    /// Produces some random bytes, of at least minLength
    /// in size, up to that length + 1024, inclusive
    /// </summary>
    /// <param name="minLength"></param>
    /// <returns></returns>
    public static byte[] GetRandomBytes(
        int minLength
    )
    {
        return GetRandomBytes(
            minLength,
            minLength + DefaultRanges.DEFAULT_BYTES_RANGE
        );
    }

    /// <summary>
    /// Gets a randomly-sized, randomly-filled byte array
    /// </summary>
    /// <param name="minLength">Minimum size of the result</param>
    /// <param name="maxLength">Maximum size of the result</param>
    /// <returns>Randomly-filled byte array</returns>
    public static byte[] GetRandomBytes(
        int minLength,
        int maxLength
    )
    {
        var bytes = new byte[RandomGenerator.Next(minLength, maxLength)];
        RandomGenerator.NextBytes(bytes);
        return bytes;
    }

    /// <summary>
    /// Gets a random email-like string. Note that this is only email-like in that it
    /// conforms to the structure:
    /// {random-string}@{random-string}.com
    /// </summary>
    /// <returns>Random email-like string</returns>
    public static string GetRandomEmail()
    {
        return GetRandomEmail(null);
    }

    /// <summary>
    /// Gets a random email-like string. Note that this is only email-like in that it
    /// conforms to the structure:
    /// {random-string}@{random-string}.com
    /// <param name="firstName">Use a first-name to guide the process</param>
    /// </summary>
    /// <returns>Random email-like string</returns>
    public static string GetRandomEmail(string firstName)
    {
        return GetRandomEmail(firstName, null);
    }

    /// <summary>
    /// Gets a random email-like string. Note that this is only email-like in that it
    /// conforms to the structure:
    /// {random-string}@{random-string}.com
    /// <param name="firstName">Use a first-name to guide the process</param>
    /// <param name="lastName">Use a last-name to guide the process</param>
    /// </summary>
    /// <returns>Random email-like string</returns>
    public static string GetRandomEmail(string firstName, string lastName)
    {
        return string.Join(
            string.Empty,
            GetRandomUserName(firstName, lastName),
            "@",
            GetRandomDomain()
        ).ToLower();
    }

    /// <summary>
    /// Returns a random domain generated from a fairly common internet
    /// company name and one of the known TLDs at time of generation
    /// </summary>
    /// <returns></returns>
    public static string GetRandomDomain()
    {
        return $@"{
            GetRandomFrom(NaturalData.CompaniesWithInternetDomains)
        }{
            GetRandomFrom(NaturalData.TopLevelDomains)
        }";
    }

    /// <summary>
    /// Produces a random username:
    /// - will have a first name
    /// - will optionally have a .surname suffix
    /// - will optionally have a numeric suffix
    /// </summary>
    /// <returns></returns>
    public static string GetRandomUserName()
    {
        return GetRandomUserName(null);
    }

    /// <summary>
    /// Produces a random username:
    /// - will have a first name
    /// - will optionally have a .surname suffix
    /// - will optionally have a numeric suffix
    /// <param name="firstName">Use a first-name to guide the process</param>
    /// </summary>
    /// <returns></returns>
    public static string GetRandomUserName(string firstName)
    {
        return GetRandomUserName(firstName, null);
    }

    /// <summary>
    /// Produces a random username:
    /// - will have a first name
    /// - will optionally have a .surname suffix
    /// - will optionally have a numeric suffix
    /// <param name="firstName">Use a first-name to guide the process</param>
    /// <param name="lastName">Use a last-name to guide the process</param>
    /// </summary>
    /// <returns></returns>
    public static string GetRandomUserName(
        string firstName,
        string lastName
    )
    {
        var result = new List<string>
        {
            firstName ?? GetRandomFirstName()
        };

        if (GetRandomBoolean())
        {
            // sometimes add a full last name with an optional joiner
            result.Add(GetRandomFrom(Joiners));
            result.Add($"{lastName ?? GetRandomLastName()}");
        }
        else if (GetRandomBoolean())
        {
            // sometimes add a last-name first-char
            result.Add(GetRandomLastName()[0].ToString());
        }

        if (GetRandomBoolean())
        {
            // sometimes add a numeric suffix
            result.Add($"{GetRandomInt(1, 99999):00}");
        }

        return string.Join("", result).ToLower();
    }

    /// <summary>
    /// Generates a city name
    /// </summary>
    /// <returns></returns>
    public static string GetRandomCityName()
    {
        var prefix = GetRandomBoolean()
            ? GetRandomFrom(NaturalData.CityPrefixes)
            : "";
        var suffix = GetRandomFrom(NaturalData.CitySuffixes);
        return $"{prefix} {GetRandomFirstName()}{suffix}".Trim();
    }

    /// <summary>
    /// Returns a random country
    /// </summary>
    /// <returns></returns>
    public static string GetRandomCountry()
    {
        return GetRandomFrom(NaturalData.Countries);
    }

    /// <summary>
    /// Returns a random country code
    /// </summary>
    /// <returns></returns>
    public static string GetRandomCountryCode()
    {
        return GetRandomFrom(NaturalData.CountryCodes);
    }

    /// <summary>
    /// Generates a street address
    /// </summary>
    /// <returns></returns>
    public static string GetRandomStreetAddress()
    {
        return GetRandomStreetAddress(null);
    }

    /// <summary>
    /// Generates a street name
    /// </summary>
    /// <returns></returns>
    public static string GetRandomStreetName()
    {
        return $@"{
            (GetRandomBoolean() ? GetRandomLastName() : GetRandomFirstName())
        } {
            GetRandomFrom(NaturalData.StreetSuffix)
        }";
    }

    /// <summary>
    /// Generates a street number (eg 12 or 134a)
    /// </summary>
    /// <returns></returns>
    public static string GetRandomStreetNumber()
    {
        var append = GetRandomBoolean()
            ? GetRandomFrom(NumberSuffixes)
            : "";
        return $"{GetRandomInt(1, 1000)}{append}";
    }

    /// <summary>
    /// Generates a street address, given a street name
    /// </summary>
    /// <param name="streetName"></param>
    /// <returns></returns>
    public static string GetRandomStreetAddress(
        string streetName
    )
    {
        return GetRandomStreetAddress(null, streetName);
    }

    /// <summary>
    /// Generates a street address, given a street number and / or name
    /// (null values are discarded &amp; random values will be generated)
    /// </summary>
    /// <param name="streetNumber"></param>
    /// <param name="streetName"></param>
    /// <returns></returns>
    public static string GetRandomStreetAddress(
        string streetNumber,
        string streetName
    )
    {
        return $@"{
            streetNumber ?? GetRandomStreetNumber()
        } {streetName ?? GetRandomStreetName()}";
    }

    /// <summary>
    /// Generates a random postal code
    /// </summary>
    /// <returns></returns>
    public static string GetRandomPostalCode()
    {
        var append = GetRandomBoolean()
            ? GetRandomAlphaString(2, 4)
            : "";
        return $"{GetRandomInt(1000, 9999)}{append}";
    }

    /// <summary>
    /// Generates a random multi-line address
    /// </summary>
    /// <returns></returns>
    public static string GetRandomAddress()
    {
        return GetRandomAddress(null, null, null);
    }

    internal static string GetRandomAddress(
        string streetAddress,
        string cityName,
        string postalCode
    )
    {
        return string.Join(
            Environment.NewLine,
            streetAddress ?? GetRandomStreetAddress(),
            cityName ?? GetRandomCityName(),
            postalCode ?? GetRandomPostalCode()
        );
    }

    private static readonly string[] Joiners =
    [
        ".",
        "-",
        "_",
        ""
    ];

    private static readonly string[] NumberSuffixes =
    [
        "a",
        "b",
        "c"
    ];

    /// <summary>
    /// Returns a random first name, sourced from unique top names:
    /// - male, international (100)
    /// - female, international (100)
    /// - south african (100)
    /// </summary>
    /// <returns></returns>
    public static string GetRandomFirstName()
    {
        return GetRandomFrom(NaturalData.FirstNames);
    }

    /// <summary>
    /// Generates a random fantasy place name
    /// </summary>
    /// <returns></returns>
    public static string GetRandomPlaceName()
    {
        var hasInfix = GetRandomBoolean();
        return $@"{
            GetRandomFrom(NaturalData.PlacePrefixes)
        }{
            (hasInfix ? GetRandomFrom(NaturalData.PlaceInfixes) : "")
        }{GetRandomFrom(NaturalData.PlaceSuffixes)}";
    }

    /// <summary>
    /// Returns a random last name, source from unique top names:
    /// - international (100)
    /// - african (100)
    /// </summary>
    /// <returns></returns>
    public static string GetRandomLastName()
    {
        return GetRandomFrom(NaturalData.LastNames);
    }

    /// <summary>
    /// Returns a random 
    /// </summary>
    /// <returns></returns>
    public static string GetRandomTopLevelDomainName()
    {
        return GetRandomFrom(NaturalData.TopLevelDomains);
    }

    /// <summary>
    /// Returns a random name of the format {FirstName} {LastName}
    /// </summary>
    /// <returns></returns>
    public static string GetRandomName()
    {
        return $"{GetRandomFirstName()} {GetRandomLastName()}";
    }

    /// <summary>
    /// Gets a random filename. Does not use the OS-level temporary filename
    /// functions
    /// </summary>
    /// <returns>String which is a random filename with a 3 character extension</returns>
    public static string GetRandomFileName()
    {
        return string.Join(
            ".",
            GetRandomString(10, 20),
            GetRandomFileExtension()
        );
    }

    /// <summary>
    /// Returns a random valid file extension
    /// </summary>
    /// <returns></returns>
    public static string GetRandomFileExtension()
    {
        return GetRandomFrom(MimeTypes.KnownFileExtensions);
    }

    /// <summary>
    /// Gets a random path which resembles a Windows path, including a leading drive
    /// </summary>
    /// <returns>String which looks like a local Windows path</returns>
    public static string GetRandomWindowsPath()
    {
        var folders = GetRandomCollection<string>(1, 4);
        // ReSharper disable once StringLiteralTypo
        var drive = GetRandomString(1, 1, "ABCDEGHIJKLMNOPQRSTUVWXYZ") + ":";
        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        return string.Join(
            "\\",
            new[]
            {
                drive
            }.And(folders.ToArray())
        );
    }

    /// <summary>
    /// Produces a collection of words between 10 and 50 words in
    /// length, inclusive
    /// </summary>
    /// <returns></returns>
    public static string GetRandomWords()
    {
        return GetRandomWords(DefaultRanges.MIN_WORDS);
    }

    /// <summary>
    /// Produces a collection of words with count of at
    /// least minWords up to and including minWords + 50
    /// </summary>
    /// <param name="minWords"></param>
    /// <returns></returns>
    public static string GetRandomWords(
        int minWords
    )
    {
        return GetRandomWords(
            minWords,
            minWords + DefaultRanges.DEFAULT_WORD_RANGE
        );
    }

    /// <summary>
    /// Gets some random pseudo-words. Note that they (probably) won't be
    /// readable words -- just a collection of strings with whitespace in between.
    /// Think of this as something like Lorei Ipsum, except with zero meaning.
    /// </summary>
    /// <param name="minWords">Minimum number of "words" to return</param>
    /// <param name="maxWords">Maximum number of "words" to return</param>
    /// <returns>Block of text with "words" and whitespace</returns>
    public static string GetRandomWords(
        int minWords,
        int maxWords
    )
    {
        var actual = GetRandomInt(minWords, maxWords);
        var words = new List<string>();
        for (var i = 0; i < actual; i++)
        {
            words.Add(GetRandomAlphaNumericString(1, 10));
        }

        return string.Join(" ", words.ToArray());
    }

    /// <summary>
    /// Generates a random string which looks a lot like an http url, to
    /// the domain only (no path or parameters)
    /// </summary>
    /// <returns>Random http-url-like string</returns>
    public static string GetRandomHttpUrl()
    {
        return string.Join(
            "",
            "http://",
            GetRandomHostname()
        );
    }

    /// <summary>
    /// Returns a random http verb, ie one of
    /// - DELETE
    /// - GET
    /// - HEAD
    /// - OPTIONS
    /// - POST
    /// - PUT
    /// - TRACE
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpMethod()
    {
        return GetRandomFrom(HttpMethods);
    }

    private static readonly string[] HttpMethods =
    [
        "DELETE",
        "GET",
        "HEAD",
        "OPTIONS",
        "POST",
        "PUT",
        "TRACE"
    ];

    /// <summary>
    /// Returns a "more common" http method:
    /// - 60% GET
    /// - 20% POST
    /// - 10% PUT
    /// - 10% DELETE
    /// </summary>
    /// <returns></returns>
    public static string GetRandomCommonHttpMethod()
    {
        var rnd = GetRandomInt(1, 100);
        return rnd switch
        {
            < 60 => "GET",
            < 80 => "POST",
            _ => GetRandomFrom(LessCommonHttpMethods)
        };
    }

    private static readonly string[] LessCommonHttpMethods =
    [
        "PUT",
        "DELETE"
    ];

    /// <summary>
    /// Produces a string which looks just like an http url with a path
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpUrlWithPath()
    {
        return string.Join(
            "/",
            GetRandomHttpUrl(),
            GetRandomPath()
        );
    }

    /// <summary>
    /// Produces a string that looks like an http url with path and parameters
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpUrlWithPathAndParameters()
    {
        return string.Join(
            "/",
            GetRandomHttpUrl(),
            $"{GetRandomHttpPathAndParameters()}"
        );
    }

    /// <summary>
    /// Produces just the path and query string for an http request
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpPathAndParameters()
    {
        return $"{GetRandomPath()}{GetRandomUrlQuery()}";
    }

    /// <summary>
    /// Generates a random HTTP url with some query parameters
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpUrlWithParameters()
    {
        return $"{GetRandomHttpUrl()}{GetRandomUrlQuery()}";
    }

    /// <summary>
    /// Generates a random string which looks a lot like an https url, to
    /// the domain only (no path or parameters)
    /// </summary>
    /// <returns>Random Https-url-like string</returns>
    public static string GetRandomHttpsUrl()
    {
        return string.Join(
            "",
            "https://",
            GetRandomHostname()
        );
    }

    /// <summary>
    /// Produces a string which looks just like an https url with a path
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpsUrlWithPath()
    {
        return string.Join(
            "/",
            GetRandomHttpsUrl(),
            GetRandomPath()
        );
    }

    /// <summary>
    /// Produces a string that looks like an https url with path and parameters
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpsUrlWithPathAndParameters()
    {
        return string.Join(
            "/",
            GetRandomHttpsUrl(),
            $"{GetRandomPath()}{GetRandomUrlQuery()}"
        );
    }

    /// <summary>
    /// Generates a random https url with some query parameters
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHttpsUrlWithParameters()
    {
        return $"{GetRandomHttpsUrl()}{GetRandomUrlQuery()}";
    }

    /// <summary>
    /// Generates a random string which looks like a relative
    /// path on the internet or any reasonable filesystem
    /// </summary>
    /// <returns></returns>
    public static string GetRandomPath()
    {
        var parts = GetRandomArray<string>(1, 3);
        return parts.JoinWith("/");
    }

    /// <summary>
    /// Returns an absolute random path (ie, always starts with /)
    /// </summary>
    /// <returns></returns>
    public static string GetRandomAbsolutePath()
    {
        return $"/{GetRandomPath()}";
    }

    /// <summary>
    /// Generates a random query string for an url
    /// </summary>
    /// <returns></returns>
    public static string GetRandomUrlQuery()
    {
        return $"?{GetRandomUrlParameters()}";
    }

    /// <summary>
    /// Generates a string of random url parameters
    /// </summary>
    /// <returns></returns>
    public static string GetRandomUrlParameters()
    {
        return Range(1, GetRandomInt(2, 5)).Select(_ => $"{GetRandomString(1)}={GetRandomString(1)}"
        ).JoinWith("&");
    }

    /// <summary>
    /// Gets a random string made up only of alphanumeric characters
    /// </summary>
    /// <param name="minLength">Minimum length required</param>
    /// <param name="maxLength">Maximum length required</param>
    /// <returns>Random string made up of only alphanumeric characters</returns>
    public static string GetRandomAlphaNumericString(
        int minLength = DefaultRanges.MINLENGTH_STRING,
        int? maxLength = null
    )
    {
        return GetRandomString(
            minLength,
            maxLength,
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"
        );
    }

    /// <summary>
    /// Gets a random string made of non-alpha-numeric (but printable) chars
    /// </summary>
    /// <param name="minChars"></param>
    /// <param name="maxChars"></param>
    /// <returns></returns>
    public static string GetRandomNonAlphaNumericString(
        int minChars = 0,
        int maxChars = 10
    )
    {
        return Range(0, GetRandomInt(minChars, maxChars))
            .Select(_ =>
                GetRandom(
                    c => c < 'A' || c > 'z',
                    () => (char)GetRandomInt(32, 255)
                )
            )
            .JoinWith("");
    }

    /// <summary>
    /// Gets a random string made up only of alphabetic characters
    /// </summary>
    /// <param name="minLength">Minimum length required</param>
    /// <param name="maxLength">Maximum length required</param>
    /// <returns>Random string made up of only alphabetic characters</returns>
    public static string GetRandomAlphaString(
        int minLength = DefaultRanges.MINLENGTH_STRING,
        int? maxLength = null
    )
    {
        return GetRandomString(minLength, maxLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
    }

    /// <summary>
    /// Gets a random string made up only of numeric characters
    /// </summary>
    /// <param name="minLength">Minimum length required</param>
    /// <param name="maxLength">Maximum length required</param>
    /// <returns>Random string made up of only numeric characters</returns>
    public static string GetRandomNumericString(
        int minLength = DefaultRanges.MINLENGTH_STRING,
        int? maxLength = null
    )
    {
        return GetRandomString(minLength, maxLength, "1234567890");
    }

    /// <summary>
    /// Gets a random enum value from the specified enum type.
    /// </summary>
    /// <typeparam name="T">Type of enum to use as a source</typeparam>
    /// <returns>Random enum value from the enum type</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when GetRandomEnum is called on a
    /// non-enum type (since there is no generic constraint for enum types, yet)
    /// </exception>
    public static T GetRandomEnum<T>()
        where T : struct, IConvertible
    {
        return GetRandomEnum<T>([]);
    }

    /// <summary>
    /// Gets a random enum value from the specified enum type.
    /// You may optionally specify enum values that you don't
    /// want in the output
    /// </summary>
    /// <typeparam name="T">Type of enum to use as a source</typeparam>
    /// <returns>Random enum value from the enum type</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when GetRandomEnum is called on a
    /// non-enum type (since there is no generic constraint for enum types, yet)
    /// </exception>
    public static T GetRandomEnum<T>(
        params T[] excludingValues
    ) where T : struct, IConvertible
    {
        return GetRandomEnum<T>(e => !excludingValues.Contains(e)
        );
    }

    /// <summary>
    /// Gets a random enum value from the specified enum type
    /// with validation for a required value
    /// </summary>
    /// <typeparam name="T">Type of enum to use as a source</typeparam>
    /// <returns>Random enum value from the enum type</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when GetRandomEnum is called on a
    /// non-enum type (since there is no generic constraint for enum types, yet)
    /// </exception>
    public static T GetRandomEnum<T>(
        Func<T, bool> validator
    )
    {
        var type = typeof(T);
        if (!type.IsEnum())
        {
            throw new ArgumentException(
                $"""
                 GetRandomEnum cannot be called on something other than an enum ('{type.Name}')
                 """,
                nameof(T)
            );
        }

        var possible = Enum.GetValues(type)
            .Cast<T>()
            .Where(validator)
            .ToArray();
        if (possible.IsEmpty())
        {
            throw new ArgumentException(
                $"""
                 Unable to select random value of type {typeof(T)}: no values remain after excluding unwanted ones"
                 """
            );
        }

        return GetRandomFrom(possible);
    }

    /// <summary>
    /// Gets a random enum value from the specified enum type
    /// </summary>
    /// <param name="enumType">Type of enum to use as a source</param>
    /// <returns>Random enum value from the enum type</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when GetRandomEnum is called on a
    /// non-enum type (since there is no generic constraint for enum types, yet)
    /// </exception>
    public static object GetRandomEnum(
        Type enumType
    )
    {
        return GetRandomEnum(enumType, _ => true);
    }

    /// <summary>
    /// Gets a random enum value from the specified enum type,
    /// providing a method for excluding based on a function
    /// </summary>
    /// <param name="enumType">Type of enum to use as a source</param>
    /// <param name="discriminator"></param>
    /// <returns>Random enum value from the enum type</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when GetRandomEnum is called on a
    /// non-enum type (since there is no generic constraint for enum types, yet)
    /// </exception>
    public static object GetRandomEnum(
        Type enumType,
        Func<object, bool> discriminator
    )
    {
        ValidateTypeIsEnum(enumType);

        var possible = Enum.GetValues(enumType).Cast<object>()
            .Where(discriminator ?? (_ => true))
            .ToArray();
        return GetRandomFrom(possible);
    }

    private static void ValidateTypeIsEnum(Type enumType)
    {
        if (!enumType.IsEnum())
        {
            throw new ArgumentException(
                $"GetRandomEnum cannot be called on something other than an enum ('{enumType.Name}')",
                nameof(enumType)
            );
        }
    }

    /// <summary>
    /// Gets a random enum value from the specified enum type,
    /// providing a method for excluding based on a function
    /// </summary>
    /// <param name="enumType">Type of enum to use as a source</param>
    /// <param name="excluding"></param>
    /// <returns>Random enum value from the enum type</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when GetRandomEnum is called on a
    /// non-enum type (since there is no generic constraint for enum types, yet)
    /// </exception>
    public static object GetRandomEnum(
        Type enumType,
        params object[] excluding
    )
    {
        if (!enumType.IsEnum())
        {
            throw new ArgumentException(
                $"GetRandomEnum cannot be called on something other than an enum ('{enumType.Name}')",
                nameof(enumType)
            );
        }

        var possible = Enum.GetValues(enumType).Cast<object>()
            .ToArray();
        return GetRandomFrom(possible, excluding);
    }

    /// <summary>
    /// Gets an empty delegate
    /// </summary>
    /// <param name="delegateType">Type of delegate</param>
    /// <returns>Action that do nothing, or function that returns the default of return type</returns>
    internal static object GetEmptyDelegate(Type delegateType)
    {
        if (!typeof(Delegate).IsAssignableFrom(delegateType))
        {
            throw new ArgumentException(
                $"{nameof(GetEmptyDelegate)} cannot be called on something other than a delegate ('{delegateType.Name}')",
                nameof(delegateType)
            );
        }

        if (delegateType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"{nameof(GetEmptyDelegate)} must be called on a concrete delegate type ('{delegateType.Name}')",
                nameof(delegateType)
            );
        }

        var method = delegateType.GetMethod("Invoke");
        if (method is null)
        {
            throw new ArgumentException(
                "Provided delegate has no method 'Invoke'",
                nameof(delegateType)
            );
        }

        return Expression.Lambda(
            delegateType,
            method.ReturnType == typeof(void)
                ? Expression.Empty()
                : Expression.Default(method.ReturnType),
            method.GetParameters()
                .Select(x => Expression.Parameter(x.ParameterType))
                .ToArray()
        ).Compile();
    }

    /// <summary>
    /// Convenience wrapper for GetRandomFrom for params signature
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T OneOf<T>(
        params T[] items
    )
    {
        return GetRandomFrom(items);
    }

    /// <summary>
    /// Gets a random item from the provided collection
    /// </summary>
    /// <param name="items">Collection of items</param>
    /// <typeparam name="T">Item type in collection</typeparam>
    /// <returns>Random value from collection; if the collection is empty, expect an exception</returns>
    public static T GetRandomFrom<T>(
        IEnumerable<T> items
    )
    {
        var itemArray = items.ToArray();
        return GetRandomFrom(itemArray);
    }

    /// <summary>
    /// Gets a random item from the provided collection
    /// </summary>
    /// <param name="items">Collection of items</param>
    /// <typeparam name="T">Item type in collection</typeparam>
    /// <returns>Random value from collection; if the collection is empty, expect an exception</returns>
    public static T GetRandomFrom<T>(
        IList<T> items
    )
    {
        var upper = items.Count - 1;
        return items[GetRandomInt(0, upper)];
    }

    /// <summary>
    /// Gets a random item from a collection, excluding items in the {butNot} collection
    /// </summary>
    /// <param name="items">Collection to search for an item to return</param>
    /// <param name="butNot">Params array of items not to be considered</param>
    /// <typeparam name="T">Item type of the collection</typeparam>
    /// <returns>Random item from the collection, when possible</returns>
    /// <exception cref="ArgumentException">Thrown when the butNot exclusion list leaves no options to select from items</exception>
    public static T GetRandomFrom<T>(
        IEnumerable<T> items,
        params T[] butNot
    )
    {
        var itemsArray = items as T[] ?? items.ToArray();
        var notHash = new HashSet<T>(butNot);
        if (itemsArray.Except(notHash).IsEmpty())
        {
            throw new ArgumentException(
                "Items collection does not contain enough items to apply the exclusion list, assuming the exclusions are actually in the source list",
                nameof(notHash)
            );
        }

        T result;
        do
        {
            result = GetRandomFrom(itemsArray);
        } while (notHash.Contains(result));

        return result;
    }

    /// <summary>
    /// Returns a random subset of items from the
    /// original collection; evaluation is lazy.
    /// </summary>
    /// <param name="items"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetSubSetOf<T>(
        IEnumerable<T> items
    )
    {
        return GetSubSetOf(
            items,
            DefaultRanges.DEFAULT_MIN_SUBSET_ITEMS
        );
    }

    /// <summary>
    /// Returns a random subset of items from the
    /// original collection; evaluation is lazy.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="minValues">minimum required values -
    /// may be overridden if the collection is too small</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetSubSetOf<T>(
        IEnumerable<T> items,
        int minValues
    )
    {
        return GetSubSetOf<T>(
            items,
            minValues,
            DefaultRanges.DEFAULT_MAX_SUBSET_ITEMS
        );
    }

    /// <summary>
    /// Returns a random subset of items from the
    /// original collection; evaluation is lazy.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="minValues">
    /// minimum number of required values
    /// - may be overridden if the collection is too small
    /// </param>
    /// <param name="maxValues">
    /// maximum number of required values
    /// - if this exceeds the collection size, that will be used instead
    /// - if this value is negative, it's counted as an offset from
    ///   the end of the source collection, eg a collection with 12
    ///   items, given maxValues == -2, produces a subset with max 10 items.
    /// </param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetSubSetOf<T>(
        IEnumerable<T> items,
        int minValues,
        int maxValues
    )
    {
        var total = items?.Count() ?? 0;
        if (total < 1)
        {
            yield break;
        }

        if (maxValues > total)
        {
            maxValues = total;
        }
        else if (maxValues < 0)
        {
            maxValues = total - maxValues;
            if (maxValues < minValues)
            {
                minValues = maxValues;
            }
        }

        if (minValues > maxValues)
        {
            var swap = maxValues;
            maxValues = minValues;
            minValues = swap;
        }

        if (minValues > total)
        {
            minValues = maxValues;
        }

        var howMany = GetRandomInt(minValues, maxValues);
        var skipped = new List<T>();
        var provided = 0;
        foreach (var item in items)
        {
            if (GetRandomBoolean())
            {
                yield return item;
                if (++provided >= howMany)
                {
                    yield break;
                }
            }
            else
            {
                skipped.Add(item);
            }
        }

        if (provided >= howMany)
        {
            yield break;
        }

        skipped.Randomize();
        foreach (var item in skipped)
        {
            yield return item;
            if (++provided >= howMany)
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// Convenience: select any of some items from a params array
    /// </summary>
    /// <param name="item"></param>
    /// <param name="more"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T AnyOneOf<T>(
        T item,
        params T[] more
    )
    {
        return GetRandomFrom(more.And(item));
    }

    /// <summary>
    /// Gets a random sub-selection of items from a larger collection
    /// </summary>
    /// <param name="items">Collection to search for items to return</param>
    /// <param name="minValues">Minimum number of items required</param>
    /// <param name="maxValues">Maximum number of items required</param>
    /// <typeparam name="T">Item type of the collection</typeparam>
    /// <returns>A new collection with a random selection of items from the first</returns>
    public static IEnumerable<T> GetRandomSelectionFrom<T>(
        IEnumerable<T> items,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        var itemArray = items as T[] ?? items.ToArray();
        if (itemArray.Length == 0)
        {
            return new T[]
            {
            };
        }

        if (minValues >= itemArray.Length)
        {
            return itemArray.Randomize();
        }

        if (maxValues > itemArray.Length)
        {
            maxValues = itemArray.Length;
        }

        var howMany = GetRandomInt(minValues, maxValues);
        var result = new List<T>();
        while (result.Count < howMany)
        {
            var toAdd = GetRandomFrom(itemArray);
            if (!result.Contains(toAdd))
            {
                result.Add(toAdd);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets a random time on a provided DateTime date
    /// </summary>
    /// <param name="theDate">Date to select a random tim eon</param>
    /// <returns>A new DateTime value which has the same calendar values as the input, but has a randomized time</returns>
    public static DateTime GetRandomTimeOn(
        DateTime theDate
    )
    {
        var min = new DateTime(
            theDate.Year,
            theDate.Month,
            theDate.Day,
            0,
            0,
            0
        );
        var max = new DateTime(
            theDate.Year,
            theDate.Month,
            theDate.Day,
            0,
            0,
            0
        );
        max = max.AddDays(1).AddMilliseconds(-1);
        return GetRandomDate(min, max);
    }

    /// <summary>
    /// Generates a random collection, given a generator function and an acceptable size range
    /// </summary>
    /// <param name="generator">Function to generate individual items for the result collection</param>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new collection of items generated by the generator function</returns>
    public static IEnumerable<T> GetRandomCollection<T>(
        Func<T> generator,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomCollectionGenerator(
            generator,
            minValues,
            maxValues
        ).ToArray();
    }

    /// <summary>
    /// Generates a random collection, given a generator function and an acceptable size range
    /// </summary>
    /// <param name="generator">Function to generate individual items for the result collection</param>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new collection of items generated by the generator function</returns>
    public static IEnumerable<T> GetRandomDistinctCollection<T>(
        Func<T> generator,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomDistinctCollectionGenerator(
            generator,
            minValues,
            maxValues
        ).ToArray();
    }

    /// <summary>
    /// Generates a random collection _generator_, given a generator function and an acceptable size range
    /// Note that since this is a _generator_, each iteration of the collection will yield
    /// different results!
    /// </summary>
    /// <param name="generator">Function to generate individual items for the result collection</param>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new collection of items generated by the generator function</returns>
    public static IEnumerable<T> GetRandomCollectionGenerator<T>(
        Func<T> generator,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        if (minValues > maxValues)
        {
            if (maxValues == DefaultRanges.MAX_ITEMS)
            {
                maxValues = minValues + DefaultRanges.MAX_ITEMS;
            }
            else
            {
                // ReSharper disable once SwapViaDeconstruction
                var swap = minValues;
                minValues = maxValues;
                maxValues = swap;
            }
        }

        generator ??= GetRandom<T>;

        var howMany = GetRandomInt(minValues, maxValues);
        for (var i = 0; i < howMany; i++)
        {
            yield return generator();
        }
    }

    /// <summary>
    /// Generates a random collection _generator_, given a generator function and an acceptable size range
    /// Note that since this is a _generator_, each iteration of the collection will yield
    /// different results!
    /// </summary>
    /// <param name="generator">Function to generate individual items for the result collection</param>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>
    /// A generator function for a collection of item type T,
    /// where items should be distinct
    /// </returns>
    public static IEnumerable<T> GetRandomDistinctCollectionGenerator<T>(
        Func<T> generator,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        if (minValues > maxValues)
        {
            if (maxValues == DefaultRanges.MAX_ITEMS)
            {
                maxValues = minValues + DefaultRanges.MAX_ITEMS;
            }
            else
            {
                // ReSharper disable once SwapViaDeconstruction
                var swap = minValues;
                minValues = maxValues;
                maxValues = swap;
            }
        }

        generator ??= GetRandom<T>;

        var howMany = GetRandomInt(minValues, maxValues);

        var hashes = new HashSet<string>();
        for (var i = 0; i < howMany; i++)
        {
            var thisRound = generator();
            var attempts = 0;
            while (
                !hashes.Add(GenerateHashFor(thisRound))
            )
            {
                if (++attempts >= MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS)
                {
                    throw new ArgumentException(
                        $"""
                         Unable to produce {howMany} distinct random values with the provided generator ({generator})
                         """
                    );
                }

                thisRound = generator();
            }

            yield return thisRound;
        }
    }

    private static string GenerateHashFor<T>(T item)
    {
        if (item is null)
        {
            return "";
        }

        var type = typeof(T);
        if (type.IsPrimitiveOrImmutable())
        {
            return $"{item.GetHashCode()}";
        }

        var str = item.Stringify();
        var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(str.AsBytes());

        var characters = hash.Select(t => t.ToString("X2")).ToArray();
        return string.Join(string.Empty, characters);
    }

    /// <summary>
    /// Generates a random array, given a generator function and an acceptable size range
    /// </summary>
    /// <param name="generator">Function to generate individual items for the result collection</param>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new array of items generated by the generator function</returns>
    public static T[] GetRandomArray<T>(
        Func<T> generator,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomCollection(generator, minValues, maxValues).ToArray();
    }

    /// <summary>
    /// Generates a random list, within the acceptable size range, using GetRandom&lt;T&gt;
    /// </summary>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new array of items generated by the generator function</returns>
    public static List<T> GetRandomList<T>(
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomList(GetRandom<T>, minValues, maxValues);
    }

    /// <summary>
    /// Generates a random list, given a generator function and an acceptable size range
    /// </summary>
    /// <param name="generator">Function to generate individual items for the result collection</param>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new array of items generated by the generator function</returns>
    public static List<T> GetRandomList<T>(
        Func<T> generator,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomCollection(generator, minValues, maxValues)
            .ToList();
    }

    /// <summary>
    /// Generates a random collection, given an acceptable size range
    /// </summary>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new collection of items generated by the GetRandom generic function</returns>
    public static IEnumerable<T> GetRandomCollection<T>(
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomCollection(
            GetRandom<T>,
            minValues,
            maxValues
        );
    }

    /// <summary>
    /// Generates a random collection, given an acceptable size range
    /// </summary>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>
    /// A new collection of items generated by the GetRandom generic function,
    /// ensuring that the items are unique.
    /// </returns>
    public static IEnumerable<T> GetRandomDistinctCollection<T>(
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomDistinctCollection(
            GetRandom<T>,
            minValues,
            maxValues
        );
    }

    /// <summary>
    /// Generates a random array, given an acceptable size range
    /// </summary>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>A new array of items generated by the GetRandom generic function</returns>
    public static T[] GetRandomArray<T>(
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomCollection<T>(
            minValues,
            maxValues
        ).ToArray();
    }

    /// <summary>
    /// Generates a random array, given an acceptable size range
    /// </summary>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>
    /// A new array of items generated by the GetRandom generic function,
    /// ensuring that results are unique
    /// </returns>
    public static T[] GetRandomDistinctArray<T>(
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomDistinctArray<T>(
            GetRandom<T>,
            minValues,
            maxValues
        ).ToArray();
    }

    /// <summary>
    /// Generates a random array, given an acceptable size range
    /// </summary>
    /// <param name="generator"></param>
    /// <param name="minValues">Minimum number of items to return</param>
    /// <param name="maxValues">Maximum number of items to return</param>
    /// <typeparam name="T">Underlying type of the collection</typeparam>
    /// <returns>
    /// A new array of items generated by the provided factory function,
    /// ensuring that results are unique
    /// </returns>
    public static T[] GetRandomDistinctArray<T>(
        Func<T> generator,
        int minValues = DefaultRanges.MIN_ITEMS,
        int maxValues = DefaultRanges.MAX_ITEMS
    )
    {
        return GetRandomDistinctCollection(
            generator,
            minValues,
            maxValues
        ).ToArray();
    }

    /// <summary>
    /// Maximum number of attempts to make when trying to generate a value different from
    /// one specified as undesirable
    /// </summary>
    public const int MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS = 1000;

    /// <summary>
    /// Attempts to get another random value, different from the provided one,
    /// using the provided generator
    /// </summary>
    /// <param name="differentFromThisValue">Undesirable value</param>
    /// <param name="usingThisGenerator">Generator function to use</param>
    /// <param name="shouldRegenerateIf">
    /// Optional function to determine whether or not
    /// a potential candidate result is acceptable
    /// </param>
    /// <typeparam name="T">Type of item t generate</typeparam>
    /// <returns>New item, different from the provided undesirable value, as long as it can be found within MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS attempts</returns>
    public static T GetAnother<T>(
        T differentFromThisValue,
        Func<T> usingThisGenerator,
        Func<T, T, bool> shouldRegenerateIf = null
    )
    {
        shouldRegenerateIf = shouldRegenerateIf ?? DefaultEqualityTest;
        return GetANewRandomValueUsing(differentFromThisValue, usingThisGenerator, IsANewValue);

        bool IsANewValue(T o) => !shouldRegenerateIf(differentFromThisValue, o);
    }

    /// <summary>
    /// Gets value of Type T, using a custom validator function to know when to stop trying
    /// and an optional generator function. Use like:
    /// var first = GetRandom&lt;IHasAName&gt;();
    /// var other = GetAnother&lt;IHasAName&gt;(o =&lt; o.Name != first.Name);
    /// </summary>
    /// <param name="validator">Validates that a generated value is acceptable (should return true when it is)</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandom<T>(
        Func<T, bool> validator
    )
    {
        return GetRandom(validator, null);
    }

    /// <summary>
    /// Gets a value of type T and applies the provided mutations to it before giving it back
    /// </summary>
    /// <param name="mutator"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandom<T>(
        Action<T> mutator
    )
    {
        var result = GetRandom<T>();
        mutator(result);
        return result;
    }

    /// <summary>
    /// Gets value of Type T, using a custom validator function to know when to stop trying
    /// and an optional generator function. Use like:
    /// var first = GetRandom&lt;IHasAName&gt;();
    /// var other = GetAnother&lt;IHasAName&gt;(o =&lt; o.Name != first.Name);
    /// </summary>
    /// <param name="validator">Validates that a generated value is acceptable (should return true when it is)</param>
    /// <param name="usingThisGenerator">Optional custom generator for the next random value, defaults to GetRandom&lt;T&gt;</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetRandom<T>(
        Func<T, bool> validator,
        Func<T> usingThisGenerator
    )
    {
        return GetANewRandomValueUsing(default(T), usingThisGenerator ?? GetRandom<T>, validator);
    }

    /// <summary>
    /// Gets another value of the same type as the specified value, but not equal to it,
    /// using the default GetRandom generic method
    /// </summary>
    /// <param name="differentFromThis">Value to avoid</param>
    /// <typeparam name="T">Type of value required</typeparam>
    /// <returns>New random value of type T, different from the input value</returns>
    public static T GetAnother<T>(
        T differentFromThis
    )
    {
        return GetAnother(differentFromThis, GetRandom<T>);
    }

    /// <summary>
    /// Gets another value of the same type as the specified values, but not equal to any of them,
    /// using the default GetRandom generic method
    /// </summary>
    /// <param name="notAnyOfThese">Values to avoid</param>
    /// <typeparam name="T">Type of value required</typeparam>
    /// <returns>New random value of type T, different from the input values</returns>
    public static T GetAnother<T>(
        IEnumerable<T> notAnyOfThese
    )
    {
        return GetAnother(notAnyOfThese, GetRandom<T>);
    }

    /// <summary>
    /// Gets another random value not found in the given collection, using the provided generator
    /// </summary>
    /// <param name="notAnyOfThese">New random value should not be any of these</param>
    /// <param name="usingThisGenerator">Function to generate each candidate result</param>
    /// <param name="areEqual">Optional function to determine if each candidate result
    /// is valid, Defaults to using .Equals, but also catering for null values.</param>
    /// <typeparam name="T">Type of value to generate</typeparam>
    /// <returns>New value of type T, not found in the input collection</returns>
    public static T GetAnother<T>(
        IEnumerable<T> notAnyOfThese,
        Func<T> usingThisGenerator,
        Func<T, T, bool> areEqual = null
    )
    {
        areEqual = areEqual ?? DefaultEqualityTest;
        var notTheseArray = notAnyOfThese.ToArray();
        bool IsANewValue(T o) => notTheseArray.All(i => !areEqual(o, i));
        return GetANewRandomValueUsing(notTheseArray, usingThisGenerator, IsANewValue);
    }

    private static bool DefaultEqualityTest<T>(
        T left,
        T right
    )
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        return left.Equals(right) && right.Equals(left);
    }

    private static T1 GetANewRandomValueUsing<T1, T2>(
        T2 differentFromThisValue,
        Func<T1> usingThisGenerator,
        Func<T1, bool> isANewValue
    )
    {
        var attempts = 0;
        do
        {
            var result = usingThisGenerator();
            if (isANewValue(result))
            {
                return result;
            }

            if (++attempts >= MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS)
            {
                throw new CannotGetAnotherDifferentRandomValueException<T2>(differentFromThisValue);
            }
        } while (true);
    }

    /// <summary>
    /// Gets a random ipv4 ip address
    /// </summary>
    /// <returns>String representing an ipv4 address</returns>
    public static string GetRandomIPv4Address()
    {
        var octets = new[]
        {
            GetRandomInt(1, 255),
            GetRandomInt(0, 255),
            GetRandomInt(0, 255),
            GetRandomInt(1, 255)
        };

        return string.Join(".", octets);
    }

    /// <summary>
    /// Generates a random ipv5 address
    /// </summary>
    /// <returns></returns>
    public static string GetRandomIPv6Address()
    {
        return GetRandomIPv6Address(false);
    }

    /// <summary>
    /// Generates a random ipv6 address
    /// - optionally ensures that the output would be compressed, ie,
    /// follows the rule:
    /// A contiguous string of zeros can be replaced with a double colon (::)
    /// but this can only be done once in an address
    /// </summary>
    /// <param name="ensureCompressed"></param>
    /// <returns></returns>
    public static string GetRandomIPv6Address(
        bool ensureCompressed
    )
    {
        var parts = new[]
        {
            GetRandomIPv6Hextet(),
            GetRandomIPv6Hextet(),
            GetRandomIPv6Hextet(),
            GetRandomIPv6Hextet(),
            GetRandomIPv6Hextet(),
            GetRandomIPv6Hextet(),
            GetRandomIPv6Hextet(),
            GetRandomIPv6Hextet()
        };
        if (ensureCompressed)
        {
            if (parts.All(s => s != IPV6_STRING_CHARS))
            {
                var idx = GetRandomInt(0, parts.Length - 1);
                parts[idx] = IPV6_ZEROS;
            }
        }

        return parts.JoinWith(":").CompressIPv6Address();
    }

    private const string IPV6_ZEROS = "0000";

    /// <summary>
    /// Generates a single group from an ipv6 address
    /// </summary>
    /// <returns></returns>
    public static string GetRandomIPv6Hextet()
    {
        return GetRandomIPv6Hextet(compress: false);
    }

    /// <summary>
    /// Generates a single group from an ipv6 address
    /// and optionally compresses it
    /// </summary>
    /// <param name="compress"></param>
    /// <returns></returns>
    public static string GetRandomIPv6Hextet(
        bool compress
    )
    {
        var result = GetRandomHexString(4, 4);
        if (!compress)
        {
            return result;
        }

        result = result.TrimStart('0');
        return result == ""
            ? "0"
            : result;
    }

    /// <summary>
    /// Generates a random hex string
    /// </summary>
    /// <returns></returns>
    public static string GetRandomHexString()
    {
        return GetRandomHexString(
            DefaultRanges.MINLENGTH_STRING
        );
    }

    /// <summary>
    /// Generates a random hex string with the
    /// provided minimum length
    /// </summary>
    /// <param name="minLength"></param>
    /// <returns></returns>
    public static string GetRandomHexString(
        int minLength
    )
    {
        return GetRandomHexString(
            minLength,
            minLength + DefaultRanges.MINLENGTH_STRING
        );
    }

    /// <summary>
    /// Produces a random string with 
    /// </summary>
    /// <param name="minLength"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetRandomHexString(
        int minLength,
        int maxLength
    )
    {
        if (minLength < 0)
        {
            throw new ArgumentException(
                "minLength must be >= 0",
                nameof(minLength)
            );
        }

        var howMany = GetRandomInt(minLength, maxLength);
        if (howMany % 2 != 0)
        {
            howMany = howMany == maxLength
                ? maxLength - 1
                : howMany + 1;
        }

        return GetRandomString(
            howMany,
            howMany,
            IPV6_STRING_CHARS
        );
    }

    /// <summary>
    /// Gets a random hostname-like string
    /// </summary>
    /// <returns>New string with at least two parts, separated by .</returns>
    public static string GetRandomHostname()
    {
        return GetRandomHostname(2);
    }

    /// <summary>
    /// Gets a random hostname-like string
    /// </summary>
    /// <param name="minParts">minimum required parts</param>
    /// <returns>new string with at least the required number of parts, max 5 parts</returns>
    public static string GetRandomHostname(int minParts)
    {
        return GetRandomHostname(minParts, 5);
    }

    /// <summary>
    /// Gets a random hostname-like string
    /// </summary>
    /// <param name="minParts">minimum required parts</param>
    /// <param name="maxParts">maximum required parts</param>
    /// <returns>new string within the required parts range</returns>
    public static string GetRandomHostname(int minParts, int maxParts)
    {
        return Run.Until(
            IsValidHostname,
            () => string.Join(
                ".",
                GetRandomCollection(
                    () => GetRandomAlphaNumericString(1),
                    minParts,
                    maxParts
                )
            )
        );
    }

    private static bool IsValidHostname(string arg)
    {
        try
        {
            var uri = new Uri($"https://{arg}");
            return uri.Host.Equals(arg, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets a random version-like string (eg 1.2.3)
    /// </summary>
    /// <param name="partCount">How many parts to have in your version string</param>
    /// <returns>Version-like string</returns>
    public static string GetRandomVersionString(
        int partCount = 3
    )
    {
        return string.Join(".", GetRandomCollection<int>(partCount, partCount));
    }

    /// <summary>
    /// Gets a random System.Version instance
    /// </summary>
    /// <returns>New System.Version with random values</returns>
    public static Version GetRandomVersion()
    {
        return new Version(
            GetRandomInt(),
            GetRandomInt(),
            GetRandomInt(),
            GetRandomInt()
        );
    }

    /// <summary>
    /// Creates a randomly-named folder within another existing folder and returns
    /// the name of that folder
    /// </summary>
    /// <param name="path">Base path within which to create the new folder</param>
    /// <returns>Just the name of the created folder (not the full path)</returns>
    public static string CreateRandomFolderIn(
        string path
    )
    {
        string folderName;
        do
        {
            folderName = GetRandomString();
        } while (Directory.Exists(Path.Combine(path, folderName)));

        Directory.CreateDirectory(Path.Combine(path, folderName));
        return folderName;
    }

    /// <summary>
    /// Creates some randomly-named folder within another existing folder and returns
    /// the names of those folders
    /// </summary>
    /// <param name="path">Base path within which to create the new folder</param>
    /// <param name="depth">How deep to go when creating the tree</param>
    /// <returns>Just the names of the created folders (not the full paths)</returns>
    public static IEnumerable<string> CreateRandomFoldersIn(
        string path,
        int depth = 2
    )
    {
        var toCreate = GetRandomCollection<string>(1).ToList();
        toCreate.ToArray().ForEach(f =>
            {
                Directory.CreateDirectory(Path.Combine(path, f));
                if (depth > 1)
                {
                    toCreate.AddRange(
                        CreateRandomFoldersIn(Path.Combine(path, f), depth - 1)
                            .Select(sub => Path.Combine(f, sub))
                    );
                }
            }
        );
        return toCreate;
    }

    /// <summary>
    /// Creates a randomly-named file within the provided folder path and returns its name
    /// </summary>
    /// <param name="path">Folder within which to create the file</param>
    /// <returns>Name of the file (name only, not full path)</returns>
    public static string CreateRandomFileIn(
        string path
    )
    {
        var fileName = GetRandomString();
        File.WriteAllBytes(Path.Combine(path, fileName), GetRandomBytes());
        return fileName;
    }

    /// <summary>
    /// Creates a randomly-named file within the provided folder path and
    /// populates it with some random text data; returns the name of the file
    /// </summary>
    /// <param name="path">Folder within which to create the file</param>
    /// <returns>Name of the file (name only, not full path)</returns>
    public static string CreateRandomTextFileIn(
        string path
    )
    {
        var fileName = GetRandomString();
        var lines = GetRandomCollection<string>(1);
        File.WriteAllLines(Path.Combine(path, fileName), lines);
        return fileName;
    }

    /// <summary>
    /// Creates a full random file tree (folders and some files, to a depth of 2) under a given path. Useful
    /// when you need to test utilities which trawl the filesystem.
    /// </summary>
    /// <param name="path">Folder in which to create the tree</param>
    /// <returns>A collection of relative paths to the files within the created tree</returns>
    public static IEnumerable<string> CreateRandomFileTreeIn(
        string path
    )
    {
        return CreateRandomFileTreeIn(path, 2);
    }

    /// <summary>
    /// Creates a full random file tree (folders and some files) under a given path. Useful
    /// when you need to test utilities which trawl the filesystem.
    /// </summary>
    /// <param name="path">Folder in which to create the tree</param>
    /// <param name="depth">How deep to make the folder structure</param>
    /// <returns>A collection of relative paths to the files within the created tree</returns>
    public static IEnumerable<string> CreateRandomFileTreeIn(
        string path,
        int depth
    )
    {
        var folders = CreateRandomFoldersIn(path, depth).ToArray();
        var result = new List<string>(folders);
        folders.ForEach(f =>
            {
                var numberOfFiles = GetRandomInt(1);
                numberOfFiles.TimesDo(() =>
                    {
                        var createdFile = CreateRandomFileIn(Path.Combine(path, f));
                        result.Add(Path.Combine(f, createdFile));
                    }
                );
            }
        );
        return result;
    }

    /// <summary>
    /// Provides a random language code
    /// </summary>
    /// <returns></returns>
    public static string GetRandomLanguageCode()
    {
        return GetRandomFrom(NaturalData.LanguageCodes);
    }

    /// <summary>
    /// Provides a random language name
    /// </summary>
    /// <returns></returns>
    public static string GetRandomLanguageName()
    {
        return GetRandomFrom(NaturalData.LanguageNames);
    }
}