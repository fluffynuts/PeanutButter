using System;
using PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

internal static class BuilderFinderExtensions
{
    private static readonly Type GenericBuilderBaseType = typeof(GenericBuilder<,>);
    private static readonly Type ObjectType = typeof(object);

    public static bool IsBuilderFor(this Type t, Type toBuild)
    {
        var builderType = TryFindBuilderTypeInClassHeirachyFor(t, toBuild);
        return builderType != null;
    }

    public static bool IsABuilder(this Type t)
    {
        return t.TryFindGenericBuilderInClassHeirachy() != null;
    }

    private static Type TryFindBuilderTypeInClassHeirachyFor(Type potentialBuilder, Type buildType)
    {
        var current = TryFindGenericBuilderInClassHeirachy(potentialBuilder);
        if (current == null)
            return null;
        var typeParameters = current.GetGenericArguments();
        return typeParameters.Length > 1 && typeParameters[1] == buildType
            ? current
            : null;
    }

    internal static Type TryFindGenericBuilderInClassHeirachy(this Type current)
    {
        while (current != ObjectType && current != null)
        {
            if (current.IsGenericType())
            {
                var genericBase = current.GetGenericTypeDefinition();
                if (genericBase == GenericBuilderBaseType)
                    break;
            }

            current = current.BaseType();
        }

        if (current == ObjectType || current == null)
            return null;
        return current;
    }
}