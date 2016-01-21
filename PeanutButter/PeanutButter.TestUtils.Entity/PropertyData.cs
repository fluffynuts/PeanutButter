using System;
using System.Linq.Expressions;

namespace PeanutButter.TestUtils.Entity
{
    public static class PropertyData
    {
        public static PropertyData<T> For<T>(T item, Expression<Func<T, object>> propertyExpression)
        {
            return new PropertyData<T>(item, propertyExpression);
        }
    }

}