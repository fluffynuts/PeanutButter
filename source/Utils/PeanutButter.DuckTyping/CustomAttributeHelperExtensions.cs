using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Contains an extension method to craete a CustomAttributeBuilder
    /// out of CustomAttributeData, largely based on code at:
    /// http://stackoverflow.com/questions/2365470/using-reflection-emit-to-copy-a-custom-attribute-to-another-method
    /// (apparently, this is how AutoFac does it!)
    /// </summary>
    public static class CustomAttributeHelperExtensions
    {
        /// <summary>
        /// Creates a CustomAttributeBuilder from the provided CustomAttributeData
        /// </summary>
        /// <param name="data">Data to use to create the CustomAttributeBuilder</param>
        /// <returns>A CustomAttributeBuilder created from the input data</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided CustomAttributeData is null</exception>
        public static CustomAttributeBuilder ToAttributeBuilder(this CustomAttributeData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var constructorArguments = new List<object>();
            foreach (var ctorArg in data.ConstructorArguments)
            {
                constructorArguments.Add(ctorArg.Value);
            }

            var propertyArguments = new List<PropertyInfo>();
            var propertyArgumentValues = new List<object>();
            var fieldArguments = new List<FieldInfo>();
            var fieldArgumentValues = new List<object>();
            foreach (var namedArg in data.NamedArguments ?? new List<CustomAttributeNamedArgument>())
            {
                var fi = namedArg.MemberInfo as FieldInfo;
                var pi = namedArg.MemberInfo as PropertyInfo;

                if (fi != null)
                {
                    fieldArguments.Add(fi);
                    fieldArgumentValues.Add(namedArg.TypedValue.Value);
                    continue;
                }
                if (pi != null)
                {
                    propertyArguments.Add(pi);
                    propertyArgumentValues.Add(namedArg.TypedValue.Value);
                }
            }
            return new CustomAttributeBuilder(
                data.Constructor,
                constructorArguments.ToArray(),
                propertyArguments.ToArray(),
                propertyArgumentValues.ToArray(),
                fieldArguments.ToArray(),
                fieldArgumentValues.ToArray());
        }
    }
}