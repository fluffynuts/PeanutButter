using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

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

        public static bool IsArrayOrAssignableFromArray(this Type t)
        {
            if (t.IsArray) return true;
            var collectionType = t.GetCollectionItemType();
            if (collectionType == null) return false;
            var specific = _genericIsAssignableFromArrayOf.MakeGenericMethod(collectionType);
            return (bool)specific.Invoke(null, new object[] { t } );
        }

        private static readonly MethodInfo _genericIsAssignableFromArrayOf
            = typeof(TypeExtensions).GetMethod("IsAssignableFromArrayOf", BindingFlags.Static | BindingFlags.Public);
        // ReSharper disable once UnusedMember.Global
        public static bool IsAssignableFromArrayOf<T>(this Type t)
        {
            return t.IsAssignableFrom(typeof(T[]));
        }

        // ReSharper disable once UnusedMember.Global
        public static bool ImplementsEnumerableGenericType(this Type t)
        {
            return t.IsGenericOfIEnumerable() || TryGetEnumerableInterface(t) != null;
        }

        public static Type TryGetEnumerableInterface(this Type srcType)
        {
            return srcType.GetInterfaces().FirstOrDefault(IsGenericOfIEnumerable);
        }

        public static bool IsGenericOfIEnumerable(this Type arg)
        {
            if (!arg.IsGenericType) return false;
            return arg.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static Type GetCollectionItemType(this Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();
            if (collectionType.IsGenericType)
                return collectionType.GenericTypeArguments[0];
            return null;
        }

        public static Type[] GetAllImplementedInterfaces(this Type interfaceType)
        {
            var result = new List<Type> { interfaceType };
            foreach (var type in interfaceType.GetInterfaces())
            {
                result.AddRange(type.GetAllImplementedInterfaces());
            }
            return result.ToArray();
        }

        private static readonly  Type _disposableInterface = typeof(IDisposable);
        public static bool IsDisposable(this Type t)
        {
            return t.GetAllImplementedInterfaces().Contains(_disposableInterface);
        }

    }
}