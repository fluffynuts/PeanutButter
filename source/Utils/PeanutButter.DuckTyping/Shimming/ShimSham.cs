using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Imported.PeanutButter.Utils;
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.DuckTyping.AutoConversion.Converters;
using PeanutButter.DuckTyping.Comparers;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Shimming
{
    /// <summary>
    /// Shim to wrap objects for ducking
    /// </summary>
    public class ShimSham : ShimShamBase, IShimSham
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once InconsistentNaming
        private readonly bool _isFuzzy;
        private readonly object[] _wrapped;
        private readonly bool _allowReadonlyDefaultsForMissingMembers;
        private readonly IPropertyInfoFetcher _propertyInfoFetcher;
        private readonly Type[] _wrappedTypes;
        private readonly bool _wrappingADuck;

        private static readonly Dictionary<Type, PropertyInfoContainer> PropertyInfos =
            new Dictionary<Type, PropertyInfoContainer>();

        private static readonly Dictionary<Type, MethodInfoContainer> MethodInfos =
            new Dictionary<Type, MethodInfoContainer>();

        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldInfos =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        private Dictionary<string, FieldInfo> _localFieldInfos;
        private Dictionary<string, MethodInfo> _localMethodInfos;
        private Dictionary<string, PropertyInfo> _localPropertyInfos;
        private readonly Dictionary<string, object> _shimmedProperties = new Dictionary<string, object>();
        private readonly HashSet<string> _unshimmableProperties = new HashSet<string>();
        private PropertyInfoContainer _mimickedPropInfos;
        private Dictionary<string, PropertyInfo> _localMimicPropertyInfos;

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Constructs a new instance of the ShimSham with a DefaultPropertyInfoFetcher
        /// </summary>
        /// <param name="toWrap">Objects to wrap (wip: only the first object is considered)</param>
        /// <param name="interfaceToMimic">Interface type to mimick</param>
        /// <param name="isFuzzy">Flag allowing or preventing approximation</param>
        /// <param name="allowReadonlyDefaultMembers">allows properties with no backing to be read as the default value for that type</param>
        public ShimSham(object[] toWrap, Type interfaceToMimic, bool isFuzzy, bool allowReadonlyDefaultMembers)
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
        public ShimSham(object toWrap, Type interfaceToMimic, bool isFuzzy, bool allowReadonlyDefaultMembers)
            : this(new[] {toWrap}, interfaceToMimic, isFuzzy, allowReadonlyDefaultMembers)
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
            IPropertyInfoFetcher propertyInfoFetcher)
        {
            if (interfaceToMimic == null)
                throw new ArgumentNullException(nameof(interfaceToMimic));
            _propertyInfoFetcher = propertyInfoFetcher ?? throw new ArgumentNullException(nameof(propertyInfoFetcher));
            _isFuzzy = isFuzzy;
            _wrapped = toWrap;
            _allowReadonlyDefaultsForMissingMembers = allowReadonlyDefaultsForMissingMembers;
            _wrappedTypes = toWrap.Select(w => w.GetType()).ToArray();
            _wrappingADuck = IsObjectADuck();
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
                    .Select(i => _propertyInfoFetcher
                        .GetProperties(i, bindingFlags))
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
                        return;
                    MethodInfos[wrappedType] = new MethodInfoContainer(
                        wrappedType
                            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                            // TODO: handle method overloads, which this won't
                            .Distinct(new MethodInfoComparer())
                            .ToArray()
                    );
                }
            }
        }

        /// <inheritdoc />
        public object GetPropertyValue(string propertyName)
        {
            if (_wrappingADuck)
            {
                return FieldValueFor(propertyName);
            }
            if (_shimmedProperties.TryGetValue(propertyName, out var shimmed))
                return shimmed;
            var foundPropInfo = FindPropertyInfoFor(propertyName);
            if (foundPropInfo == null)
            {
                return GetDefaultValueFor(_localMimicPropertyInfos[propertyName].PropertyType);
            }
            var propInfo = foundPropInfo.Value;

            if (!propInfo.PropertyInfo.CanRead || propInfo.Getter == null)
            {
                // TODO: throw for the correct wrapped type for this property
                throw new WriteOnlyPropertyException(_wrappedTypes[0], propertyName);
            }
            return DuckIfRequired(propInfo, propertyName);
        }

        private object DuckIfRequired(PropertyInfoCacheItem propertyInfoCacheItem, string propertyName)
        {
            if (_shimmedProperties.TryGetValue(propertyName, out var existingShim))
                return existingShim;
            var getter = propertyInfoCacheItem.Getter;
            var propValue = getter();

            var correctType = _localMimicPropertyInfos[propertyName].PropertyType;
            if (propValue == null)
            {
                return GetDefaultValueFor(correctType);
            }
            var propValueType = propertyInfoCacheItem.PropertyInfo.PropertyType;
            if (correctType.IsAssignableFrom(propValueType))
                return propValue;
            var converter = ConverterLocator.GetConverter(propValueType, correctType);
            if (converter != null)
                return ConvertWith(converter, propValue, correctType);
            if (EnumConverter.TryConvert(propValueType, correctType, propValue, out var result))
                return result;
            if (correctType.ShouldTreatAsPrimitive())
                return GetDefaultValueFor(correctType);
            if (CannotShim(propertyName, propValue, correctType))
                return null;
            var duckType = MakeTypeToImplement(correctType, _isFuzzy);
            var asDict = propValue.TryConvertToDictionary();
            var instance = Activator.CreateInstance(duckType,
                asDict == null
                    // ReSharper disable once RedundantExplicitArrayCreation
                    ? new object[] {new object[] {propValue}}
                    : new object[] {asDict});
            _shimmedProperties[propertyName] = instance;
            return instance;
        }

        private bool CannotShim(string propertyName, object propValue, Type targetType)
        {
            if (_unshimmableProperties.Contains(propertyName))
                return true;
            var result = !propValue.InternalCanDuckAs(targetType, _isFuzzy, false);
            if (result)
                _unshimmableProperties.Add(propertyName);
            return result;
        }

        /// <inheritdoc />
        public void SetPropertyValue(string propertyName, object newValue)
        {
            if (_wrappingADuck)
            {
                SetFieldValue(propertyName, newValue);
                return;
            }

            var foundPropInfo = FindPropertyInfoFor(propertyName);
            if (foundPropInfo == null)
            {
                throw new PropertyNotFoundException(_wrappedTypes[0], propertyName);
            }
            
            var propInfo = foundPropInfo.Value;

            if (!propInfo.PropertyInfo.CanWrite || propInfo.Setter == null)
            {
                // TODO: throw for correct wrapped type for this particular property
                throw new ReadOnlyPropertyException(_wrappedTypes[0], propertyName);
            }
            var mimickedPropInfo = _localMimicPropertyInfos[propertyName];
            var newValueType = newValue?.GetType();
            var mimickedType = mimickedPropInfo.PropertyType;
            if (newValueType == null)
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
            var instance = Activator.CreateInstance(duckType, new object[] {new[] {newValue}});
            _shimmedProperties[propertyName] = instance;
        }


        /// <inheritdoc />
        public void CallThroughVoid(string methodName, params object[] parameters)
        {
            CallThrough(methodName, parameters);
        }

        /// <inheritdoc />
        public object CallThrough(string methodName, object[] parameters)
        {
            if (_wrappingADuck)
            {
                throw new NotImplementedException("Cannot call-through when there is no wrapped object");
            }
            // TODO: throw for correct wrapped type
            var wrappedType = _wrappedTypes[0];
            if (!_localMethodInfos.TryGetValue(methodName, out var methodInfo))
                throw new MethodNotFoundException(wrappedType, methodName);
            if (_isFuzzy)
                parameters = AttemptToOrderCorrectly(parameters, methodInfo);
            // FIXME: find the correct wrapped object to invoke on
            var result = methodInfo.Invoke(_wrapped[0], parameters);
            return result;
        }

        private object[] AttemptToOrderCorrectly(
            object[] parameters,
            MethodInfo methodInfo)
        {
            var methodParameters = methodInfo.GetParameters();
            if (parameters.Length != methodParameters.Length)
                throw new ParameterCountMismatchException(parameters.Length, methodInfo);
            var srcTypes = parameters.Select(o => o?.GetType()).ToArray();
            var dstTypes = methodParameters.Select(p => p.ParameterType).ToArray();
            if (AlreadyInCorrectOrderByType(srcTypes, dstTypes))
                return
                    parameters; // no need to change anything here and we don't have to care about parameters with the same type
            if (dstTypes.Distinct().Count() != dstTypes.Length)
                throw new UnresolveableParameterOrderMismatchException(dstTypes, methodInfo);
            return Reorder(parameters, dstTypes);
        }

        private object[] Reorder(object[] parameters, Type[] dstTypes)
        {
            return dstTypes
                .Select(type => FindBestMatchFor(type, parameters))
                .ToArray();
        }

        private static object FindBestMatchFor(Type type, object[] parameters)
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
                    return false;
                if (src != dst)
                    return false;
            }
            return true;
        }

        private void LocallyCachePropertyInfos()
        {
            // TODO: cache all wrapped type property infos
            var wrappedType = _wrappedTypes[0];
            _localPropertyInfos = _isFuzzy
                ? PropertyInfos[wrappedType].FuzzyPropertyInfos
                : PropertyInfos[wrappedType].PropertyInfos;
            if (_wrappingADuck)
                _localFieldInfos = FieldInfos[wrappedType];
        }

        private void LocallyCacheMimickedPropertyInfos()
        {
            _localMimicPropertyInfos = _isFuzzy
                ? _mimickedPropInfos.FuzzyPropertyInfos
                : _mimickedPropInfos.PropertyInfos;
        }

        private void LocallyCacheMethodInfos()
        {
            // TODO: get all cached method infos for all types
            var wrappedType = _wrappedTypes[0];
            _localMethodInfos = _isFuzzy
                ? MethodInfos[wrappedType].FuzzyMethodInfos
                : MethodInfos[wrappedType].MethodInfos;
        }

        private void StaticallyCachePropertyInfosFor(object[] cacheAll, bool cacheFieldInfosToo)
        {
            lock (PropertyInfos)
            {
                // TODO: cache for all objects
                var toCacheFor = cacheAll[0];
                var type = toCacheFor.GetType();
                if (PropertyInfos.ContainsKey(type))
                    return;
                PropertyInfos[type] = new PropertyInfoContainer(
                    _propertyInfoFetcher
                        .GetPropertiesFor(toCacheFor,
                            BindingFlags.Instance | BindingFlags.Public));
                if (!cacheFieldInfosToo)
                    return;
                FieldInfos[type] = type
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                    .ToDictionary(
                        fi => fi.Name,
                        fi => fi
                    );
            }
        }

        private bool IsObjectADuck()
        {
            return _wrappedTypes.Any(t => t.GetCustomAttributes(true).OfType<IsADuckAttribute>().Any());
        }

        private readonly ListDictionary _propertyInfoLookupCache = new ListDictionary();

        private struct PropertyInfoCacheItem
        {
            public PropertyInfo PropertyInfo;
            public Action<object> Setter;
            public Func<object> Getter;
        }
        
        private PropertyInfoCacheItem? FindPropertyInfoFor(string propertyName)
        {
            PropertyInfoCacheItem cacheItem;
            if (_propertyInfoLookupCache.Contains(propertyName))
                return (PropertyInfoCacheItem) _propertyInfoLookupCache[propertyName];
            if (_shimmedProperties.TryGetValue(propertyName, out _))
            {
                cacheItem = CreateCacheItemFor(_localMimicPropertyInfos[propertyName]);
                _propertyInfoLookupCache[propertyName] = cacheItem;
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

            cacheItem = CreateCacheItemFor(pi);
            _propertyInfoLookupCache[propertyName] = cacheItem;
            return cacheItem;
        }

        private PropertyInfoCacheItem CreateCacheItemFor(PropertyInfo propertyInfo)
        {
            return new PropertyInfoCacheItem
            {
                PropertyInfo = propertyInfo,
                Setter = CreateSetterExpressionFor(propertyInfo),
                Getter = CreateGetterExpressionFor(propertyInfo)
            };
        }

        private void SetFieldValue(string propertyName, object newValue)
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
                throw new BackingFieldForPropertyNotFoundException(_wrappedTypes[0], propertyName);
            return fieldInfo;
        }


        // borrowed from http://stackoverflow.com/questions/17660097/is-it-possible-to-speed-this-method-up/17669142#17669142
        //  -> use compiled lambdas for property get / set which provides a performance
        //      boost in that the runtime doesn't have to perform as much checking

        private Action<object> CreateSetterExpressionFor(PropertyInfo propertyInfo)
        {
            var methodInfo = propertyInfo.GetSetMethod();
            if (methodInfo == null)
                return null;
            var exValue = Expression.Parameter(typeof(object), "p");
            // FIXME: find the correct wrapped object to target
            var exTarget = Expression.Constant(_wrapped[0]);
            var exBody = Expression.Call(exTarget, methodInfo,
                Expression.Convert(exValue, propertyInfo.PropertyType));

            var lambda = Expression.Lambda<Action<object>>(exBody, exValue);
            var action = lambda.Compile();
            return action;
        }

        private void TrySetValue(object newValue, Type newValueType, PropertyInfoCacheItem propInfo)
        {
            var pType = propInfo.PropertyInfo.PropertyType;
            var setter = propInfo.Setter;
            if (pType.IsAssignableFrom(newValueType))
            {
                setter(newValue);
                return;
            }
            var converter = ConverterLocator.GetConverter(newValueType, pType);
            if (converter == null)
                throw new InvalidOperationException(
                    $"Unable to set property: no converter for {newValueType.Name} => {pType.Name}");
            var converted = ConvertWith(converter, newValue, pType);
            setter(converted);
        }

        private Func<object> CreateGetterExpressionFor(PropertyInfo propertyInfo)
        {
            var methodInfo = propertyInfo.GetGetMethod();
            if (methodInfo == null)
                return null;

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