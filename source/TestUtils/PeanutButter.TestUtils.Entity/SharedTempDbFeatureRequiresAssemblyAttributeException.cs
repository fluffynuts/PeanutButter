using System;
using PeanutButter.TestUtils.Entity.Attributes;

namespace PeanutButter.TestUtils.Entity
{
    public class SharedTempDbFeatureRequiresAssemblyAttributeException: Exception
    {
        private static readonly string RequiredAttributeTypeName =
            nameof(AllowSharedTempDbInstancesAttribute).Replace("Attribute", "");
        public SharedTempDbFeatureRequiresAssemblyAttributeException(
            Type testFixtureType
        )
            : base($"The UseSharedTempDb class attribute on {testFixtureType.Name} requires that assembly {testFixtureType.Assembly.GetName().Name} have the attribute {RequiredAttributeTypeName}.\r\n\r\nTry adding the following to the top of a class file:\n[assembly: PeanutButter.TestUtils.Entity.Attributes.{RequiredAttributeTypeName}]")
        {
        }
    }
}