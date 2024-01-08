namespace PeanutButter.Utils.Tests
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
            Expect(result)
                .To.Equal("StringProperty");
        }

        [Test]
        public void GetMemberPathFor_ShouldBeAbleToResolveChildPaths()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ExpressionUtil.GetMemberPathFor<SomeThing>(o => o.Child.StringProperty);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal("Child.StringProperty");
        }


        [Test]
        public void GetPropertyTypeFor_ShouldReturnTypeOfPropertyReferencedByExpression()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ExpressionUtil.GetPropertyTypeFor<SomeThing>(o => o.StringProperty);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(typeof(string));
        }


    }
}
