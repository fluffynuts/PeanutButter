using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    public static class EnumerableExtensions
    {
        public static void ShouldMatchDataIn<T>(this IEnumerable<T> src, IEnumerable<T> other)
        {
            src.ForEach(item => Assert.IsTrue(
                other.Any(
                    otherItem => item.DeepEquals(otherItem)
                )
               )
            );

            other.ForEach(item => Assert.IsTrue(
                src.Any(otherItem => otherItem.DeepEquals(item))));
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
