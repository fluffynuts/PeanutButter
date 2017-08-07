using System.Configuration;
using PeanutButter.RandomGenerators;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    public class ConnectiongStringSettingsBuilder :
        GenericBuilder<ConnectiongStringSettingsBuilder, ConnectionStringSettings>
    {
        public override ConnectiongStringSettingsBuilder WithRandomProps()
        {
            return base.WithRandomProps().WithProp(o => o.LockItem = false);
        }
    }
}