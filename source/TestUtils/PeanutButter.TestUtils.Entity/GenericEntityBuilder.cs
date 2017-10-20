using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity
{
    public class GenericEntityBuilder<TBuilder, TEntity> : GenericBuilder<TBuilder, TEntity> 
        where TBuilder: GenericBuilder<TBuilder, TEntity>, new()
    {
        private class CheckedProperty
        {
            public object Parent { get; private set; }
            public string PropertyName { get; private set; }

            public CheckedProperty(object parent, string name)
            {
                Parent = parent;
                PropertyName = name;
            }
        }

        public override TBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithProp(o => CheckMaxLengths(o));
        }

        private void CheckMaxLengths(object obj, List<CheckedProperty> checkedProperties = null)
        {
            if (obj == null)
                return;
            checkedProperties = checkedProperties ?? new List<CheckedProperty>();
            var thisObjectType = obj.GetType();
            var propInfos = thisObjectType.GetProperties();
            propInfos.ForEach(pi =>
            {
                if (checkedProperties.Any(cp => cp.PropertyName == pi.Name && cp.Parent == obj))
                    return;
                checkedProperties.Add(new CheckedProperty(obj, pi.Name));
                if (CanCheckMaxLengthsOn(pi))
                    CheckMaxLengths(pi.GetValue(obj), checkedProperties);
                else
                    ConstrainMaxlengthStringOn(obj, pi);
            });
        }

        private bool CanCheckMaxLengthsOn(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod.IsVirtual &&
                    !propertyInfo.PropertyType.IsCollection();
        }

        private static readonly Type _stringType = typeof(string);
        private void ConstrainMaxlengthStringOn(object o, PropertyInfo pi)
        {
            if (pi.PropertyType != _stringType)
                return;
            var prescribedMaxLength = GetMaxLengthFor(pi);
            if (!prescribedMaxLength.HasValue)
                return;
            var propertyValue = (string) pi.GetValue(o);
            if (propertyValue == null)
                return;
            if (propertyValue.Length > prescribedMaxLength.Value)
            {
                pi.SetValue(o, propertyValue.Substring(0, prescribedMaxLength.Value));
            }
        }

        private int? GetMaxLengthFor(PropertyInfo pi)
        {
            var maxLengthAttrib = pi.GetCustomAttributes<MaxLengthAttribute>().FirstOrDefault();
            if (maxLengthAttrib != null)
                return maxLengthAttrib.Length;
            var stringLengthAttrib = pi.GetCustomAttributes<StringLengthAttribute>().FirstOrDefault();
            return stringLengthAttrib?.MaximumLength;
        }
    }
}
