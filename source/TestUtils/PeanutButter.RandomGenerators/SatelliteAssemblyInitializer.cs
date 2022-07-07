using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    internal static class SatelliteAssemblyInitializer
    {
        static SatelliteAssemblyInitializer()
        {
            var loadedPeanutButters = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => AutoLoadAssemblies.Contains(asm.GetName().Name))
                .ToArray();
            foreach (var asm in loadedPeanutButters)
            {
                InitRandomValueGenIn(asm);
            }
        }
        
        private static readonly AssemblyName MyName = typeof(SatelliteAssemblyInitializer)
            .Assembly.GetName();

        private static readonly HashSet<string> AutoLoadAssemblies = new(
            new[]
            {
                "PeanutButter.TestUtils.AspNetCore"
            }
        );

        private static readonly HashSet<Assembly> SeenAssemblies = new();
        private static readonly Assembly ThisAssembly = typeof(SatelliteAssemblyInitializer).Assembly;

        public static void InitializeSatelliteAssemblies<T>()
        {
            var containerAssembly = typeof(T).Assembly;
            InitRandomValueGenIn(containerAssembly);
        }

        private static void InitRandomValueGenIn(Assembly asm)
        {
            if (asm == ThisAssembly ||
                SeenAssemblies.Contains(asm))
            {
                return;
            }

            SeenAssemblies.Add(asm);

            var initializers = asm.GetTypes()
                .Where(t => t.IsNotPublic && t.Name == nameof(RandomValueGen))
                .Select(t => t.GetMethods().Where(mi => mi.IsStatic).ToArray())
                .SelectMany(m => m)
                .Where(m => m.Name == "Init" && m.GetParameters().Length == 0)
                .ToArray();
            initializers.ForEach(mi => mi.Invoke(null, new object[0]));
        }
    }
}