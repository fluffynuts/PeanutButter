using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.Entity.Tests
{
    [TestFixture]
    public class TestFluentTransformationExtensions
    {
        [Test]
        public void Transform_ShouldTransform()
        {
            //---------------Set up test pack-------------------
            var source = new
            {
                FirstName = RandomValueGen.GetRandomString(),
                LastName = RandomValueGen.GetRandomString()
            };
            var expected = source.FirstName + " " + source.LastName;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = source.Transform(o => new {FullName = o.FirstName + " " + o.LastName});

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result.FullName);
        }

    }
}
