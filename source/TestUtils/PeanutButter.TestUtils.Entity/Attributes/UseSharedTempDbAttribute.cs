using System;

namespace PeanutButter.TestUtils.Entity.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class UseSharedTempDbAttribute: Attribute
    {
        public string Name { get; }
        public UseSharedTempDbAttribute(string name)
        {
            Name = name;
        }
    }
}