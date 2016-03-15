using System;
using System.Linq;
using GenericBuilderTestArtifactBuilders;
using GenericBuilderTestArtifactEntities;
using GenericBuilderTestArtifactEntities.Sub1;
using NUnit.Framework;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestGenericBuilderLocator
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            EnforceLoadingArtifactAssemblies();
        }

        public class SomeClassWithKnownBuilder
        {
        }

        public class SomeClassWithKnownBuilderBuilder: GenericBuilder<SomeClassWithKnownBuilderBuilder, SomeClassWithKnownBuilder>
        {
        }

        [Test]
        public void TryFindExistingBuilderFor_WhenHaveExistingBuilder_ShouldUseIt()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.TryFindExistingBuilderFor(typeof(SomeClassWithKnownBuilder));

            //---------------Test Result -----------------------
            Assert.AreEqual(typeof(SomeClassWithKnownBuilderBuilder), result);
        }

        public class SomeClassWithoutABuilder1
        {
        }

        [Test]
        public void FindOrGenerateDynamicBuilderFor_GivenTypeWithNoBuilder_FirstCallShouldCreateBuilderType()
        {
            //---------------Set up test pack-------------------
            var type = typeof(SomeClassWithoutABuilder1);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(type);

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            var genericBuilderType = typeof(GenericBuilder<,>);
            var baseType = result.BaseType;
            Assert.AreEqual(genericBuilderType, baseType.GetGenericTypeDefinition());
            Assert.AreEqual(type, baseType.GetGenericArguments()[1]);
        }


        public class SomeClassWithoutABuilder2
        {
        }

        [Test]
        public void FindOrGenerateDynamicBuilderFor_GivenTypeWithNoBuilder_SecondCallShouldReturnSameBuilderType()
        {
            //---------------Set up test pack-------------------
            var type = typeof(SomeClassWithoutABuilder2);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result1 = GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(type);
            var result2 = GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(type);

            //---------------Test Result -----------------------
            Assert.AreEqual(result1, result2);
        }

        [Test]
        public void TryFindExistingBuilderFor_ShouldBeAbleToFindBuilderInAnotherAssembly()
        {
            //---------------Set up test pack-------------------
            var type = typeof(SomeEntityWithBuilder);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var builderType = GenericBuilderLocator.TryFindExistingBuilderFor(type);

            //---------------Test Result -----------------------
            Assert.AreEqual(typeof(SomeEntityWithBuilderBuilder), builderType);
        }

        private static void EnforceLoadingArtifactAssemblies()
        {
            var someEntityType = typeof (SomeEntityWithBuilder);
            var someEntityBuilderType = typeof (SomeEntityWithBuilderBuilder);
            Assert.IsNotNull(someEntityType);
            Assert.IsNotNull(someEntityBuilderType);
            var otherBuilder = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .ToArray();
            var builders = otherBuilder.Where(t => t.IsBuilderFor(typeof (SomeEntityWithBuilder)));
            CollectionAssert.IsNotEmpty(builders);
        }

        [Test]
        public void TryFindExistingBuilderFor_WhenCannotFindExistingBuilderInAnotherAssembly_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            var type = typeof(SomeEntityWithoutBuilder);

            //---------------Assert Precondition----------------
            var existingBuilder = AppDomain.CurrentDomain
                                            .GetAssemblies()
                                            .SelectMany(a => a.GetTypes())
                                            .Where(t => t.IsBuilderFor(type));
            CollectionAssert.IsEmpty(existingBuilder);
            //---------------Execute Test ----------------------
            var builderType = GenericBuilderLocator.TryFindExistingBuilderFor(type);

            //---------------Test Result -----------------------
            Assert.IsNull(builderType);
        }

        [Test]
        public void GetBuilderFor_WhenCannotFindExistingBuilderForType_ShouldGenerateIt()
        {
            //---------------Set up test pack-------------------
            var type = typeof(AnotherEntityWithoutBuilder);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var builder = GenericBuilderLocator.GetBuilderFor(type);
            var dynamicBuilder = GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(type);

            //---------------Test Result -----------------------
            Assert.IsNotNull(builder);
            Assert.AreEqual(dynamicBuilder, builder);
        }

        [Test]
        public void TryFindExistingBuilderFor_WhenHaveMultipleBuilders_ShouldReturnBuilderWithClosestNamespaceMatch()
        {
            //---------------Set up test pack-------------------
            var entityType = typeof(AnotherEntityWithBuilder);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.TryFindExistingBuilderFor(entityType);

            //---------------Test Result -----------------------
            Assert.AreEqual(typeof(GenericBuilderTestArtifactBuilders.Sub1.AnotherEntityWithBuilderBuilder),
                            result);
        }

        [Test]
        public void GetBuilderForType_GivenNullType_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.GetBuilderFor(null);

            //---------------Test Result -----------------------
            Assert.IsNull(result);
        }

        [Test]
        public void TryFindExistingBuilderFor_GivenNullType_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.TryFindExistingBuilderFor(null);

            //---------------Test Result -----------------------
            Assert.IsNull(result);
        }


        [Test]
        public void FindOrGenerateDynamicBuilderFor_GivenNull_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(null);

            //---------------Test Result -----------------------
            Assert.IsNull(result);
        }


    }
}
