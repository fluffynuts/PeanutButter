using System;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils
{
    public static class AssemblyExtensions
    {
        public static Type FindTypeByName(this Assembly assembly, string typeName)
        {
            return assembly
                .GetExportedTypes()
                .FirstOrDefault(t => t.Name == typeName);
        }
    }
}