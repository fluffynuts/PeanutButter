using System.Linq;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators.Core.Tests;

public static class Matchers
{
    public static ICollectionMore<T> Vary<T>(
        this ICollectionTo<T> continuation
    )
    {
        return continuation.AddMatcher(
            actual =>
            {
                if (actual is null)
                {
                    return new EnforcedMatcherResult(
                        false,
                        "collection is null"
                    );
                }
                var arr = actual as T[] ?? actual.ToArray();

                if (arr.Count() < 2)
                {
                    return new EnforcedMatcherResult(
                        false,
                        "collection must contain at least 2 items to test variance"
                    );
                }

                var distinctCount = arr.Distinct().Count();
                var passed = distinctCount > 1;

                return new MatcherResult(
                    passed,
                    () => $"Expected some variance in the collection:\n{arr.Stringify()}"
                );
            }
        );
    }
}