using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace PeanutButter.Utils.Entity
{
    public static class Extensions
    {
        public static IEnumerable<T> AddRange<T>(this IDbSet<T> dbSet, params T[] items) where T: class
        {
            var concrete = dbSet as DbSet<T>;
            if (concrete != null)
                return concrete.AddRange(items);
            items.ForEach(item => dbSet.Add(item));
            return items;
        }

        public static IEnumerable<T> AddRange<T>(this IDbSet<T> dbSet, IEnumerable<T> items) where T : class
        {
            return dbSet.AddRange(items.ToArray());
        }

        public static IEnumerable<T> AddRange<T>(this ICollection<T> dbSet, params T[] items) where T: class
        {
            items.ForEach(item => dbSet.Add(item));
            return items;
        }

        public static IEnumerable<T> AddRange<T>(this ICollection<T> dbSet, IEnumerable<T> items) where T: class
        {
            dbSet.AddRange(items.ToArray());
            return items;
        }

        public static IEnumerable<T> RemoveRange<T>(this IDbSet<T> dbSet, params T[] items) where T : class
        {
            var concrete = dbSet as DbSet<T>;
            if (concrete != null)
                return concrete.RemoveRange(items);
            items.ForEach(item => dbSet.Remove(item));
            return items;
        }

        public static IEnumerable<T> RemoveRange<T>(this IDbSet<T> dbSet, IEnumerable<T> items) where T : class
        {
            dbSet.RemoveRange(items.ToArray());
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

        public static void Clear<T>(this IDbSet<T> dbSet) where T : class
        {
            var entities = dbSet.ToArray();
            dbSet.RemoveRange(entities);
        }

        public static T AddNew<T>(this IDbSet<T> dbSet, Action<T> initializer = null, Action<T> runAfterAdd = null) where T : class
        {
            var entity = dbSet.Create();
            if (initializer != null)
                initializer(entity);
            dbSet.Add(entity);
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
