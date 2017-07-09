using System;
using static PeanutButter.DuckTyping.Extensions.DuckTypingExtensionSharedMethods;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides a set of extension methods to enable duck-typing
    /// </summary>
    public static class DuckTypingObjectExtensions
    {
        /// <summary>
        /// Tests whether or not a given object can be accurately duck-typed to the
        /// requested interface T. Accurate duck-typing requires exact matching of
        /// property names and types as well as method names and signatures.
        /// </summary>
        /// <param name="src">Object to inspect</param>
        /// <typeparam name="T">Desired interface to duck to</typeparam>
        /// <returns>True when the object can be accurately duck-typed; false otherwise. A false result here may not necessarily mean a false result from CanFuzzyDuckAs</returns>
        public static bool CanDuckAs<T>(this object src)
        {
            return src.InternalCanDuckAs<T>(false, false);
        }

        /// <summary>
        /// Tests whether or not a given object can be approximately duck-typed to the
        /// requested interface T. Fuzzy ducking will attempt to bridge when there are case
        /// mismatches in property and method names as well as attempting auto-conversion
        /// between underlying and exposed types (eg: a Guid property could be backed by
        /// an underlying string property) and will attempt parameter re-ordering when
        /// ducking methods, if required and possible.
        /// </summary>
        /// <param name="src">Object to inspect</param>
        /// <typeparam name="T">Desired interface to duck to</typeparam>
        /// <returns>True when the object can be approximately duck-typed; false otherwise</returns>
        public static bool CanFuzzyDuckAs<T>(this object src)
        {
            return src.InternalCanDuckAs<T>(true, false);
        }

        /// <summary>
        /// Attempts to accurately duck-type an object to an interface.
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <typeparam name="T">Interface required</typeparam>
        /// <returns>
        /// If accurate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null.
        /// </returns>
        public static T DuckAs<T>(this object src) where T : class
        {
            return src.DuckAs<T>(false);
        }

        /// <summary>
        /// Attempts to accurately duck-type an object to an interface.
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <param name="throwOnError">Flag to allow throwing and exception when ducking cannot be achieved. 
        /// The thrown exception contains information about what caused ducking to fail.</param>
        /// <typeparam name="T">Interface required</typeparam>
        /// <returns>
        /// If accurate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null when throwOnError is false or throws
        /// an UnDuckableException if throwOnError is true.
        /// </returns>
        public static T DuckAs<T>(this object src, bool throwOnError) where T : class
        {
            return src.InternalDuckAs<T>(false, throwOnError);
        }

        /// <summary>
        /// Attempts to approximately duck-type an object to a a provided interface type
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <param name="toType">Type of interface required</param>
        /// <returns>
        /// If approximate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null.
        /// </returns>
        public static object FuzzyDuckAs(this object src, Type toType)
        {
            return NonGenericDuck(
                src, toType, false, 
                GenericFuzzyDuckAsMethod
            );
        }

        /// <summary>
        /// Attempts to approximately duck-type an object to a a provided interface type
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <param name="toType">Type of interface required</param>
        /// <param name="throwOnError">Flag to determine whether failure to duck should throw an exception</param>
        /// <returns>
        /// If approximate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null when throwOnError is false or throws
        /// an UnDuckableException when throwOnError is true.
        /// </returns>
        public static object FuzzyDuckAs(this object src, Type toType, bool throwOnError)
        {
            return UnwrapTargetInvokationFor(
                () => NonGenericDuck(
                    src, toType, throwOnError, GenericFuzzyDuckAsMethod)
            );
        }

        /// <summary>
        /// Attempts to accurately duck-type an object to a a provided interface type
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <param name="toType">Type of interface required</param>
        /// <returns>
        /// If accurate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null.
        /// </returns>
        public static object DuckAs(this object src, Type toType)
        {
            return NonGenericDuck(src, toType, false, GenericDuckAsMethod);
        }

        /// <summary>
        /// Attempts to accurately duck-type an object to a a provided interface type
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <param name="toType">Type of interface required</param>
        /// <param name="throwOnError">Flag to determine whether failure to duck should throw an exception</param>
        /// <returns>
        /// If accurate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null when throwOnError is false or throws
        /// an UnDuckableException when throwOnError is true.
        /// </returns>
        public static object DuckAs(this object src, Type toType, bool throwOnError)
        {
            return UnwrapTargetInvokationFor(
                () => NonGenericDuck(src, toType, throwOnError, GenericDuckAsMethod)
            );
        }

        /// <summary>
        /// Attempts to approximately duck-type an object to a a provided interface type
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <paramtype name="T">Type of interface required</paramtype>
        /// <returns>
        /// If approximate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null.
        /// </returns>
        public static T FuzzyDuckAs<T>(this object src) where T : class
        {
            return src.FuzzyDuckAs<T>(false);
        }

        /// <summary>
        /// Attempts to approximately duck-type an object to a a provided interface type
        /// </summary>
        /// <param name="src">Object to duck-type</param>
        /// <param name="throwOnError">Flag to determine whether failure to duck should throw an exception</param>
        /// <paramtype name="T">Type of interface required</paramtype>
        /// <returns>
        /// If approximate ducking is possible, this method returns a new object which wraps
        /// around the input object, exposing the properties and methods of the requested
        /// interface whilst calling through to the underlying object. If ducking cannot be
        /// achieved, this method silently returns null when throwOnError is false or throws
        /// an UnDuckableException when throwOnError is true.
        /// </returns>
        public static T FuzzyDuckAs<T>(this object src, bool throwOnError) where T : class
        {
            return src.InternalDuckAs<T>(true, throwOnError);
        }

    }
}