using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PeanutButter.Utils.Entity
{
    public static class Extensions
    {
        public static IEnumerable<T> AddRange<T>(this IDbSet<T> collection, params T[] items) where T: class
        {
            var concrete = collection as DbSet<T>;
            if (concrete != null)
                return concrete.AddRange(items);
            items.ForEach(item => collection.Add(item));
            return items;
        }

        public static IEnumerable<T> AddRange<T>(this IDbSet<T> collection, IEnumerable<T> items) where T : class
        {
            return collection.AddRange(items.ToArray());
        }

        public static IEnumerable<T> AddRange<T>(this ICollection<T> collection, params T[] items) where T: class
        {
            items.ForEach(collection.Add);
            return items;
        }

        public static IEnumerable<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items) where T: class
        {
            collection.AddRange(items.ToArray());
            return items;
        }

        public static IEnumerable<T> RemoveRange<T>(this IDbSet<T> collection, params T[] items) where T : class
        {
            var concrete = collection as DbSet<T>;
            if (concrete != null)
                return concrete.RemoveRange(items);
            items.ForEach(item => collection.Remove(item));
            return items;
        }

        public static IEnumerable<T> RemoveRange<T>(this IDbSet<T> collection, IEnumerable<T> items) where T : class
        {
            collection.RemoveRange(items.ToArray());
            return items;
        }

        public static int RemoveRange<T>(this ICollection<T> collection, params T[] items) where T : class
        {
            return items.Aggregate(0, (acc, cur) => acc += collection.Remove(cur) ? 1 : 0);
        }

        public static int RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items) where T : class
        {
            return collection.RemoveRange(items.ToArray());
        }

        public static void Clear<T>(this IDbSet<T> collection) where T : class
        {
            var entities = collection.ToArray();
            collection.RemoveRange(entities);
        }

        public static T AddNew<T>(this IDbSet<T> collection, Action<T> initializer = null, Action<T> runAfterAdd = null) where T : class
        {
            var entity = collection.Create();
            if (initializer != null)
                initializer(entity);
            collection.Add(entity);
            if (runAfterAdd != null)
                runAfterAdd(entity);
            return entity;
        }

        public static T AddNew<T>(this ICollection<T> collection, Action<T> initializer = null, Action<T> runAfterAdd = null) where T : class, new()
        {
            var entity = new T();
            if (initializer != null)
                initializer(entity);
            collection.Add(entity);
            if (runAfterAdd != null)
                runAfterAdd(entity);
            return entity;
        }

    }
}
