using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.DuckTyping.AutoConversion.Converters;
using PeanutButter.DuckTyping.Comparers;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Shimming
{
    /// <summary>
    /// Provides the required shimming to duck a dictionary object to an interface
    /// </summary>
    public class DictionaryShimSham : ShimShamBase, IShimSham
    {
        private readonly Type _interfaceToMimick;
        private readonly IDictionary<string, object>[] _data;
        private readonly Dictionary<string, PropertyInfo> _mimickedProperties;
        private readonly bool _isFuzzy;


        /// <summary>
        /// Constructs an instance of the DictionaryShimSham
        /// </summary>
        /// <param name="toWrap">Dictionary to wrap</param>
        /// <param name="interfaceToMimick">Interface that must be mimicked</param>
        // ReSharper disable once UnusedMember.Global
        public DictionaryShimSham(
            IDictionary<string, object> toWrap,
            Type interfaceToMimick)
            : this(new[] {toWrap}, interfaceToMimick)
        {
        }

        /// <summary>
        /// Constructs an instance of the DictionaryShimSham
        /// </summary>
        /// <param name="toWrap">Dictionaries to wrap (wip: only the first is considered)</param>
        /// <param name="interfaceToMimick">Interface that must be mimicked</param>
        public DictionaryShimSham(
            IDictionary<string, object>[] toWrap,
            Type interfaceToMimick)
        {
            _interfaceToMimick = interfaceToMimick;
            var incoming = (toWrap?.ToArray() ?? new Dictionary<string, object>[0])
                .Where(d => d != null)
                .ToArray();

            _data = incoming.Length == 0
                ? new IDictionary<string, object>[] {new Dictionary<string, object>()}
                : incoming;
            _isFuzzy = IsFuzzy(_data);
            _mimickedProperties = interfaceToMimick
                .GetAllImplementedInterfaces()
                .SelectMany(itype => itype.GetProperties())
                .Distinct(new PropertyInfoComparer())
                .ToDictionary(pi => pi.Name, pi => pi,
                    _isFuzzy ? Comparers.Comparers.FuzzyComparer : Comparers.Comparers.NonFuzzyComparer);
            ShimShimmableProperties();
        }

        private readonly Dictionary<string, object> _shimmedProperties = new Dictionary<string, object>();

        private void ShimShimmableProperties()
        {
            // TODO: examine all dictionaries
            var data = _data[0];
            foreach (var kvp in _mimickedProperties)
            {
                if (kvp.Value.PropertyType.ShouldTreatAsPrimitive())
                    continue;
                if (!data.ContainsKey(kvp.Key))
                    data[kvp.Key] = new Dictionary<string, object>();
                var type = MakeTypeToImplement(kvp.Value.PropertyType, _isFuzzy);
                var toWrap = data[kvp.Key];
                var asDict = toWrap as IDictionary<string, object>;
                // ReSharper disable RedundantExplicitArrayCreation
                var firstArg = asDict == null
                    ? new object[] {new object[] {toWrap}}
                    : new object[] {new IDictionary<string, object>[] {asDict}};
                // ReSharper restore RedundantExplicitArrayCreation
                _shimmedProperties[kvp.Key] = Activator.CreateInstance(type, firstArg);
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
            // TODO: examine all dictionaries, or at least, the correct one
            var data = _data[0];
            CheckPropertyExists(propertyName);
            if (_shimmedProperties.TryGetValue(propertyName, out var propValue))
                return propValue;
            var mimickedProperty = GetMimickedProperty(propertyName);
            var key = _isFuzzy
                ? FuzzyFindKeyFor(propertyName) ?? propertyName
                : propertyName;

            if (!data.TryGetValue(key, out propValue))
                return GetDefaultValueFor(mimickedProperty.PropertyType);
            // ReSharper disable once UseMethodIsInstanceOfType
            var propType = propValue.GetType();
            if (mimickedProperty.PropertyType.IsAssignableFrom(propType))
                return propValue;
            var converter = ConverterLocator.GetConverter(propType, mimickedProperty.PropertyType);
            if (converter != null)
                return ConvertWith(converter, propValue, mimickedProperty.PropertyType);
            return EnumConverter.TryConvert(propType, mimickedProperty.PropertyType, propValue, out var result)
                ? result
                : GetDefaultValueFor(mimickedProperty.PropertyType);
        }

        private readonly Dictionary<string, string> _keyResolutionCache = new Dictionary<string, string>();
        private readonly FuzzyKeyFinder _fuzzyKeyFinder = new FuzzyKeyFinder();

        private string FuzzyFindKeyFor(string propertyName)
        {
            lock (_keyResolutionCache)
            {
                // TODO: find the correct dictionary to examine for this key
                var data = _data[0];
                if (_keyResolutionCache.TryGetValue(propertyName, out var resolvedKey))
                    return resolvedKey;
                resolvedKey = _fuzzyKeyFinder.FuzzyFindKeyFor(data, propertyName);
                if (resolvedKey != null)
                    _keyResolutionCache[propertyName] = resolvedKey;
                return resolvedKey;
            }
        }

        /// <summary>
        /// Attempts to set the value of the named property
        /// </summary>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="newValue">Value to set. The value may be converted to match the underlying type when required.</param>
        public void SetPropertyValue(string propertyName, object newValue)
        {
            // TODO: find the correct dictionary to work with for this propertyName
            var data = _data[0];
            CheckPropertyExists(propertyName);
            var mimickedProperty = GetMimickedProperty(propertyName);
            var newValueType = newValue?.GetType();
            if (newValueType == null)
                SetDefaultValueForType(data, propertyName, mimickedProperty);
            else if (mimickedProperty.PropertyType.IsAssignableFrom(newValueType))
                data[propertyName] = newValue;
            else
            {
                var converter = ConverterLocator.GetConverter(newValueType, mimickedProperty.PropertyType);
                if (converter == null)
                    SetDefaultValueForType(data, propertyName, mimickedProperty);
                data[propertyName] = ConvertWith(converter, newValue, mimickedProperty.PropertyType);
            }
        }

        private void SetDefaultValueForType(
            IDictionary<string, object> data,
            string propertyName,
            PropertyInfo mimickedProperty
        )
        {
            data[propertyName] = GetDefaultValueFor(mimickedProperty.PropertyType);
        }

        private bool IsFuzzy(IDictionary<string, object>[] data)
        {
            // TODO: examine all dictionaries
            var current = data[0];
            var keys = current.Keys;
            var first = keys.FirstOrDefault(k => k.ToLower() != k.ToUpper());
            if (first == null)
                return true;
            var lower = first.ToLower();
            var upper = first.ToUpper();
            return current.TryGetValue(lower, out var _) && 
                data[0].TryGetValue(upper, out _);
        }

        private PropertyInfo GetMimickedProperty(string propertyName)
        {
            if (!_mimickedProperties.TryGetValue(propertyName, out var result))
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
            // TODO: think about possibly calling a stored action / func with the given name and parameters...
            throw new NotImplementedException();
        }
    }
}