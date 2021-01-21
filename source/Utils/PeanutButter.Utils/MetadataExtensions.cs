// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
/// <summary>
/// Provides extension methods to set and retrieve metadata on any object.
/// Under the hood, these methods use a ConditionalWeakTable to store your
/// metadata, so the metadata is garbage-collected when your managed objects
/// are garbage-collected.
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class MetadataExtensions
    {
        private static readonly ConditionalWeakTable<object, Dictionary<string, object>> Table =
            new ConditionalWeakTable<object, Dictionary<string, object>>();

#if BUILD_PEANUTBUTTER_INTERNAL
#else // This is only used for testing and is not designed for consumers
        internal static int TrackedObjectCount() {
            var keys = Table.GetPropertyValue("Keys") as IEnumerable<object>;
            return keys?.Count()
                ?? throw new InvalidOperationException("Reaching into ConditionalWeakTable for the Keys collection has failed");
        }
#endif

        /// <summary>
        /// Sets an arbitrary piece of metadata on a managed object. This metadata
        /// has the same lifetime as your object, meaning it will be garbage-collected
        /// when your object is garbage-collected, assuming nothing else is referencing
        /// it.
        /// </summary>
        /// <param name="parent">Object to store metadata against</param>
        /// <param name="key">Name of the metadata item to set</param>
        /// <param name="value">Value to store</param>
        public static void SetMetadata(
            this object parent,
            string key,
            object value
        )
        {
            if (parent == null)
            {
                throw new NotSupportedException("Cannot set metadata for null");
            }

            using (new AutoLocker(Lock))
            {
                var data = GetMetadataFor(parent) ?? AddMetadataFor(parent);
                data[key] = value;
            }
        }

        /// <summary>
        /// Attempts to retrieve a piece of metadata for an object. When the
        /// metadata key for the object is unknown, returns the default value
        /// for the type requested, eg 0 for ints, null for strings and objects.
        /// Note that if metadata exists for the requested key but not for the
        /// type requested, a type-casting exception will be thrown.
        /// </summary>
        /// <param name="parent">Parent object to query against</param>
        /// <param name="key">Key to query for</param>
        /// <typeparam name="T">Type of data</typeparam>
        /// <returns></returns>
        public static T GetMetadata<T>(
            this object parent,
            string key
        )
        {
            return parent.GetMetadata<T>(key, default(T));
        }

        /// <summary>
        /// Attempts to retrieve a piece of metadata for an object. When the
        /// metadata key for the object is unknown, returns the default value
        /// for the type requested, eg 0 for ints, null for strings and objects.
        /// Note that if metadata exists for the requested key but not for the
        /// type requested, a type-casting exception will be thrown.
        /// This overload allows specifying a default value.
        /// </summary>
        /// <param name="parent">Parent object to query against</param>
        /// <param name="key">Key to query for</param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T">Type of data</typeparam>
        /// <returns></returns>
        public static T GetMetadata<T>(
            this object parent,
            string key,
            T defaultValue
        )
        {
            using (new AutoLocker(Lock))
            {
                var data = GetMetadataFor(parent);
                if (data == null)
                {
                    return defaultValue;
                }

                return data.TryGetValue(key, out var result)
                    ? (T) result // WILL fail hard if the caller doesn't match the stored type
                    : defaultValue;
            }
        }

        /// <summary>
        /// Tests if a parent object has a piece of metadata with the provided type.
        /// </summary>
        /// <param name="parent">Parent object to search against</param>
        /// <param name="key">Key to search for</param>
        /// <typeparam name="T">Expected type of metadata</typeparam>
        /// <returns></returns>
        public static bool HasMetadata<T>(
            this object parent,
            string key
        )
        {
            return parent.TryGetMetadata<T>(key, out var _);
        }

        /// <summary>
        /// Try get the named metadata for the provided type
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryGetMetadata<T>(
            this object parent,
            string key,
            out T result
        )
        {
            result = default;
            if (parent == null)
            {
                return false;
            }

            using (new AutoLocker(Lock))
            {
                var data = GetMetadataFor(parent);
                if (data == null)
                {
                    return false;
                }

                if (!data.TryGetValue(key, out var stored))
                {
                    return false;
                }

                if (!CanCast<T>(stored))
                {
                    return false;
                }

                result = (T)stored;
                return true;
            }
        }

        private static bool CanCast<T>(object stored)
        {
            try
            {
                // ReSharper disable once UnusedVariable
                var defaultValue = default(T);
                var cast = (T) stored;
                // Just want to run some actual logic to ensure that the
                //  cast isn't optimised away
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                return cast.Equals(defaultValue)
                    // ReSharper disable once ConditionalTernaryEqualBranch
                    ? true
                    : true;
            }
            catch
            {
                return false;
            }
        }

        private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        private static Dictionary<string, object> GetMetadataFor(object parent)
        {
            return Table.TryGetValue(parent, out var result)
                ? result
                : null;
        }

        private static Dictionary<string, object> AddMetadataFor(object parent)
        {
            var result = new Dictionary<string, object>();
            Table.Add(parent, result);
            return result;
        }
    }
}