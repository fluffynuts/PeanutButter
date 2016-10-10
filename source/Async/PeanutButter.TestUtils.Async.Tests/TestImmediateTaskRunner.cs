using NUnit.Framework;
using PeanutButter.Async;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.Async.Tests
{
    [TestFixture]
    public class TestImmediateTaskRunner
    {
        [Test]
        public void ImmediateTaskRunner_ContinuationTest()
        {
            //---------------Set up test pack-------------------
            var sut = ImmediateTaskRunnerBuilder.BuildDefault();
            var firstCalled = false;
            var secondCalled = false;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Run(() => firstCalled = true)
                .Using(sut).ContinueWith(t => { secondCalled = true; });

            //---------------Test Result -----------------------
            Assert.IsTrue(firstCalled);
            Assert.IsTrue(secondCalled);
        }

        [Test]
        public void ImmediateTaskRunner_ContinuationWithResultTest()
        {
            //---------------Set up test pack-------------------
            var sut = ImmediateTaskRunnerBuilder
                .Create()
                .WithSupportForTaskOfType<int>()
                .WithSupportForContinuationOfType<int, int>()
                .Build();
            var firstCalled = false;
            var secondCalled = false;
            var expected = RandomValueGen.GetRandomInt(1, 10);
            var result = -1;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Run(() =>
            {
                firstCalled = true;
                return expected;
            })
            .Using(sut).ContinueWith(t =>
            {
                secondCalled = true;
                result = expected;
                return result;
            });

            //---------------Test Result -----------------------
            Assert.IsTrue(firstCalled);
            Assert.IsTrue(secondCalled);
            Assert.AreEqual(expected, result);
        }
    }
}
