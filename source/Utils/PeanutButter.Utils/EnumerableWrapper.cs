using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Used to describe a wrapper
    /// - IsValid should flag whether or not the wrapping was successful
    /// </summary>
    public interface IWrapper
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
    public class EnumerableWrapper : IEnumerable, IWrapper
    {
        /// <inheritdoc />
        public bool IsValid { get; }

        private readonly object _toWrap;

        private static readonly BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
        private static readonly Type EnumeratorType = typeof(IEnumerator);
        private static readonly PropertyInfo[] EnumeratorProps = EnumeratorType.GetProperties(PublicInstance);
        private static readonly MethodInfo[] EnumeratorMethods = EnumeratorType.GetMethods(PublicInstance);


        /// <inheritdoc />
        public EnumerableWrapper(object toWrap)
        {
            _toWrap = toWrap;
            var getEnumeratorMethod = toWrap.GetType()
                .GetMethod(nameof(GetEnumerator));
            IsValid = getEnumeratorMethod != null &&
                IsEnumeratorType(getEnumeratorMethod.ReturnType);
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
            return PropsAreAtLeast(
                    EnumeratorProps,
                    returnType.GetProperties(PublicInstance)) &&
                MethodsAreAtLeast(
                    EnumeratorMethods,
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
    public class EnumerableWrapper<T> : EnumerableWrapper, IEnumerable<T>
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
    public class EnumeratorWrapper<T> : IEnumerator<T>, IWrapper
    {
        /// <inheritdoc />
        public bool IsValid { get; set; }
        
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
                _moveNextMethod != null &&
                _resetMethod != null;
        }

        private void GrabEnumeratorReturnMembers()
        {
            if (_getEnumeratorMethod == null)
            {
                return;
            }

            var enumeratorReturnType = _getEnumeratorMethod.ReturnType;
            _currentPropInfo = enumeratorReturnType.GetProperty(nameof(Current));
            var methods = enumeratorReturnType.GetMethods(PublicInstance);
            _moveNextMethod = methods.FirstOrDefault(
                mi => mi.Name == nameof(MoveNext) &&
                    mi.ReturnType == typeof(bool) &&
                    mi.GetParameters().Length == 0);
            _resetMethod = methods.FirstOrDefault(
                mi => mi.Name == nameof(Reset) &&
                    mi.ReturnType == typeof(void) &&
                    mi.GetParameters().Length == 0);
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
            _resetMethod.Invoke(_wrapped, NO_ARGS);
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
            return getValue.TryChangeType<T>(out var converted)
                ? converted
                : default(T);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // nothing to do
        }
    }
}