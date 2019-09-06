using System.ComponentModel;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.Demo.Strict
{
    [Order(3)]
    [Description("Often, ducks are most useful on anonymous objects")]
    public class ReadingFromAnonymousObjects : Demo
    {
        public override void Run()
        {
            var data = new
            {
                Id = 1,
                Name = "Grace Hopper"
            };
            var ducked = data.DuckAs<IReadOnlyEntity>();
            var writableDucked = data.DuckAs<IReadWriteEntity>();

            Log("Ducked:\n", ducked);
            Log("But anonymous objects are read-only, so attempting writable duck presents:\n",
                writableDucked);
        }
    }
}