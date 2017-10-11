using System;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Exception thrown when the PeanutButter runtime is unable to
    /// automatically generate a required builder for a type. Happens most
    /// often if the type is inaccessible to the PeanutButter runtime. If the
    /// type to be built is internal, it is recommended to make internals visible
    /// to PeanutButter.RandomGenerators; otherwise the builder cannot be generated
    /// and the consuming code will need to provide a builder which, if it is public,
    /// will be discovered and used instead of attempting to generate a builder.
    /// </summary>
    public class UnableToCreateDynamicBuilderException : Exception
    {
        /// <summary>
        /// Type for which a builder was sought
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Constructs a new instasnce of the exception
        /// </summary>
        /// <param name="type">Type for which the runtime was attempting to generate a builder</param>
        /// <param name="typeLoadException">The actual exception thrown; may assist in resolving the issue</param>
        public UnableToCreateDynamicBuilderException(Type type, TypeLoadException typeLoadException): 
            base($"Unable to create dynamic builder for type {type.PrettyName()}. If {type.PrettyName()} is internal, you should make InternalsVisibleTo \"PeanutButter.RandomGenerators.GeneratedBuilders\". If {type.PrettyName()} is nested, ensure that all parents are public.",
                typeLoadException)
        {
            Type = type;
        }
    }
}