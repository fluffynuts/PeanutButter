using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PeanutButter.Utils;
using static PeanutButter.Utils.PyLike;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Context to use when getting a random timespan
    /// </summary>
    public enum TimeSpanContexts
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
    public class RandomValueGen
    {
        private static readonly Dictionary<Type, Func<object>> PrimitiveGenerators =
            new Dictionary<Type, Func<object>>()
            {
                {typeof(int), () => GetRandomInt()},
                {typeof(byte), () => Convert.ToByte(GetRandomInt(0, 255))},
                {typeof(char), () => Convert.ToChar(GetRandomInt(0, 255))},
                {typeof(long), () => GetRandomLong()},
                {typeof(float), () => Convert.ToSingle(GetRandomDecimal(decimal.MinValue, decimal.MaxValue))},
                {typeof(double), () => Convert.ToDouble(GetRandomDecimal(decimal.MinValue, decimal.MaxValue))},
                {typeof(decimal), () => GetRandomDecimal(decimal.MinValue, decimal.MaxValue)},
                {typeof(DateTime), () => GetRandomDate()},
                {typeof(string), () => GetRandomString()},
                {typeof(bool), () => GetRandomBoolean()}
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
            var type = typeof(T);
            if (type.IsEnum())
                return (T) GetRandomEnum(type);
            return (T) GetRandomValue(typeof(T));
        }

        /// <summary>
        /// Gets a random value of the specified type by attempting to find the correct
        /// random generator method and invoking it. Works on primitives (eg int, string),
        /// enums and complex objects. When invoked on a complex object, will attempt to fine
        /// (or, if necessary, create) a GenericBuilder to produce the result
        /// </summary>
        /// <param name="type">Type to generate a random value of</param>
        /// <returns>New instance of the specified type. Should be different every time, when possible.</returns>
        public static object GetRandomValue(
            Type type)
        {
            if (type == null)
                throw new ArgumentException(nameof(type));
            if (type.IsGenericType() && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            return PrimitiveGenerators.TryGetValue(
                type ?? throw new ArgumentNullException(nameof(type)),
                out var randomGenerator
            )
                ? randomGenerator()
                : GetRandomValueForType(type);
        }

        private static object GetRandomValueForType(
            Type type)
        {
            var builder = GetBuilderFor(type);
            if (builder == null)
                throw new Exception(
                    "Can't get random value for type: '" + type.Name +
                    "': either too complex or I missed a simple type?");
            return builder.GenericWithRandomProps().GenericBuild();
        }

        private static IGenericBuilder GetBuilderFor(
            Type type)
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
            /// Default minimum integer value returned
            /// </summary>
            public const int MIN_INT_VALUE = 0;

            /// <summary>
            /// Default maximum integer value returned
            /// </summary>
            public const int MAX_INT_VALUE = 10;

            /// <summary>
            /// Default minimum long value returned
            /// </summary>
            public const int MIN_LONG_VALUE = 0;

            /// <summary>
            /// Default maximum long value returned
            /// </summary>
            public const int MAX_LONG_VALUE = 1000;

            /// <summary>
            /// Default minimum number of items in a random collection
            /// </summary>
            public const int MIN_ITEMS = 0;

            /// <summary>
            /// Default Maximum number of items in a random collection
            /// </summary>
            public const int MAX_ITEMS = 10;
        }

        private static readonly Random RandomGenerator = new Random();
        private const string DEFAULT_RANDOM_STRING_CHARS = "abcdefghijklmnopqrstuvwxyz1234567890";

        /// <summary>
        /// Returns a random integer within the specified range
        /// </summary>
        /// <param name="minValue">Minimum value to consider</param>
        /// <param name="maxValue">Maximum value to consider</param>
        /// <returns>Random integer between minValue and maxValue (inclusive)</returns>
        public static int GetRandomInt(
            int minValue = DefaultRanges.MIN_INT_VALUE,
            int maxValue = DefaultRanges.MAX_INT_VALUE)
        {
            return (int) GetRandomLong(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random boolean value
        /// </summary>
        /// <returns>True or False</returns>
        public static bool GetRandomBoolean()
        {
            return GetRandomInt(1, 100) < 50;
        }

        private static readonly string[] MimeTypes =
        {
            "text/plain",
            "text/html",
            "image/png",
            "application/pdf",
            "image/jpeg"
        };

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Gets a pseudo-random mimetype (picks from a short list of known mime types)
        /// </summary>
        /// <returns>String which is a valid mime type</returns>
        public static string GetRandomMIMEType()
        {
            var idx = GetRandomInt(0, MimeTypes.Length - 1);
            return MimeTypes[idx];
        }

        /// <summary>
        /// Returns a random long within the specified range
        /// </summary>
        /// <param name="minValue">Minimum value to consider</param>
        /// <param name="maxValue">Maximum value to consider</param>
        /// <returns>Random integer between minValue and maxValue (inclusive)</returns>
        public static long GetRandomLong(
            long minValue = DefaultRanges.MIN_LONG_VALUE,
            long maxValue = DefaultRanges.MAX_LONG_VALUE)
        {
            if (minValue > maxValue)
            {
                var swap = minValue;
                minValue = maxValue;
                maxValue = swap;
            }

            var dec = RandomGenerator.NextDouble();
            var range = maxValue - minValue + 1;
            return minValue + (long) (range * dec);
        }

        /// <summary>
        /// Gets a random string
        /// </summary>
        /// <param name="minLength">Minimum length required</param>
        /// <param name="maxLength">Maximum length required. When left null, will be the minium length plus DefaultRanges.MINLENGTH_STRING</param>
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
            if (charSet == null) charSet = DEFAULT_RANDOM_STRING_CHARS;
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
            DateTime? minDate = null,
            DateTime? maxDate = null,
            bool dateOnly = false,
            DateTime? minTime = null,
            DateTime? maxTime = null)
        {
            return GetRandomDate(DateTimeKind.Local, minDate, maxDate, dateOnly, minTime, maxTime);
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
            DateTime? minDate = null,
            DateTime? maxDate = null,
            bool dateOnly = false,
            DateTime? minTime = null,
            DateTime? maxTime = null
        )
        {
            var dateRangeLower = new DateTime(1990, 1, 1);
            const int dateRangeYears = 30;

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
                kind);
            return dateOnly
                ? sanitised.StartOfDay()
                : RangeCheckTimeOnRandomDate(minTime, maxTime, sanitised);
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
            return GetRandomDateRange(DateTimeKind.Local, minDate, maxDate, dateOnly, minTime, maxTime);
        }

        /// <summary>
        /// Gets a random timespan
        /// </summary>
        /// <param name="min">Minimum length</param>
        /// <param name="max">Maximum length</param>
        /// <param name="context">Context for the min/max length</param>
        /// <returns></returns>
        public static TimeSpan GetRandomTimeSpan(
            double min = DefaultRanges.MIN_INT_VALUE,
            double max = DefaultRanges.MAX_INT_VALUE,
            TimeSpanContexts context = TimeSpanContexts.Minutes)
        {
            var howMany = GetRandomDouble(min, max);
            return _timespanGenerators[context](howMany);
        }

        private static Dictionary<TimeSpanContexts, Func<double, TimeSpan>> _timespanGenerators
            = new Dictionary<TimeSpanContexts, Func<double, TimeSpan>>()
            {
                [TimeSpanContexts.Ticks] = i => TimeSpan.FromTicks((long) i),
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
            var fromDate = GetRandomDate(kind, minDate, maxDate, dateOnly, minTime, maxTime);
            var toDate = GetRandomDate(kind, minDate, maxDate, dateOnly, minTime, maxTime);
            return new DateRange(fromDate, toDate);
        }

        internal static DateTime RangeCheckTimeOnRandomDate(
            DateTime? minTime,
            DateTime? maxTime,
            DateTime value)
        {
            var baseDate = new DateTime(value.Year, value.Month, value.Day);
            minTime = baseDate.Add(minTime?.TimeOfDay ?? TimeSpan.Zero);
            maxTime = baseDate.Add(maxTime?.TimeOfDay ?? TimeSpan.FromDays(1).Subtract(TimeSpan.FromTicks(1)));

            if (minTime > maxTime)
            {
                var swap = minTime;
                minTime = maxTime;
                maxTime = swap;
            }

            return value > maxTime || value < minTime
                ? GetRandomDate(minTime, maxTime)
                : value;
        }

        /// <summary>
        /// Gets a random double value within the specified range
        /// </summary>
        /// <param name="min">Minimum value to consider</param>
        /// <param name="max">Maximum value to consider</param>
        /// <returns>Double value within the specified range</returns>
        public static double GetRandomDouble(
            double min = DefaultRanges.MIN_INT_VALUE,
            double max = DefaultRanges.MAX_INT_VALUE
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
            decimal min = 0,
            decimal max = DefaultRanges.MAX_INT_VALUE
        )
        {
            return (decimal) GetRandomDouble((double) min, (double) max);
        }

        /// <summary>
        /// Gets a randomly-sized, randomly-filled byte array
        /// </summary>
        /// <param name="minLength">Minimum size of the result</param>
        /// <param name="maxLength">Maximum size of the result</param>
        /// <returns>Randomly-filled byte array</returns>
        public static byte[] GetRandomBytes(
            int minLength = DefaultRanges.MINLENGTH_BYTES,
            int maxLength = DefaultRanges.MAXLENGTH_BYTES
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
            return string.Join(string.Empty, GetRandomString(), "@", GetRandomString(), ".com");
        }

        /// <summary>
        /// Gets a random filename. Does not use the OS-level temporary filename
        /// functions
        /// </summary>
        /// <returns>String which is a random filename with a 3 character extension</returns>
        public static string GetRandomFileName()
        {
            return string.Join(".", GetRandomString(10, 20), GetRandomString(3, 3));
        }

        /// <summary>
        /// Gets a random path which resembles a Windows path, including a leading drive
        /// </summary>
        /// <returns>String which looks like a local Windows path</returns>
        public static string GetRandomWindowsPath()
        {
            var folders = GetRandomCollection<string>(1, 4);
            var drive = GetRandomString(1, 1, "ABCDEGHIJKLMNOPQRSTUVWXYZ") + ":";
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            return string.Join(
                Path.DirectorySeparatorChar.ToString(),
                new[] {drive}.And(folders.ToArray()));
        }

        /// <summary>
        /// Gets some random pseudo-words. Note that they (probably) won't be
        /// readable words -- just a collection of strings with whitespace in between.
        /// Think of this as something like Lorei Ipsum, except with zero meaning.
        /// </summary>
        /// <param name="min">Minimum number of "words" to return</param>
        /// <param name="max">Maximum number of "words" to return</param>
        /// <returns>Block of text with "words" and whitespace</returns>
        public static string GetRandomWords(
            int min = 10,
            int max = 50)
        {
            var actual = GetRandomInt(min, max);
            var words = new List<string>();
            for (var i = 0; i < actual; i++)
            {
                words.Add(GetRandomAlphaNumericString(1, 10));
            }

            return string.Join(" ", words.ToArray());
        }

        /// <summary>
        /// Generates a random string which looks a lot like an http url
        /// </summary>
        /// <returns>Random http-url-like string</returns>
        public static string GetRandomHttpUrl()
        {
            return string.Join(
                "/",
                "http:",
                string.Empty,
                GetRandomAlphaNumericString(3, 12) + $".{GetRandomString(2, 3)}",
                GetRandomAlphaNumericString(0, 20)).ToLowerInvariant();
        }

        /// <summary>
        /// Generates a random HTTP url with some query parameters
        /// </summary>
        /// <returns></returns>
        public static string GetRandomHttpUrlWithParameters()
        {
            var parameters = Range(1, GetRandomInt(2, 5)).Select(
                i => $"{GetRandomString(1)}={GetRandomString(1)}");
            return $"{GetRandomHttpUrl()}?{parameters.JoinWith("&")}";
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
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
        }

        /// <summary>
        /// Gets a random string made of non-alpha-numeric (but printable) chars
        /// </summary>
        /// <param name="minChars"></param>
        /// <param name="maxChars"></param>
        /// <returns></returns>
        public static string GetRandomNonAlphaNumericString(
            int minChars = 0,
            int maxChars = 10)
        {
            return Range(0, GetRandomInt(1, 10))
                .Select(i =>
                    GetRandom(c => c < 'A' || c > 'z',
                        () => (char) GetRandomInt(32, 255)))
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
            int? maxLength = null)
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
            int? maxLength = null)
        {
            return GetRandomString(minLength, maxLength, "1234567890");
        }

        /// <summary>
        /// Gets a random enum value from the specified enum type
        /// </summary>
        /// <typeparam name="T">Type of enum to use as a source</typeparam>
        /// <returns>Random enum value from the enum type</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when GetRandomEnum is called on a
        /// non-enum type (since there is no generic constraint for enum types, yet)
        /// </exception>
        public static T GetRandomEnum<T>() where T : struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum())
                throw new ArgumentException(
                    "GetRandomEnum cannot be called on something other than an enum ('" +
                    type.Name + "')");
            var possible = Enum.GetValues(type).Cast<T>();
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
            Type enumType)
        {
            if (!enumType.IsEnum())
                throw new ArgumentException(
                    "GetRandomEnum cannot be called on something other than an enum ('" +
                    enumType.Name + "')");
            var possible = Enum.GetValues(enumType).Cast<object>();
            return GetRandomFrom(possible);
        }

        /// <summary>
        /// Gets a random item from the provided collection
        /// </summary>
        /// <param name="items">Collection of items</param>
        /// <typeparam name="T">Item type in collection</typeparam>
        /// <returns>Random value from collection; if the collection is empty, expect an exception</returns>
        public static T GetRandomFrom<T>(
            IEnumerable<T> items)
        {
            var itemArray = items as T[] ?? items.ToArray();
            var upper = itemArray.Length - 1;
            return itemArray.Skip(GetRandomInt(0, upper)).First();
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
            params T[] butNot)
        {
            var itemsArray = items as T[] ?? items.ToArray();
            if (itemsArray.Except(butNot).IsEmpty())
                throw new ArgumentException(
                    "Items collection does not contain enough items to apply the exclusion list, assuming the exclusions are actually in the source list");
            T result;
            do
            {
                result = GetRandomFrom(itemsArray);
            } while (butNot.Contains(result));

            return result;
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
            int maxValues = DefaultRanges.MAX_ITEMS)
        {
            var itemArray = items as T[] ?? items.ToArray();
            if (itemArray.Length == 0)
                return new T[] { };
            if (minValues >= itemArray.Length)
                return itemArray.Randomize();
            if (maxValues > itemArray.Length)
                maxValues = itemArray.Length;
            var howMany = GetRandomInt(minValues, maxValues);
            var result = new List<T>();
            while (result.Count < howMany)
            {
                var toAdd = GetRandomFrom(itemArray);
                if (!result.Contains(toAdd))
                    result.Add(toAdd);
            }

            return result;
        }

        /// <summary>
        /// Gets a random time on a provided DateTime date
        /// </summary>
        /// <param name="theDate">Date to select a random tim eon</param>
        /// <returns>A new DateTime value which has the same calendar values as the input, but has a randomized time</returns>
        public static DateTime GetRandomTimeOn(
            DateTime theDate)
        {
            var min = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
            var max = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
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
            // TODO: stop using default values, ie, explode out the methods
            var howMany = GetRandomInt(minValues, maxValues);
            var result = new List<T>();
            for (var i = 0; i < howMany; i++)
            {
                result.Add(generator());
            }

            return result;
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
            // TODO: stop using default values, ie, explode out the methods
            return GetRandomCollection(generator, minValues, maxValues).ToArray();
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
            // TODO: stop using default values, ie, explode out the methods
            return GetRandomCollection(GetRandom<T>, minValues, maxValues);
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
            // TODO: stop using default values, ie, explode out the methods
            return GetRandomCollection<T>(minValues, maxValues).ToArray();
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
            Func<T, bool> isANewValue = o => !shouldRegenerateIf(differentFromThisValue, o);
            return GetANewRandomValueUsing(differentFromThisValue, usingThisGenerator, isANewValue);
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
            Func<T> usingThisGenerator = null
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
            T differentFromThis)
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
            IEnumerable<T> notAnyOfThese)
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
            Func<T, bool> isANewValue = o => notAnyOfThese.All(i => !areEqual(o, i));
            return GetANewRandomValueUsing(notAnyOfThese, usingThisGenerator, isANewValue);
        }

        private static bool DefaultEqualityTest<T>(
            T left,
            T right)
        {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            return left.Equals(right) && right.Equals(left);
        }

        private static T1 GetANewRandomValueUsing<T1, T2>(
            T2 differentFromThisValue,
            Func<T1> usingThisGenerator,
            Func<T1, bool> isANewValue)
        {
            var attempts = 0;
            do
            {
                var result = usingThisGenerator();
                if (isANewValue(result))
                    return result;
                if (++attempts >= MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS)
                    throw new CannotGetAnotherDifferentRandomValueException<T2>(differentFromThisValue);
            } while (true);
        }

        /// <summary>
        /// Gets a random ipv4 ip address
        /// </summary>
        /// <returns>String representing an ipv4 address</returns>
        public static string GetRandomIPv4Address()
        {
            var octets = GetRandomCollection(() => GetRandomInt(0, 255), 4, 4);
            return string.Join(".", octets);
        }

        /// <summary>
        /// Gets a random hostname-like string
        /// </summary>
        /// <returns>New string with at least two parts, separated by .</returns>
        public static string GetRandomHostname()
        {
            return string.Join(".", GetRandomCollection<string>(2));
        }

        /// <summary>
        /// Gets a random version-like string (eg 1.2.3)
        /// </summary>
        /// <param name="partCount">How many parts to have in your version string</param>
        /// <returns>Version-like string</returns>
        public static string GetRandomVersionString(
            int partCount = 3)
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
            string path)
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
            int depth = 2)
        {
            var toCreate = GetRandomCollection<string>(1).ToList();
            toCreate.ToArray().ForEach(
                f =>
                {
                    Directory.CreateDirectory(Path.Combine(path, f));
                    if (depth > 1)
                    {
                        toCreate.AddRange(
                            CreateRandomFoldersIn(Path.Combine(path, f), depth - 1)
                                .Select(sub => Path.Combine(f, sub)));
                    }
                });
            return toCreate;
        }

        /// <summary>
        /// Creates a randomly-named file within the provided folder path and returns its name
        /// </summary>
        /// <param name="path">Folder within which to create the file</param>
        /// <returns>Name of the file (name only, not full path)</returns>
        public static string CreateRandomFileIn(
            string path)
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
            string path)
        {
            var fileName = GetRandomString();
            var lines = GetRandomCollection<string>(1);
            File.WriteAllLines(Path.Combine(path, fileName), lines);
            return fileName;
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
            int depth = 2)
        {
            var folders = CreateRandomFoldersIn(path, depth).ToArray();
            var result = new List<string>(folders);
            folders.ForEach(
                f =>
                {
                    var numberOfFiles = GetRandomInt(1);
                    numberOfFiles.TimesDo(
                        () =>
                        {
                            var createdFile = CreateRandomFileIn(Path.Combine(path, f));
                            result.Add(Path.Combine(f, createdFile));
                        });
                });
            return result;
        }
    }
}