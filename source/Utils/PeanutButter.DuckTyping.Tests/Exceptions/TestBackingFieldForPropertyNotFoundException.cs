using System;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests.Exceptions
{
    [TestFixture]
    public class TestBackingFieldForPropertyNotFoundException: AssertionHelper
    {
        [Test]
        public void Type_ShouldInheritFrom_NotImplementedException()
        {
            //--------------- Arrange -------------------
            var sut = typeof(BackingFieldForPropertyNotFoundException);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            sut.ShouldInheritFrom<NotImplementedException>();

            //--------------- Assert -----------------------
        }

        [Test]
        public void Construct_ShouldSetMessage()
        {
            //--------------- Arrange -------------------
            var property = GetRandomString();
            var owningType = typeof(TestBackingFieldForPropertyNotFoundException);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = new BackingFieldForPropertyNotFoundException(
                owningType,
                property);

            //--------------- Assert -----------------------
            Expect(sut.Message, 
                Does.Contain($"find backing field _{property} on type {owningType.Name}")
            );
        }
    }
}
