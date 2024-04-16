using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

// ReSharper disable UnusedMember.Local
#pragma warning disable 168

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;
namespace Imported.PeanutButter.RandomGenerators;
#else
using PeanutButter.Utils;
namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Locator class which attempts to find suitable builders on demand
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public 
#endif
    static class GenericBuilderLocator
{
    /// <summary>
    /// Attempts to find and instantiate a generic builder for the type provided
    /// </summary>
    /// <param name="type">Type to find or create a builder for</param>
    /// <returns>a builder, hopefully!</returns>
    public static IGenericBuilder GetGenericBuilderInstanceFor(Type type)
    {
        var builderType = GetBuilderFor(type);
        return Activator.CreateInstance(builderType) as IGenericBuilder;
    }

    /// <summary>
    /// Attempts to find a GenericBuilder type which is capable of building the
    /// provided type. Will cause generation of the builder if an existing type
    /// cannot be found.
    /// </summary>
    /// <param name="type">Type for which a builder is required</param>
    /// <returns>GenericBuilder type which can be constructed and used to build!</returns>
    public static Type GetBuilderFor(Type type)
    {
        return TryFindExistingBuilderFor(type)
            ?? FindOrGenerateDynamicBuilderFor(type);
    }

    /// <summary>
    /// Searches for an existing builder for the given type, first considering
    /// the same assembly as the provided type and then considering all assemblies
    /// within the AppDomain of the provided type.
    /// </summary>
    /// <param name="type">Type to search for a builder for</param>
    /// <returns>GenericBuilder type or null if no suitable builder was found</returns>
    public static Type TryFindExistingBuilderFor(Type type)
    {
        if (type == null)
            return null;
        lock (BuilderTypeCache)
        {
            if (BuilderTypeCache.TryGetValue(type, out var result))
                return result;
        }

        return TryFindExistingBuilderAndCacheFor(type);
    }

    /// <summary>
    /// Resets the builder type cache, in case you really need that to happen
    /// </summary>
    public static void InvalidateBuilderTypeCache()
    {
        lock (BuilderTypeCache)
        {
            BuilderTypeCache.Clear();
        }
    }

    private static readonly Dictionary<Type, Type> BuilderTypeCache =
        new Dictionary<Type, Type>();

    private static Type TryFindExistingBuilderAndCacheFor(Type type)
    {
        var result = TryFindBuilderInCurrentAssemblyFor(type)
            ?? TryFindBuilderInAnyOtherAssemblyInAppDomainFor(type);
        CacheBuilderType(type, result);
        return result;
    }

    private static void CacheBuilderType(Type type, Type builderType)
    {
        if (type == null)
            return;
        lock (BuilderTypeCache)
        {
            BuilderTypeCache[type] = builderType;
        }
    }

    private static void TryCacheBuilderType(Type builderType)
    {
        try
        {
            var genericBuilder = builderType.TryFindGenericBuilderInClassHeirachy();
            if (genericBuilder.GenericTypeArguments.Length != 2)
                return;
            var builtType = genericBuilder.GenericTypeArguments[1]; // Naive, but let's run with it
            CacheBuilderType(builderType, builtType);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to cache builder type {builderType?.Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Attempts to find a dynamic builder (ie, generated GenericBuilder type)
    /// for the provided type. Will cause generation of a new GenericBuilder implementation
    /// if an existing one cannot be found.
    /// </summary>
    /// <param name="type">Type to find a builder for</param>
    /// <returns>GenericBuilder type which is capable of building the provided type</returns>
    public static Type FindOrGenerateDynamicBuilderFor(Type type)
    {
        var result = GenericBuilderBase.ReuseOrGenerateDynamicBuilderFor(type);
        CacheBuilderType(type, result);
        return result;
    }

    private static Type[] TryGetExportedTypesFrom(Assembly asm)
    {
        try
        {
            return asm.GetExportedTypes();
        }
        catch
        {
            return new Type[]
            {
            };
        }
    }

    internal static Type TryFindBuilderInAnyOtherAssemblyInAppDomainFor(Type propertyType)
    {
        try
        {
            LoadImmediateAssembliesIfRequired();
            var allBuilders = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a != propertyType.Assembly && !a.IsDynamic)
                .SelectMany(TryGetExportedTypesFrom)
                .Where(t => t.IsABuilder())
                .ToArray();
            allBuilders.ForEach(TryCacheBuilderType);

            var types = allBuilders.Where(t => t.IsBuilderFor(propertyType))
                .ToArray();
            if (!types.Any())
                return null;
            return types.Length == 1
                ? types.First()
                : FindClosestNamespaceMatchFor(propertyType, types);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(
                "Error whilst searching for user builder for type '" + propertyType.PrettyName() +
                "' in all loaded assemblies: " + ex.Message
            );
            return null;
        }
    }

    private static readonly object ReferenceLoadLock = new object();
    private static bool _haveLoadedImmediateAssemblies;

    private static void LoadImmediateAssembliesIfRequired()
    {
        lock (ReferenceLoadLock)
        {
            if (_haveLoadedImmediateAssemblies)
                return;
            _haveLoadedImmediateAssemblies = true;
            AppDomain.CurrentDomain.GetAssemblies().ForEach(LoadReferencedAssemblies);
        }
    }

    private static void LoadReferencedAssemblies(Assembly asm)
    {
        try
        {
            Debug.WriteLine($"Attempting to load references of: {asm.FullName}");
            asm.GetReferencedAssemblies().ForEach(
                rasm =>
                {
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == rasm.FullName))
                    {
                        Debug.WriteLine($" -- {rasm.FullName} already loaded!");
                        return;
                    }

                    try
                    {
                        Assembly.Load(rasm.FullName);
                        Debug.WriteLine($" -> Loaded {rasm.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(
                            $"Unable to load referenced assembly {rasm.FullName} for {asm.FullName}: {ex.Message}"
                        );
                    }
                }
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to enumerate referenced assemblies for {asm.FullName}: {ex.Message}");
        }
    }

    private static Type TryFindBuilderInCurrentAssemblyFor(Type propType)
    {
        try
        {
            var allCurrentAsmBuilders = propType.GetAssembly().GetTypes()
                .Where(t => t.IsABuilder())
                .ToArray();
            allCurrentAsmBuilders.ForEach(TryCacheBuilderType);
            return allCurrentAsmBuilders.FirstOrDefault(t => t.IsBuilderFor(propType));
        }
        catch (Exception ex)
        {
#if NETSTANDARD
#else
                Trace.WriteLine("Error whilst searching for user builder for type '" + propType.PrettyName() +
                                "' in type's assembly: " + ex.Message);
#endif
            return null;
        }
    }

    private static Type FindClosestNamespaceMatchFor(Type propertyType, IEnumerable<Type> types)
    {
        if (propertyType?.Namespace == null) // R# is convinced this might happen :/
            return null;
        var seekNamespace = propertyType.Namespace.Split('.');
        return types.Aggregate(
            (Type)null,
            (acc, cur) =>
            {
                if (acc?.Namespace == null || cur.Namespace == null)
                    return cur;
                var accParts = acc.Namespace.Split('.');
                var curParts = cur.Namespace.Split('.');
                var accMatchIndex = seekNamespace.MatchIndexFor(accParts);
                var curMatchIndex = seekNamespace.MatchIndexFor(curParts);
                return accMatchIndex > curMatchIndex
                    ? acc
                    : cur;
            }
        );
    }
}