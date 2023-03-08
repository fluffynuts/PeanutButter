using System.Linq;
using System.Text.RegularExpressions;

// pretend this is imported from PB.Utils - just for the cross-ref from DuckTyping
// ReSharper disable once CheckNamespace
namespace Imported.PeanutButter.Utils
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Replace all matching Regex patterns in input with the given string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="replaceWith"></param>
        /// <param name="patterns"></param>
        /// <returns></returns>
        public static string RegexReplaceAll(
            this string input,
            string replaceWith,
            params Regex[] patterns
        )
        {
            return patterns.Aggregate(
                input,
                (acc, cur) => cur.Replace(acc, replaceWith)
            );
        }
        
    }
}