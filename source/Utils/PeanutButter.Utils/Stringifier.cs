using System;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides convenience functions to get reasonable string representations of objects and collections
    /// </summary>
    public static class Stringifier
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
                : $"[ {string.Join(", ", objs.Select(o => Stringify(o))) } ]";
        }

        /// <summary>
        /// Provides a reasonable human-readable string representation of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Human-readable representation of object</returns>
        public static string Stringify(object obj)
        {
            return SafeStringifier(obj, 0);
        }

        private const int MAX_STRINGIFY_DEPTH = 10;
        private const int INDENT_SIZE = 2;

        private static readonly Dictionary<Type, Func<object, string>> _primitiveStringifiers 
            = new Dictionary<Type, Func<object, string>>()
            {
                [typeof(string)] = o => $"\"{o}\"",
                [typeof(bool)] = o => o.ToString().ToLower()
            };

        private static string SafeStringifier(object obj, int level)
        {
            if (obj == null) return "null";
            var objType = obj.GetType();
            if (level >= MAX_STRINGIFY_DEPTH || Types.Primitives.Contains(objType))
            {
                Func<object, string> strategy;
                return _primitiveStringifiers.TryGetValue(objType, out strategy)
                        ? strategy(obj)
                        : obj.ToString();
            }
            try
            {
                var props = obj.GetType().GetProperties();
                var indentMinus1 = new string(' ', level * INDENT_SIZE);
                var indent = indentMinus1 + new string(' ', INDENT_SIZE);
                var joinWith = props.Aggregate(new List<string>(), (acc, cur) =>
                {
                    var propValue = cur.GetValue(obj);
                    acc.Add(string.Join(
                        "", 
                        cur.Name, 
                        ": ",  
                        SafeStringifier(propValue, level+1)));
                    return acc;
                }).JoinWith($"\n{indent}");
                return "{\n" + string.Join(
                    "\n{indent}",
                    $"{indent}{joinWith}"
                    ) + $"\n{indentMinus1}}}";
            }
            catch
            {
                return obj.ToString();
            }
        }

    }
}