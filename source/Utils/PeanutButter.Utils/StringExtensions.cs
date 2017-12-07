using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides utility extensions for strings
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class StringExtensions
    {
        /// <summary>
        /// Replaces patterns matched by the given regex pattern with the given replaceWith string
        /// </summary>
        /// <param name="input">Starting string</param>
        /// <param name="pattern">Regex pattern to search for</param>
        /// <param name="replaceWith">String to replace occurrences with</param>
        /// <returns>New string with matches replaces</returns>
        public static string RegexReplace(this string input, string pattern, string replaceWith)
        {
            var regex = new Regex(pattern);
            return regex.Replace(input, replaceWith);
        }

        /// <summary>
        /// Convenience extension to return another string if the input is null or empty
        /// </summary>
        /// <param name="input">String to test</param>
        /// <param name="alternative">String to return if the input was null or empty</param>
        /// <returns>The original string when it is not null or empty; the alternative when the original is null or empty</returns>
        public static string Or(this string input, string alternative)
        {
            return string.IsNullOrEmpty(input)
                ? alternative
                : input;
        }

        /// <summary>
        /// Attempts conversion from a string value to a boolean value matching the following (case-insensitive) to True:
        /// - "yes"
        /// - "y"
        /// - "1"
        /// - "true"
        /// All other string values are considered to be false
        /// </summary>
        /// <param name="input">String to attempt to convert</param>
        /// <returns>True for truthy values, False otherwise</returns>
        public static bool AsBoolean(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            return _truthy.Any(item => item == input.ToLower());
        }

        private static readonly string[] _truthy = {"yes", "y", "1", "true"};

        /// <summary>
        /// Searches a master string for occurrences of any of the given strings
        /// </summary>
        /// <param name="haystack">String to search</param>
        /// <param name="needles">Strings to search for</param>
        /// <returns>True if any of the needles are found in the haystack; False otherwise</returns>
        public static bool ContainsOneOf(this string haystack, params string[] needles)
        {
            return MultiContains(haystack,
                needles,
                h => needles.Any(n => h.Contains(n.ToLower(
#if NETSTANDARD
#else
                    CultureInfo.CurrentCulture
#endif
                ))));
        }

        /// <summary>
        /// Searches a master string for occurrences of any of the given strings
        /// </summary>
        /// <param name="haystack">String to search</param>
        /// <param name="needles">Strings to search for</param>
        /// <returns>True if all of the needles are found in the haystack; False otherwise</returns>
        public static bool ContainsAllOf(this string haystack, params string[] needles)
        {
            return MultiContains(haystack,
                needles,
                h => needles.All(n => h.Contains(n.ToLower(CultureInfo.CurrentCulture))));
        }

        private static bool MultiContains(
            string haystack,
            // ReSharper disable once UnusedParameter.Local
            string[] needles,
            Func<string, bool> operation
        )
        {
            if (needles.Length == 0)
                throw new ArgumentException("No needles provided to search haystack for");
            if (needles.Any(n => n == null))
                throw new ArgumentException("Null needle provided. Boo!");
            if (haystack == null)
                return false;
            haystack = haystack.ToLower(CultureInfo.InvariantCulture);
            return operation(haystack);
        }

        /// <summary>
        /// Tests if a string starts with one of the provided search strings
        /// </summary>
        /// <param name="src">String to test</param>
        /// <param name="search">Strings to look for at the start of {src}</param>
        /// <returns>True if {src} starts with any one of provided search strings; False otherwise</returns>
        public static bool StartsWithOneOf(this string src, params string[] search)
        {
            if (src == null)
                return false;
            src = src.ToLower(CultureInfo.InvariantCulture);
            return search.Any(s => src.StartsWith(s.ToLower(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Calls AsBytes extension method with the UTF8 encoding
        /// </summary>
        /// <param name="src">String to operate on</param>
        /// <returns>Byte array representing string, from UTF8 encoding</returns>
        public static byte[] AsBytes(this string src)
        {
            return src.AsBytes(Encoding.UTF8);
        }

        /// <summary>
        /// Convenience function to convert a string to a byte array
        /// </summary>
        /// <param name="src">String to convert</param>
        /// <param name="encoding">Encoding to use</param>
        /// <returns>Byte array of the {src} string when decoded as UTF-8</returns>
        public static byte[] AsBytes(this string src, Encoding encoding)
        {
            return src == null
                ? null
                : encoding.GetBytes(src);
        }

        /// <summary>
        /// Converts a string to a Stream of bytes, assuming utf-8 encoding
        /// </summary>
        /// <param name="src">String to convert</param>
        /// <returns>Stream or null if src is null</returns>
        public static Stream AsStream(this string src)
        {
            return src.AsStream(Encoding.UTF8);
        }

        /// <summary>
        /// Lowercases a string collection with Invariant culture. Tolerates nulls.
        /// </summary>
        /// <param name="src">Collection to lower-case</param>
        /// <returns>Input, lower-cased</returns>
        public static IEnumerable<string> ToLower(
            this IEnumerable<string> src
        )
        {
            return src.ToLower(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Lowercases a string collection with Invariant culture. Tolerates nulls.
        /// </summary>
        /// <param name="src">Collection to lower-case</param>
        /// <param name="cultureInfo">Culture to use in the operation</param>
        /// <returns>Input, lower-cased</returns>
        public static IEnumerable<string> ToLower(
            this IEnumerable<string> src,
            CultureInfo cultureInfo
        )
        {
            return src.Select(s => s?.ToLower(cultureInfo));
        }

        /// <summary>
        /// Uppercases a string collection with Invariant culture. Tolerates nulls.
        /// </summary>
        /// <param name="src">Collection to lower-case</param>
        /// <returns>Input, lower-cased</returns>
        public static IEnumerable<string> ToUpper(
            this IEnumerable<string> src
        )
        {
            return src.ToUpper(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Uppercases a string collection with Invariant culture. Tolerates nulls.
        /// </summary>
        /// <param name="src">Collection to lower-case</param>
        /// <param name="cultureInfo">Culture to use in the operation. Note that .NET's ToUpper doesn't accept a culture, so really, your only choices here are "Invariant" or "whatever .Net uses by default".</param>
        /// <returns>Input, lower-cased</returns>
        public static IEnumerable<string> ToUpper(
            this IEnumerable<string> src,
            CultureInfo cultureInfo
        )
        {
            return src.Select(s =>
                cultureInfo.Equals(CultureInfo.InvariantCulture)
                    ? s?.ToUpperInvariant()
                    : s?.ToUpper()
            );
        }

        /// <summary>
        /// Converts a string to a Stream of bytes with the provided encoding
        /// </summary>
        /// <param name="src">String to convert</param>
        /// <param name="encoding">Encoding to use</param>
        /// <returns>Stream or null if src is null</returns>
        public static Stream AsStream(this string src, Encoding encoding)
        {
            return src?.AsBytes(encoding)?.AsStream();
        }

        /// <summary>
        /// Attempts to encode the given byte array as if it contained a
        /// utf8-encoded string
        /// </summary>
        /// <param name="data">Bytes to encode</param>
        /// <returns>The utf8 string, if possible; will return null if given null</returns>
        public static string AsString(this byte[] data)
        {
            return data.AsString(Encoding.UTF8);
        }

        /// <summary>
        /// Attempts to encode the given byte array as if it contained a
        /// string encoded with the given encoding
        /// </summary>
        /// <param name="data">Bytes to encode</param>
        /// <param name="encoding"></param>
        /// <returns>The string, if possible; will return null if given null</returns>
        public static string AsString(this byte[] data, Encoding encoding)
        {
            return data == null ? null : encoding.GetString(data);
        }

        /// <summary>
        /// Convenience function to wrap a given byte array in a MemoryStream.
        /// </summary>
        /// <param name="src">Bytes to wrapp</param>
        /// <returns>Stream wrapping the bytes or null if the source is null</returns>
        public static Stream AsStream(this byte[] src)
        {
            return src == null ? null : new MemoryStream(src);
        }

        /// <summary>
        /// Tests if a string represents an integer value
        /// </summary>
        /// <param name="src">String to test</param>
        /// <returns>True if the string can be converted to an integer; False otherwise</returns>
        public static bool IsInteger(this string src)
        {
            return int.TryParse(src, out var _);
        }

        /// <summary>
        /// Performs exceptionless conversion of a string to an integer
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>The integer value of the string; 0 if it cannot be converted</returns>
        public static int AsInteger(this string value)
        {
            var interestingPart = GetLeadingIntegerCharsFrom(value ?? string.Empty);
            int.TryParse(interestingPart, out var result);
            return result;
        }

        /// <summary>
        /// Turns string.IsNullOrWhiteSpace into an extension method for fluency
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns>True if is null or whitespace; False otherwise</returns>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Turns string.IsNullOrEmpty into an extension method for fluency
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns>True if is null or whitespace; False otherwise</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Returns the base64-encoded representation of a string value
        /// </summary>
        /// <param name="value">Input string value</param>
        /// <returns>The base64-encoded representation of the string, or null if the string is null</returns>
        public static string ToBase64(this string value)
        {
            return value?.AsBytes()?.ToBase64();
        }

        /// <summary>
        /// Returns "0" if the input string is empty or null
        /// </summary>
        /// <param name="input">String to test</param>
        /// <returns>Original string or "0" if empty or null</returns>
        public static string ZeroIfEmptyOrNull(this string input)
        {
            return input.DefaultIfEmptyOrNull("0");
        }

        /// <summary>
        /// Returns a given fallback value if the input string is whitespace or null
        /// </summary>
        /// <param name="input">String to test</param>
        /// <param name="fallback">Fallback value if the input is whitespace or null</param>
        /// <returns>Original string or the given fallback if the input is whitespace or null</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static string DefaultIfEmptyOrNull(this string input, string fallback)
        {
            return string.IsNullOrWhiteSpace(input)
                ? fallback
                : input;
        }

        /// <summary>
        /// Safely trims a string, returning an empty string if given null
        /// </summary>
        /// <param name="input">String to trim</param>
        /// <param name="trimChars">Optional params of chars to trim, passed to standard String.Trim() method</param>
        /// <returns>Empty string if input is null, otherwise trimmed input</returns>
        public static string SafeTrim(this string input, params char[] trimChars)
        {
            return input?.Trim(trimChars) ?? "";
        }

        /// <summary>
        /// Converts an input string to kebab-case
        /// </summary>
        /// <param name="input">string to convert</param>
        /// <returns>kebab-cased-output</returns>
        public static string ToKebabCase(this string input)
        {
            return input?
                .Replace('_', '-')
                .Replace(' ', '-')
                .SplitOnCapitalsAnd('-')
                .Select(s => s.ToLower())
                .JoinWith("-");
        }

        /// <summary>
        /// Converts an input string to snake_case
        /// </summary>
        /// <param name="input">string to convert</param>
        /// <returns>snake_cased_output</returns>
        public static string ToSnakeCase(this string input)
        {
            return input.ToKebabCase()?.Replace('-', '_');
        }

        /// <summary>
        /// Converts an input string to PascalCase
        /// </summary>
        /// <param name="input">string to convert</param>
        /// <returns>PascalCasedOutput</returns>
        public static string ToPascalCase(this string input)
        {
            return input?
                .SplitOnCapitalsAnd('-', '_')
                .Select(ToUpperCasedFirstLetter)
                .JoinWith("");
        }

        /// <summary>
        /// Converts an input string to camelCase
        /// </summary>
        /// <param name="input">string to convert</param>
        /// <returns>camelCasedOutput</returns>
        public static string ToCamelCase(this string input)
        {
            return input.ToPascalCase().ToLowerCasedFirstLetter();
        }

        private static Random _randomField;
        private static Random Random => _randomField ?? (_randomField = new Random(DateTime.Now.Millisecond));

        /// <summary>
        /// Returns the input string in RaNdOMizEd case
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToRandomCase(this string input)
        {
            return string.Join("", input
                .Select(c => c.ToString())
                .Select(c => Random.NextDouble() < 0.5
                    ? c.ToLowerInvariant()
                    : c.ToUpperInvariant()
                ));
        }

        /// <summary>
        /// Converts an input string to words, where possible
        /// eg: kebab-case => "kebab case"
        ///     snake_case => "snake case"
        ///     PascalCase => "pascal case"
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToWords(this string input)
        {
            return input.ToKebabCase()?.Replace("-", " ");
        }

        /// <summary>
        /// Lower-cases the first letter in your string
        /// </summary>
        /// <param name="input">string to operate on</param>
        /// <returns>string with lower-cased first letter or null if input was null</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static string ToLowerCasedFirstLetter(this string input)
        {
            return (input?.Length ?? 0) > 0
                // ReSharper disable once PossibleNullReferenceException
                ? $"{input[0].ToString().ToLower()}{input.Substring(1)}"
                : input;
        }

        /// <summary>
        /// Upper-cases the first letter in your string
        /// </summary>
        /// <param name="input">string to operate on</param>
        /// <returns>string with upper-cased first letter or null if input was null</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static string ToUpperCasedFirstLetter(this string input)
        {
            return (input?.Length ?? 0) > 0
                // ReSharper disable once PossibleNullReferenceException
                ? $"{input[0].ToString().ToUpper()}{input.Substring(1)}"
                : input;
        }

#if NETSTANDARD
        public static string ToLower(this string input, CultureInfo ci)
        {
            return input.ToLower();
        }
#endif

        private static IEnumerable<string> SplitOnCapitalsAnd(this string input, params char[] others)
        {
            var collector = new List<char>();
            foreach (var c in input)
            {
                var asString = c.ToString();
                var upper = asString.ToUpper();
                var isOtherMatch = others.Contains(c);
                if (collector.Any() &&
                    (upper == asString || isOtherMatch))
                {
                    yield return string.Join("", collector);
                    collector.Clear();
                }
                if (!isOtherMatch)
                    collector.Add(c);
            }
            yield return string.Join("", collector);
        }

        private static string GetLeadingIntegerCharsFrom(string value)
        {
            var collected = new List<string>();
            var intMarker = 0;
            value.ForEach(c =>
            {
                if (intMarker > 1)
                    return;
                var asString = c.ToString();
                if ("1234567890".Contains(asString))
                {
                    intMarker = 1;
                    collected.Add(asString);
                    return;
                }
                if (intMarker == 1)
                {
                    intMarker++;
                }
            });
            return collected.JoinWith(string.Empty);
        }
    }
}