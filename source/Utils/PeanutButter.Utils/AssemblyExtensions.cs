using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides extension methods for Assemblies
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class AssemblyExtensions
    {
        /// <summary>
        /// Finds a Type by name within a given assembly. Returns null
        /// if the type cannot be found.
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <param name="typeName">The name of the Type to find</param>
        /// <returns>First type maching given name or null if no match found</returns>
        public static Type FindTypeByName(this Assembly assembly, string typeName)
        {
            return assembly
                .GetExportedTypes()
                .FirstOrDefault(t => t.Name == typeName);
        }


        /// <summary>
        /// Walks the assembly dependency tree
        /// </summary>
        /// <param name="asm"></param>
        /// <returns></returns>
        public static IEnumerable<Assembly[]> WalkDependencies(
            this Assembly asm
        )
        {
            var queue = new Queue<Assembly[]>();
            queue.Enqueue(asm.InArray());
            var seen = new HashSet<string>();
            var reported = new HashSet<string>();
            var loadedAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .ToArray();
            while (queue.Any())
            {
                var current = queue.Dequeue();
                if (current.Length == 0)
                {
                    yield break;
                }

                yield return current
                    .Where(a => !reported.Contains(a.FullName))
                    .ToArray();
                foreach (var a in current)
                {
                    reported.Add(a.FullName);
                    if (!seen.Add(a.FullName))
                    {
                        continue;
                    }

                    var refs = a.GetReferencedAssemblies()
                        .Select(n => $"{n}")
                        .Select(
                            n => Tuple.Create(
                                n,
                                loadedAssemblies
                                    .FirstOrDefault(
                                        aa => $"{aa.GetName()}" == n
                                    ) ?? Assembly.Load(n)
                            )
                        )
                        .Where(o => o.Item2 is not null)
                        .Select(o => o.Item2)
                        .ToArray();

                    queue.Enqueue(refs);
                }
            }
        }
    }
}