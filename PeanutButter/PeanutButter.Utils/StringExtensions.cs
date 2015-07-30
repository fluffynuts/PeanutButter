using System.Linq;
using System.Text.RegularExpressions;

namespace PeanutButter.Utils
{
    public static class StringExtensions
    {
        public static string RegexReplace(this string input, string pattern, string replaceWith)
        {
            var regex = new Regex(pattern);
            return regex.Replace(input, replaceWith);
        }

        public static string Or(this string input, string alternative)
        {
            return string.IsNullOrEmpty(input) ? alternative : input;
        }

        private static readonly string[] _truthy = {"yes", "y", "1", "true"};
        public static bool AsBoolean(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            return _truthy.Any(item => item == input.ToLower());
        }
    }
}