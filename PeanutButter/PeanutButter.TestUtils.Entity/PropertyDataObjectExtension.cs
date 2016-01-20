using System;
using System.Linq.Expressions;

namespace PeanutButter.TestUtils.Entity
{
    public static class PropertyDataObjectExtension
    {
        public static PropertyData<T> PropertyDataFor<T>(this T item, Expression<Func<T, object>> memberExpression)
        {
            return new PropertyData<T>(item, memberExpression);
        }
    }
}