using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

internal static class SatelliteAssemblyInitializer
{
    static SatelliteAssemblyInitializer()
    {
        var loadedPeanutButters = FindAutoLoadAssemblies();
        foreach (var asm in loadedPeanutButters)
        {
            InitRandomValueGenIn(asm);
        }
    }

    private static IEnumerable<Assembly> FindAutoLoadAssemblies()
    {
        foreach (var name in AutoLoadAssemblies)
        {
            var asm = TryLoad(name);
            if (asm is not null)
            {
                yield return asm;
            }
        }

        Assembly TryLoad(string name)
        {
            try
            {
                return Assembly.Load(name);
            }
            catch
            {
                return null;
            }
        }
    }

    private static readonly string[] AutoLoadAssemblies =
    [
        "PeanutButter.TestUtils.AspNetCore"
    ];

    private static readonly HashSet<Assembly> SeenAssemblies = new();
    private static readonly Assembly ThisAssembly = typeof(SatelliteAssemblyInitializer).Assembly;

    public static void InitializeSatelliteAssemblies<T>()
    {
        var containerAssembly = typeof(T).Assembly;
        InitRandomValueGenIn(containerAssembly);
    }

    private static void InitRandomValueGenIn(Assembly asm)
    {
        lock (SeenAssemblies)
        {
            if (asm == ThisAssembly ||
                !SeenAssemblies.Add(asm))
            {
                return;
            }

            var initializers = asm.GetTypes()
                .Where(t => t.IsNotPublic && t.Name == nameof(RandomValueGen))
                .Select(t => t.GetMethods().Where(mi => mi.IsStatic).ToArray())
                .SelectMany(m => m)
                .Where(m => m.Name == "Init" && m.GetParameters().Length == 0)
                .ToArray();
            initializers.ForEach(mi => mi.Invoke(null, []));
        }
    }
}