using System.ComponentModel;
using PeanutButter.DuckTyping.Demo.Strict;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.Fuzzy
{
    [Order(7)]
    [Description(
        @"Sometimes what looks like an obvious conversion to a human stumps a computer;
It would be nice if we could just duck that all away.
")]
    public class VerySimilarObjects : Demo
    {
        public override void Run()
        {
            var data = new
            {
                Id = "123",
                Name = 321
            };
            
            var ducked = data.FuzzyDuckAs<IReadOnlyEntity>();
            Log("Unducked:\n", data);
            Log("Ducked:\n", ducked);
        }
    }
}