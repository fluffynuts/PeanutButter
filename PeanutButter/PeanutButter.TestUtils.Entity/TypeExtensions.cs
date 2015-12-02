using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.TestUtils.Entity
{
    public static class TypeExtensions
    {
        public static void ShouldHaveRequiredProperty(this Type type, string name)
        {
            type.ShouldHaveProperty(name);
            var propertyInfo = Generic.TypeExtensions.GetPropertyForPath(type, name);
            var requiredAttribute = propertyInfo.GetCustomAttributes(true).OfType<RequiredAttribute>().FirstOrDefault();
            Assert.IsNotNull(requiredAttribute, "Expected property '{0}' to be decorated with [Required]", name);
        }

    }
}
