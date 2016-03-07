using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.RandomGenerators
{
    public static class GenericBuilderLocator
    {
        public static Type GetBuilderFor(Type type)
        {
            return TryFindExistingBuilderFor(type)
                    ?? FindOrGenerateDynamicBuilderFor(type);
        }

        public static Type TryFindExistingBuilderFor(Type type)
        {
            return TryFindBuilderInCurrentAssemblyFor(type)
                   ?? TryFindBuilderInAnyOtherAssemblyInAppDomainFor(type);
        }

        public static Type FindOrGenerateDynamicBuilderFor(Type type)
        {
            return GenericBuilderBase.FindOrGenerateDynamicBuilderFor(type);
        }

        private static Type[] TryGetExportedTypesFrom(Assembly asm)
        {
            try
            {
                return asm.GetExportedTypes();
            }
            catch
            {
                return new Type[] {};
            }
        }

        private static Type TryFindBuilderInAnyOtherAssemblyInAppDomainFor(Type propertyType)
        {
            try
            {
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a != propertyType.Assembly && !a.IsDynamic)
                    .SelectMany(TryGetExportedTypesFrom)
                    .Where(t => t.IsBuilderFor(propertyType))
                    .ToArray();
                if (!types.Any())
                    return null;
                return types.Length == 1 
                    ? types.First() 
                    : FindClosestNamespaceMatchFor(propertyType, types);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error whilst searching for user builder for type '" + propertyType.PrettyName() + "' in all loaded assemblies: " + ex.Message);
                return null;
            }
        }

        private static Type TryFindBuilderInCurrentAssemblyFor(Type propType)
        {
            try
            {
                return propType.Assembly.GetTypes()
                    .FirstOrDefault(t => t.IsBuilderFor(propType));
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error whilst searching for user builder for type '" + propType.PrettyName() + "' in type's assembly: " + ex.Message);
                return null;
            }
        }

        private static Type FindClosestNamespaceMatchFor(Type propertyType, IEnumerable<Type> types)
        {
            if (propertyType?.Namespace == null)    // R# is convinced this might happen :/
                return null;
            var seekNamespace = propertyType.Namespace.Split('.');
            return types.Aggregate((Type) null, (acc, cur) =>
            {
                if (acc?.Namespace == null || cur.Namespace == null)
                    return cur;
                var accParts = acc.Namespace.Split('.');
                var curParts = cur.Namespace.Split('.');
                var accMatchIndex = seekNamespace.MatchIndexFor(accParts);
                var curMatchIndex = seekNamespace.MatchIndexFor(curParts);
                return accMatchIndex > curMatchIndex ? acc : cur;
            });
        }
    }
}