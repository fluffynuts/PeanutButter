using System;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace PeanutButter.DuckTyping.Tests.Extensions
{
  [TestFixture]
    public class TestCustomAttributeHelperExtensions: AssertionHelper
    {
        [Test]
        public void ToAttributeBuilder_GivenNullCustomAttributeData_ShouldThrow()
        {
            //--------------- Arrange -------------------
            CustomAttributeData data = null;
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => data.ToAttributeBuilder(), 
                Throws.Exception
                    .InstanceOf<ArgumentNullException>()
                    .With.Message.Contains("data"));

            //--------------- Assert -----------------------
        }
    }

}
