using System;

namespace PeanutButter.RandomGenerators
{
    public static class BuilderFinderExtensions
    {
        private static Type _genericBuilderBaseType = typeof(GenericBuilder<,>);
        private static Type _objectType = typeof(object);

        public static bool IsBuilderFor(this Type t, Type toBuild)
        {
            var builderType = TryFindBuilderTypeInClassHeirachyFor(t, toBuild);
            return builderType != null;
        }

        private static Type TryFindBuilderTypeInClassHeirachyFor(Type potentialBuilder, Type buildType)
        {
            var current = potentialBuilder;
            while (current != _objectType && current != null)
            {
                if (current.IsGenericType)
                {
                    var genericBase = current.GetGenericTypeDefinition();
                    if (genericBase == _genericBuilderBaseType)
                        break;
                }
                current = current.BaseType;
            }
            if (current == _objectType || current == null)
                return null;
            var typeParameters = current.GetGenericArguments();
            return typeParameters.Length > 1 && typeParameters[1] == buildType 
                ? current 
                : null;
        }
    }
}