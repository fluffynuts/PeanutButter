using System.Configuration;
using PeanutButter.RandomGenerators;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    public class ConnectionStringSettingsBuilder :
        GenericBuilder<ConnectionStringSettingsBuilder, ConnectionStringSettings>
    {
        public override ConnectionStringSettingsBuilder WithRandomProps()
        {
            return base.WithRandomProps().WithProp(o => o.LockItem = false);
        }
    }
}