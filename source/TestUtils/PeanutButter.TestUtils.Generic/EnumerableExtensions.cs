using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    public static class EnumerableExtensions
    {
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
                enumerable.Any(otherItem => otherItem.DeepEquals(item))));
        }

        public static void ShouldMatchDataInAndOrderOf<T1, T2>(
            this IEnumerable<T1> src, IEnumerable<T2> other
        )
        {
            var srcArray = src.EmptyIfNull().ToArray();
            var otherArray = other.EmptyIfNull().ToArray();
            Assert.AreEqual(srcArray.Length, otherArray.Length, $"Collection sizes differ: {srcArray.Length} vs {otherArray.Length}");
            for (var i = 0; i < srcArray.Length; i++)
            {
                PropertyAssert.AreDeepEqual(srcArray[i], otherArray[i]);
            }
        }

        [Obsolete("Please use DeepEquals from PeanutButter.Utils instead")]
        public static bool Matches<T>(this T src, T other)
        {
            return src.DeepEquals(other);
        }

        public static bool IsEquivalentTo<T>(this IEnumerable<T> src, IEnumerable<T> other)
        {
            if (src == null && other == null) return true;
            if (src == null || other == null) return false;
            return src.Count() == other.Count() &&
                   !src.Except(other).Any();
        }  

        public static void ShouldHaveUnique<T>(this IEnumerable<T> src, Func<T, bool> matcher)
        {
            if (!src.HasUnique(matcher))
                Assert.Fail("Expected single unique result");
        } 

    }
}
