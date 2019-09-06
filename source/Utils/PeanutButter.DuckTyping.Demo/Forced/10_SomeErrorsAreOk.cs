using System;
using System.ComponentModel;
using PeanutButter.DuckTyping.Demo.Strict;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.Forced
{
    public interface IExtendedReadOnlyEntity : IReadOnlyEntity
    {
        int Count { get; }
        bool Flag { get; }
        DateTime LastSeen { get; }
    }

    [Order(10)]
    [Description(
@"Sometimes we'd like to 'do the best we can'.
In those cases, we can force ducking and we'll get back default
values for missing members. Missing methods will explode.")]
    public class SomeErrorsAreOk: Demo
    {
        public override void Run()
        {
            var data = new { id = 666 };
            var ducked = data.ForceFuzzyDuckAs<IExtendedReadOnlyEntity>();
            Log("Original data:\n", data);
            EmptyLine();
            Log("Ducked:\n", ducked);
        }
    }
}