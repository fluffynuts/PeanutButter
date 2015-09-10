using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.Reflection.Extensions;

namespace PeanutButter.Reflection.Tests
{
    [TestFixture]
    public class TestTypeEnumeratorFilter
    {
        [TestCase(MemberAccessibility.Private)]
        [TestCase(MemberAccessibility.Protected)]
        [TestCase(MemberAccessibility.Internal)]
        [TestCase(MemberAccessibility.ProtectedInternal)]
        [TestCase(MemberAccessibility.Public)]
        public void FilterTypes_AtLeastAsVisibleAs_ShouldReturnTypesWithEqualOrGreaterVisibility(MemberAccessibility minimumExpectedAccessibility)
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AtLeastAsAccessibleAs(minimumExpectedAccessibility);
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() >= minimumExpectedAccessibility));
        }

        [TestCase(MemberAccessibility.Private)]
        [TestCase(MemberAccessibility.Protected)]
        [TestCase(MemberAccessibility.Internal)]
        [TestCase(MemberAccessibility.ProtectedInternal)]
        [TestCase(MemberAccessibility.Public)]
        public void FilterTypes_AtMostAsVisibleAs_GivenVisibility_ShouldReturnTypesWithLessOrEqualVisibility(MemberAccessibility maximumExpectedAccessibility)
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AtMostAsAccessibleAs(maximumExpectedAccessibility);
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() <= maximumExpectedAccessibility));
        }

        [TestCase(MemberAccessibility.Private)]
        [TestCase(MemberAccessibility.Protected)]
        [TestCase(MemberAccessibility.Internal)]
        [TestCase(MemberAccessibility.ProtectedInternal)]
        public void FilterTypes_MoreVisibleThan_GivenVisibility_ShouldReturnTypesWithGreaterVisibility(MemberAccessibility minimumAccessibility)
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().MoreAccessibleThan(minimumAccessibility);
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() > minimumAccessibility));
        }

        [Test]
        public void FilterTypes_MoreVisibleThan_GivenPublicVisibility_ShouldReturnNoTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().MoreAccessibleThan(MemberAccessibility.Public);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, enumerable.Count());
        }

        [TestCase(MemberAccessibility.Protected)]
        [TestCase(MemberAccessibility.Internal)]
        [TestCase(MemberAccessibility.ProtectedInternal)]
        [TestCase(MemberAccessibility.Public)]
        public void FilterTypes_LessVisibleThan_GivenVisibility_ShouldReturnTypesWithLessVisibility(MemberAccessibility maximumAccessibility)
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().LessAccessibleThan(maximumAccessibility);
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() < maximumAccessibility));
        }

        [Test]
        public void FilterTypes_LessVisibleThan_GivenPrivateVisibility_ShouldReturnNoTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().LessAccessibleThan(MemberAccessibility.Private);
            //---------------Test Result -----------------------
            Assert.AreEqual(0, enumerable.Count());
        }

        [Test]
        public void FilterTypes_PublicVisibility_ShouldReturnTypesWithExpectedVisibility()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().PublicAccessibility();
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() == MemberAccessibility.Public));
        }

        [Test]
        public void FilterTypes_ProtectedInternalVisibility_ShouldReturnTypesWithExpectedVisibility()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().ProtectedInternalAccessibility();
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() == MemberAccessibility.ProtectedInternal));
        }

        [Test]
        public void FilterTypes_InternalVisibility_ShouldReturnTypesWithExpectedVisibility()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().InternalAccessibility();
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() == MemberAccessibility.Internal));
        }

        [Test]
        public void FilterTypes_ProtectedVisibility_ShouldReturnTypesWithExpectedVisibility()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().ProtectedAccessibility();
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() == MemberAccessibility.Protected));
        }

        [Test]
        public void FilterTypes_PrivateVisibility_ShouldReturnTypesWithExpectedVisibility()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().PrivateAccessibility();
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => type.GetAccessibility() == MemberAccessibility.Private));
        }

        [Test]
        public void FilterTypes_DescendantsOf_GivenTestBaseClass_ShouldReturnTypesThatAreAssignableAsTestBaseClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            explorer.AddAssembly(typeof(AssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().DescendantsOf(typeof(TestBaseClass));
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => typeof(TestBaseClass).IsAssignableFrom(type)));
        }

        [Test]
        public void FilterTypes_DescendantsOf_GivenTestBaseClass_ShouldNotReturnAnyTypesThatAreInterfaces()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().DescendantsOf(typeof(TestBaseClass));
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            Assert.That(enumerable, Is.All.Matches<Type>(type => !type.IsInterface));
        }

        [Test]
        public void FilterTypes_DescendantsOf_GivenTestBaseClass_ShouldReturnTypesIncludingTestChildClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().DescendantsOf(typeof(TestBaseClass));
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestChildClass));
        }

        [Test]
        public void FilterTypes_DescendantsOf_GivenTestBaseClass_ShouldReturnTypesExcludingTestBaseClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().DescendantsOf(typeof(TestBaseClass));
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            CollectionAssert.DoesNotContain(enumerable, typeof(TestBaseClass));
        }

        [Test]
        public void FilterTypes_DescendantsOf_GivenAncestorTypeIsSealed_ShouldReturnNoTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            explorer.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().DescendantsOf(typeof(TestSealedClass));
            //---------------Test Result -----------------------
            Assert.AreEqual(0, enumerable.Count());
        }

        [Test]
        public void FilterTypes_AncestorsOf_GivenTestDescendantClass_ShouldReturnTypesExcludingTestDescendantClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AncestorsOf(typeof(TestDescendantClass));
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            CollectionAssert.DoesNotContain(enumerable, typeof(TestDescendantClass));
        }

        [Test]
        public void FilterTypes_AncestorsOf_GivenTestDescendantClass_ShouldReturnTypesIncludingTestBaseClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AncestorsOf(typeof(TestDescendantClass));
            //---------------Test Result -----------------------
            Assert.AreNotEqual(0, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestBaseClass));
        }

        [Test]
        public void FilterTypes_ChildrenOf_GivenTypeOfTestBaseClass_ShouldReturnTwoTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            explorer.AddAssembly(typeof(AssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().ChildrenOf(typeof(TestBaseClass));
            //---------------Test Result -----------------------
            Assert.AreEqual(2, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestChildClass));
            CollectionAssert.Contains(enumerable, typeof(TestChildClass.TestNestedClass));
        }

        [Test]
        public void FilterTypes_Sealed_GivenChildrenOfTestChildClass_ShouldReturnOnlyTypeOfTestDescendantClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            explorer.AddAssembly(typeof(AssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().ChildrenOf(typeof(TestChildClass)).AndAre.Sealed();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestDescendantClass));
        }

        [Test]
        public void FilterTypes_Nested_GivenChildrenOfTestBaseClass_ShouldReturnTwoTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            explorer.AddAssembly(typeof(AssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().ChildrenOf(typeof(TestBaseClass)).AndAre.Nested();
            //---------------Test Result -----------------------
            Assert.AreEqual(2, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestChildClass));
            CollectionAssert.Contains(enumerable, typeof(TestChildClass.TestNestedClass));
        }

        [Test]
        public void FilterTypes_Nested_GivenChildrenOfTestBaseClass_ShouldReturnOnlyTypeOfTestNestedClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            explorer.AddAssembly(typeof(AssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().ChildrenOf(typeof(TestBaseClass)).AndAre.Nested(typeof(TestChildClass));
            //---------------Test Result -----------------------
            Assert.AreEqual(1, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestChildClass.TestNestedClass));
        }

        [Test]
        public void FilterTypes_AttributedWith_GivenTypeOfDummySealedAttribute_ShouldReturnOnlyTypeOfTestBaseClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AttributedWith<DummySealedAttribute>();
            //---------------Test Result -----------------------
            Assert.AreEqual(1, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestBaseClass));
        }

        [Test]
        public void FilterTypes_AttributedWith_GivenTypeOfDummyBaseAttribute_ShouldReturnTypeOfTestChildClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AttributedWith(typeof(DummyBaseAttribute));
            //---------------Test Result -----------------------
            Assert.AreEqual(2, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestChildClass));
        }

        [Test]
        public void FilterTypes_AssignableFrom_GivenTypeOfTestChildClass_ShouldReturnTypesIncludingTypeOfTestBaseClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AssignableFrom(typeof(TestChildClass));
            //---------------Test Result -----------------------
            Assert.AreEqual(2, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestChildClass));
            CollectionAssert.Contains(enumerable, typeof(TestBaseClass));
        }

        [Test]
        public void FilterTypes_AssignableAs_GivenTypeOfTestChildClass_ShouldReturnExpectedTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().AssignableAs(typeof(TestChildClass));
            //---------------Test Result -----------------------
            Assert.AreEqual(3, enumerable.Count());
            CollectionAssert.Contains(enumerable, typeof(TestChildClass));
            CollectionAssert.Contains(enumerable, typeof(TestDescendantClass));
            CollectionAssert.Contains(enumerable, typeof(TestOtherDescendantClass));
        }

        [Test]
        public void FilterTypes_GenericTypeDefinition_ShouldReturnTypesIncludingTypeOfTestGenericTypeClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().GenericTypeDefinitions();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(TestGenericTypeClass<>));
        }

        [Test]
        public void FilterTypes_GenericParametersPresent_ShouldReturnTypesIncludingTypeOfTestGenericTypeClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().GenericParametersPresent();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(TestGenericTypeClass<>));
            CollectionAssert.DoesNotContain(enumerable, typeof(TestGenericParametersNotPresentClass));
        }

        [Test]
        public void FilterTypes_GenericParametersPresent_ShouldReturnTypesIncludingTypeOfTestGenericParametersPresentClass()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().GenericParametersPresent();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(TestGenericTypeClass<>));
            CollectionAssert.Contains(enumerable, typeof(TestGenericParametersPresentClass<>));
        }

        [Test]
        public void FilterTypes_GenericTypes_ShouldReturnTypesIncludingPartialConstructedGenericTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().GenericTypes();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(TestGenericTypeClass<>));
            CollectionAssert.Contains(enumerable, typeof(TestGenericParametersPresentClass<>));
        }

        [Test]
        public void FilterTypes_Interfaces_ShouldReturnTypesIncludingTestDiscoveryInterface()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().Interfaces();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(ITestDiscoveryInterface));
        }

        [Test]
        public void FilterTypes_Enumerations_ShouldReturnTypesIncludingTestDiscoveryEnum()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().Enumerations();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(TestDiscoveryEnum));
        }

        [Test]
        public void FilterTypes_Attributes_ShouldReturnTypesIncludingDummyBaseAttribute()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().Attributes();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(DummyBaseAttribute));
        }

        [Test]
        public void FilterTypes_Structures_ShouldReturnTypesIncludingTestDiscoveryStruct()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().Structures();
            //---------------Test Result -----------------------
            CollectionAssert.Contains(enumerable, typeof(TestDiscoveryStruct));
        }

        [Test]
        public void FilterTypes_ReferenceTypes_ShouldReturnTypesExcludingTestDiscoveryStruct()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            IEnumerable<Type> enumerable = explorer.FilterTypes().ReferenceTypes();
            //---------------Test Result -----------------------
            CollectionAssert.DoesNotContain(enumerable, typeof(TestDiscoveryStruct));
        }

        private static AssemblyExplorer CreateExplorer()
        {
            var result = new AssemblyExplorer();
            result.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            return result;
        }

        private struct TestDiscoveryStruct
        {
#pragma warning disable 649
            // ReSharper disable once UnusedField.Compiler
            public int First;
#pragma warning restore 649
        }

        private enum TestDiscoveryEnum
        {
            // ReSharper disable UnusedMember.Local
            None = 0,
            First = 1,
            Second = 2
            // ReSharper restore UnusedMember.Local
        }

        private interface ITestDiscoveryInterface
        {
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        private sealed class DummySealedAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        private class DummyBaseAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        private sealed class DummyChildAttribute : DummyBaseAttribute
        {
        }

        [DummySealed]
        private class TestBaseClass
        {
        }

        [DummyBase]
        private class TestSealedClass
        {
        }

        private class TestGenericTypeClass<TGenericParameter>
        {
            private readonly TGenericParameter _value;

            protected TestGenericTypeClass(TGenericParameter value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return _value.ToString();
            }
        }

        private sealed class TestGenericParametersNotPresentClass : TestGenericTypeClass<int>
        {
            public TestGenericParametersNotPresentClass(int value)
                : base(value)
            {
            }
        }

        private sealed class TestGenericParametersPresentClass<TDescendantParameter> : TestGenericTypeClass<int>
        {
            public TestGenericParametersPresentClass(int value)
                : base(value)
            {
            }

            public override string ToString()
            {
                return base.ToString() + "[" + typeof(TDescendantParameter).Name + "]";
            }
        }

        [DummyChild]
        private class TestChildClass : TestBaseClass, IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }

            public class TestNestedClass : TestBaseClass
            {
            }
        }

        private sealed class TestDescendantClass : TestChildClass, IReflectableType
        {
            public TypeInfo GetTypeInfo()
            {
                throw new NotImplementedException();
            }
        }

        private class TestOtherDescendantClass : TestChildClass, IReflectableType
        {
            public TypeInfo GetTypeInfo()
            {
                throw new NotImplementedException();
            }
        }

        protected class TestNestedProtected
        {
        }

        internal class TestNestedInternal
        {
        }

        protected internal class TestNestedProtectedInternal
        {
        }

        public class TestNestedPublic
        {
        }
    }
}
