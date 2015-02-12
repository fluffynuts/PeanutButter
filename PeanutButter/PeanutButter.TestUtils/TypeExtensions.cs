using System;
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

        // TODO: implement ShouldBeAbstract

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

    }
}
