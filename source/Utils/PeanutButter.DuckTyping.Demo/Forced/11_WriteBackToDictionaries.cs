using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Demo.Forced
{
    public interface ISpaceship
    {
        string Designation { get; set; }
        string Class { get; set; }
        int Crew { get; set; }
        bool IsWarship { get; set; }
    }

    [Order(11)]
    [Description(
@"A useful placed for forced ducking is when we realise
that we can write back to an empty dictionary through the
interface. Note that forcing a FuzzyDuck on a dictionary
requires the underlying dictionary to be case-insensitive
for writeback to happen properly.")]
    public class WriteBackToDictionaries: Demo
    {
        public override void Run()
        {
            var dict = new Dictionary<string, object>();
            var ducked = dict.ForceDuckAs<ISpaceship>();
            
            Log("Before we meddle, source dictionary is empty:");
            Log("Ducked:\n", ducked);
            
            EmptyLine();
            WaitForKey();
            EmptyLine();
            
            ducked.Class = "Majestic";
            ducked.Designation = "Upper Echelon";
            ducked.Crew = 442;
            ducked.IsWarship = true;
            
            Log("Ducked, After meddling:\n", ducked);
            EmptyLine();
            Log("Backing dictionary:\n",
                dict.Select(k => $"{k.Key}: {k.Value}")
                    .JoinWith("\n"));
        }
    }
}