using System;
using System.Linq;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using NExpect.Implementations;

namespace PeanutButter.RandomGenerators.Tests
{
    internal static class GenericBuilderTestMatchers
    {
        internal static IStringMore LookLikeEmailAddress(
            this ITo<string> to
        )
        {
            to.AddMatcher(email =>
            {
                var passed = !string.IsNullOrWhiteSpace(email) &&
                       email.IndexOf("@", StringComparison.Ordinal) > 0 &&
                       email.IndexOf("@", StringComparison.Ordinal) < email.Length - 2 &&
                       email.IndexOf(".", StringComparison.Ordinal) > 0 &&
                       email.IndexOf(".", StringComparison.Ordinal) < email.Length - 2;
                return new MatcherResult(
                    passed,
                    $"Expected \"{email}\" {passed.AsNot()}to look like an email address"
                );
            });
            return to.More();
        }

        internal static IStringMore LookLikeUrl(
            this ITo<string> to
        )
        {
            to.AddMatcher(actual =>
            {
                var proto = "://";
                var passed = !string.IsNullOrWhiteSpace(actual) &&
                       actual.Contains(proto) &&
                       !actual.StartsWith(proto) &&
                       !actual.EndsWith("://");
                return new MatcherResult(
                    passed,
                    $"Expected \"{actual}\" {passed.AsNot()}to look like an url"
                );
            });
            return to.More();
        }

        internal static IStringMore AllNumeric(
            this IBe<string> be
        )
        {
            be.AddMatcher(actual =>
            {
                var passed = actual.All(c => "0123456789".Contains(c));
                return new MatcherResult(
                    passed,
                    $"Expected \"{actual}\" {passed.AsNot()}to be all numeric"
                );
            });

            return be.More();
        }
    }
}