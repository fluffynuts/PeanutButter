using System;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides extension methods for Assemblies
    /// </summary>
    public static class AssemblyExtensions
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
    }
}