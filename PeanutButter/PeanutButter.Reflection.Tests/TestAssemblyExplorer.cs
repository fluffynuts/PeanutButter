using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace PeanutButter.Reflection.Tests
{
    [TestFixture]
    public class TestAssemblyExplorer
    {
        [Test]
        public void AddAssembly_GivenNullAssembly_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = new AssemblyExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => explorer.AddAssembly(null));
            //---------------Test Result -----------------------
            Assert.AreEqual("assembly", exception.ParamName);
        }

        [Test]
        public void AddAssembly_GivenAddedTwoEqualAssemblies_ShouldNotReAddTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = new AssemblyExplorer();
            Assembly assembly = typeof(TestAssemblyExplorer).Assembly;
            explorer.AddAssembly(assembly);
            //---------------Assert Precondition----------------
            Assert.AreNotEqual(0, explorer.Types.Count);
            //---------------Execute Test ----------------------
            int expectedNumberOfTypes = explorer.Types.Count;
            explorer.AddAssembly(assembly);
            //---------------Test Result -----------------------
            Assert.AreEqual(expectedNumberOfTypes, explorer.Types.Count);
        }

        [Test]
        public void AddAssembly_GivenAddedTwoDifferentAssemblies_ShouldAddTypesFromBoth()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = new AssemblyExplorer();
            explorer.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            Assembly assembly = typeof(AssemblyExplorer).Assembly;
            //---------------Assert Precondition----------------
            Assert.AreNotEqual(0, explorer.Types.Count);
            //---------------Execute Test ----------------------
            int numberOfTypesBeforeAddition = explorer.Types.Count;
            explorer.AddAssembly(assembly);
            //---------------Test Result -----------------------
            Assert.AreNotEqual(numberOfTypesBeforeAddition, explorer.Types.Count);
        }

        [Test]
        public void AddAssembly_GivenValidAssembly_ShouldDiscoverPublicTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = new AssemblyExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            explorer.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            bool hasPublicType = explorer.Types.Contains(typeof(TestAssemblyExplorer));
            //---------------Test Result -----------------------
            Assert.IsTrue(hasPublicType);
        }

        [Test]
        public void AddAssembly_GivenValidAssembly_ShouldDiscoverInternalTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = new AssemblyExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            explorer.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            bool hasInternalTypes = explorer.Types.Contains(typeof(TestNestedInternal));
            //---------------Test Result -----------------------
            Assert.IsTrue(hasInternalTypes);
        }

        [Test]
        public void Clone_GivenExpectedTypes_ShouldCreateIdenticalExplorerWithExpectedTypes()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer originalExplorer = new AssemblyExplorer();
            originalExplorer.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            Assert.IsNotEmpty(originalExplorer.Types);
            //---------------Execute Test ----------------------
            AssemblyExplorer clonedExplorer = originalExplorer.Clone();
            //---------------Test Result -----------------------
            CollectionAssert.AreEquivalent(originalExplorer.Types, clonedExplorer.Types);
        }

        [Test]
        public void Clone_GivenCloneIsModified_ShouldNotAffectOriginalExplorer()
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer originalExplorer = new AssemblyExplorer();
            originalExplorer.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            //---------------Assert Precondition----------------
            Assert.IsNotEmpty(originalExplorer.Types);
            //---------------Execute Test ----------------------
            AssemblyExplorer clonedExplorer = originalExplorer.Clone();
            //---------------Test Result -----------------------
            CollectionAssert.AreEquivalent(originalExplorer.Types, clonedExplorer.Types);
            clonedExplorer.AddAssembly<AssemblyExplorer>();
            CollectionAssert.AreNotEquivalent(originalExplorer.Types, clonedExplorer.Types);
        }

        [TestCase(typeof(TestNestedPrivate))]
        [TestCase(typeof(TestNestedProtected))]
        [TestCase(typeof(TestNestedInternal))]
        [TestCase(typeof(TestNestedProtectedInternal))]
        [TestCase(typeof(TestNestedPublic))]
        public void Construct_GivenValidAssembly_ShouldDiscoverNestedTypes(Type nestedType)
        {
            //---------------Set up test pack-------------------
            AssemblyExplorer explorer = CreateExplorer();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            bool hasTypes = explorer.Types.Contains(nestedType);
            //---------------Test Result -----------------------
            Assert.IsTrue(hasTypes);
        }

        private static AssemblyExplorer CreateExplorer()
        {
            AssemblyExplorer result = new AssemblyExplorer();
            result.AddAssembly(typeof(TestAssemblyExplorer).Assembly);
            return result;
        }

        private class TestBaseClass
        {
        }

        private class TestSealedClass
        {
        }

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

        private class TestNestedPrivate
        {
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
