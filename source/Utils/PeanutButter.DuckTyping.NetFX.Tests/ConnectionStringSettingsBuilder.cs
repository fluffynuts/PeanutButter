using System.Configuration;
using PeanutButter.RandomGenerators;

namespace PeanutButter.DuckTyping.NetFX.Tests;

public class ConnectionStringSettingsBuilder :
    GenericBuilder<ConnectionStringSettingsBuilder, ConnectionStringSettings>
{
    public override ConnectionStringSettingsBuilder WithRandomProps()
    {
        return base.WithRandomProps().WithProp(o => o.LockItem = false);
    }
}
