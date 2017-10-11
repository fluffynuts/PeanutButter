using System;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    internal static class BuilderFinderExtensions
    {
        private static readonly Type _genericBuilderBaseType = typeof(GenericBuilder<,>);
        private static readonly Type _objectType = typeof(object);

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
            while (current != _objectType && current != null)
            {
                if (current.IsGenericType())
                {
                    var genericBase = current.GetGenericTypeDefinition();
                    if (genericBase == _genericBuilderBaseType)
                        break;
                }
                current = current.BaseType();
            }
            if (current == _objectType || current == null)
                return null;
            return current;
        }
    }
}