using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils
{
    public static class TypeExtensions
    {
        public static Type[] Ancestry(this Type type)
        {
            var heirachy = new List<Type>();
            do
            {
                heirachy.Add(type);
            } while ((type = type.BaseType) != null);
            heirachy.Reverse();
            return heirachy.ToArray();
        }

        public static Dictionary<string, object> GetAllConstants(this Type type)
        {
            // hybrid of http://stackoverflow.com/questions/10261824/how-can-i-get-all-constants-of-a-type-by-reflection
            //  and https://ruscoweb.wordpress.com/2011/02/09/c-using-reflection-to-get-constant-values/
            return type.GetFields(BindingFlags.Public |
                                  BindingFlags.Static |
                                  BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .ToDictionary(x => x.Name, y => y.GetRawConstantValue());
        } 

        public static Dictionary<string, T> GetAllConstants<T>(this Type type)
        {
            return type.GetAllConstants()
                        .Where(kvp => kvp.Value is T)
                        .ToDictionary(x => x.Key, y => (T)y.Value);
        } 

        public static IEnumerable<object> GetAllConstantValues(this Type type)
        {
            return type.GetAllConstants().Select(kvp => kvp.Value);
        } 

        public static IEnumerable<T> GetAllConstantValues<T>(this Type type)
        {
            return type.GetAllConstantValues()
                        .OfType<T>();
        } 

        public static bool HasDefaultConstructor(this Type type)
        {
            return type.GetConstructors()
                        .Any(c => c.GetParameters().Length == 0);
        }
    }
}