using System;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.Reflection.Extensions;

namespace PeanutButter.Reflection.Tests
{
    [TestFixture]
    public class TestExtensions
    {
        [Test]
        public void GetAccessibility_GivenDeclaredWithEventPrivateAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var eventName = "PrivateEvent";
            var eventInfo = GetTestEventInfo(eventName);
            //---------------Assert Precondition----------------
            Assert.NotNull(eventInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = eventInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Private, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenEventDeclaredWithProtectedAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var eventName = "ProtectedEvent";
            var eventInfo = GetTestEventInfo(eventName);
            //---------------Assert Precondition----------------
            Assert.NotNull(eventInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = eventInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Protected, actualAccessibility);
        }

        [Test]
        public void
            GetAccessibility_GivenEventDeclaredWithProtectedInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var eventName = "ProtectedInternalEvent";
            var eventInfo = GetTestEventInfo(eventName);
            //---------------Assert Precondition----------------
            Assert.NotNull(eventInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = eventInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.ProtectedInternal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenEventDeclaredWithInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var eventName = "InternalEvent";
            var eventInfo = GetTestEventInfo(eventName);
            //---------------Assert Precondition----------------
            Assert.NotNull(eventInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = eventInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Internal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenEventDeclaredWithPublicAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var eventName = "PublicEvent";
            var eventInfo = GetTestEventInfo(eventName);
            //---------------Assert Precondition----------------
            Assert.NotNull(eventInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = eventInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Public, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenMethodDeclaredWithPrivateAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var methodName = "PrivateMethod";
            var methodInfo = GetTestMethodInfo(methodName);
            //---------------Assert Precondition----------------
            Assert.NotNull(methodInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = methodInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Private, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenMethodDeclaredWithProtectedAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var methodName = "ProtectedMethod";
            var methodInfo = GetTestMethodInfo(methodName);
            //---------------Assert Precondition----------------
            Assert.NotNull(methodInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = methodInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Protected, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenMethodDeclaredWithProtectedInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var methodName = "ProtectedInternalMethod";
            var methodInfo = GetTestMethodInfo(methodName);
            //---------------Assert Precondition----------------
            Assert.NotNull(methodInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = methodInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.ProtectedInternal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenMethodDeclaredWithInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var methodName = "InternalMethod";
            var methodInfo = GetTestMethodInfo(methodName);
            //---------------Assert Precondition----------------
            Assert.NotNull(methodInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = methodInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Internal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenMethodDeclaredWithPublicAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var methodName = "PublicMethod";
            var methodInfo = GetTestMethodInfo(methodName);
            //---------------Assert Precondition----------------
            Assert.NotNull(methodInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = methodInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Public, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenFieldDeclaredWithPrivateAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var fieldName = "PrivateField";
            var fieldInfo = GetTestFieldInfo(fieldName);
            //---------------Assert Precondition----------------
            Assert.NotNull(fieldInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = fieldInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Private, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenFieldDeclaredWithProtectedAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var fieldName = "ProtectedField";
            var fieldInfo = GetTestFieldInfo(fieldName);
            //---------------Assert Precondition----------------
            Assert.NotNull(fieldInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = fieldInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Protected, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenFieldDeclaredWithProtectedInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var fieldName = "ProtectedInternalField";
            var fieldInfo = GetTestFieldInfo(fieldName);
            //---------------Assert Precondition----------------
            Assert.NotNull(fieldInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = fieldInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.ProtectedInternal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenFieldDeclaredWithInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var fieldName = "InternalField";
            var fieldInfo = GetTestFieldInfo(fieldName);
            //---------------Assert Precondition----------------
            Assert.NotNull(fieldInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = fieldInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Internal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenFieldDeclaredWithPublicAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var fieldName = "PublicField";
            var fieldInfo = GetTestFieldInfo(fieldName);
            //---------------Assert Precondition----------------
            Assert.NotNull(fieldInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = fieldInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Public, actualAccessibility);
        }


        [Test]
        public void GetAccessibility_GivenPropertyDeclaredWithPrivateAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var propertyName = "PrivateProperty";
            var propertyInfo = GetTestPropertyInfo(propertyName);
            //---------------Assert Precondition----------------
            Assert.NotNull(propertyInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = propertyInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Private, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenPropertyDeclaredWithProtectedAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var propertyName = "ProtectedProperty";
            var propertyInfo = GetTestPropertyInfo(propertyName);
            //---------------Assert Precondition----------------
            Assert.NotNull(propertyInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = propertyInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Protected, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenPropertyDeclaredWithProtectedInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var propertyName = "ProtectedInternalProperty";
            var propertyInfo = GetTestPropertyInfo(propertyName);
            //---------------Assert Precondition----------------
            Assert.NotNull(propertyInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = propertyInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.ProtectedInternal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenPropertyDeclaredWithInternalAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var propertyName = "InternalProperty";
            var propertyInfo = GetTestPropertyInfo(propertyName);
            //---------------Assert Precondition----------------
            Assert.NotNull(propertyInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = propertyInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Internal, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenPropertyDeclaredWithPublicAccessibility_ShouldReturnExpectedAccessibility()
        {
            //---------------Set up test pack-------------------
            var propertyName = "PublicProperty";
            var propertyInfo = GetTestPropertyInfo(propertyName);
            //---------------Assert Precondition----------------
            Assert.NotNull(propertyInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = propertyInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Public, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenPropertyWithLessAccessibleGetAccessor_ShouldReturnLeastAccessibilityOfLeastAccessibleAccessor()
        {
            //---------------Set up test pack-------------------
            var propertyName = "PropertyWithLessAccessibleGetAccessor";
            var propertyInfo = GetTestPropertyInfo(propertyName);
            //---------------Assert Precondition----------------
            Assert.NotNull(propertyInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = propertyInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Protected, actualAccessibility);
        }

        [Test]
        public void GetAccessibility_GivenPropertyWithLessAccessibleSetAccessor_ShouldReturnLeastAccessibilityOfLeastAccessibleAccessor()
        {
            //---------------Set up test pack-------------------
            var propertyName = "PropertyWithLessAccessibleSetAccessor";
            var propertyInfo = GetTestPropertyInfo(propertyName);
            //---------------Assert Precondition----------------
            Assert.NotNull(propertyInfo);
            //---------------Execute Test ----------------------
            var actualAccessibility = propertyInfo.GetAccessibility();
            //---------------Test Result -----------------------
            Assert.AreEqual(MemberAccessibility.Private, actualAccessibility);
        }

        private static EventInfo GetTestEventInfo(string eventName)
        {
            return typeof (TestClassWithEventsOfVaryingAccessibility).GetEvent(eventName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        private static FieldInfo GetTestFieldInfo(string fieldName)
        {
            return typeof (TestClassWithFieldsOfVaryingAccessibility).GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        private static MethodInfo GetTestMethodInfo(string methodName)
        {
            return typeof (TestClassWithMethodsOfVaryingAccessibility).GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        private static PropertyInfo GetTestPropertyInfo(string propertyName)
        {
            return typeof (TestClassWithPropertiesOfVaryingAccessibility).GetProperty(propertyName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        // ReSharper disable UnusedMember.Local

        private class TestClassWithEventsOfVaryingAccessibility
        {
#pragma warning disable 67
            private event Action PrivateEvent;
            protected event Action ProtectedEvent;
            protected internal event Action ProtectedInternalEvent;
            internal event Action InternalEvent;
            public event Action PublicEvent;
#pragma warning restore 67
        }

        private class TestClassWithMethodsOfVaryingAccessibility
        {
            
            private int PrivateMethod(int foo)
            {
                return foo + 1;
            }

            protected int ProtectedMethod(int foo)
            {
                return foo + 1;
            }

            protected internal int ProtectedInternalMethod(int foo)
            {
                return foo + 1;
            }

            internal int InternalMethod(int foo)
            {
                return foo + 1;
            }

            public int PublicMethod(int foo)
            {
                return foo + 1;
            }
        }


        private class TestClassWithFieldsOfVaryingAccessibility
        {
            // ReSharper disable InconsistentNaming
#pragma warning disable 169

            private int PrivateField;
            protected int ProtectedField;
            protected internal int ProtectedInternalField;
            internal int InternalField;
            public int PublicField;
#pragma warning restore 169
            // ReSharper restore InconsistentNaming
        }

        private class TestClassWithPropertiesOfVaryingAccessibility
        {
            private int PrivateProperty {  get { return 0; } }
            protected int ProtectedProperty { get { return 0; } }
            protected internal int ProtectedInternalProperty { get { return 0; } }
            internal int InternalProperty { get { return 0; } }
            public int PublicProperty { get { return 0; } }

            public int PropertyWithLessAccessibleGetAccessor
            {
                protected get { return 0; }
                // ReSharper disable once ValueParameterNotUsed
                set { }
            }
            public int PropertyWithLessAccessibleSetAccessor
            {
                get { return 0; }
                // ReSharper disable once ValueParameterNotUsed
                private set { }
            }
        }

        // ReSharper restore UnusedMember.Local
    }
}
