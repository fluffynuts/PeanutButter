using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic.NUnitAbstractions;
using PeanutButter.Utils;
using Assert = PeanutButter.TestUtils.Generic.NUnitAbstractions.Assert;

// ReSharper disable UnusedMember.Global

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
            if (hasMethod)
                return;
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
            ShouldInheritFrom(type, typeof(T));
        }

        /// <summary>
        /// Asserts that the type being operated on is assignable to TBase
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="TBase"></typeparam>
        public static void ShouldBeAssignableFrom<TBase>(this Type type)
        {
            type.ShouldBeAssignableFrom(typeof(TBase));
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

        /// <summary>
        /// Asserts that the given type being operated on should not
        /// be assignable from the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="shouldNotBeImplemented"></param>
        public static void ShouldNotBeAssignableFrom(this Type type, Type shouldNotBeImplemented)
        {
            if (shouldNotBeImplemented.IsAssignableFrom(type))
                Assert.Fail(type.PrettyName() + " should not implement " + shouldNotBeImplemented.PrettyName());
        }

        /// <summary>
        /// Asserts that the type being operated on should inherit
        /// from the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="shouldBeAncestor"></param>
        public static void ShouldInheritFrom(this Type type, Type shouldBeAncestor)
        {
            if (shouldBeAncestor.IsInterface)
                Assert.Fail(shouldBeAncestor.PrettyName() + " is not a class");
            ShouldBeAssignableFrom(type, shouldBeAncestor);
        }

        /// <summary>
        /// Asserts that the type being operated on
        /// should not inherit from the given type parameter T
        /// </summary>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldNotInheritFrom<T>(this Type type)
        {
            type.ShouldNotInheritFrom(typeof(T));
        }

        /// <summary>
        /// Asserts that the type being operated on
        /// should not inherit from the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="shouldNotBeAncestor"></param>
        public static void ShouldNotInheritFrom(this Type type, Type shouldNotBeAncestor)
        {
            if (shouldNotBeAncestor.IsInterface)
                Assert.Fail(shouldNotBeAncestor.PrettyName() + " is not a class");
            ShouldNotBeAssignableFrom(type, shouldNotBeAncestor);
        }

        /// <summary>
        /// Asserts that the type being operated on is abstract
        /// </summary>
        /// <param name="type"></param>
        public static void ShouldBeAbstract(this Type type)
        {
            if (!type.IsAbstract)
                Assert.Fail(type.PrettyName() + " should be abstract");
        }

        /// <summary>
        /// Asserts that, when the type is constructed with the specified
        /// constructor parameter set to null, the constructor shoult throw
        /// an ArgumentNullException, using ConstructorTestUtils
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameterName"></param>
        /// <param name="expectedType"></param>
        /// <exception cref="Exception"></exception>
        public static void ShouldThrowWhenConstructorParameterIsNull(this Type type, string parameterName,
            Type expectedType)
        {
            var methodName = nameof(ConstructorTestUtils.ShouldExpectNonNullParameterFor);
            var methodInfo = typeof(ConstructorTestUtils).GetMethod(methodName);
            if (methodInfo == null)
                throw new AssertionException("Can't find method '" + methodName + "' on '" + typeof(ConstructorTestUtils).PrettyName() + "'");
            var genericMethod = methodInfo.MakeGenericMethod(type);
            try
            {
                genericMethod.Invoke(null, new object[] {parameterName, expectedType});
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>
        /// Provides an array containing the names of all
        /// virtual properties on a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] VirtualProperties(this Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(pi => pi.GetGetMethod().IsVirtual)
                .Select(pi => pi.Name)
                .ToArray();
        }

        /// <summary>
        /// Asserts that a type has the specified property by name and type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="withType"></param>
        /// <param name="shouldBeVirtual"></param>
        public static void ShouldHaveProperty(this Type type, string name, Type withType = null,
            bool shouldBeVirtual = false)
        {
            var propertyInfo = GetPropertyForPath(type, name);
            if (withType != null && withType != propertyInfo.PropertyType)
                Assert.AreEqual(withType, propertyInfo.PropertyType,
                    "Found property '" + name + "' but not with expected type '" + withType.PrettyName() + "'");
            if (shouldBeVirtual)
                Assert.IsTrue(propertyInfo.GetAccessors().First().IsVirtual);
        }

        /// <summary>
        /// Asserts that a type does not have the specified property
        /// by name, and optionally by name and type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="withType"></param>
        public static void ShouldNotHaveProperty(this Type type, string name, Type withType = null)
        {
            Assert.IsNotNull(type, "Cannot interrogate properties on NULL type");
            var propertyInfo = FindPropertyInfoForPath(type, name);
            if (withType == null)
                Assert.IsNull(propertyInfo, $"Expected not to find property {name} on type {type.Name}");
            if (propertyInfo != null && propertyInfo.PropertyType == withType)
                Assert.Fail($"Expected not to find property {name} with type {withType} on type {type.Name}");
        }

        /// <summary>
        /// Asserts that a type does not have the property specifed by
        /// name and type set by generic parameter T
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldNotHaveProperty<T>(
            this Type type,
            string name
        )
        {
            type.ShouldNotHaveProperty(name, typeof(T));
        }

        /// <summary>
        /// Asserts that a read-only property specified by
        /// name and property type exists on the type
        /// being operated on
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="withType"></param>
        public static void ShouldHaveReadOnlyProperty(
            this Type type,
            string name,
            Type withType = null
        )
        {
            var propInfo = FindPropertyInfoForPath(type, name, Assert.Fail);
            if (withType != null)
                Assert.AreEqual(withType, propInfo.PropertyType,
                    $"Expected {type.Name}.{name} to have type {withType}, but found {propInfo.PropertyType}");
            Assert.IsNull(propInfo.GetSetMethod(), $"Expected {type.Name}.{name} to be read-only");
        }

        /// <summary>
        /// Asserts that a read-only property specified by
        /// name and property type (T) exists on the type
        /// being operated on
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public static void ShouldHaveReadOnlyProperty<T>(this Type type, string name)
        {
            type.ShouldHaveReadOnlyProperty(name, typeof(T));
        }

        /// <summary>
        /// Finds the names of all properties which are not found on
        /// both the master type type and the comparison type; does
        /// not care about property types
        /// </summary>
        /// <param name="masterType"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static string[] NonIntersectingPropertiesFor(this Type masterType, Type comparisonType)
        {
            var props1 = masterType.PropertyNames();
            var props2 = comparisonType.PropertyNames();
            return props1.Except(props2).Union(props2.Except(props1)).ToArray();
        }

        /// <summary>
        /// Finds the names of all properties which are the shared
        /// by both the master type and the comparison type; does not
        /// care about property type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="otherType"></param>
        /// <returns></returns>
        public static string[] IntersectinPropertiesFor(this Type type, Type otherType)
        {
            return type.PropertyNames()
                .Intersect(otherType.PropertyNames())
                .ToArray();
        }

        /// <summary>
        /// Convenience function to get the names of all properties
        /// on a type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string[] PropertyNames(this Type type)
        {
            return type.GetProperties().Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// Searches for the PropertyInfo specifed by a property path
        /// which may contain dots, eg "Foo" or "Foo.Bar". Throws when
        /// the property cannot be found.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Searches for the PropertyInfo specifed by a property path
        /// which may contain dots, eg "Foo" or "Foo.Bar". Calls
        /// the (optional) toCallWhenTraversalFails action when traversal fails, ie
        /// when part of the path "Foo.Bar.Quux" cannot be followed. Returns
        /// null when the property cannot be found.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <param name="toCallWhenTraversalFails"></param>
        /// <returns></returns>
        public static PropertyInfo FindPropertyInfoForPath(
            this Type type,
            string path,
            Action<string> toCallWhenTraversalFails = null
        )
        {
            toCallWhenTraversalFails = toCallWhenTraversalFails ??
                                       (s =>
                                       {
                                       });
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

        /// <summary>
        /// Asserts that the type has the expected property by name
        /// and property type matching T
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        public static void ShouldHaveProperty<T>(this Type type, string name)
        {
            type.ShouldHaveProperty(name, typeof(T));
        }

        /// <summary>
        /// Asserts that the type has a non-public method with the provided
        /// name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public static void ShouldHaveNonPublicMethod(this Type type, string name)
        {
            var methodInfo = type.GetMethod(name,
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
                Assert.Fail($"Method not found: {name}");
            // ReSharper disable once PossibleNullReferenceException
            if (methodInfo.IsPublic)
                Assert.Fail($"Expected method '{name}' not to be public");
        }


        /// <summary>
        /// Tests if an instance of the provided type can have null
        /// assigned to it; ie strings, nullables and reference types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CanBeAssignedNull(this Type type)
        {
            // accepted answer from
            // http://stackoverflow.com/questions/1770181/determine-if-reflected-property-can-be-assigned-null#1770232
            // conveniently located as an extension method
            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// Tests if the type being operated on specifically matches
        /// or is a nullable-match for the other type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool MatchesOrIsNullableOf(this Type type, Type other)
        {
            var underlyingType = type.GetNullableGenericUnderlyingType();
            var underlyingOther = other.GetNullableGenericUnderlyingType();
            return underlyingType == underlyingOther;
        }

        /// <summary>
        /// Gets the underlying type constraint for a nullable or returns the original
        ///  type if the original type was not a Nullable<T>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetNullableGenericUnderlyingType(this Type type)
        {
            return (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
                ? type
                : Nullable.GetUnderlyingType(type);
        }

        /// <summary>
        /// Tests that an enum has a specified value by name
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valueName"></param>
        public static void ShouldHaveEnumValue(
            this Type type,
            string valueName
        )
        {
            type.ShouldHaveEnumValueInternal(valueName, null);
        }

        /// <summary>
        /// Tests that an enum has a specified value by name and value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="valueName"></param>
        /// <param name="expectedValue"></param>
        public static void ShouldHaveEnumValue(
            this Type type,
            string valueName,
            int expectedValue
        )
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
                    if (expectedValue == null || (int) value == expectedValue.Value)
                        return;
                    Assert.Fail(
                        $"Could not find enum key \"{valueName}\" with value \"{expectedValue}\" on enum {type.PrettyName()}"
                    );
                }
            }
            Assert.Fail($"Could not find value \"{valueName}\" on enum {type.PrettyName()}");
        }
    }
}