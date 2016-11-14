using System;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.DuckTyping.Tests.Exceptions
{
    [TestFixture]
    public class TestWriteOnlyPropertyException: AssertionHelper
    {
        [Test]
        public void Type_ShouldInheritFrom_NotImplementedException()
        {
            //--------------- Arrange -------------------
            var sut = typeof(WriteOnlyPropertyException);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldInheritFrom<NotImplementedException>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void Construct_ShouldSetMessage()
        {
            //--------------- Arrange -------------------
            var propertyName = RandomValueGen.GetRandomString();
            var owningType = (new { }).GetType();
            var expected = $"Property {propertyName} on type {owningType.Name} is read-only";
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = new WriteOnlyPropertyException(owningType, propertyName);

            //--------------- Assert -----------------------
            Expect(result.Message, Is.EqualTo(expected));
        }

    }
}