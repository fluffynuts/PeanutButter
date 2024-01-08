using System;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestGenericBuilderInstanceCreationException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (GenericBuilderInstanceCreationException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        public class SomeBuilder
        {
        }

        public class SomeEntity
        {
        }

        [Test]
        public void Construct_ShouldSetMessage()
        {
            //---------------Set up test pack-------------------
            var builderType = typeof (SomeBuilder);
            var entityType = typeof (SomeEntity);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new GenericBuilderInstanceCreationException(builderType, entityType);

            //---------------Test Result -----------------------
            Expect(sut.Message)
                .To.Contain(builderType.Name)
                .And
                .To.Contain(entityType.Name)
                .And
                .To.Contain("parameterless constructor")
                .And
                .To.Contain("override CreateInstance");
        }


    }
}
