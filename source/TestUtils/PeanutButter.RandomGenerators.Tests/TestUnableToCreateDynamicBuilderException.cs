using System;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestUnableToCreateDynamicBuilderException: Exception
    {
        [Test]
        public void Type_ShouldInheritFromException()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(UnableToCreateDynamicBuilderException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_Given_Type_And_TypeLoaderException_ShouldSetProperties()
        {
            //---------------Set up test pack-------------------
            var type = GetType();
            var innerException = new TypeLoadException($"Access denied: {RandomValueGen.GetRandomString()}");

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new UnableToCreateDynamicBuilderException(type, innerException);

            //---------------Test Result -----------------------
            var typeName = type.PrettyName();
            Assert.AreEqual($"Unable to create dynamic builder for type {typeName}. If {typeName} is internal, you should make InternalsVisibleTo \"PeanutButter.RandomGenerators.GeneratedBuilders\"",
                sut.Message);
            Assert.AreEqual(type, sut.Type);
            Assert.AreEqual(innerException, sut.InnerException);
        }
    }
}