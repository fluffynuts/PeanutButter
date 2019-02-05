using System;
using System.Collections.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Base class for UniqueRandomValueGenerator&lt;T&gt;
    /// </summary>
    public abstract class UniqueRandomValueGenerator
    {
        /// <summary>
        /// Gets the next unique value as an object so it can be used
        /// in reflection and suchlike.
        /// </summary>
        /// <returns></returns>
        public abstract object NextObjectValue();

        private static readonly Type _genericType =
            typeof(UniqueRandomValueGenerator<>);

        /// <summary>
        /// Produces an instance of an UniqueRandomValueGenerator for the
        /// provided Type t
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static UniqueRandomValueGenerator For(Type t)
        {
            var specific = _genericType.MakeGenericType(t);
            return Activator.CreateInstance(specific) as
                       UniqueRandomValueGenerator;
        }
    }

    /// <summary>
    /// Generates unique random values per instance
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UniqueRandomValueGenerator<T> : UniqueRandomValueGenerator
    {
        private readonly HashSet<T> _used = new HashSet<T>();

        /// <summary>
        /// Get the next value, Typed
        /// </summary>
        /// <returns></returns>
        public T Next()
        {
            return (T)NextObjectValue();
        }

        /// <inheritdoc />
        public override object NextObjectValue()
        {
            if (typeof(T).IsNumericType())
            {
                var id = GetRandom(i => !_used.Contains(i), GenerateNumber);
                _used.Add(id);
                return id;
            }

            var result = GetRandom<T>(o => !_used.Contains(o), GetRandom<T>);
            _used.Add(result);
            return result;
        }

        private T GenerateNumber()
        {
            var intValue = (object) GenerateInt();
            return (T) intValue;
        }

        private int GenerateInt()
        {
            var max = _used.Count * 2;
            if (max < 10)
                max = 10;
            return GetRandomInt(1, max);
        }
    }
}