using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestGenericBuilderLocator
    {
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

    }
}
