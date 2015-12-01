using System;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoResetter
    {
        [Test]
        public void Construct_ShouldTakeTwoActions()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new AutoResetter(() => { }, () => { }));

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldCallFirstActionOnceOnly()
        {
            //---------------Set up test pack-------------------
            var constructCalls = 0;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new AutoResetter(() => constructCalls++,() => { });

            //---------------Test Result -----------------------
            Assert.AreEqual(1, constructCalls);
        }


        [Test]
        public void Type_ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AutoResetter);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Dispose_ShouldCallDisposalActionExactlyOnce()
        {
            //---------------Set up test pack-------------------
            var disposeCalls = 0;
            var sut = new AutoResetter(() => { }, () => disposeCalls++);
            //---------------Assert Precondition----------------
            Assert.AreEqual(0, disposeCalls);

            //---------------Execute Test ----------------------
            sut.Dispose();

            //---------------Test Result -----------------------
            Assert.AreEqual(1, disposeCalls);
        }

        [Test]
        public void Dispose_ShouldNotRecallDisposeActionWhenCalledAgain()
        {
            //---------------Set up test pack-------------------
            var disposeCalls = 0;
            var sut = new AutoResetter(() => { }, () => disposeCalls++);
            //---------------Assert Precondition----------------
            Assert.AreEqual(0, disposeCalls);

            //---------------Execute Test ----------------------
            sut.Dispose();
            sut.Dispose();

            //---------------Test Result -----------------------
            Assert.AreEqual(1, disposeCalls);
        }

        [Test]
        public void Construct_GivenFuncWhichReturnsT_ShouldProvideTValueToSecondFuncDuringDisposal()
        {
            //---------------Set up test pack-------------------
            var initial = RandomValueGen.GetRandomInt();
            var output = -1;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (new AutoResetter<int>(() => initial, v => output = v))
            {
            }

            //---------------Test Result -----------------------
            Assert.AreEqual(initial, output);
        }

        [Test]
        public void Dispose_ShouldOnlyCallActionOfT_Once()
        {
            //---------------Set up test pack-------------------
            var disposeCalls = 0;
            var sut = new AutoResetter<int>(() => 0, v => disposeCalls++);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.Dispose();
            Assert.AreEqual(1, disposeCalls);
            sut.Dispose();

            //---------------Test Result -----------------------
            Assert.AreEqual(1, disposeCalls);
        }




    }
}
