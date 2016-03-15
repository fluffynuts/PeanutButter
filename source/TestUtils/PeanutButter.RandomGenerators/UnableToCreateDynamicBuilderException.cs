using System;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.RandomGenerators
{
    public class UnableToCreateDynamicBuilderException : Exception
    {
        public Type Type { get; private set; }

        public UnableToCreateDynamicBuilderException(Type type, TypeLoadException typeLoadException): 
            base($"Unable to create dynamic builder for type {type.PrettyName()}. If {type.PrettyName()} is internal, you should make InternalsVisibleTo \"PeanutButter.RandomGenerators.GeneratedBuilders\"",
                typeLoadException)
        {
            Type = type;
        }
    }
}