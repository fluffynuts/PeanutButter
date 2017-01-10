using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides utility extensions for strings
    /// </summary>
    public static class StringExtensions
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
            return string.IsNullOrEmpty(input) ? alternative : input;
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
            return MultiContains(haystack, needles, h => needles.Any(n => h.Contains(n.ToLower(CultureInfo.CurrentCulture))));
        }

        /// <summary>
        /// Searches a master string for occurrences of any of the given strings
        /// </summary>
        /// <param name="haystack">String to search</param>
        /// <param name="needles">Strings to search for</param>
        /// <returns>True if all of the needles are found in the haystack; False otherwise</returns>
        public static bool ContainsAllOf(this string haystack, params string[] needles)
        {
            return MultiContains(haystack, needles, h => needles.All(n => h.Contains(n.ToLower(CultureInfo.CurrentCulture))));
        }

        private static bool MultiContains(
            string haystack,
            // ReSharper disable once UnusedParameter.Global
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
        /// Convenience function to convert a string to a byte array via UTF-8 decoding
        /// </summary>
        /// <param name="src">String to convert</param>
        /// <returns>Byte array of the {src} string when decoded as UTF-8</returns>
        public static byte[] AsBytes(this string src)
        {
            return src == null
                    ? null
                    : Encoding.UTF8.GetBytes(src);
        }

        /// <summary>
        /// Tests if a string represents an integer value
        /// </summary>
        /// <param name="src">String to test</param>
        /// <returns>True if the string can be converted to an integer; False otherwise</returns>
        public static bool IsInteger(this string src)
        {
            int value;
            return int.TryParse(src, out value);
        }

        /// <summary>
        /// Performs exceptionless conversion of a string to an integer
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>The integer value of the string; 0 if it cannot be converted</returns>
        public static int AsInteger(this string value)
        {
            int result;
            var interestingPart = GetLeadingIntegerCharsFrom(value ?? string.Empty);
            int.TryParse(interestingPart, out result);
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