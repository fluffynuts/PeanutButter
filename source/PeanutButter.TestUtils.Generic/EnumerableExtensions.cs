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
            src.ForEach(item => Assert.IsTrue(other.Any(otherItem => Matches<T>(item, otherItem))));
            other.ForEach(item => Assert.IsTrue(src.Any(otherItem => Matches<T>(item, otherItem))));
        }

        public static bool Matches<T>(this T src, T other)
        {
            try
            {
                PropertyAssert.AllPropertiesAreEqual(src, other);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsEquivalentTo<T>(this IEnumerable<T> src, IEnumerable<T> other)
        {
            if (src == null && other == null) return true;
            if (src == null || other == null) return false;
            return src.Count() == other.Count() &&
                   !src.Except(other).Any();
        }  

    }
}
