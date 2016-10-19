using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    public class ShimSham
    {
        private readonly object _wrapped;
        private readonly Type _wrappedType;
        private readonly bool _wrappingADuck;

        private static Dictionary<Type, Dictionary<string, PropertyInfo>> _propertInfos = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private static Dictionary<Type, Dictionary<string, FieldInfo>> _fieldInfos = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private Dictionary<string, PropertyInfo> _localPropertyInfos;
        private Dictionary<string, FieldInfo> _localFieldInfos;

        public ShimSham(object toWrap)
        {
            _wrapped = toWrap;
            _wrappedType = toWrap.GetType();
            _wrappingADuck = IsObjectADuck();
            StaticallyCachePropertyInfosFor(_wrappedType, _wrappingADuck);
            LocallyCachePropertyInfos();
        }

        // ReSharper disable once UnusedMember.Global

        public object GetPropertyValue(string propertyName)
        {
            if (_wrappingADuck)
            {
                return FieldValueFor(propertyName);
            }

            var propInfo = FindPropertyInfoFor(propertyName);
            if (!propInfo.CanRead)
            {
                throw new WriteOnlyPropertyException(_wrappedType, propertyName);
            }
            return propInfo.GetValue(_wrapped);
        }

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
            propInfo.SetValue(_wrapped, newValue);
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
            var methodInfo = _wrappedType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                                .FirstOrDefault(mi => mi.Name == methodName);
            if (methodInfo == null)
                throw new MethodNotFoundException(_wrappedType, methodName);
            var result =  methodInfo.Invoke(_wrapped, parameters);
            return result;
        }

        private void LocallyCachePropertyInfos()
        {
            _localPropertyInfos = _propertInfos[_wrappedType];
            if (_wrappingADuck)
                _localFieldInfos = _fieldInfos[_wrappedType];
        }

        private static void StaticallyCachePropertyInfosFor(Type toCacheFor, bool cacheFieldInfosToo)
        {
            lock (_propertInfos)
            {
                if (_propertInfos.ContainsKey(toCacheFor))
                    return;
                _propertInfos[toCacheFor] = toCacheFor
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                    .ToDictionary(
                                        pi => pi.Name,
                                        pi => pi
                                    );
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