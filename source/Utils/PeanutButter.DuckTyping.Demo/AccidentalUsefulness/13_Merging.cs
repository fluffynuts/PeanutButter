using System.Collections.Generic;
using System.ComponentModel;
using PeanutButter.DuckTyping.Demo.Strict;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.AccidentalUsefulness
{
    [Order(13)]
    [Description(
        @"Sometimes our data is inconveniently split across multiple objects,
and we'd like to consolidate it, perhaps to simplify the dependencies
of another method / class.")]
    public class Merging : Demo
    {
        public override void Run()
        {
            var data1 = new
            {
                Id = 5
            };

            var data2 = new
            {
                Name = "Audrey Tang"
            };

            var allData = new object[] { data1, data2 };
            var ducked = allData.MergeAs<IReadOnlyEntity>();
            Log("Merging objects...");
            Log("data1:\n", data1);
            Log("data2:\n", data2);
            Log("Ducked:\n", ducked);

            var dict1 = new Dictionary<string, object>()
            {
                ["Id"] = 6
            };
            var dict2 = new Dictionary<string, object>()
            {
                ["Name"] = "Jutta Degener"
            };

            EmptyLine();
            Log("Merging dictionaries...");
            var ducked2 = new[] { dict1, dict2 }.MergeAs<IReadOnlyEntity>();
            Log(ducked2);
        }
    }
}