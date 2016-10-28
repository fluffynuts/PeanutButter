using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping
{
    public class ShimSham
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsFuzzy { get; }
        private readonly object _wrapped;
        private readonly Type _wrappedType;
        private readonly bool _wrappingADuck;

        private static readonly Dictionary<Type, PropertyInfoContainer> _propertyInfos = new Dictionary<Type, PropertyInfoContainer>();
        private static readonly Dictionary<Type, MethodInfoContainer> _methodInfos = new Dictionary<Type, MethodInfoContainer>();
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfos = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private static readonly MethodInfo _getDefaultMethodGeneric = typeof(ShimSham).GetMethod("GetDefaultFor", BindingFlags.NonPublic | BindingFlags.Static);

        private Dictionary<string, PropertyInfo> _localPropertyInfos;
        private Dictionary<string, FieldInfo> _localFieldInfos;
        private Dictionary<string, MethodInfo> _localMethodInfos;
        private readonly Dictionary<string, object> _shimmedProperties = new Dictionary<string, object>();
        private readonly HashSet<string> _unshimmableProperties = new HashSet<string>();
        private TypeMaker _typeMaker;
        private readonly MethodInfo _genericMakeType = typeof(TypeMaker).GetMethod("MakeTypeImplementing");
        private readonly MethodInfo _genericFuzzyMakeType = typeof(TypeMaker).GetMethod("MakeFuzzyTypeImplementing");
        private PropertyInfoContainer _mimickedPropInfos;
        private Dictionary<string, PropertyInfo> _localMimickPropertyInfos;

        // ReSharper disable once MemberCanBePrivate.Global
        public ShimSham(object toWrap, Type interfaceToMimick, bool isFuzzy)
        {
            if (interfaceToMimick == null) throw new ArgumentNullException(nameof(interfaceToMimick));
            IsFuzzy = isFuzzy;
            _wrapped = toWrap;
            _wrappedType = toWrap.GetType();
            _wrappingADuck = IsObjectADuck();
            StaticallyCachePropertyInfosFor(_wrappedType, _wrappingADuck);
            StaticallyCachePropertInfosFor(interfaceToMimick);
            StaticallyCacheMethodInfosFor(_wrappedType);
            LocallyCachePropertyInfos();
            LocallyCacheMethodInfos();
            LocallyCacheMimickedPropertyInfos();
        }

        private void StaticallyCachePropertInfosFor(Type interfaceToMimick)
        {
            _mimickedPropInfos = new PropertyInfoContainer(
                interfaceToMimick.GetAllImplementedInterfaces()
                    .Select(i => i.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public))
                    .SelectMany(c => c)
                    .ToArray()
            );
        }

        private void StaticallyCacheMethodInfosFor(Type wrappedType)
        {
            lock (_methodInfos)
            {
                if (_methodInfos.ContainsKey(wrappedType))
                    return;
                _methodInfos[wrappedType] = new MethodInfoContainer(
                    wrappedType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                );
            }
        }

        // ReSharper disable once UnusedMember.Global

        public object GetPropertyValue(string propertyName)
        {
            if (_wrappingADuck)
            {
                return FieldValueFor(propertyName);
            }
            object shimmed;
            if (_shimmedProperties.TryGetValue(propertyName, out shimmed))
                return shimmed;
            var propInfo = FindPropertyInfoFor(propertyName);
            if (!propInfo.CanRead)
            {
                throw new WriteOnlyPropertyException(_wrappedType, propertyName);
            }
            return DuckIfRequired(propInfo, propertyName);
        }

        private object DuckIfRequired(PropertyInfo wrappedPropertyInfo, string propertyName)
        {
            object existingShim;
            if (_shimmedProperties.TryGetValue(propertyName, out existingShim))
                return existingShim;

            var propValue = wrappedPropertyInfo.GetValue(_wrapped);

            var correctType = _localMimickPropertyInfos[propertyName].PropertyType;
            if (propValue == null)
            {
                return GetDefaultValueFor(correctType);
            }
            var propValueType = propValue.GetType();
            if (correctType.IsAssignableFrom(propValueType))
                return propValue;
            var converter = ConverterLocator.GetConverter(propValueType, correctType);
            if (converter != null)
                return ConvertWith(converter, propValue, correctType);
            if (correctType.ShouldTreatAsPrimitive())
                return GetDefaultValueFor(correctType);
            if (CannotShim(propertyName, propValueType, correctType))
                return null;
            var duckType = MakeTypeToImplement(correctType);
            var instance = Activator.CreateInstance(duckType, new[] { propValue });
            _shimmedProperties[propertyName] = instance;
            return instance;
        }

        private object ConvertWith(
            IConverter converter, 
            object propValue, 
            Type toType)
        {
            var convertMethod = converter.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                    .Single(mi => mi.Name == "Convert" && mi.ReturnType == toType);
            return convertMethod.Invoke(converter, new object[] { propValue });
        }

        private bool CannotShim(string propertyName, Type srcType, Type targetType)
        {
            if (_unshimmableProperties.Contains(propertyName))
                return true;
            var result = !srcType.CanDuckAs(targetType, IsFuzzy);
            if (result)
                _unshimmableProperties.Add(propertyName);
            return result;
        }

        private Type MakeTypeToImplement(Type type)
        {
            var typeMaker = (_typeMaker ?? (_typeMaker = new TypeMaker()));
            var genericMethod = IsFuzzy ? _genericFuzzyMakeType : _genericMakeType;
            var specific = genericMethod.MakeGenericMethod(type);
            return specific.Invoke(typeMaker, null) as Type;
        }

        private static object GetDefaultValueFor(Type correctType)
        {
            return _getDefaultMethodGeneric
                                .MakeGenericMethod(correctType)
                                .Invoke(null, null);

        }


        // ReSharper disable once UnusedMember.Local
