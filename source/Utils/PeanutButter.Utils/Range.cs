using System;
using System.Collections.Generic;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a Python-like Range method
    /// </summary>
    public static class PyLike
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
            yield break;
        }

        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(
            IEnumerable<T1> left,
            IEnumerable<T2> right
        )
        {
            var leftEnumerator = left.GetEnumerator();
            var rightEnumerator = right.GetEnumerator();
            while (leftEnumerator.MoveNext() && rightEnumerator.MoveNext())
            {
                yield return Tuple.Create(leftEnumerator.Current, rightEnumerator.Current);
            }
            yield break;
        }
    }
}