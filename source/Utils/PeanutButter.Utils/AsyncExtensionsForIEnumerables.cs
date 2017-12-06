using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Useful extensions for IEnumerable&lt;T&gt; collections, with async in mind
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class AsyncExtensionsForIEnumerables
    {
        /// <summary>
        /// The missing ForEach method - async variant. Don't forget to await on it!
        /// </summary>
        /// <param name="collection">Subject collection to operate over</param>
        /// <param name="toRun">Action to run on each member of the collection</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> collection,
            Func<T, Task> toRun
        )
        {
            foreach (var item in collection)
                await toRun(item);
        }

        /// <summary>
        /// The missing ForEach method - asynchronous variant which also provides the current item index
        /// -> DON'T forget to await!
        /// </summary>
        /// <param name="collection">Subject collection to operate over</param>
        /// <param name="toRunWithIndex">Action to run on each member of the collection</param>
        /// <typeparam name="T">Item type of the collection</typeparam>
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> collection,
            Func<T, int, Task> toRunWithIndex
        )
        {
            var idx = 0;
            await collection.ForEachAsync(async (o) =>
            {
                await toRunWithIndex(o, idx++);
            });
        }

        /// <summary>
        /// Allows awaiting on making an async result of IEnumerable&lt;T&gt;
        /// into an array. Think of "ToArray" for an async result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static async Task<T[]> ToArrayAsync<T>(this Task<IEnumerable<T>> src)
        {
            return (await src)?.ToArray();
        }

        /// <summary>
        /// Allows awaiting on making an async result of IEnumerable&lt;T&gt;
        /// into an array. Think of "ToArray" for an async result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static async Task<T[]> ToArrayAsync<T>(this Task<T[]> src)
        {
            return (await src)?.ToArray();
        }

        /// <summary>
        /// Allows awaiting on making an async result of IEnumerable&lt;T&gt;
        /// into an array. Think of "ToArray" for an async result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public static async Task<T[]> ToArrayAsync<T>(this Task<List<T>> src)
        {
            return (await src)?.ToArray();
        }

        /// <summary>
        /// Provides an awaitable Aggregate for collections
        /// </summary>
        /// <typeparam name="TAccumulator"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items">Collection to operate on</param>
        /// <param name="seed">Seed accumulator object</param>
        /// <param name="reducer">Function to call on each item, reducing the overall result.
        /// It will be given the accumulator from the last round (or the seed if it is the first
        /// time it is called) and should return an accumulator which the next round can use.</param>
        /// <returns></returns>
        public static async Task<TAccumulator> AggregateAsync<TAccumulator, TItem>(
            this IEnumerable<TItem> items,
            TAccumulator seed,
            Func<TAccumulator, TItem, Task<TAccumulator>> reducer
        )
        {
            foreach (var item in items ?? new TItem[0])
            {
                seed = await reducer(seed, item);
            }
            return seed;
        }


        /// <summary>
        /// Provides an awaitable variant of Select()
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="src">Source collection to operate on</param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static async Task<TOut[]> SelectAsync<TIn, TOut>(
            this IEnumerable<TIn> src,
            Func<TIn, Task<TOut>> transform
        )
        {
            if (src == null)
                return await Task.FromResult(null as TOut[]);
            return await Task.WhenAll(src.Select(transform));
        }

        /// <summary>
        /// Provides an awaitable .Where() where your discriminator
        /// function can be async. Will return an empty collection if
        /// operating on null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src">Source collection</param>
        /// <param name="discriminator">Discriminator function</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> WhereAsync<T>(
            this IEnumerable<T> src,
            Func<T, Task<bool>> discriminator
        )
        {
            var result = new List<T>();
            await (src ?? new T[0]).ForEachAsync(async i =>
            {
                if (await discriminator(i))
                    result.Add(i);
            });
            return result;
        }
    }
}