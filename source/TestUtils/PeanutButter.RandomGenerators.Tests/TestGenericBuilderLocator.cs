using System;
using System.Linq;
using GenericBuilderTestArtifactBuilders;
using GenericBuilderTestArtifactEntities;
using GenericBuilderTestArtifactEntities.Sub1;
using GenericBuilderTestLoadLoadedAssemblyObject;
using GenericBuilderTestNotLoadedAssembly;
using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleNullReferenceException

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
            Expect(result).To.Equal(typeof(SomeClassWithKnownBuilderBuilder));
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
            Expect(result).Not.To.Be.Null();
            var genericBuilderType = typeof(GenericBuilder<,>);
            var baseType = result.BaseType;
            Expect(genericBuilderType).To.Equal(baseType.GetGenericTypeDefinition());
            Expect(type).To.Equal(baseType.GetGenericArguments()[1]);
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
            Expect(builderType).To.Equal(typeof(SomeEntityWithBuilderBuilder));
        }

        private static void EnforceLoadingArtifactAssemblies()
        {
            var someEntityType = typeof (SomeEntityWithBuilder);
            var someEntityBuilderType = typeof (SomeEntityWithBuilderBuilder);
            Assert.IsNotNull(someEntityType);
            Assert.IsNotNull(someEntityBuilderType);
            var otherBuilder = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(
                    a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch
                        {
                            return new Type[0];
                        }
                    }
                ).ToArray();
            var builders = otherBuilder.Where(t => t.IsBuilderFor(typeof (SomeEntityWithBuilder)));
            CollectionAssert.IsNotEmpty(builders);
        }

        [Test]
        public void TryFindExistingBuilderFor_WhenCannotFindExistingBuilderInAnotherAssembly_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            var type = typeof(SomeEntityWithoutBuilder);

            //---------------Assert Precondition----------------
            var existingBuilders = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(
                    a =>
                    {
                        try
                        {
                            return a.GetTypes();
                        }
                        catch
                        {
                            return new Type[0];
                        }
                    }
                )
                .Where(t => t.IsBuilderFor(type))
                .ToArray();
            Expect(existingBuilders).To.Be.Empty();
            //---------------Execute Test ----------------------
            var builderType = GenericBuilderLocator.TryFindExistingBuilderFor(type);

            //---------------Test Result -----------------------
            Expect(builderType).To.Be.Null();
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
            Expect(builder).Not.To.Be.Null();
            Expect(dynamicBuilder).To.Equal(builder);
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
            Expect(typeof(GenericBuilderTestArtifactBuilders.Sub1.AnotherEntityWithBuilderBuilder))
                .To.Equal(result);
        }

        [Test]
        public void GetBuilderForType_GivenNullType_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.GetBuilderFor(null);

            //---------------Test Result -----------------------
            Expect(result).To.Be.Null();
        }

        [Test]
        public void TryFindExistingBuilderFor_GivenNullType_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.TryFindExistingBuilderFor(null);

            //---------------Test Result -----------------------
            Expect(result).To.Be.Null();
        }


        [Test]
        public void FindOrGenerateDynamicBuilderFor_GivenNull_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GenericBuilderLocator.FindOrGenerateDynamicBuilderFor(null);

            //---------------Test Result -----------------------
            Expect(result).To.Be.Null();
        }

        [Test]
        public void InvalidateBuilderTypeCache_ShouldNotThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(GenericBuilderLocator.InvalidateBuilderTypeCache).Not.To.Throw();

            //--------------- Assert -----------------------
        }

        [Test]
        public void ShouldFindBuilderInReferencedAssemblyNotYetLoaded()
        {
            // Arrange
            // Pre-Assert
            // Act
            var builderType = GenericBuilderLocator.TryFindBuilderInAnyOtherAssemblyInAppDomainFor(
                typeof(ObjectToBuildWithNotLoadedBuilder)
            );
            // Assert
            Expect(builderType).Not.To.Be.Null();
        }
    }

    public class RequiredToForceActualDependence
    {
        // But should not exist in TestGenericBuilderLocator so as to
        //  prevent loading
        private Type _moo = typeof(NotLoadedBuilder);
    }
}
