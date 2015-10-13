using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
{
    [TestFixture]
    public class TestExpressionHelper
    {
        public class SomeThing
        {
            public string StringProperty { get; set; }
            public SomeThing Child { get; set; }
        }

        [Test]
        public void GetMemberPathFor_ShouldReturnPathToImmediateMemberFromExpression()
        {
            //---------------Set up test pack-------------------
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ExpressionUtil.GetMemberPathFor<SomeThing>(o => o.StringProperty);

            //---------------Test Result -----------------------
            Assert.AreEqual("StringProperty", result);
        }

        [Test]
        public void GetMemberPathFor_ShouldBeAbleToResolveChildPaths()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ExpressionUtil.GetMemberPathFor<SomeThing>(o => o.Child.StringProperty);

            //---------------Test Result -----------------------
            Assert.AreEqual("Child.StringProperty", result);
        }


        [Test]
        public void GetPropertyTypeFor_ShouldReturnTypeOfPropertyReferencedByExpression()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ExpressionUtil.GetPropertyTypeFor<SomeThing>(o => o.StringProperty);

            //---------------Test Result -----------------------
            Assert.AreEqual(typeof(string), result);
        }


    }
}
