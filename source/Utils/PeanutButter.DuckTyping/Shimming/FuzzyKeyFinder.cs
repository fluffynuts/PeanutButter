using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Shimming
{
    internal class FuzzyKeyFinder
    {
        private readonly Func<IDictionary<string, object>, string, string>[] _fuzzyKeyResolvers =
        {
            (dict, search) => dict.Keys.FirstOrDefault(k => k == search),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, " ")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, ".")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, ":")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, "_")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, "-"))
        };

        public string FuzzyFindKeyFor(IDictionary<string, object> data, string propertyName)
        {
            return _fuzzyKeyResolvers.Aggregate(
                null as string,
                (acc, cur) => acc ?? cur(data, propertyName)
            );
        }
    }
}