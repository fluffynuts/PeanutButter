using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Imported.PeanutButter.Utils;
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
using Imported.PeanutButter.DuckTyping.AutoConversion;
using Imported.PeanutButter.DuckTyping.AutoConversion.Converters;
using Imported.PeanutButter.DuckTyping.Comparers;
using Imported.PeanutButter.DuckTyping.Exceptions;
using Imported.PeanutButter.DuckTyping.Extensions;

#else
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.DuckTyping.AutoConversion.Converters;
using PeanutButter.DuckTyping.Comparers;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;
#endif

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Shimming
#else
namespace PeanutButter.DuckTyping.Shimming
#endif
{
    /// <summary>
    /// Shim to wrap objects for ducking
    /// </summary>
    // (probably) required to be public for source embedding
    public class ShimSham : ShimShamBase, IShimSham
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        private readonly bool _isFuzzy;
        private readonly object[] _wrapped;
        private readonly bool _allowReadonlyDefaultsForMissingMembers;
        private readonly IPropertyInfoFetcher _propertyInfoFetcher;
        private readonly Type[] _wrappedTypes;
        private bool _wrappingADuck;
        private bool _wrappingADictionaryDuck;

        private static readonly Dictionary<Type, PropertyInfoContainer> PropertyInfos = new();
        private static readonly Dictionary<Type, MethodInfoContainer> MethodInfos = new();
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfos = new();

        private Dictionary<string, FieldInfo> _localFieldInfos;
        private Dictionary<string, MethodInfo[]> _localMethodInfos;
        private Dictionary<string, PropertyInfo> _localPropertyInfos;
        private readonly Dictionary<int, object> _shimmedProperties = new();
        private readonly HashSet<int> _unshimmableProperties = new();
        private PropertyInfoContainer _mimickedPropInfos;
        private Dictionary<string, PropertyInfo> _localMimicPropertyInfos;
        private Dictionary<string, Type> _localMimicPropertyTypes = new();

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Constructs a new instance of the ShimSham with a DefaultPropertyInfoFetcher
        /// </summary>
        /// <param name="toWrap">Objects to wrap (wip: only the first object is considered)</param>
        /// <param name="interfaceToMimic">Interface type to mimick</param>
        /// <param name="isFuzzy">Flag allowing or preventing approximation</param>
        /// <param name="allowReadonlyDefaultMembers">allows properties with no backing to be read as the default value for that type</param>
        public ShimSham(
            object[] toWrap,
            Type interfaceToMimic,
            bool isFuzzy,
            bool allowReadonlyDefaultMembers
        )
            : this(toWrap, interfaceToMimic, isFuzzy, allowReadonlyDefaultMembers, new DefaultPropertyInfoFetcher())
        {
        }

        /// <summary>
        /// Constructs a new instance of the ShimSham with the provided property info fetcher
        /// </summary>
        /// <param name="toWrap">Object to wrap</param>
        /// <param name="interfaceToMimic">Interface type to mimick</param>
        /// <param name="isFuzzy">Flag allowing or preventing approximation</param>
        /// <exception cref="ArgumentNullException">Thrown if the mimick interface or property info fetch are null</exception>
        /// <param name="allowReadonlyDefaultMembers">allows properties with no backing to be read as the default value for that type</param>
        // ReSharper disable once UnusedMember.Global
        public ShimSham(
            object toWrap,
            Type interfaceToMimic,
            bool isFuzzy,
            bool allowReadonlyDefaultMembers
        )
            : this(
                new[]
                {
                    toWrap
                },
                interfaceToMimic,
                isFuzzy,
                allowReadonlyDefaultMembers
            )
        {
        }

        /// <summary>
        /// Constructs a new instance of the ShimSham with the provided property info fetcher
        /// </summary>
        /// <param name="toWrap">Objects to wrap (wip: only the first object is considered)</param>
        /// <param name="interfaceToMimic">Interface type to mimick</param>
        /// <param name="isFuzzy">Flag allowing or preventing approximation</param>
        /// <param name="allowReadonlyDefaultsForMissingMembers">Whether to allow returning default(T) for properties which are missing on the wrapped source(s)</param>
        /// <param name="propertyInfoFetcher">Utility to fetch property information from the provided object and interface type</param>
        /// <exception cref="ArgumentNullException">Thrown if the mimick interface or property info fetch are null</exception>
        // ReSharper disable once MemberCanBePrivate.Global
        public ShimSham(
            object[] toWrap,
            Type interfaceToMimic,
            bool isFuzzy,
            bool allowReadonlyDefaultsForMissingMembers,
            IPropertyInfoFetcher propertyInfoFetcher
        )
        {
            if (interfaceToMimic is null)
            {
                throw new ArgumentNullException(nameof(interfaceToMimic));
            }

            _propertyInfoFetcher = propertyInfoFetcher ?? throw new ArgumentNullException(nameof(propertyInfoFetcher));
            _isFuzzy = isFuzzy;
            _wrapped = toWrap;
            _allowReadonlyDefaultsForMissingMembers = allowReadonlyDefaultsForMissingMembers;
            _wrappedTypes = toWrap.Select(w => w.GetType()).ToArray();
            ExamineObjectForDuckiness();
            StaticallyCachePropertyInfosFor(_wrapped, _wrappingADuck);
            StaticallyCachePropertyInfosFor(interfaceToMimic);
            StaticallyCacheMethodInfosFor(_wrappedTypes);
            LocallyCachePropertyInfos();
            LocallyCacheMethodInfos();
            LocallyCacheMimickedPropertyInfos();
        }

        private void StaticallyCachePropertyInfosFor(Type interfaceToMimic)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public;
            _mimickedPropInfos = new PropertyInfoContainer(
                interfaceToMimic.GetAllImplementedInterfaces()
                    .And(interfaceToMimic)
                    .Distinct()
                    .Select(i => _propertyInfoFetcher
                        .GetProperties(i, bindingFlags)
                    )
                    .SelectMany(c => c)
                    .ToArray()
            );
        }

        private void StaticallyCacheMethodInfosFor(Type[] wrappedTypes)
        {
            lock (MethodInfos)
            {
                foreach (var wrappedType in wrappedTypes)
                {
                    if (MethodInfos.ContainsKey(wrappedType))
                    {
                        return;
                    }

                    MethodInfos[wrappedType] = new MethodInfoContainer(
                        wrappedType.GetMethods(
                            BindingFlags.Instance | BindingFlags.Public
                        )
                    );
                }
            }
        }

        /// <inheritdoc />
        public object GetPropertyValue(string propertyName)
        {
            CheckForImpendingStackOverflow();
            var propCode = propertyName.GetHashCode();
            if (_wrappingADuck)
            {
                return ReadDuckedProperty(propertyName);
            }

            if (_shimmedProperties.TryGetValue(propCode, out var shimmed))
            {
                return shimmed;
            }

            var propInfo = FindPropertyInfoFor(propCode, propertyName);
            if (propInfo == null)
            {
                return GetDefaultValueFor(
                    _localMimicPropertyTypes[propertyName]
                );
            }

            if (!propInfo.CanRead || propInfo.Getter == null)
            {
                // TODO: throw for the correct wrapped type for this property
                throw new WriteOnlyPropertyException(_wrappedTypes[0], propertyName);
            }

            return DuckIfRequired(propInfo, propertyName);
        }

        private object ReadDuckedProperty(string propertyName)
        {
            if (_wrappingADictionaryDuck)
            {
                if (ReadPropertyDirectly(propertyName, out var value))
                {
                    return value;
                }
            }
            else
            {
                if (ReadBackingFieldValue(propertyName, out var o))
                {
                    return o;
                }
            }

            throw new InvalidOperationException(
                $"Unable to read property '{propertyName}' from ducked type '{_wrappedTypes[0]}'"
            );
        }

        private bool ReadBackingFieldValue(
            string propertyName,
            out object o
        )
        {
            var fieldInfo = FindPrivateBackingFieldFor(propertyName);
            if (fieldInfo is not null)
            {
                {
                    o = fieldInfo.GetValue(_wrapped[0]);
                    return true;
                }
            }

            o = default;
            return false;
        }

        private bool ReadPropertyDirectly(
            string propertyName,
            out object value
        )
        {
            var itemType = _wrappedTypes[0];
            if (PropertyInfos.TryGetValue(itemType, out var props))
            {
                if (props.PropertyInfos.TryGetValue(propertyName, out var pi))
                {
                    {
                        value = pi.GetValue(_wrapped[0]);
                        return true;
                    }
                }

                throw new InvalidOperationException(
                    $"Unable to find property '{propertyName}' on type '{itemType}'"
                );
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        public void SetPropertyValue(
            string propertyName,
            object newValue
        )
        {
            CheckForImpendingStackOverflow();
            var propCode = propertyName.GetHashCode();
            if (_wrappingADuck)
            {
                WriteDuckedProperty(propertyName, newValue);
                return;
            }

            var propInfo = FindPropertyInfoFor(propCode, propertyName);
            if (propInfo == null)
            {
                throw new PropertyNotFoundException(_wrappedTypes[0], propertyName);
            }

            if (!propInfo.CanWrite || propInfo.Setter is null)
            {
                // TODO: throw for correct wrapped type for this particular property
                throw new ReadOnlyPropertyException(_wrappedTypes[0], propertyName);
            }

            var mimickedType = _localMimicPropertyTypes[propertyName];
            var newValueType = newValue?.GetType();
            if (newValueType is null)
            {
                var defaultValue = GetDefaultValueFor(mimickedType);
                TrySetValue(defaultValue, mimickedType, propInfo);
                return;
            }

            if (mimickedType.IsAssignableFrom(newValueType))
            {
                TrySetValue(newValue, newValueType, propInfo);
                return;
            }

            var duckType = MakeTypeToImplement(mimickedType, _isFuzzy);
            var instance = Activator.CreateInstance(
                duckType,
                new object[]
                {
                    new[]
                    {
                        newValue
                    }
                }
            );
            _shimmedProperties[propCode] = instance;
        }

        private void WriteDuckedProperty(
            string propertyName,
            object newValue
        )
        {
            if (_wrappingADictionaryDuck)
            {
                var itemType = _wrappedTypes[0];
                if (PropertyInfos.TryGetValue(itemType, out var props))
                {
                    if (props.PropertyInfos.TryGetValue(propertyName, out var pi))
                    {
                        pi.SetValue(_wrapped[0], newValue);
                        return;
                    }

                    throw new InvalidOperationException(
                        $"Unable to find property '{propertyName}' on type '{itemType}'"
                    );
                }
            }
            else
            {
                var fieldInfo = FindPrivateBackingFieldFor(propertyName);
                if (fieldInfo is not null)
                {
                    fieldInfo.SetValue(_wrapped[0], newValue);
                    return;
                }
            }

            throw new InvalidOperationException(
                $"Unable to set property '{propertyName}' on ducked type '{_wrappedTypes[0]}'"
            );
        }

        private object DuckIfRequired(
            PropertyInfoCacheItem propertyInfoCacheItem,
            string propertyName
        )
        {
            if (_shimmedProperties.TryGetValue(propertyName.GetHashCode(), out var existingShim))
            {
                return existingShim;
            }

            var getter = propertyInfoCacheItem.Getter;
            var propValue = getter();

            var correctType = _localMimicPropertyTypes[propertyName];
            if (propValue == null)
            {
                return GetDefaultValueFor(correctType);
            }

            var propValueType = propertyInfoCacheItem.PropertyType;
            if (correctType.IsAssignableFrom(propValueType))
            {
                return propValue;
            }

            var converter = ConverterLocator.TryFindConverter(propValueType, correctType);
            if (converter != null)
            {
                return ConvertWith(converter, propValue, correctType);
            }

            if (EnumConverter.TryConvert(propValueType, correctType, propValue, out var result))
            {
                return result;
            }

            if (correctType.ShouldTreatAsPrimitive())
            {
                return GetDefaultValueFor(correctType);
            }

            if (CannotShim(propertyName.GetHashCode(), propValue, correctType))
            {
                return null;
            }

            var duckType = MakeTypeToImplement(correctType, _isFuzzy);
            var asDict = propValue.TryConvertToDictionary();
            var instance = Activator.CreateInstance(
                duckType,
                asDict == null
                    // ReSharper disable once RedundantExplicitArrayCreation
                    ? new object[]
                    {
                        new object[]
                        {
                            propValue
                        }
                    }
                    : new object[]
                    {
                        asDict
                    }
            );
            _shimmedProperties[propertyName.GetHashCode()] = instance;
            return instance;
        }

        private bool CannotShim(
            int propCode,
            object propValue,
            Type targetType
        )
        {
            if (_unshimmableProperties.Contains(propCode))
            {
                return true;
            }

            var result = !propValue.InternalCanDuckAs(targetType, _isFuzzy, false);
            if (result)
            {
                _unshimmableProperties.Add(propCode);
            }

            return result;
        }


        /// <inheritdoc />
        public void CallThroughVoid(
            string methodName,
            params object[] parameters
        )
        {
            CallThrough(methodName, parameters);
        }

        /// <inheritdoc />
        public object CallThrough(
            string methodName,
            object[] arguments
        )
        {
            if (_wrappingADuck)
            {
                throw new NotImplementedException("Cannot call-through when there is no wrapped object");
            }

            // TODO: throw for correct wrapped type
            var wrappedType = _wrappedTypes[0];
            if (!_localMethodInfos.TryGetValue(methodName, out var methodInfos))
            {
                throw new MethodNotFoundException(wrappedType, methodName);
            }

            var argumentTypes = arguments.Select(o => o?.GetType()).ToArray();
            var k = Tuple.Create(methodName, argumentTypes);
            var methodInfo = CachedMethodResolutions.FindOrAdd(
                k,
                () => TryFindBestParameterMatchFor(methodName, argumentTypes, methodInfos)
            );
            if (_isFuzzy)
            {
                arguments = AttemptToOrderCorrectly(arguments, methodInfo);
            }

            // FIXME: find the correct wrapped object to invoke on
            var result = methodInfo.Invoke(_wrapped[0], arguments);
            return result;
        }

        private readonly ConcurrentDictionary<Tuple<string, Type[]>, MethodInfo> CachedMethodResolutions = new();

        private MethodInfo TryFindBestParameterMatchFor(
            string methodName,
            Type[] argumentTypes,
            MethodInfo[] methodInfos
        )
        {
            if (methodInfos.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No underlying methods by name '{methodName}'"
                );
            }

            if (methodInfos.Length == 1)
            {
                return methodInfos[0];
            }

            MethodInfo closeMatch = null;
            foreach (var methodInfo in methodInfos)
            {
                var parameterTypes = methodInfo.GetParameters().Select(o => o.ParameterType).ToArray();
                if (parameterTypes.Length != argumentTypes.Length)
                {
                    continue;
                }

                if (argumentTypes.IsEqualTo(parameterTypes))
                {
                    return methodInfo;
                }

                if (closeMatch is null && argumentTypes.IsEquivalentTo(parameterTypes))
                {
                    closeMatch = methodInfo;
                }
            }

            if (closeMatch is not null)
            {
                return closeMatch; // let the caller take care of argument re-ordering
            }

            throw new InvalidOperationException(
                $"Unable to find matching underlying method '{methodName}' with parameter types {string.Join(",", argumentTypes.Select(t => (object)t))}"
            );
        }

        private object[] AttemptToOrderCorrectly(
            object[] parameters,
            MethodInfo methodInfo
        )
        {
            var methodParameters = methodInfo.GetParameters();
            if (parameters.Length != methodParameters.Length)
            {
                throw new ParameterCountMismatchException(parameters.Length, methodInfo);
            }

            var srcTypes = parameters.Select(o => o?.GetType()).ToArray();
            var dstTypes = methodParameters.Select(p => p.ParameterType).ToArray();
            if (AlreadyInCorrectOrderByType(srcTypes, dstTypes))
            {
                return
                    parameters; // no need to change anything here and we don't have to care about parameters with the same type
            }

            if (dstTypes.Distinct().Count() != dstTypes.Length)
            {
                throw new UnresolveableParameterOrderMismatchException(dstTypes, methodInfo);
            }

            return Reorder(parameters, dstTypes);
        }

        private object[] Reorder(
            object[] parameters,
            Type[] dstTypes
        )
        {
            return dstTypes
                .Select(type => FindBestMatchFor(type, parameters))
                .ToArray();
        }

        private static object FindBestMatchFor(
            Type type,
            object[] parameters
        )
        {
            return parameters.FirstOrDefault(p => p.GetType() == type);
        }

        private static bool AlreadyInCorrectOrderByType(
            Type[] srcTypes,
            Type[] dstTypes
        )
        {
            for (var i = 0; i < srcTypes.Length; i++)
            {
                var src = srcTypes[i];
                var dst = dstTypes[i];
                if (src == null && dst.IsPrimitive)
                {
                    return false;
                }

                if (src != dst)
                {
                    return false;
                }
            }

            return true;
        }

        private void LocallyCachePropertyInfos()
        {
            var wrappedType = _wrappedTypes[0];
            _localPropertyInfos = _isFuzzy
                ? PropertyInfos[wrappedType].FuzzyPropertyInfos
                : PropertyInfos[wrappedType].PropertyInfos;
            if (_wrappingADuck)
            {
                _localFieldInfos = FieldInfos[wrappedType];
            }
        }

        private void LocallyCacheMimickedPropertyInfos()
        {
            _localMimicPropertyInfos = _isFuzzy
                ? _mimickedPropInfos.FuzzyPropertyInfos
                : _mimickedPropInfos.PropertyInfos;
            _localMimicPropertyTypes = _localMimicPropertyInfos
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.PropertyType,
                    _isFuzzy
                        ? StringComparer.OrdinalIgnoreCase
                        : StringComparer.Ordinal
                );
        }

        private void LocallyCacheMethodInfos()
        {
            // TODO: get all cached method infos for all types
            var wrappedType = _wrappedTypes[0];
            _localMethodInfos = _isFuzzy
                ? MethodInfos[wrappedType].FuzzyMethodInfos
                : MethodInfos[wrappedType].MethodInfos;
        }

        private void StaticallyCachePropertyInfosFor(
            object[] cacheAll,
            bool cacheFieldInfosToo
        )
        {
            lock (PropertyInfos)
            {
                // TODO: cache for all objects
                var toCacheFor = cacheAll[0];
                var type = toCacheFor.GetType();
                if (PropertyInfos.ContainsKey(type))
                {
                    return;
                }

                PropertyInfos[type] = new PropertyInfoContainer(
                    _propertyInfoFetcher
                        .GetPropertiesFor(
                            toCacheFor,
                            BindingFlags.Instance | BindingFlags.Public
                        )
                );
                if (!cacheFieldInfosToo)
                {
                    return;
                }

                FieldInfos[type] = type
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .ToDictionary(
                        fi => fi.Name,
                        fi => fi
                    );
            }
        }

        private void ExamineObjectForDuckiness()

        {
            for (var i = 0; i < _wrapped.Length; i++)
            {
                var type = _wrappedTypes[i];
                if (type.GetCustomAttributes(true).OfType<IsADuckAttribute>().Any())
                {
                    _wrappingADuck = true;
                }

                var shimField = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.Name == "_shim");
                if (shimField is null)
                {
                    return;
                }

                var shimTarget = shimField.GetValue(_wrapped[i]);
                _wrappingADictionaryDuck = shimTarget is DictionaryShimSham;
            }
        }

        private readonly Dictionary<int, PropertyInfoCacheItem> _propertyInfoLookupCache = new();

        private class PropertyInfoCacheItem
        {
            public Type PropertyType;
            public Action<object> Setter;
            public Func<object> Getter;
            public bool CanRead;
            public bool CanWrite;
        }

        private PropertyInfoCacheItem FindPropertyInfoFor(
            int propCode,
            string propertyName
        )
        {
            if (_propertyInfoLookupCache.TryGetValue(propCode, out var cached))
            {
                return cached;
            }

            if (_shimmedProperties.ContainsKey(propCode))
            {
                var cacheItem = CreateCacheItemFor(
                    _localMimicPropertyInfos[propertyName]
                );
                _propertyInfoLookupCache[propCode] = cacheItem;
                return cacheItem;
            }

            if (!_localPropertyInfos.TryGetValue(propertyName, out var pi))
            {
                if (_allowReadonlyDefaultsForMissingMembers)
                {
                    return null;
                }

                // TODO: throw for the correct type
                throw new PropertyNotFoundException(_wrappedTypes[0], propertyName);
            }

            var result = CreateCacheItemFor(pi);
            _propertyInfoLookupCache[propCode] = result;
            return result;
        }

        private PropertyInfoCacheItem CreateCacheItemFor(PropertyInfo propertyInfo)
        {
            return new PropertyInfoCacheItem
            {
                PropertyType = propertyInfo.PropertyType,
                CanRead = propertyInfo.CanRead,
                CanWrite = propertyInfo.CanWrite,
                Setter = CreateSetterExpressionFor(propertyInfo),
                Getter = CreateGetterExpressionFor(propertyInfo)
            };
        }

        private void SetFieldValue(
            string propertyName,
            object newValue
        )
        {
            var fieldInfo = FindPrivateBackingFieldFor(propertyName);
            // FIXME: find the correct wrapped object to invoke on
            fieldInfo.SetValue(_wrapped[0], newValue);
        }

        private object FieldValueFor(string propertyName)
        {
            var fieldInfo = FindPrivateBackingFieldFor(propertyName);
            // FIXME: find the correct wrapped object to invoke on
            return fieldInfo.GetValue(_wrapped[0]);
        }

        private FieldInfo FindPrivateBackingFieldFor(string propertyName)
        {
            var seek = "_" + propertyName;
            if (!_localFieldInfos.TryGetValue(seek, out var fieldInfo))
                // TODO: throw for the correct type
            {
                throw new BackingFieldForPropertyNotFoundException(_wrappedTypes[0], propertyName);
            }

            return fieldInfo;
        }


        // borrowed from http://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142
        //  -> use compiled lambdas for property get / set which provides a performance
        //      boost in that the runtime doesn't have to perform as much checking

        private Action<object> CreateSetterExpressionFor(PropertyInfo propertyInfo)
        {
            var methodInfo = propertyInfo.GetSetMethod();
            if (methodInfo == null)
            {
                return null;
            }

            var exValue = Expression.Parameter(typeof(object), "p");
            // FIXME: find the correct wrapped object to target
            var exTarget = Expression.Constant(_wrapped[0]);
            var exBody = Expression.Call(
                exTarget,
                methodInfo,
                Expression.Convert(exValue, propertyInfo.PropertyType)
            );

            var lambda = Expression.Lambda<Action<object>>(exBody, exValue);
            var action = lambda.Compile();
            return action;
        }

        private static bool DebugEnabled = Environment.GetEnvironmentVariable(
            "DEBUG_DUCKTYPING"
        ).AsBoolean();

        private void TrySetValue(
            object newValue,
            Type newValueType,
            PropertyInfoCacheItem propInfo
        )
        {
            var pType = propInfo.PropertyType;
            var setter = propInfo.Setter;
            if (pType.IsAssignableFrom(newValueType))
            {
                setter(newValue);
                return;
            }

            var converter = ConverterLocator.TryFindConverter(newValueType, pType);
            if (converter is null)
            {
                throw new InvalidOperationException(
                    $"Unable to set property: no converter for {newValueType.Name} => {pType.Name}"
                );
            }

            var converted = ConvertWith(converter, newValue, pType);
            setter(converted);
        }

        private static void CheckForImpendingStackOverflow()
        {
            if (!DebugEnabled)
            {
                return;
            }

            var s = new StackTrace();
            if (HaveReEnteredTooManyTimes(s.GetFrames()))
            {
                throw new InvalidOperationException(
                    """
                    Looks like we're heading for a stack overflow
                    """
                );
            }
        }

        private static bool HaveReEnteredTooManyTimes(
            StackFrame[] frames
        )
        {
            var level = frames.Aggregate(
                0,
                (
                    acc,
                    cur
                ) =>
                {
                    var thisMethod = cur.GetMethod();
                    var thisType = thisMethod.DeclaringType;
                    if (
                        thisType == typeof(ShimSham) &
                        thisMethod.Name == nameof(TrySetValue)
                    )
                    {
                        return acc + 1;
                    }

                    return acc;
                }
            );
            return level >= 10;
        }

        private Func<object> CreateGetterExpressionFor(PropertyInfo propertyInfo)
        {
            var methodInfo = propertyInfo.GetGetMethod();
            if (methodInfo == null)
            {
                return null;
            }

            // FIXME: find the correct target
            var exTarget = Expression.Constant(_wrapped[0]);
            var exBody = Expression.Call(exTarget, methodInfo);
            var exBody2 = Expression.Convert(exBody, typeof(object));

            var lambda = Expression.Lambda<Func<object>>(exBody2);

            var action = lambda.Compile();
            return action;
        }
    }
}