using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable ConstantNullCoalescingCondition
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
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
        /// <summary>
        /// Enumerates the ancestry of a Type
        /// </summary>
        /// <param name="type">Starting Type</param>
        /// <returns>The Type ancestry, starting from Object</returns>
        public static Type[] Ancestry(this Type type)
        {
            return type.AncestryUntil(null);
        }

        /// <summary>
        /// Enumerates the ancestry of a Type, from the given type
        /// - if the given type is not found in the ancestry, the entire ancestry
        ///   will be returned
        /// - you may provide a generic type without parameters, eg GenericBuilder&lt;,&gt;
        ///   in which case the search is from the first occurence of that generic base type
        ///   within the ancestry tree
        /// </summary>
        /// <param name="type">Type to operate on (final type in the result)</param>
        /// <param name="from">Type to truncate history at (first type in the result, when found)</param>
        /// <returns></returns>
        public static Type[] AncestryUntil(
            this Type type,
            Type from)
        {
            var stopAtIsGenericDefinition =
                from != null &&
                from.IsGenericType &&
                from.GetGenericArguments().All(
                    a => a.GUID == Guid.Empty
                );
            var hierarchy = new List<Type>();
            do
            {
                if (from != null)
                {
                    if (type == from)
                    {
                        break;
                    }

                    if (stopAtIsGenericDefinition &&
                        type.IsGenericType &&
                        type.GetGenericTypeDefinition() == from)
                    {
                        break;
                    }
                }

                hierarchy.Add(type);
            } while ((type = type.BaseType()) != null);

            // we typically want this list from most to least ancient
            hierarchy.Reverse();
            return hierarchy.ToArray();
        }

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
                .ToDictionary(x => x.Key, y => (T) y.Value);
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
#if NETSTANDARD
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
            var specific = GenericIsAssignableFromArrayOf.MakeGenericMethod(collectionType);
            return (bool) specific.Invoke(null, new object[] { t });
        }

        /// <summary>
        /// Provides an extension method mimicking the full framework
        /// IsEnum for a single point of code usage
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsEnum(this Type t)
        {
            return t
#if NETSTANDARD
                .GetTypeInfo()
#endif
                .IsEnum;
        }

