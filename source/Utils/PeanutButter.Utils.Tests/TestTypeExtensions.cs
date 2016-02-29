using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestTypeExtensions
    {
        [Test]
        public void Ancestry_WorkingOnTypeOfObject_ShouldReturnOnlyObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(object);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.Ancestry();

            //---------------Test Result -----------------------
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(typeof(object), result[0]);
        }

        public class SimpleType
        {
        }

        [Test]
        public void Ancestry_WorkingOnSimpleType_ShouldReturnItAndObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(SimpleType);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.Ancestry();

            //---------------Test Result -----------------------
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(typeof(object), result[0]);
            Assert.AreEqual(typeof(SimpleType), result[1]);
        }

        public class InheritedType: SimpleType
        {
        }

        [Test]
        public void Ancestry_WorkingOnInheritedType_ShouldReturnItAndObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(InheritedType);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.Ancestry();

            //---------------Test Result -----------------------
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(typeof(object), result[0]);
            Assert.AreEqual(typeof(SimpleType), result[1]);
            Assert.AreEqual(typeof(InheritedType), result[2]);
        }


    }
}
