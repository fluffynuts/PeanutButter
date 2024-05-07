using System;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;

// ReSharper disable ExpressionIsAlwaysNull
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestCustomAttributeHelperExtensions
    {
        [Test]
        public void ToAttributeBuilder_GivenNullCustomAttributeData_ShouldThrow()
        {
            //--------------- Arrange -------------------
            CustomAttributeData data = null;
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => data.ToAttributeBuilder())
                .To.Throw<ArgumentNullException>()
                .With.Message.Containing("data");

            //--------------- Assert -----------------------
        }
    }
}