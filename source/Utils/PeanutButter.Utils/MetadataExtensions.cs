using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PeanutButter.Utils
{
#if BUILD_PEANUTBUTTER_INTERNAL
    internal 
#else
    public 
#endif
        static class MetadataExtensions
    {
        private static readonly ConditionalWeakTable<object, Dictionary<string, object>> _table =
            new ConditionalWeakTable<object, Dictionary<string, object>>();

        internal static int TrackedObjectCount() {
            var keys = _table.GetPropertyValue("Keys") as IEnumerable<object>;
            return keys?.Count() 
                ?? throw new InvalidOperationException("Reaching into ConditionalWeakTable for the Keys collection has failed");
        }

        internal static void SetMetadata(
            this object parent,
            string key,
            object value
        )
        {
            using (new AutoLocker(_lock))
            {
                var data = GetMetadataFor(parent) ?? AddMetadataFor(parent);
                data[key] = value;
            }
        }

        internal static T GetMetadata<T>(
            this object parent,
            string key
        )
        {
            using (new AutoLocker(_lock))
            {
                var data = GetMetadataFor(parent);
                if (data == null)
                    return default(T);

                return data.TryGetValue(key, out var result)
                    ? (T) result    // WILL fail hard if the caller doesn't match the stored type
                    : default(T);
            }
        }

        internal static bool HasMetadata<T>(
            this object parent,
            string key
        )
        {
            using (new AutoLocker(_lock))
            {
                var data = GetMetadataFor(parent);
                if (data == null)
                    return false;
                return data.TryGetValue(key, out var stored) && 
                    CanCast<T>(stored);
            }
        }

        private static bool CanCast<T>(object stored)
        {
            try
            {
                // ReSharper disable once UnusedVariable
                var interesting = default(T)?.Equals((T)stored);   // TODO: could this be optimised away?
                return true;
            }
            catch
            {
                return false;
            }
        }


        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private static Dictionary<string, object> GetMetadataFor(object parent)
        {
            return _table.TryGetValue(parent, out var result)
                ? result
                : null;
        }

        private static Dictionary<string, object> AddMetadataFor(object parent)
        {
            var result = new Dictionary<string, object>();
            _table.Add(parent, result);
            return result;

        }
    }
}