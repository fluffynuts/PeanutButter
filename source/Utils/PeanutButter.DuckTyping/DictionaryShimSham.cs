using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Provides the required shimming to duck a dictionary object to an interface
    /// </summary>
    public class DictionaryShimSham : ShimShamBase, IShimSham
    {
        private readonly Type _interfaceToMimick;
        private readonly IDictionary<string, object> _data;
        private readonly Dictionary<string, PropertyInfo> _mimickedProperties;
        private readonly bool _isFuzzy;
        

        /// <summary>
        /// Constructs an instance of the DictionaryShimSham
        /// </summary>
        /// <param name="toWrap">Dictionary to wrap</param>
        /// <param name="interfaceToMimick">Interface that must be mimicked</param>
        public DictionaryShimSham(
            IDictionary<string, object> toWrap,
            Type interfaceToMimick)
        {
            _interfaceToMimick = interfaceToMimick;
            _data = toWrap ?? new Dictionary<string, object>();
            _isFuzzy = IsFuzzy(_data);
            _mimickedProperties = interfaceToMimick
                .GetAllImplementedInterfaces()
                .SelectMany(itype => itype.GetProperties())
                .Distinct(new PropertyInfoComparer())
                .ToDictionary(pi => pi.Name, pi => pi, 
                    _isFuzzy ? Comparers.FuzzyComparer : Comparers.NonFuzzyComparer);
            ShimShimmableProperties();
        }

        private readonly Dictionary<string, object> _shimmedProperties = new Dictionary<string, object>();

        private void ShimShimmableProperties()
        {
            foreach (var kvp in _mimickedProperties)
            {
                if (kvp.Value.PropertyType.ShouldTreatAsPrimitive())
                    continue;
                if (!_data.ContainsKey(kvp.Key))
                    _data[kvp.Key] = new Dictionary<string, object>();
                var type = MakeTypeToImplement(kvp.Value.PropertyType, _isFuzzy);
                _shimmedProperties[kvp.Key] = Activator.CreateInstance(type, _data[kvp.Key]);
            }
        }

        /// <summary>
        /// Gets the value of a property by name
        /// </summary>
        /// <param name="propertyName">Name of the property to get the value of</param>
        /// <returns>The value of the property, where possible. 
        /// May return the default value for the property type when it is not found
        /// and may attempt automatic conversion when the type to represent does not
        /// match the underlying type</returns>
        public object GetPropertyValue(string propertyName)
        {
            CheckPropertyExists(propertyName);
            object propValue;
            if (_shimmedProperties.TryGetValue(propertyName, out propValue))
                return propValue;
            var mimickedProperty = GetMimickedProperty(propertyName);
            if (!_data.TryGetValue(propertyName, out propValue))
                return GetDefaultValueFor(mimickedProperty.PropertyType);
            // ReSharper disable once UseMethodIsInstanceOfType
            var propType = propValue.GetType();
            if (mimickedProperty.PropertyType.IsAssignableFrom(propType))
                return propValue;
            var converter = ConverterLocator.GetConverter(propType, mimickedProperty.PropertyType);
            if (converter != null)
                return ConvertWith(converter, propValue, mimickedProperty.PropertyType);
            return GetDefaultValueFor(mimickedProperty.PropertyType);
        }

        /// <summary>
        /// Attempts to set the value of the named property
        /// </summary>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="newValue">Value to set. The value may be converted to match the underlying type when required.</param>
        public void SetPropertyValue(string propertyName, object newValue)
        {
            CheckPropertyExists(propertyName);
            var mimickedProperty = GetMimickedProperty(propertyName);
            var newValueType = newValue?.GetType();
            if (newValueType == null)
                SetDefaultValueForType(propertyName, mimickedProperty);
            else if (mimickedProperty.PropertyType.IsAssignableFrom(newValueType))
                _data[propertyName] = newValue;
            else
            {
                var converter = ConverterLocator.GetConverter(newValueType, mimickedProperty.PropertyType);
                if (converter == null)
                    SetDefaultValueForType(propertyName, mimickedProperty);
                _data[propertyName] = ConvertWith(converter, newValue, mimickedProperty.PropertyType);
            }
        }

        private void SetDefaultValueForType(string propertyName, PropertyInfo mimickedProperty)
        {
            _data[propertyName] = GetDefaultValueFor(mimickedProperty.PropertyType);
        }

        private bool IsFuzzy(IDictionary<string, object> data)
        {
            var keys = data.Keys;
            var first = keys.FirstOrDefault(k => k.ToLower() != k.ToUpper());
            if (first == null)
                return true;
            var lower = first.ToLower();
            var upper = first.ToUpper();
            object obj;
            return data.TryGetValue(lower, out obj) && data.TryGetValue(upper, out obj);
        }

        private PropertyInfo GetMimickedProperty(string propertyName)
        {
            PropertyInfo result;
            if (!_mimickedProperties.TryGetValue(propertyName, out result))
                throw new PropertyNotFoundException(_interfaceToMimick, propertyName);
            return result;
        }

        private void CheckPropertyExists(string propertyName)
        {
            if (!_mimickedProperties.ContainsKey(propertyName))
                throw new PropertyNotFoundException(_data.GetType(), propertyName);
        }

        /// <summary>
        /// Required to implement the IShimSham interface, but not implemented for
        /// dictionaries as the concept doesn't make sense
        /// </summary>
        /// <param name="methodName">Name of the method to not call through to</param>
        /// <param name="parameters">Parameters to ignore</param>
        /// <exception cref="NotImplementedException">Exception which is always thrown</exception>
        public void CallThroughVoid(string methodName, params object[] parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Required to implement the IShimSham interface, but not implemented for
        /// dictionaries as the concept doesn't make sense
        /// </summary>
        /// <param name="methodName">Name of the method to not call through to</param>
        /// <param name="parameters">Parameters to ignore</param>
        /// <exception cref="NotImplementedException">Exception which is always thrown</exception>
        public object CallThrough(string methodName, object[] parameters)
        {
            throw new NotImplementedException();
        }
    }
}