#pragma warning disable S1144 // Unused private types or members should be removed
        private static T GetDefaultFor<T>()
        {
            return default(T);
        }
#pragma warning restore S1144 // Unused private types or members should be removed

        // ReSharper disable once UnusedMember.Global
        public void SetPropertyValue(string propertyName, object newValue)
        {

            if (_wrappingADuck)
            {
                SetFieldValue(propertyName, newValue);
                return;
            }

            var propInfo = FindPropertyInfoFor(propertyName);
            if (!propInfo.CanWrite)
            {
                throw new ReadOnlyPropertyException(_wrappedType, propertyName);
            }
            var mimickedPropInfo = _localMimickPropertyInfos[propertyName];
            var newValueType = newValue?.GetType();
            if (newValueType == null)
            {
                propInfo.SetValue(_wrapped, GetDefaultValueFor(mimickedPropInfo.PropertyType));
                return;
            }
            if (mimickedPropInfo.PropertyType.IsAssignableFrom(newValueType))
            {
                TrySetValue(newValue, newValueType, propInfo);
                return;
            }
            var duckType = MakeTypeToImplement(mimickedPropInfo.PropertyType);
            var instance = Activator.CreateInstance(duckType, new[] { newValue });
            _shimmedProperties[propertyName] = instance;
        }

        private void TrySetValue(object newValue, Type newValueType, PropertyInfo propInfo)
        {
            if (propInfo.PropertyType.IsAssignableFrom(newValueType))
            {
                propInfo.SetValue(_wrapped, newValue);
                return;
            }
            var converter = ConverterLocator.GetConverter(newValueType, propInfo.PropertyType);
            if (converter == null)
                throw new InvalidOperationException($"Unable to set property: no converter for {newValueType.Name} => {propInfo.PropertyType.Name}");
            var converted = ConvertWith(converter, newValue, propInfo.PropertyType);
            propInfo.SetValue(_wrapped, converted);
        }

        // ReSharper disable once UnusedMember.Global
        public void CallThroughVoid(string methodName, params object[] parameters)
        {
            CallThrough(methodName, parameters);
        }

        // ReSharper disable once UnusedMethodReturnValue.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public object CallThrough(string methodName, object[] parameters)
        {
            if (_wrappingADuck)
            {
                throw new NotImplementedException("Cannot call-through when there is no wrapped object");
            }
            MethodInfo methodInfo;
            if (!_localMethodInfos.TryGetValue(methodName, out methodInfo))
                throw new MethodNotFoundException(_wrappedType, methodName);
            if (IsFuzzy)
                parameters = AttemptToOrderCorrectly(parameters, methodInfo);
            var result = methodInfo.Invoke(_wrapped, parameters);
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
                return parameters;  // no need to change anything here and we don't have to care about parameters with the same type
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

        private object FindBestMatchFor(Type type, object[] parameters)
        {
            return parameters.FirstOrDefault(p => p.GetType() == type);
        }

        private bool AlreadyInCorrectOrderByType(
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
            _localPropertyInfos = IsFuzzy
                                    ? _propertyInfos[_wrappedType].FuzzyPropertyInfos
                                    : _propertyInfos[_wrappedType].PropertyInfos;
            if (_wrappingADuck)
                _localFieldInfos = _fieldInfos[_wrappedType];
        }

        public void LocallyCacheMimickedPropertyInfos()
        {
            _localMimickPropertyInfos = IsFuzzy
                                       ? _mimickedPropInfos.FuzzyPropertyInfos
                                       : _mimickedPropInfos.PropertyInfos;
        }

        private void LocallyCacheMethodInfos()
        {
            _localMethodInfos = IsFuzzy
                                ? _methodInfos[_wrappedType].FuzzyMethodInfos
                                : _methodInfos[_wrappedType].MethodInfos;
        }

        private static void StaticallyCachePropertyInfosFor(Type toCacheFor, bool cacheFieldInfosToo)
        {
            lock (_propertyInfos)
            {
                if (_propertyInfos.ContainsKey(toCacheFor))
                    return;
                _propertyInfos[toCacheFor] = new PropertyInfoContainer(toCacheFor
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public));
                if (!cacheFieldInfosToo)
                    return;
                _fieldInfos[toCacheFor] = toCacheFor
                                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                                .ToDictionary(
                                    fi => fi.Name,
                                    fi => fi
                                );
            }
        }

        private bool IsObjectADuck()
        {
            return _wrappedType.GetCustomAttributes(true).OfType<IsADuckAttribute>().Any();
        }

        private PropertyInfo FindPropertyInfoFor(string propertyName)
        {
            object shimmed;
            if (_shimmedProperties.TryGetValue(propertyName, out shimmed))
            {
                return _localMimickPropertyInfos[propertyName];
            }
            PropertyInfo propInfo;
            if (!_localPropertyInfos.TryGetValue(propertyName, out propInfo))
                throw new PropertyNotFoundException(_wrappedType, propertyName);
            return propInfo;
        }

        private void SetFieldValue(string propertyName, object newValue)
        {
            var fieldInfo = FindPrivateBackingFieldFor(propertyName);
            fieldInfo.SetValue(_wrapped, newValue);
        }

        private object FieldValueFor(string propertyName)
        {
            var fieldInfo = FindPrivateBackingFieldFor(propertyName);
            return fieldInfo.GetValue(_wrapped);
        }

        private FieldInfo FindPrivateBackingFieldFor(string propertyName)
        {
            var seek = "_" + propertyName;
            FieldInfo fieldInfo;
            if (!_localFieldInfos.TryGetValue(seek, out fieldInfo))
                throw new BackingFieldForPropertyNotFoundException(_wrappedType, propertyName);
            return fieldInfo;
        }
    }

}