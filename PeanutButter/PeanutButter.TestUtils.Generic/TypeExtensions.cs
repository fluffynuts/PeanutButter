using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace PeanutButter.TestUtils.Generic
{
    public static class TypeExtensions
    {
        public static bool HasActionMethodWithName(this Type type, string methodName)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                       .Any(mi => mi.Name == methodName &&
                                  !mi.GetParameters().Any() &&
                                  mi.ReturnParameter.ParameterType == typeof(void));
        }

        public static void ShouldHaveActionMethodWithName(this Type type, string methodName)
        {
            var hasMethod = type.HasActionMethodWithName(methodName);
            if (hasMethod) return;
            Assert.Fail("Expected to find method '" + methodName + "' on type '" + type.PrettyName() + "' but didn't.");
        }

        public static void ShouldImplement<T>(this Type type)
        {
            var shouldImplementType = typeof(T);
            if (!shouldImplementType.IsInterface)
                Assert.Fail(type.PrettyName() + " is not an interface");
            type.ShouldBeAssignableFrom(shouldImplementType);
        }

        public static void ShouldInheritFrom<T>(this Type type)
        {
            // syntactic sugar
            ShouldInheritFrom(type, typeof (T));
        }

        public static void ShouldBeAssignableFrom(this Type type, Type shouldBeImplemented)
        {
            if (!shouldBeImplemented.IsAssignableFrom(type))
                Assert.Fail(type.PrettyName() + " should implement " + shouldBeImplemented.PrettyName());
        }

        public static void ShouldNotImplement<T>(this Type type)
        {
            var shouldNotImplementType = typeof(T);
            type.ShouldNotBeAssignableFrom(shouldNotImplementType);
        }

        public static void ShouldNotBeAssignableFrom(this Type type, Type shouldNotBeImplemented)
        {
            if (shouldNotBeImplemented.IsAssignableFrom(type))
                Assert.Fail(type.PrettyName() + " should not implement " + shouldNotBeImplemented.PrettyName());
        }

        public static void ShouldInheritFrom(this Type type, Type shouldBeAncestor)
        {
            if (shouldBeAncestor.IsInterface)
                Assert.Fail(shouldBeAncestor.PrettyName() + " is not a class");
            ShouldBeAssignableFrom(type, shouldBeAncestor);
        }

        public static void ShouldNotInheritFrom(this Type type, Type shouldNotBeAncestor)
        {
            if (shouldNotBeAncestor.IsInterface)
                Assert.Fail(shouldNotBeAncestor.PrettyName() + " is not a class");
            ShouldNotBeAssignableFrom(type, shouldNotBeAncestor);
        }

        public static string PrettyName(this Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    var underlyingType = type.GetGenericArguments()[0];
                    return string.Format("{0}?", PrettyName(underlyingType));
                }
                var baseName = type.FullName.Substring(0, type.FullName.IndexOf("`"));
                var parts = baseName.Split(new[] { '.' });
                return parts.Last() + "<" + string.Join(", ", type.GetGenericArguments().Select(PrettyName)) + ">";
            }
            else
                return type.Name;
        }

        public static void ShouldBeAbstract(this Type type)
        {
            if (!type.IsAbstract)
                Assert.Fail(type.PrettyName() + " should be abstract");
        }

        public static void ShouldThrowWhenConstructorParameterIsNull(this Type type, string parameterName, Type expectedType)
        {
            var methodName = "ShouldExpectNonNullParameterFor";
            var methodInfo = typeof(ConstructorTestUtils).GetMethod(methodName);
            Assert.IsNotNull(methodInfo, "Can't find method '" + methodName + "' on '" + typeof(ConstructorTestUtils).PrettyName() + "'");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            try
            {
                genericMethod.Invoke(null, new object[] { parameterName, expectedType });
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public static string[] VirtualProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => pi.GetGetMethod().IsVirtual)
                .Select(pi => pi.Name)
                .ToArray();
        }
        public static void ShouldHaveProperty(this Type type, string name, Type withType = null, bool shouldBeVirtual = false)
        {
            var propertyInfo = GetPropertyForPath(type, name);
            if (withType != null && withType != propertyInfo.PropertyType)
                Assert.AreEqual(withType, propertyInfo.PropertyType, "Found property '" + name + "' but not with expected type '" + withType.PrettyName() + "'");
            if (shouldBeVirtual)
                Assert.IsTrue(propertyInfo.GetAccessors().First().IsVirtual);
        }

        public static string[] NonIntersectingPropertiesFor(this Type type, Type otherType)
        {
            var props1 = type.PropertyNames();
            var props2 = otherType.PropertyNames();
            return props1.Except(props2).Union(props2.Except(props1)).ToArray();
        }

        public static string[] IntersectinPropertiesFor(this Type type, Type otherType)
        {
            return type.PropertyNames()
                .Intersect(otherType.PropertyNames())
                .ToArray();
        }

        public static string[] PropertyNames(this Type type)
        {
            return type.GetProperties().Select(p => p.Name).ToArray();
        }

        public static PropertyInfo GetPropertyForPath(Type type, string name)
        {
            var propertyPath = name.Split('.');
            var traversed = new List<string>();
            PropertyInfo finalProperty = null;
            foreach (var part in propertyPath)
            {
                traversed.Add(part);

                var property = type
                    .GetProperties()
                    .FirstOrDefault(pi => pi.Name == part);
                Assert.IsNotNull(property, "Could not find property '" + string.Join(".", traversed) + "' on type " + type.PrettyName());
                if (traversed.Count == propertyPath.Length)
                {
                    finalProperty = property;
                    break;
                }
                type = property.PropertyType;
            }
            return finalProperty;
        }

        public static void ShouldHaveProperty<T>(this Type type, string name)
        {
            type.ShouldHaveProperty(name, typeof(T));
        }

        public static T[] AsArray<T>(this T input)
        {
            return new[] {input};
        }

    }
}
