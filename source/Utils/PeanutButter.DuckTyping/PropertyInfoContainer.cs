using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    public class PropertyInfoContainer
    {
        public Dictionary<string, PropertyInfo> PropertyInfos { get; }
        public Dictionary<string, PropertyInfo> FuzzyPropertyInfos { get; }
        public PropertyInfoContainer(
            PropertyInfo[] propertyInfos
        )
        {
            var distinct = propertyInfos
                            .Distinct(new PropertyInfoComparer())
                            .ToArray();
            PropertyInfos = distinct.ToDictionary(
                pi => pi.Name,
                pi => pi,
                StringComparer.InvariantCulture
            );
            FuzzyPropertyInfos = distinct.ToDictionary(
                pi => pi.Name,
                pi => pi,
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}