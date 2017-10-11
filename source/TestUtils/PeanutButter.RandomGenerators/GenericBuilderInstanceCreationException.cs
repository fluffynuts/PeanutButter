using System;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Exception thrown then the default method for constructing entities
    /// within a GenericBuilder fails, normally because the entity being
    /// built has no parameterless constructor.
    /// </summary>
    public class GenericBuilderInstanceCreationException : Exception
    {
        /// <summary>
        /// Constructs a new instance of the exception
        /// </summary>
        /// <param name="builderType">Type of the builder</param>
        /// <param name="entityType">Type of the entity to be built</param>
        public GenericBuilderInstanceCreationException(Type builderType, Type entityType)
            : base ($"{entityType.Name} does not have a parameterless constructor or is not a class Type. " + 
                  $"You must override CreateInstance {builderType.PrettyName()} for this type to " + 
                  "provide an instance to work with.")
        {
        }
    }
}