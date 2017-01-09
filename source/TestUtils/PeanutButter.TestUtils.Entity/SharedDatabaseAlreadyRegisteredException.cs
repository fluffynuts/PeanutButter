using System;
using System.Reflection;
using PeanutButter.TestUtils.Entity.Attributes;

namespace PeanutButter.TestUtils.Entity
{
    public class SharedDatabaseAlreadyRegisteredException : Exception
    {
        public SharedDatabaseAlreadyRegisteredException(string name)
            : base($"The shared database {name} is already registered")
        {
        }
    }

    public class SharedTempDbFeatureRequiresAssemblyAttributeException: Exception
    {
        private static readonly string _requiredAttributeTypeName = 
            nameof(AllowSharedTempDbInstancesAttribute).Replace("Attribute", "");
        public SharedTempDbFeatureRequiresAssemblyAttributeException(
            Type testFixtureType,
            Assembly asm
         )
            : base($"The UseSharedTempDb class attribute on {testFixtureType.Name} requires that assembly {asm.GetName().Name} have the attribute {_requiredAttributeTypeName}.\r\n\r\nTry adding the following to the top of a class file:\n[assembly: PeanutButter.TestUtils.Entity.Attributes.{_requiredAttributeTypeName}]")
        {
        }
    }
}