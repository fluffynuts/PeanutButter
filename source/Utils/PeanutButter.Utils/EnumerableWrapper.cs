using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Used to describe a wrapper
    /// - IsValid should flag whether or not the wrapping was successful
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface IEnumerableWrapper
    {
        /// <summary>
        /// Flag: communicates if the wrapping was successful. Unsuccessful wraps
        /// will result in empty enumerations.
        /// </summary>
        bool IsValid { get; }
    }

    /// <summary>
    /// Wraps an object which would be an acceptable enumerable in a foreach
    /// (due to .NET compile-time duck-typing) into an actual IEnumerator
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class EnumerableWrapper : IEnumerable, IEnumerableWrapper
    {
        /// <inheritdoc />
        public bool IsValid { get; }

        private readonly object _toWrap;

        private static readonly BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private static readonly Type EnumeratorType = typeof(IEnumerator);
        private static readonly PropertyInfo[] EnumeratorProps = EnumeratorType.GetProperties(PublicInstance);

        private static readonly MethodInfo[] RequiredEnumeratorMethods =
            EnumeratorType.GetMethods(PublicInstance)
                // Reset is optional!
                .Where(mi => mi.Name != nameof(IEnumerator.Reset))
                .ToArray();


        /// <summary>
        /// Construct an EnumerableWrapper around a (hopefully) enumerable object
        /// </summary>
        /// <param name="toWrap"></param>
        public EnumerableWrapper(object toWrap)
        {
            if (toWrap == null)
            {
                IsValid = false;
                return;
            }

            _toWrap = toWrap;
            var getEnumeratorMethod = toWrap.GetType()
                .GetMethod(nameof(GetEnumerator));
            IsValid = getEnumeratorMethod != null &&
                (IsEnumeratorType(getEnumeratorMethod.ReturnType) ||
                    IsEnumeratorType(getEnumeratorMethod.Invoke(toWrap, new object[0])?.GetType()));
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator()
        {
            return MakeEnumerator<object>();
        }

        /// <summary>
        /// Creates the enumerator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected IEnumerator<T> MakeEnumerator<T>()
        {
            var result = new EnumeratorWrapper<T>(_toWrap);
            return result.IsValid
                ? result
                : new EnumeratorWrapper<T>(new T[0]); // fake no results
        }

        private bool IsEnumeratorType(
            Type returnType)
        {
            return returnType != null &&
                PropsAreAtLeast(
                    EnumeratorProps,
                    returnType.GetProperties(PublicInstance)) &&
                MethodsAreAtLeast(
                    RequiredEnumeratorMethods,
                    returnType.GetMethods(PublicInstance));
        }

        private bool MethodsAreAtLeast(
            MethodInfo[] required,
            MethodInfo[] test)
        {
            return required.Select(r => test.Any(t => MethodsMatch(r, t)))
                .Aggregate(true, (acc, cur) => acc && cur);
        }

        private bool MethodsMatch(
            MethodInfo left,
            MethodInfo right)
        {
            if (left.Name != right.Name ||
                !left.ReturnType.IsAssignableFrom(right.ReturnType))
            {
                return false;
            }

            var leftParams = left.GetParameters();
            var rightParams = right.GetParameters();
            if (leftParams.Length != rightParams.Length)
            {
                return false;
            }

            return leftParams.Zip(rightParams, Tuple.Create)
                .Aggregate(
                    true,
                    (acc, cur) => acc && cur.Item1.ParameterType == cur.Item2.ParameterType
                );
        }


        private bool PropsAreAtLeast(
            PropertyInfo[] required,
            PropertyInfo[] test)
        {
            return required
                .Select(l => test.Any(r => PropsMatch(l, r)))
                .Aggregate(true, (acc, cur) => acc && cur);
        }

        private bool PropsMatch(
            PropertyInfo left,
            PropertyInfo right)
        {
            return left.Name == right.Name &&
                left.PropertyType.IsAssignableFrom(right.PropertyType);
        }
    }

    /// <summary>
    /// Provides the typed EnumerableWrapper
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class EnumerableWrapper<T> : EnumerableWrapper, IEnumerable<T>
    {
        /// <inheritdoc />
        public EnumerableWrapper(object toWrap) : base(toWrap)
        {
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return MakeEnumerator<T>();
        }
    }

    /// <summary>
    /// Wraps an object which would be an acceptable enumerator in a foreach
    /// (due to .NET compile-time duck-typing) into an actual IEnumerator
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class EnumeratorWrapper<T> : IEnumerator<T>, IEnumerableWrapper
    {
        /// <inheritdoc />
        public bool IsValid { get; private set; }

        private PropertyInfo _currentPropInfo;
        private MethodInfo _moveNextMethod;
        private MethodInfo _resetMethod;
        private readonly object _wrapped;
        private readonly MethodInfo _getEnumeratorMethod;

        private static readonly object[] NO_ARGS = new object[0];
        private static BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        /// <inheritdoc />
        public EnumeratorWrapper(object toWrap)
        {
            var wrappedType = toWrap.GetType();
            var methods = wrappedType.GetMethods(PublicInstance);

            _getEnumeratorMethod = methods.FirstOrDefault(
                mi => mi.Name == "GetEnumerator"
            );

            GrabEnumeratorReturnMembers();
            ValidateEnumeratorResult();

            if (IsValid)
            {
                _wrapped = _getEnumeratorMethod?.Invoke(toWrap, NO_ARGS);
            }
        }

        private void ValidateEnumeratorResult()
        {
            IsValid = _currentPropInfo != null &&
                _currentPropInfo.CanRead &&
                typeof(T).IsAssignableFrom(_currentPropInfo.PropertyType) &&
                _moveNextMethod != null;
        }

        private void GrabEnumeratorReturnMembers()
        {
            if (_getEnumeratorMethod == null)
            {
                return;
            }

            var enumeratorReturnTypes = new List<Type> { _getEnumeratorMethod.ReturnType };
            if (enumeratorReturnTypes[0].IsInterface)
            {
                enumeratorReturnTypes.AddRange(
                    GetAllImplementedInterfaces(
                        enumeratorReturnTypes[0]
                    )
                );
            }

            var currentProps = enumeratorReturnTypes.SelectMany(
                t => t.GetProperties().Where(pi => pi.Name == nameof(Current))
            ).ToArray();
            _currentPropInfo =
                currentProps.FirstOrDefault(pi => pi.PropertyType == typeof(object))
                ?? currentProps.FirstOrDefault();
            var methods = enumeratorReturnTypes.SelectMany(t => t.GetMethods(PublicInstance))
                .ToArray();
            _moveNextMethod = methods.FirstOrDefault(
                mi => mi.Name == nameof(MoveNext) &&
                    mi.ReturnType == typeof(bool) &&
                    mi.GetParameters().Length == 0);
            _resetMethod = methods.FirstOrDefault(
                mi => mi.Name == nameof(Reset) &&
                    mi.ReturnType == typeof(void) &&
                    mi.GetParameters().Length == 0);
        }

        private static Type[] GetAllImplementedInterfaces(Type inspectType)
        {
            var result = new List<Type>();
            if (inspectType.IsInterface)
            {
                result.Add(inspectType);
            }

            foreach (var type in inspectType.GetInterfaces())
            {
                result.AddRange(GetAllImplementedInterfaces(type));
            }

            return result.Distinct().ToArray();
        }

        /// <summary>
        /// Implements the MoveNext functionality of IEnumerable
        /// </summary>
        public bool MoveNext()
        {
            return (bool) _moveNextMethod.Invoke(_wrapped, NO_ARGS);
        }

        /// <summary>
        /// Implements the Reset functionality of IEnumerable
        /// </summary>
        public void Reset()
        {
            // Not always required -- optionally implemented
            // https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerator.reset?view=netframework-4.8
            _resetMethod?.Invoke(_wrapped, NO_ARGS);
        }

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public T Current => TryConvert(_currentPropInfo.GetValue(_wrapped));

        private T TryConvert(object getValue)
        {
            // this should always succeed, but in the case where the underlying
            //  enumerator is misbehaving, we just spit out default(T)
            if (getValue is T matched)
            {
                return matched;
            }

            return TryChangeType(getValue, out var converted)
                ? converted
                : default(T);
        }

        private static bool TryChangeType(
            object input,
            out T output)
        {
            if (input is T immediateResult)
            {
                output = immediateResult;
                return true;
            }

            var result = TryChangeType(input, typeof(T), out var outputObj);
            output = (T) outputObj;
            return result;
        }

        /// <summary>
        /// Analogous to TryParse methods, this will attempt to convert a value to
        /// the type requiredType, returning true if it can, and populating the output parameter
        /// </summary>
        /// <param name="input">Value to work on</param>
        /// <param name="requiredType">The required type</param>
        /// <param name="output">Output parameter to collect result</param>
        /// <returns>True when can ChangeType, false otherwise</returns>
        public static bool TryChangeType(
            object input,
            Type requiredType,
            out object output)
        {
            try
            {
                output = Convert.ChangeType(input, requiredType);
                return true;
            }
            catch
            {
                var method = GenericDefaultOfT.MakeGenericMethod(requiredType);
                output = method.Invoke(null, new object[0]);
                return false;
            }
        }

        private static readonly MethodInfo GenericDefaultOfT = typeof(EnumerableWrapper)
            .GetMethod(nameof(Default), BindingFlags.Static | BindingFlags.NonPublic);

        private static TTarget Default<TTarget>()
        {
            return default(TTarget);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // nothing to do
        }
    }
}