using System;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.RandomGenerators
{
    public class GenericBuilderInstanceCreationException : Exception
    {
        public GenericBuilderInstanceCreationException(Type builderType, Type entityType)
            : base ($"{entityType.Name} does not have a default constructor or is not a class Type. " + 
                  $"You must override CreateInstance {builderType.PrettyName()} for this type to " + 
                  "provide an instance to work with.")
        {
        }
    }
}