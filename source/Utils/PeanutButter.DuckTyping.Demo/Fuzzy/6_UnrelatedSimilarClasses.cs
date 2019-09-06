using System.ComponentModel;
using PeanutButter.DuckTyping.Demo.Strict;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.Fuzzy
{
    [Order(6)]
    [Description(
        @"Sometimes the incoming structure is icky and we'd like properly-named stuff")]
    public class UnrelatedSimilarClassesWithMatchingTypes : Demo
    {
        public override void Run()
        {
            var data = new
            {
                id = 2,
                name = "Leah Culver"
            };
            
            var ducked = data.DuckAs<IReadOnlyEntity>();
            var fuzzyDucked = data.FuzzyDuckAs<IReadOnlyEntity>();
            Log("Ducked:\n", ducked);
            Log("(not really surprising)");
            Log("Fuzzy Ducked:\n", fuzzyDucked);
            Log(" -- whee! --");
        }
    }
}