#if NETSTANDARD
        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static MethodInfo GetMethod(this Type t, string name, BindingFlags flags)
        {
            return t.GetTypeInfo().GetMethod(name, flags);
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static PropertyInfo[] GetProperties(this Type t)
        {
            return t.GetTypeInfo().GetProperties();
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static PropertyInfo GetProperty(this Type t, string name)
        {
            return t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static PropertyInfo GetProperty(this Type t, string name, BindingFlags flags)
        {
            return t.GetTypeInfo().GetProperty(name, flags);
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static PropertyInfo[] GetProperties(this Type t, BindingFlags flags)
        {
            return t.GetTypeInfo().GetProperties(flags);
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static FieldInfo[] GetFields(this Type t, BindingFlags flags)
        {
            return t.GetTypeInfo().GetFields(flags);
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static bool IsAssignableFrom(this Type t, Type other)
        {
            return t.GetTypeInfo().IsAssignableFrom(other);
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static Type[] GetInterfaces(this Type t)
        {
            return t.GetTypeInfo().GetInterfaces();
        }

        /// <summary>
        /// provides the method normally found on a .net framework
        /// Type object, but reachabole on .net standard only via GetTypeInfo();
        /// </summary>
        public static Type[] GetGenericArguments(this Type t)
        {
            return t.GetTypeInfo().GetGenericArguments();
        }
#endif

        /// <summary>
        /// Provides an extension method mimicking the full framework
        /// GetAssembly for a single point of code usage
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Assembly GetAssembly(this Type t)
        {
#if NETSTANDARD
            return t?.GetTypeInfo()?.Assembly;
#else
            return t?.Assembly;
#endif
        }

        /// <summary>
        /// Provides an extension method mimicking the full framework
        /// BaseType for a single point of code usage
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type BaseType(this Type type)
        {
            return type
#if NETSTANDARD
                .GetTypeInfo()
#endif
                .BaseType;
        }

        /// <summary>
        /// Provides an extension method mimicking the full framework
        /// IsGenericType for a single point of code usage
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsGenericType(this Type t)
        {
            return t
#if NETSTANDARD
                .GetTypeInfo()
#endif
                .IsGenericType;
        }


        private static readonly MethodInfo GenericIsAssignableFromArrayOf
            = typeof(TypeExtensions).GetMethod(nameof(IsAssignableFromArrayOf),
                BindingFlags.Static | BindingFlags.Public);

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
        /// Returns true if the provided type implements IDictionary&lt;,&gt;
        /// anywhere in the type heirachy
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ImplementsIDictionaryGenericType(this Type type)
        {
            return type.GetAllImplementedInterfaces()
                .Any(t => t.IsIDictionary());
        }

        /// <summary>
        /// Returns true if a type directly implements IDictionary&lt;,&gt;
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsIDictionary(this Type type)
        {
            return type.IsGenericType() &&
                type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
        }

        /// <summary>
        /// Tries to get the key and value types for an object, if
        /// it implements IDictionary&lt;TKey,TValue&gt;. Returns true
        /// if sucessful (with the out parameters set) or false if
        /// the provided type does not implement IDictionary&lt;,&gt;
        /// </summary>
        /// <param name="type"></param>
        /// <param name="keyType"></param>
        /// <param name="valueType"></param>
        /// <returns></returns>
        public static bool TryGetDictionaryKeyAndValueTypes(
            this Type type,
            out Type keyType,
            out Type valueType
        )
        {
            var dictionaryInterface = type.GetAllImplementedInterfaces()
                .FirstOrDefault(t => t.IsIDictionary());
            if (dictionaryInterface == null)
            {
                keyType = null;
                valueType = null;
                return false;
            }

            keyType = dictionaryInterface.GenericTypeArguments[0];
            valueType = dictionaryInterface.GenericTypeArguments[1];
            return true;
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
            var enumerableInterface = collectionType.TryGetEnumerableInterface();
            if (enumerableInterface != null)
                return enumerableInterface.GenericTypeArguments[0];
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
            return t.GetAllImplementedInterfaces().Contains(DisposableInterface);
        }

        private static readonly Type DisposableInterface = typeof(IDisposable);

        /// <summary>
        /// Provides a "pretty" name for a type, taking into account
        /// generics and nullable types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string PrettyName(this Type type)
        {
            if (type == null)
                return "(null Type)";
            if (!type.IsGenericType())
                return type.Name;

            if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = type.GetGenericArguments()[0];
                return $"{underlyingType.PrettyName()}?";
            }

            var parts = type.FullName
                    // ReSharper disable once ConstantNullCoalescingCondition
                    ?.Substring(0, type.FullName?.IndexOf("`") ?? 0)
                    .Split('.') ??
                new[] { type.Name };
            return string.Join("",
                parts.Last(),
                "<",
                string.Join(", ",
                    type.GetGenericArguments().Select(PrettyName)),
                ">");
        }

        /// <summary>
        /// Rudimentary test for if a type is a collection type, testing for
        /// IEnumerable&lt;&gt; interface implementation as well as some baked-in
        /// known generic types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCollection(this Type type)
        {
            if (type == null)
                return false;
            return type.IsArray ||
                (type.IsGenericType() &&
                    (type.ImplementsEnumerableGenericType() ||
                        CollectionGenerics.Contains(type.GetGenericTypeDefinition())));
        }

        /// <summary>
        /// Determines if an object of this type can be assigned null
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CanBeAssignedNull(this Type type)
        {
            // accepted answer from
            // http://stackoverflow.com/questions/1770181/determine-if-reflected-property-can-be-assigned-null#1770232
            // conveniently located as an extension method
            return !type.IsValueType() || Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Provides single method to determine IsValueType (shimmed for NETSTANDARD)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueType(this Type type)
        {
            return type
#if NETSTANDARD
                .GetTypeInfo()
#endif
                .IsValueType;
        }


        private static readonly Type[] CollectionGenerics =
        {
            typeof(ICollection<>),
            typeof(IEnumerable<>),
            typeof(List<>),
            typeof(IList<>),
            typeof(IDictionary<,>),
            typeof(Dictionary<,>)
        };


        /// <summary>
        /// Returns true if the type being operated on can be directly assigned
        /// or implicitly upcast to the target type
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsAssignableOrUpCastableTo(this Type src, Type target)
        {
            return target.IsAssignableFrom(src) ||
                src.CanImplicitlyCastTo(target);
        }

        /// <summary>
        /// Returns true if the type being operated on can be
        /// implicitly upcast to the target type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CanImplicitlyCastTo(
            this Type source,
            Type target
        )
        {
            if (source is null)
            {
                throw new ArgumentException("source cannot be null", nameof(source));
            }

            if (target is null)
            {
                throw new ArgumentException("target cannot be null", nameof(target));
            }

            return source.DefaultValue().TryImplicitlyCastTo(target, out _);
        }

        /// <summary>
        /// Returns true if the type being operated on can be
        /// implicitly upcast to the target type (value types only, so far)
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="srcValue"></param>
        /// <param name="castValue"></param>
        /// <returns></returns>
        public static bool TryImplicitlyCastTo(
            this object srcValue,
            Type targetType,
            out object castValue
        )
        {
            var srcType = srcValue?.GetType();
            
            if (srcType == targetType)
            {
                castValue = srcValue;
                return true;
            }

            castValue = targetType.DefaultValue();
            var convertSpecific = TryConvertGeneric.MakeGenericMethod(targetType);

            bool canConvert;
            try
            {
                castValue = convertSpecific.Invoke(null, new[] { srcValue });
                canConvert = true;
            }
            catch
            {
                canConvert = false;
            }

            return canConvert;
        }

        private static readonly ConcurrentDictionary<Type, object> DefaultTypeValues
            = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Returns the default value for the type being operated on
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object DefaultValue(this Type type)
        {
            if (DefaultTypeValues.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var method = DefaultValueGeneric.MakeGenericMethod(type);
            var result = method.Invoke(null, null);
            DefaultTypeValues.TryAdd(type, result);
            return result;
        }

        /// <summary>
        /// Cross-target shim for the IsInterface property,
        /// found on Type in NetFramework and on Type.GetTypeInf()
        /// on NETSTANDARD
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsInterface(this Type type)
        {
#if NETSTANDARD
            return type?.GetTypeInfo().IsInterface ?? false;
#else
            return type?.IsInterface ?? false;
#endif
        }

        /// <summary>
        /// Determines whether the provided type is a known numeric type
        /// (ie int / short / byte / double / float / decimal )
        /// </summary>
        /// <param name="type">Type to operate on</param>
        /// <returns></returns>
        public static bool IsNumericType(this Type type)
        {
            return NumericTypes.Contains(type);
        }

        private static readonly Type ObjectType = typeof(object);

        /// <summary>
        /// Determines whether the type being operated on is an ancestor of the other type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool IsAncestorOf(this Type type, Type other)
        {
            if (other == ObjectType)
            {
                return true;
            }

            var baseType = type.BaseType();
            if (baseType == null)
            {
                return false;
            }

            return baseType == other || baseType.IsAncestorOf(other);
        }


        /// <summary>
        /// Tests if the type being operated on implements the provided interfaceType
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool Implements<T>(this Type type) where T : class
        {
            return type.Implements(typeof(T));
        }

        /// <summary>
        /// Tests if the type being operated on implements the provided interfaceType
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool Implements(this Type type, Type interfaceType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new InvalidOperationException($"{interfaceType} is not an interface type");
            }

            return type.GetInterfaces().Contains(interfaceType);
        }

        private static readonly Type[] NumericTypes =
        {
            typeof(int),
            typeof(short),
            typeof(byte),
            typeof(double),
            typeof(float),
            typeof(decimal)
        };


        private static readonly MethodInfo TryConvertGeneric =
            typeof(TypeExtensions).GetMethod(nameof(TryConvert), BindingFlags.Static | BindingFlags.NonPublic);


        private static T2 TryConvert<T2>(T2 value)
        {
            return value;
        }

        private static readonly MethodInfo DefaultValueGeneric =
            typeof(TypeExtensions).GetMethod(nameof(DefaultValue), BindingFlags.Static | BindingFlags.NonPublic);

        private static T DefaultValue<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Returns true if all public properties and methods are either virtual or abstract\
        /// (ie can be properly overridden)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AllPublicInstancePropertiesAndMethodsAreVirtualOrAbstract(this Type type)
        {
            return type.AllPublicInstancePropertiesAreVirtualOrAbstract() &&
                type.AllPublicInstanceMethodsAreVirtualOrAbstract();
        }

        /// <summary>
        /// Returns true if all instance properties on the provided type are
        /// either virtual or abstract (ie, overridable)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AllPublicInstancePropertiesAreVirtualOrAbstract(
            this Type type
        )
        {
            return type.GetProperties(PUBLIC_INSTANCE)
                .All(pi => pi.IsVirtualOrAbstract());
        }

        /// <summary>
        /// Returns true if all instance methods on the provided type are
        /// either virtual or abstract (ie, overridable)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool AllPublicInstanceMethodsAreVirtualOrAbstract(
            this Type type
        )
        {
            var methods = type.GetMethods(PUBLIC_INSTANCE)
                .Where(mi => !NonVirtualObjectMethods.Contains(mi.Name))
                .ToArray();
            return methods.All(mi => mi.IsVirtualOrAbstract());
        }

        private static HashSet<string> NonVirtualObjectMethods
            => _objectMethods ??= new HashSet<string>(
                typeof(object).GetMethods(PUBLIC_INSTANCE)
                    .Where(mi => !mi.IsVirtual)
                    .Select(mi => mi.Name)
            );

        private static HashSet<string> _objectMethods;

        private const BindingFlags PUBLIC_INSTANCE = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Retrieves the value of the "top-most" property in an inheritance hierarchy
        /// which matches the given name and type
        /// </summary>
        /// <param name="data"></param>
        /// <param name="propertyName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T GetTopMostPropertyValue<T>(
            this object data,
            string propertyName
        )
        {
            var propInfo = data.FindTopMostProperty<T>(propertyName);
            return (T) propInfo.GetValue(data);
        }

        /// <summary>
        /// Sets the "top-most" property in an ancestry, useful for setting
        /// properties marked as "new" when you have access to the object
        /// cast to an ancestor type
        /// </summary>
        /// <param name="data"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void SetTopMostProperty<T>(
            this object data,
            string propertyName,
            T value
        )
        {
            var propInfo = data.FindTopMostProperty<T>(propertyName);
            propInfo.SetValue(data, value);
        }

        internal static PropertyInfo FindTopMostProperty<T>(
            this object data,
            string propertyName
        )
        {
            var type = data.GetType();
            var propertyType = typeof(T);
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => pi.Name == propertyName && (
                    propertyType == typeof(object) || pi.PropertyType == propertyType)
                )
                .ToArray();
            if (!props.Any())
            {
                throw new InvalidOperationException(
                    $"{type} has no property named {propertyName} with type {propertyType}"
                );
            }

            var lookup = props.ToDictionary(prop => prop.DeclaringType, prop => prop);
            var ancestry = type.Ancestry().Reverse();
            var firstMatch = ancestry.FirstOrDefault(t => lookup.ContainsKey(t));
            if (firstMatch is null)
            {
                // shouldn't get here, but if we do, throw a better exception than null-deref
                throw new InvalidOperationException(
                    $"Unable to determine top-most '{propertyName}' property for {type}"
                );
            }

            return lookup[firstMatch];
        }

        /// <summary>
        /// returns true if the given method is virtual or abstract
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static bool IsVirtualOrAbstract(this MethodInfo methodInfo)
        {
            return methodInfo.IsVirtual || methodInfo.IsAbstract;
        }

        /// <summary>
        /// returns true if the type is not an interface or an abstract type
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsConcrete(this Type t)
        {
            return !(t.IsInterface || t.IsAbstract);
        }

        /// <summary>
        /// returns true if the given property is virtual or abstract
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsVirtualOrAbstract(this PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetMethod?.IsVirtualOrAbstract() ?? true) &&
                (propertyInfo.SetMethod?.IsVirtualOrAbstract() ?? true);
        }

        /// <summary>
        /// returns tru if the provided object implements the expected interface
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="expected"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static bool Implements<T>(
            this T obj,
            Type expected
        )
        {
            // prefer object actual type, fall back on generic type
            var type = obj?.GetType() ?? typeof(T);
            if (!expected.IsInterface)
            {
                throw new InvalidOperationException(
                    $"{nameof(Implements)} tests for implemented interfaces, not for ancestry");
            }

            var interfaces = type.GetInterfaces();
            if (expected.IsGenericType)
            {
                return interfaces
                    .Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == expected
                    );
            }

            return interfaces.Any(i => i == expected);
        }

        /// <summary>
        /// Tests if the provided type is nullable
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static bool IsNullableType(this Type arg)
        {
            if (NullableTypes.TryGetValue(arg, out var cachedResult))
            {
                return cachedResult;
            }

            var method = GenericGetDefaultValueMethod.MakeGenericMethod(arg);
            var defaultValueForType = method.Invoke(null, new object[] { });
            var result = defaultValueForType == null;
            return NullableTypes[arg] = result;
        }

        private static readonly MethodInfo GenericGetDefaultValueMethod =
            typeof(TypeExtensions).GetMethod(
                nameof(GetDefaultValueFor),
                BindingFlags.NonPublic | BindingFlags.Static
            );

        private static T GetDefaultValueFor<T>()
        {
            return default(T);
        }

        private static readonly ConcurrentDictionary<Type, bool> NullableTypes = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Attempts to set a static property or field value
        /// </summary>
        /// <param name="t"></param>
        /// <param name="fieldOrPropertyName"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void SetStatic<T>(
            this Type t,
            string fieldOrPropertyName,
            T value
        )
        {
            var member = PropertyOrField.Find(
                t,
                fieldOrPropertyName
            );
            member.SetValue(null, value);
        }

        /// <summary>
        /// Attempts to get a static property or field value
        /// </summary>
        /// <param name="t"></param>
        /// <param name="fieldOrPropertyName"></param>
        /// <typeparam name="T"></typeparam>
        public static T GetStatic<T>(
            this Type t,
            string fieldOrPropertyName
        )
        {
            var member = PropertyOrField.Find(
                t,
                fieldOrPropertyName
            );
            return (T) member.GetValue(null);
        }
    }
}