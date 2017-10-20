using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.TestUtils.Generic.NUnitAbstractions;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    /// <summary>
    /// Extensions for IEnumerables
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Tests if src collection does not match data in other collection
        ///     - type-agnostic: types with the same "shape" will match
        ///     - does not care about order
        /// </summary>
        /// <param name="src">Primary source collection</param>
        /// <param name="other">Collection to compare with</param>
        /// <typeparam name="T1">Type of collection 1</typeparam>
        /// <typeparam name="T2">Type of collection 2</typeparam>
        public static void ShouldMatchDataIn<T1, T2>(
            this IEnumerable<T1> src,
            IEnumerable<T2> other
        )
        {
            var enumerable = src as T1[] ?? src.ToArray();
            enumerable.ForEach(item => Assert.IsTrue(
                    other.Any(
                        otherItem => item.DeepEquals(otherItem)
                    )
                )
            );

            other.ForEach(item => Assert.IsTrue(
                enumerable.Any(otherItem => otherItem.DeepEquals(item)
                )));
        }

        /// <summary>
        /// Tests that two collections have the same data in the same order
        /// </summary>
        /// <param name="src"></param>
        /// <param name="other"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        public static void ShouldMatchDataInAndOrderOf<T1, T2>(
            this IEnumerable<T1> src, IEnumerable<T2> other
        )
        {
            var srcArray = src.EmptyIfNull().ToArray();
            var otherArray = other.EmptyIfNull().ToArray();
            Assert.AreEqual(srcArray.Length, otherArray.Length,
                $"Collection sizes differ: {srcArray.Length} vs {otherArray.Length}");
            for (var i = 0; i < srcArray.Length; i++)
            {
                PropertyAssert.AreDeepEqual(srcArray[i], otherArray[i]);
            }
        }

        /// <summary>
        /// Tests if two collections have equivalency (same items, order not
        /// guaranteed). Will rely on .Equals() on T.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="other"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsEquivalentTo<T>(this IEnumerable<T> src, IEnumerable<T> other)
        {
            if (src == null && other == null) return true;
            if (src == null || other == null) return false;
            var srcArray = src as T[] ?? src.ToArray();
            var otherArray = other as T[] ?? other.ToArray();
            return srcArray.Length == otherArray.Length &&
                   !srcArray.Except(otherArray).Any();
        }

        /// <summary>
        /// Asserts that the provided Func filters to one unique value
        /// </summary>
        /// <param name="src"></param>
        /// <param name="matcher"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldHaveUnique<T>(this IEnumerable<T> src, Func<T, bool> matcher)
        {
            if (!src.HasUnique(matcher))
                Assert.Fail("Expected single unique result");
        }

        /// <summary>
        /// Asserts that the collection being operated on contains exactly
        /// one value deep equal to the provided one.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="seek"></param>
        /// <param name="ignoreProperties"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldContainOneDeepEqualTo<T>(this IEnumerable<T> src, T seek,
            params string[] ignoreProperties)
        {
            var srcArray = src as T[] ?? src.ToArray();
            if (srcArray.ContainsOneDeepEqualTo(seek, ignoreProperties))
                return;
            var message = srcArray.ContainsAtLeastOneDeepEqualTo(seek, ignoreProperties)
                ? "more than one match"
                : "no matches";
                Assertions.Throw($"Expected to find one {seek.Stringify()} in {srcArray.Stringify()} but found {message}");
        }

        /// <summary>
        /// Asserts that the collection being operated on contains at least
        /// one value deep equal to the provided one.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="seek"></param>
        /// <param name="ignoreProperties"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldContainAtLeastOneDeepEqualTo<T>(
            this IEnumerable<T> src, T seek, params string[] ignoreProperties
        )
        {
            var localArray = src as T[] ?? src.ToArray();
            if (localArray.IsEmpty())
                Assertions.Throw($"Expected to find {seek.Stringify()} in empty collection");
            if (!localArray.ContainsAtLeastOneDeepEqualTo(seek, ignoreProperties))
                Assertions.Throw($"Expected to find {seek.Stringify()} in {localArray.Stringify()}");
        }
    }
}