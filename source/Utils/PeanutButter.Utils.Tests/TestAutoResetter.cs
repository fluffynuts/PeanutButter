using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;
using NExpect;

namespace PeanutButter.Utils.Tests
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
            // ReSharper disable once ObjectCreationAsStatement
            Expect(() => new AutoResetter(() => { }, () => { }))
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldCallFirstActionOnceOnly()
        {
            //---------------Set up test pack-------------------
            var constructCalls = 0;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            new AutoResetter(() => constructCalls++,() => { });

            //---------------Test Result -----------------------
            Expect(constructCalls).To.Equal(1);
        }


        [Test]
        public void Type_ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AutoResetter);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut).To.Implement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Dispose_ShouldCallDisposalActionExactlyOnce()
        {
            //---------------Set up test pack-------------------
            var disposeCalls = 0;
            var sut = new AutoResetter(() => { }, () => disposeCalls++);
            //---------------Assert Precondition----------------
            Expect(disposeCalls).To.Equal(0);

            //---------------Execute Test ----------------------
            sut.Dispose();

            //---------------Test Result -----------------------
            Expect(disposeCalls).To.Equal(1);
        }

        [Test]
        public void Dispose_ShouldNotRecallDisposeActionWhenCalledAgain()
        {
            //---------------Set up test pack-------------------
            var disposeCalls = 0;
            var sut = new AutoResetter(() => { }, () => disposeCalls++);
            //---------------Assert Precondition----------------
            Expect(disposeCalls).To.Equal(0);

            //---------------Execute Test ----------------------
            sut.Dispose();
            sut.Dispose();

            //---------------Test Result -----------------------
            Expect(disposeCalls).To.Equal(1);
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
            Expect(initial).To.Equal(output);
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
            Expect(disposeCalls).To.Equal(1);
            sut.Dispose();

            //---------------Test Result -----------------------
            Expect(disposeCalls).To.Equal(1);
        }
    }
}
