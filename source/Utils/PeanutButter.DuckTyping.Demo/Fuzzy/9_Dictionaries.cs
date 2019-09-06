using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PeanutButter.DuckTyping.Demo.Strict;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Demo.Fuzzy
{
    [Order(9)]
    [Description(
        @"Actually, dictionaries came before NameValueCollections.
PeanutButter.DuckTyping supports these and objects because
they don't bring in external dependencies. You can also duck
Newtonsoft JSON objects, after using PeanutButter.JObjectExtensions.")]
    public class Dictionaries : Demo
    {
        public override void Run()
        {
            var dict = new Dictionary<string, object>()
            {
                ["Id"] = 3,
                ["naME"] = "Rebecca Heineman"
            };
            
            var ducked = dict.FuzzyDuckAs<IReadOnlyEntity>();
            Log("Dictionary:\n", dict.Select(kvp => $"{kvp.Key}: {kvp.Value}").JoinWith("\n"));
            EmptyLine();
            Log("Ducked:\n", ducked);
        }
    }
}