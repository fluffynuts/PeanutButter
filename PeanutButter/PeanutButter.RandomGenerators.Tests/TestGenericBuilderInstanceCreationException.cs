using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            StringAssert.Contains(builderType.Name, sut.Message);
            StringAssert.Contains(entityType.Name, sut.Message);
            StringAssert.Contains("default constructor", sut.Message);
            StringAssert.Contains("override CreateInstance", sut.Message);
        }


    }
}
