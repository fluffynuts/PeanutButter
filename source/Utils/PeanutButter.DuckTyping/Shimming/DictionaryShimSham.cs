using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Imported.PeanutButter.Utils;
using Imported.PeanutButter.Utils.Dictionaries;
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
    /// Provides the required shimming to duck a dictionary object to an interface
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    public
#else
    public
#endif
    class DictionaryShimSham : ShimShamBase, IShimSham
    {
        private readonly Type _typeToMimic;
        private readonly IDictionary<string, object> _data;
        private readonly Dictionary<string, PropertyInfo> _mimickedProperties;
        private readonly bool _isFuzzy;


        /// <summary>
        /// Constructs an instance of the DictionaryShimSham
        /// </summary>
        /// <param name="toWrap">Dictionary to wrap</param>
        /// <param name="typeToMimic">Interface that must be mimicked</param>
        // ReSharper disable once UnusedMember.Global
        public DictionaryShimSham(
            IDictionary<string, object> toWrap,
            Type typeToMimic)
            : this(new[] { toWrap }, typeToMimic)
        {
        }

        /// <summary>
        /// Constructs an instance of the DictionaryShimSham
        /// </summary>
        /// <param name="toWrap">Dictionaries to wrap (wip: only the first is considered)</param>
        /// <param name="typeToMimic">Interface that must be mimicked</param>
        public DictionaryShimSham(
            IDictionary<string, object>[] toWrap,
            Type typeToMimic)
        {
            _typeToMimic = typeToMimic;
            var incoming = (toWrap?.ToArray() ?? new Dictionary<string, object>[0])
                .Where(d => d != null)
                .ToArray();

            _data = incoming.Length == 0
                ? new Dictionary<string, object>()
                : incoming.Length == 1
                    ? incoming[0]
                    : new MergeDictionary<string, object>(incoming);
            _isFuzzy = IsFuzzy(_data);
            _mimickedProperties = typeToMimic
                .GetAllImplementedInterfaces()
                .And(typeToMimic)
                .SelectMany(interfaceType => interfaceType.GetProperties())
                .Distinct(new PropertyInfoComparer())
                .ToDictionary(pi => pi.Name, pi => pi,
                    _isFuzzy
                        ? Comparers.Comparers.FuzzyComparer
                        : Comparers.Comparers.NonFuzzyComparer);
            ShimShimmableProperties();
        }

        private readonly Dictionary<string, object> _shimmedProperties = new Dictionary<string, object>();

        private void ShimShimmableProperties()
        {
            foreach (var kvp in _mimickedProperties)
            {
                if (kvp.Value.PropertyType.ShouldTreatAsPrimitive())
                {
                    continue;
                }

                if (!_data.ContainsKey(kvp.Key))
                {
                    _data[kvp.Key] = new Dictionary<string, object>();
                }

                var type = MakeTypeToImplement(kvp.Value.PropertyType, _isFuzzy);
                var toWrap = _data[kvp.Key];
                var asDict = toWrap as IDictionary<string, object>;
                // ReSharper disable RedundantExplicitArrayCreation
                var firstArg = asDict == null
                    ? new object[] { new object[] { toWrap } }
                    : new object[] { new IDictionary<string, object>[] { asDict } };
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
            CheckPropertyExists(propertyName);
            if (_shimmedProperties.TryGetValue(propertyName, out var propValue))
            {
                return propValue;
            }

            var mimickedProperty = GetMimickedProperty(propertyName);
            var key = _isFuzzy
                ? FuzzyFindKeyFor(propertyName) ?? propertyName
                : propertyName;

            if (!_data.TryGetValue(key, out propValue))
            {
                return GetDefaultValueFor(mimickedProperty.PropertyType);
            }

            if (propValue is null)
            {
                return TryResolveNullValueFOr(mimickedProperty);
            }


            // ReSharper disable once UseMethodIsInstanceOfType
            var propType = propValue.GetType();
            if (mimickedProperty.PropertyType.IsAssignableFrom(propType))
            {
                return propValue;
            }

            var converter = ConverterLocator.TryFindConverter(propType, mimickedProperty.PropertyType);
            if (converter != null)
            {
                return ConvertWith(converter, propValue, mimickedProperty.PropertyType);
            }

            return EnumConverter.TryConvert(propType, mimickedProperty.PropertyType, propValue, out var result)
                ? result
                : GetDefaultValueFor(mimickedProperty.PropertyType);
        }

        private object TryResolveNullValueFOr(PropertyInfo mimickedProperty)
        {
            if (mimickedProperty.PropertyType.IsNullableType())
            {
                return null;
            }

            if (_isFuzzy)
            {
                return GetDefaultValueFor(mimickedProperty.PropertyType);
            }

            throw new InvalidOperationException(
                $"Somehow a strict duck has been constructed around a non-nullable property with null backing value at {mimickedProperty.Name}"
            );
        }

        private readonly Dictionary<string, string> _keyResolutionCache = new Dictionary<string, string>();

        private string FuzzyFindKeyFor(string propertyName)
        {
            lock (_keyResolutionCache)
            {
                if (_keyResolutionCache.TryGetValue(propertyName, out var resolvedKey))
                {
                    return resolvedKey;
                }

                resolvedKey = _data.FuzzyFindKeyFor(propertyName);
                if (resolvedKey != null)
                {
                    _keyResolutionCache[propertyName] = resolvedKey;
                }

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
            CheckPropertyExists(propertyName);
            var mimickedProperty = GetMimickedProperty(propertyName);
            var newValueType = newValue?.GetType();
            if (newValueType == null)
            {
                SetDefaultValueForType(_data, propertyName, mimickedProperty);
            }
            else if (mimickedProperty.PropertyType.IsAssignableFrom(newValueType))
            {
                _data[propertyName] = newValue;
            }
            else
            {
                var converter = ConverterLocator.TryFindConverter(
                    newValueType,
                    mimickedProperty.PropertyType
                );
                if (converter is null)
                {
                    SetDefaultValueForType(
                        _data,
                        propertyName,
                        mimickedProperty
                    );
                    return;
                }

                _data[propertyName] = ConvertWith(
                    converter,
                    newValue,
                    mimickedProperty.PropertyType
                );
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

        private bool IsFuzzy(IDictionary<string, object> data)
        {
            var current = data;
            var keys = current.Keys;
            var first = keys.FirstOrDefault(k => k.ToLower() != k.ToUpper());
            if (first == null)
            {
                return true;
            }

            var lower = first.ToLower();
            var upper = first.ToUpper();
            return current.TryGetValue(lower, out _) &&
                data.TryGetValue(upper, out _);
        }

        private PropertyInfo GetMimickedProperty(string propertyName)
        {
            return _mimickedProperties.TryGetValue(propertyName, out var result)
                ? result
                : throw new PropertyNotFoundException(_typeToMimic, propertyName);
        }

        private void CheckPropertyExists(string propertyName)
        {
            if (!_mimickedProperties.ContainsKey(propertyName))
            {
                throw new PropertyNotFoundException(_data.GetType(), propertyName);
            }
        }

        /// <summary>
        /// Required to implement the IShimSham interface, but not implemented for
        /// dictionaries as the concept doesn't make sense
        /// </summary>
        /// <param name="methodName">Name of the method to not call through to</param>
        /// <param name="arguments">Parameters to ignore</param>
        /// <exception cref="NotImplementedException">Exception which is always thrown</exception>
        public void CallThroughVoid(string methodName, params object[] arguments)
        {
            if (!_data.TryGetValue(methodName, out var func) ||
                func is null ||
                !VoidFunctionAccepts(
                    func,
                    arguments,
                    out var parameterTypes
                )
            )
            {
                // TODO: this should be caught at duck-time
                throw new MethodNotFoundException(_typeToMimic, methodName);
            }

            var preparedArguments = PrepareArguments(arguments, parameterTypes);
            InvokeNonVoidFunc(func, preparedArguments, null);
        }

        /// <summary>
        /// Required to implement the IShimSham interface, but not implemented for
        /// dictionaries as the concept doesn't make sense
        /// </summary>
        /// <param name="methodName">Name of the method to not call through to</param>
        /// <param name="arguments">Parameters to ignore</param>
        /// <exception cref="NotImplementedException">Exception which is always thrown</exception>
        public object CallThrough(string methodName, object[] arguments)
        {
            if (!_data.TryGetValue(methodName, out var func) ||
                func is null ||
                !NonVoidFunctionAccepts(
                    func,
                    arguments,
                    out var parameterTypes,
                    out _
                )
            )
            {
                // TODO: this should be caught at duck-time
                throw new NotImplementedException(
                    func is null
                    ? $"{_typeToMimic.Name}.{methodName} not implemented in underlying dictionary"
                    : $"{_typeToMimic.Name}.{methodName} not fully in underlying dictionary: arguments must be in order and args and return type must either have identical type or be easily convertable"
                );
            }

            var preparedArguments = PrepareArguments(arguments, parameterTypes);
            var argumentTypes = arguments.Select(a => a?.GetType()).ToArray();
            var returnType = DetermineReturnTypeFor(methodName, argumentTypes);
            return InvokeNonVoidFunc(func, preparedArguments, returnType);
        }

        private Type DetermineReturnTypeFor(
            string methodName,
            Type[] argumentTypes
        )
        {
            // FIXME: should be able to fuzzy-duck to method
            // with unique types in different order
            var method = _typeToMimic.GetMethods()
                .Where(mi => mi.Name == methodName)
                .FirstOrDefault(mi =>
                {
                    var parameterTypes = mi.GetParameters().Select(p => p.ParameterType).ToArray();
                    if (parameterTypes.Length != argumentTypes.Length)
                    {
                        return false;
                    }

                    var idx = -1;
                    foreach (var pt in parameterTypes)
                    {
                        idx++;
                        var argType = argumentTypes[idx];
                        if (pt == argType)
                        {
                            continue;
                        }

                        if (argType.IsAssignableOrUpCastableTo(pt))
                        {
                            continue;
                        }

                        if (argType is null && pt.IsNullableType())
                        {
                            continue;
                        }

                        return false;
                    }

                    return true;
                });
            return method?.ReturnType
                ?? throw new InvalidOperationException(
                    $"Can't determine return type for {methodName} with provided parameters"
                );
        }

        private object[] PrepareArguments(
            object[] arguments,
            Type[] parameterTypes)
        {
            return arguments.Select((arg, idx) =>
            {
                var argType = arg?.GetType();
                if (arg is null ||
                    arg.GetType() == parameterTypes[idx])
                {
                    return arg;
                }

                if (parameterTypes[idx].IsAssignableFrom(argType))
                {
                    return arg;
                }

                var converter = ConverterLocator.TryFindConverter(argType, parameterTypes[idx]);
                return converter.Convert(arg);
            }).ToArray();
        }

        private object InvokeNonVoidFunc(
            object func,
            object[] arguments,
            Type returnType
        )
        {
            var invokeMethod = func.GetType().GetMethod(nameof(Func<int>.Invoke));
            var result = invokeMethod?.Invoke(func, arguments);
            if (returnType is null)
            {
                return null;
            }

            if (result is null)
            {
                return returnType.IsNullableType()
                    ? null
                    : returnType.DefaultValue();
            }

            var resultType = result.GetType();
            if (resultType == returnType)
            {
                return result;
            }

            var converter = ConverterLocator.TryFindConverter(resultType, returnType);
            return converter?.Convert(result)
                ?? throw new InvalidOperationException(
                    $"Can't convert result from {resultType} to {returnType}"
                );
        }

        // TODO: should be moved out into a shared location & used at duck-type
        // to prevent erroneous ducks
        private bool NonVoidFunctionAccepts(
            object value,
            object[] arguments,
            out Type[] parameterTypes,
            out Type returnType
        )
        {
            parameterTypes = null;
            returnType = null;
            var funcType = value.GetType();
            if (!funcType.IsGenericType)
            {
                return false;
            }

            var parameterCount = Array.IndexOf(FuncGenerics, funcType.GetGenericTypeDefinition());
            if (parameterCount != arguments.Length)
            {
                if (parameterCount > FuncGenerics.Length)
                {
                    throw new NotSupportedException(
                        $"methods with more than {FuncGenerics.Length} parameters are not supported");
                }

                return false;
            }

            var genericParameters = funcType.GetGenericArguments();
            parameterTypes = genericParameters.Take(genericParameters.Length - 1).ToArray();
            returnType = genericParameters.Last();
            var zipped = arguments.Zip(
                parameterTypes,
                (argument, parameterType) =>
                    new
                    {
                        argumentType = argument?.GetType(),
                        parameterType
                    }
            );
            return zipped.Aggregate(
                true,
                (acc, cur) =>
                {
                    if (!acc)
                    {
                        return false;
                    }

                    if (cur.argumentType is null &&
                        cur.parameterType.IsNullableType())
                    {
                        return true;
                    }

                    if (cur.argumentType.IsAssignableOrUpCastableTo(cur.parameterType))
                    {
                        return true;
                    }

                    return cur.argumentType == cur.parameterType ||
                        ConverterLocator.HaveConverterFor(cur.argumentType, cur.parameterType);
                });
        }

        private bool VoidFunctionAccepts(
            object value,
            object[] arguments,
            out Type[] parameterTypes
        )
        {
            parameterTypes = null;
            var actionType = value.GetType();
            if (!actionType.IsGenericType)
            {
                return false;
            }

            var parameterCount = Array.IndexOf(
                ActionGenerics,
                actionType.GetGenericTypeDefinition()
            ) + 1;
            if (parameterCount != arguments.Length)
            {
                if (parameterCount > ActionGenerics.Length)
                {
                    throw new NotSupportedException(
                        $"methods with more than {ActionGenerics.Length} parameters are not supported");
                }

                return false;
            }

            parameterTypes = actionType.GetGenericArguments();
            var zipped = arguments.Zip(
                parameterTypes,
                (argument, parameterType) =>
                    new
                    {
                        argumentType = argument?.GetType(),
                        parameterType
                    }
            );
            return zipped.Aggregate(
                true,
                (acc, cur) =>
                {
                    if (!acc)
                    {
                        return false;
                    }

                    if (cur.argumentType is null &&
                        cur.parameterType.IsNullableType())
                    {
                        return true;
                    }

                    if (cur.argumentType.IsAssignableOrUpCastableTo(cur.parameterType))
                    {
                        return true;
                    }

                    return cur.argumentType == cur.parameterType ||
                        ConverterLocator.HaveConverterFor(cur.argumentType, cur.parameterType);
                });
        }


        private static readonly Type[] FuncGenerics =
        {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>),
            typeof(Func<,,,,,>),
            typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>),
            typeof(Func<,,,,,,,,>),
            typeof(Func<,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,>)
        };

        private static readonly Type[] ActionGenerics =
        {
            typeof(Action<>),
            typeof(Action<,>),
            typeof(Action<,,>),
            typeof(Action<,,,>),
            typeof(Action<,,,,>),
            typeof(Action<,,,,,>),
            typeof(Action<,,,,,,>),
            typeof(Action<,,,,,,,>),
            typeof(Action<,,,,,,,,>),
            typeof(Action<,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,,>)
        };
    }
}