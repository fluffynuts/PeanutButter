using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a Python-like Range method
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public 
#endif
        static class PyLike
    {
        /// <summary>
        /// Produces a sequence of consecutive ints from zero to (stop - 1)
        /// </summary>
        /// <param name="stop">upper bound of sequence, not included in result</param>
        /// <returns>Sequence of ints</returns>
        public static IEnumerable<int> Range(int stop)
        {
            return Range(0, stop);
        }

        /// <summary>
        /// Produces a sequence of consecutive ints from start to (stop - 1)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns></returns>
        public static IEnumerable<int> Range(int start, int stop)
        {
            return Range(start, stop, 1);
        }

        /// <summary>
        /// produces a sequence of ints from start to stop (not inclusive), stepping by step
        /// </summary>
        /// <param name="start">first item to expect in sequence</param>
        /// <param name="stop">go no higher, young padawan!</param>
        /// <param name="step">step up by this amount each time</param>
        /// <returns>Sequence of ints</returns>
        public static IEnumerable<int> Range(int start, int stop, int step)
        {
            while (start < stop)
            {
                yield return start;
                start += step;
            }
        }

        /// <summary>
        /// "Zips" two collections together so you can enumerate over them. The
        ///   length of the enumeration is determined by the shortest collection
        /// </summary>
        /// <param name="left">left collection</param>
        /// <param name="right">right collection</param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <returns>Enumeration over the collections (Tuple of T1, T2)</returns>
        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(
            IEnumerable<T1> left,
            IEnumerable<T2> right
        )
        {
            if (AnyAreNull(left, right))
                yield break;

            using (var leftEnumerator = left.GetEnumerator())
            using (var rightEnumerator = right.GetEnumerator())
            {
                while (MoveAll(leftEnumerator, rightEnumerator))
                {
                    yield return Tuple.Create(leftEnumerator.Current, rightEnumerator.Current);
                }
            }
        }

        /// <summary>
        /// "Zips" three collections together so you can enumerate over them. The
        ///   length of the enumeration is determined by the shortest collection
        /// </summary>
        /// <param name="left">left collection</param>
        /// <param name="middle">right collection</param>
        /// <param name="right">right collection</param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns>Enumeration over the collections (Tuple of T1, T2, T3)</returns>
        public static IEnumerable<Tuple<T1, T2, T3>> Zip<T1, T2, T3>(
            IEnumerable<T1> left,
            IEnumerable<T2> middle,
            IEnumerable<T3> right
        )
        {
            if (AnyAreNull(left, middle, right))
                yield break;

            using (var leftEnumerator = left.GetEnumerator())
            using (var middleEnumerator = middle.GetEnumerator())
            using (var rightEnumerator = right.GetEnumerator())
            {
                while (MoveAll(leftEnumerator, middleEnumerator, rightEnumerator))
                {
                    yield return Tuple.Create(
                        leftEnumerator.Current,
                        middleEnumerator.Current,
                        rightEnumerator.Current
                    );
                }
                yield break;
            }
        }

        private static bool AnyAreNull(params object[] objects)
        {
            return objects.Any(o => o == null);
        }

        private static bool MoveAll(params IEnumerator[] e)
        {
            return e.Aggregate(true, (acc, cur) => acc && cur.MoveNext());
        }
    }
}