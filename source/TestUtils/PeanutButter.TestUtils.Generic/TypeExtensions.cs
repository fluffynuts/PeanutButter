using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.TestUtils.Generic.NUnitAbstractions;
using PeanutButter.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.Generic
{
    /// <summary>
    /// Provides extension methods on Type objects
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Tests that a type has a method with the given name and a void return
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static bool HasActionMethodWithName(this Type type, string methodName)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                       .Any(mi => mi.Name == methodName &&
                                  !mi.GetParameters().Any() &&
                                  mi.ReturnParameter?.ParameterType == typeof(void));
        }

        /// <summary>
        /// Asserts that a type has a method with the given name and void return
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        public static void ShouldHaveActionMethodWithName(this Type type, string methodName)
        {
            var hasMethod = type.HasActionMethodWithName(methodName);
            if (hasMethod) return;
            Assert.Fail("Expected to find method '" + methodName + "' on type '" + type.PrettyName() + "' but didn't.");
        }

        /// <summary>
        /// Asserts that a type implements the provided interface T
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldImplement<T>(this Type type)
        {
            var shouldImplementType = typeof(T);
            type.ShouldImplement(shouldImplementType);
        }

        /// <summary>
        /// Asserts that a type implements the provided interface type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="shouldImplementType">Provided interface type</param>
        public static void ShouldImplement(this Type type, Type shouldImplementType)
        {
            if (!shouldImplementType.IsInterface)
                Assert.Fail(type.PrettyName() + " is not an interface");
            type.ShouldBeAssignableFrom(shouldImplementType);
        }

        /// <summary>
        /// Asserts that the type being operated on inherits T
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldInheritFrom<T>(this Type type)
        {
            // syntactic sugar
            ShouldInheritFrom(type, typeof (T));
        }

        /// <summary>
        /// Asserts that the type being operated on is assignable to TBase
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="TBase"></typeparam>
        public static void ShouldBeAssignableFrom<TBase>(this Type type)
        {
            type.ShouldBeAssignableFrom(typeof (TBase));
        }

        /// <summary>
        /// Asserts that the type being operated on is assigneable from the provided type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="shouldBeImplemented">Provided type to test against</param>
        public static void ShouldBeAssignableFrom(this Type type, Type shouldBeImplemented)
        {
            if (!shouldBeImplemented.IsAssignableFrom(type))
                Assert.Fail(type.PrettyName() + " should implement " + shouldBeImplemented.PrettyName());
        }

        /// <summary>
        /// Asserts that the type being operated on does not implement interface T
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
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

        public static void ShouldNotInheritFrom<T>(this Type type)
        {
            type.ShouldNotInheritFrom(typeof(T));
        }
        public static void ShouldNotInheritFrom(this Type type, Type shouldNotBeAncestor)
        {
            if (shouldNotBeAncestor.IsInterface)
                Assert.Fail(shouldNotBeAncestor.PrettyName() + " is not a class");
            ShouldNotBeAssignableFrom(type, shouldNotBeAncestor);
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

        public static void ShouldNotHaveProperty(this Type type, string name, Type withType = null)
        {
            Assert.IsNotNull(type, "Cannot interrogate properties on NULL type");
            var propertyInfo = FindPropertyInfoForPath(type, name);
            if (withType == null)
                Assert.IsNull(propertyInfo, $"Expected not to find property {name} on type {type.Name}");
            if (propertyInfo != null && propertyInfo.PropertyType == withType)
                Assert.Fail($"Expected not to find property {name} with type {withType} on type {type.Name}");
        }

        public static void ShouldNotHaveProperty<T>(this Type type, string name)
        {
            type.ShouldNotHaveProperty(name, typeof (T));
        }

        public static void ShouldHaveReadOnlyProperty(this Type type, string name, Type withType = null)
        {
            var propInfo = FindPropertyInfoForPath(type, name, Assert.Fail);
            if (withType != null)
                Assert.AreEqual(withType, propInfo.PropertyType, 
                    $"Expected {type.Name}.{name} to have type {withType}, but found {propInfo.PropertyType}");
            Assert.IsNull(propInfo.GetSetMethod(), $"Expected {type.Name}.{name} to be read-only");
        }

        public static void ShouldHaveReadOnlyProperty<T>(this Type type, string name)
        {
            type.ShouldHaveReadOnlyProperty(name, typeof (T));
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
            string traversalFailure = null;
            var propertyInfo = FindPropertyInfoForPath(type, name, s => traversalFailure = s);
            return propertyInfo ?? ThrowForPropertyTraversalFailure(type, traversalFailure);
        }

        private static PropertyInfo ThrowForPropertyTraversalFailure(Type type, string traversalFailure)
        {
            Assertions.Throw($"Unable to traverse property {traversalFailure} on type {type.Name}");
            throw new Exception("Assertions.Throw should prevent getting here");
        }

        public static PropertyInfo FindPropertyInfoForPath(this Type type, string path, Action<string> toCallWhenTraversalFails = null)
        {
            toCallWhenTraversalFails = toCallWhenTraversalFails ?? (s => { });
            var propertyPath = path.Split('.');
            var traversed = new List<string>();
            PropertyInfo finalProperty = null;
            foreach (var part in propertyPath)
            {
                traversed.Add(part);

                var property = type
                    .GetProperties()
                    .FirstOrDefault(pi => pi.Name == part);
                if (property == null)
                {
                    toCallWhenTraversalFails(string.Join(".", traversed));
                    return null;
                }
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

        public static void ShouldHaveNonPublicMethod(this Type type, string name)
        {
            var methodInfo = type.GetMethod(name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
                Assert.Fail($"Method not found: {name}");
            if (methodInfo.IsPublic)
                Assert.Fail($"Expected method '{name}' not to be public");
        }


//        public static bool IsNotCollection(this Type type)
//        {
//            return !type.IsCollection();
//        }
//
//        public static bool IsCollection(this Type type)
//        {
//            if (type == null)
//                return false;
//            return type.IsArray ||
//                   (type.IsGenericType &&
//                    CollectionGenerics.Contains(type.GetGenericTypeDefinition()));
//        }

        public static bool CanBeAssignedNull(this Type type)
        {
            // accepted answer from
            // http://stackoverflow.com/questions/1770181/determine-if-reflected-property-can-be-assigned-null#1770232
            // conveniently located as an extension method
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool MatchesOrIsNullableOf(this Type type, Type other)
        {
            var underlyingType = type.GetUnderlyingType();
            var underlyingOther = other.GetUnderlyingType();
            return underlyingType == underlyingOther;
        }

        private static Type GetUnderlyingType(this Type type)
        {
            return (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
                ? type
                : Nullable.GetUnderlyingType(type);
        }

        public static void ShouldHaveEnumValue(this Type type, string valueName)
        {
            type.ShouldHaveEnumValueInternal(valueName, null);
        }

        public static void ShouldHaveEnumValue(this Type type, string valueName, int expectedValue)
        {
            type.ShouldHaveEnumValueInternal(valueName, expectedValue);
        }

        private static void ShouldHaveEnumValueInternal(this Type type, string valueName, int? expectedValue)
        {
            if (!type.IsEnum)
                throw new InvalidOperationException($"{type.PrettyName()} is not an enum type");
            var enumValues = Enum.GetValues(type);
            foreach (var value in enumValues)
            {
                var thisValueName = Enum.GetName(type, value);
                if (thisValueName == valueName)
                {
                    if (expectedValue == null || (int)value == expectedValue.Value)
                        return;
                    Assert.Fail(
                        $"Could not find enum key \"{valueName}\" with value \"{expectedValue}\" on enum {type.PrettyName()}"
                    );
                }
            }
            Assert.Fail($"Could not find value \"{valueName}\" on enum {type.PrettyName()}");
        }

        private static readonly Type[] CollectionGenerics =
        {
            typeof(ICollection<>),
            typeof(IEnumerable<>),
            typeof(List<>),
            typeof(IList<>),
            typeof(IDictionary<,>),
            typeof(Dictionary<,>)
        };
    }
}
