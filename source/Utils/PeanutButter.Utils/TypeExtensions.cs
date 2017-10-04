using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Helper extensions for Types
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public 
#endif
    static class TypeExtensions
    {
#if NETSTANDARD1_6
#else
        /// <summary>
        /// Enumerates the ancestry of a Type
        /// </summary>
        /// <param name="type">Starting Type</param>
        /// <returns>The Type ancestry, starting from Object</returns>
        public static Type[] Ancestry(this Type type)
        {
            var heirachy = new List<Type>();
            do
            {
                heirachy.Add(type);
            }
            while ((type = type.BaseType) != null);

            heirachy.Reverse();
            return heirachy.ToArray();
        }
#endif

        /// <summary>
        /// Returns a dictionary of all constant values defined on a Type
        /// </summary>
        /// <param name="type">Source type to search for constants</param>
        /// <returns>Dictionary of constants, keyed by constant name</returns>
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

        /// <summary>
        /// Returns a dictionary of all constant values of a specified Type found on a Type
        /// </summary>
        /// <param name="type">Type to search for constants</param>
        /// <typeparam name="T">Only return constants of this Type</typeparam>
        /// <returns>Dictionary of all constant values on a specified type</returns>
        public static Dictionary<string, T> GetAllConstants<T>(this Type type)
        {
            return type.GetAllConstants()
                        .Where(kvp => kvp.Value is T)
                        .ToDictionary(x => x.Key, y => (T)y.Value);
        } 

        /// <summary>
        /// Returns a collection of all the constant values defined on a Type
        /// </summary>
        /// <param name="type">Type to search for constants</param>
        /// <returns>Collection of the constant values without their defined names</returns>
        public static IEnumerable<object> GetAllConstantValues(this Type type)
        {
            return type.GetAllConstants().Select(kvp => kvp.Value);
        } 

        /// <summary>
        /// Returns a collection of all the constant values defined on a Type, restricted to the required Type T
        /// </summary>
        /// <param name="type">Type to search for constants</param>
        /// <typeparam name="T">Only return constants of this Type</typeparam>
        /// <returns>Collection of constant values from the source type which match the Type T restriction</returns>
        public static IEnumerable<T> GetAllConstantValues<T>(this Type type)
        {
            return type.GetAllConstantValues()
                        .OfType<T>();
        } 

        /// <summary>
        /// Tests if a Type has a default constructor (ie, a constructor with no parameters)
        /// </summary>
        /// <param name="type">Type to inspect</param>
        /// <returns>True when the type has a parameterless constructor; False otherwise. Note that a constructor with parameters which have all default values is not considered valid.</returns>
        public static bool HasDefaultConstructor(this Type type)
        {
            return type
#if NETSTANDARD1_6
                .GetTypeInfo()
#endif
                .GetConstructors()
                        .Any(c => c.GetParameters().Length == 0);
        }

        /// <summary>
        /// Tests if a type is an array or could be assigned from an array
        /// </summary>
        /// <param name="t">Type to check</param>
        /// <returns>True if {t} is an Array type or could have an array type assigned to it; False otherwise</returns>
        public static bool IsArrayOrAssignableFromArray(this Type t)
        {
            if (t.IsArray) return true;
            var collectionType = t.GetCollectionItemType();
            if (collectionType == null) return false;
            var specific = _genericIsAssignableFromArrayOf.MakeGenericMethod(collectionType);
            return (bool)specific.Invoke(null, new object[] { t } );
        }

#if NETSTANDARD1_6
        public static MethodInfo GetMethod(this Type t, string name, BindingFlags flags)
        {
            return t.GetTypeInfo().GetMethod(name, flags);
        }

        public static PropertyInfo[] GetProperties(this Type t)
        {
            return t.GetTypeInfo().GetProperties();
        }

        public static PropertyInfo GetProperty(this Type t, string name, BindingFlags flags)
        {
            return t.GetTypeInfo().GetProperty(name, flags);
        }

        public static PropertyInfo[] GetProperties(this Type t, BindingFlags flags)
        {
            return t.GetTypeInfo().GetProperties(flags);
        }
        public static FieldInfo[] GetFields(this Type t, BindingFlags flags)
        {
            return t.GetTypeInfo().GetFields(flags);
        }

        public static bool IsAssignableFrom(this Type t, Type other)
        {
            return t.GetTypeInfo().IsAssignableFrom(other);
        }

        public static Type[] GetInterfaces(this Type t)
        {
            return t.GetTypeInfo().GetInterfaces();
        }

        public static Type[] GetGenericArguments(this Type t)
        {
            return t.GetTypeInfo().GetGenericArguments();
        }

#endif

        public static Assembly GetAssembly(this Type t)
        {
#if NETSTANDARD1_6
            return t?.GetTypeInfo()?.Assembly;
#else
            return t?.Assembly;
#endif
        }

        public static bool IsGenericType(this Type t)
        {
#if NETSTANDARD1_6
            return t.GetTypeInfo().IsGenericType;
#else
            return t.IsGenericType;
#endif
        }


        private static readonly MethodInfo _genericIsAssignableFromArrayOf
            = typeof(TypeExtensions).GetMethod(nameof(IsAssignableFromArrayOf), BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// Tests if a type is a generic of a given generic type (eg typeof(List&lt;&gt;))
        /// </summary>
        /// <param name="t">type to operate on</param>
        /// <param name="genericTest">type to test against (eg typeof(List&lt;&gt;))</param>
        /// <returns>True if the input type is a match, false otherwise</returns>
        public static bool IsGenericOf(this Type t, Type genericTest)
        {
            return t.IsGenericType() && t.GetGenericTypeDefinition() == genericTest;
        }

        /// <summary>
        /// Tests if a type is assignable from an array of T
        /// </summary>
        /// <param name="t">Type to test</param>
        /// <typeparam name="T">Item type of array which calling code would like to assign</typeparam>
        /// <returns>True if the parameter type is assignable from an array of T</returns>
        public static bool IsAssignableFromArrayOf<T>(this Type t)
        {
            return t.IsAssignableFrom(typeof(T[]));
        }

        /// <summary>
        /// Tests if a type implements IEnumerable&lt;&gt;
        /// </summary>
        /// <param name="t">Type to test</param>
        /// <returns>True if the source type implements IEnumerable&lt;&gt;; False otherwise</returns>
        // ReSharper disable once UnusedMember.Global
        public static bool ImplementsEnumerableGenericType(this Type t)
        {
            return t.IsGenericOfIEnumerable() || TryGetEnumerableInterface(t) != null;
        }

        /// <summary>
        /// Attempts to get the implemented Generic IEnumerable interface for a type, if possible
        /// </summary>
        /// <param name="srcType">Type to search for the interface</param>
        /// <returns>Generic IEnumerable type implemented if found or null otherwise</returns>
        public static Type TryGetEnumerableInterface(this Type srcType)
        {
            return srcType.IsGenericOfIEnumerable()
                ? srcType
                : srcType.GetInterfaces().FirstOrDefault(IsGenericOfIEnumerable);
        }

        /// <summary>
        /// Attempts to get the item type (T)
        /// for a Type which is assumed to implement IEnumerable&lt;T&gt;
        /// </summary>
        /// <param name="srcType">Type to search for the IEnumerable &lt;T&gt; interface and underlying type</param>
        /// <returns>IEnumerable&lt;&gt; item type (T) implemented if found or null otherwise</returns>
        public static Type TryGetEnumerableItemType(this Type srcType)
        {
            return srcType.TryGetEnumerableInterface()
                ?.GenericTypeArguments[0];
        }

        /// <summary>
        /// Tests if a type directly implements the generic IEnumerable interface
        /// </summary>
        /// <param name="arg">Type to test</param>
        /// <returns>True if it does implement the generic IEnumerable; false otherwise</returns>
        public static bool IsGenericOfIEnumerable(this Type arg)
        {
            return arg.IsGenericOf(typeof(IEnumerable<>));
        }

        /// <summary>
        /// Attempts to get the item type of a colleciton
        /// </summary>
        /// <param name="collectionType">Type to inspect</param>
        /// <returns>Item type, if it can be found, or null</returns>
        public static Type GetCollectionItemType(this Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();
            if (collectionType.IsGenericType())
                return collectionType.GenericTypeArguments[0];
            return null;
        }

        /// <summary>
        /// Attempts to find all implemented interfaces (and inherited ones) for a Type
        /// </summary>
        /// <param name="inspectType">Type to inspect</param>
        /// <returns>Array of all interfaces which are implemented</returns>
        public static Type[] GetAllImplementedInterfaces(this Type inspectType)
        {
            var result = new List<Type> { inspectType };
            foreach (var type in inspectType.GetInterfaces())
            {
                result.AddRange(type.GetAllImplementedInterfaces());
            }
            return result.Distinct().ToArray();
        }

        /// <summary>
        /// Tests if a type implements IDisposable
        /// </summary>
        /// <param name="t">Type to test</param>
        /// <returns>True if it implements IDisposable; false otherwise</returns>
        public static bool IsDisposable(this Type t)
        {
            return t.GetAllImplementedInterfaces().Contains(_disposableInterface);
        }
        private static readonly Type _disposableInterface = typeof(IDisposable);


        public static bool IsAssignableOrUpcastableTo(this Type src, Type target)
        {
            return target.IsAssignableFrom(src) ||
                    src.CanImplicitlyUpcastTo(target);
        }

        public static bool CanImplicitlyUpcastTo(
            this Type src, Type target
        )
        {
            var convertSpecific = _tryConvertGeneric.MakeGenericMethod(target);
            var defaultSpecific = _defaultvalueGeneric.MakeGenericMethod(src);
            var defaultValue = defaultSpecific.Invoke(null, null);
            bool canConvert;
            try
            {
                convertSpecific.Invoke(null, new[] { defaultValue });
                canConvert = true;
            }
            catch
            {
                canConvert = false;
            }
            return canConvert;
        }


        private static MethodInfo _tryConvertGeneric = 
            typeof(TypeExtensions).GetMethod(nameof(TryConvert), BindingFlags.Static | BindingFlags.NonPublic);


        private static T2 TryConvert<T2>(T2 value)
        {
            return value;
        }

        private static MethodInfo _defaultvalueGeneric =
            typeof(TypeExtensions).GetMethod(nameof(DefaultValue), BindingFlags.Static | BindingFlags.NonPublic);

        private static T DefaultValue<T>()
        {
            return default(T);
        }
    }
}