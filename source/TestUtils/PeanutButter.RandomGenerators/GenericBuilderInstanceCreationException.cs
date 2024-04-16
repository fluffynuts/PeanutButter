using System;
using PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Exception thrown then the default method for constructing entities
/// within a GenericBuilder fails, normally because the entity being
/// built has no parameterless constructor.
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class GenericBuilderInstanceCreationException : Exception
{
    /// <summary>
    /// Constructs a new instance of the exception
    /// </summary>
    /// <param name="builderType">Type of the builder</param>
    /// <param name="entityType">Type of the entity to be built</param>
    public GenericBuilderInstanceCreationException(Type builderType, Type entityType)
        : base(
            $"{entityType.Name} does not have a parameterless constructor or is not a class Type. " +
            $"You must override {builderType.PrettyName()}.CreateInstance for this type to " +
            "provide an instance to work with."
        )
    {
    }
}