using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoResetter
    {
        [Test]
        public void ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AutoResetter);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut).To.Implement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [TestFixture]
        public class Construction
        {
            [Test]
            public void ShouldTakeTwoActions()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                // ReSharper disable once ObjectCreationAsStatement
                Expect(() => new AutoResetter(() =>
                    {
                    }, () =>
                    {
                    }))
                    .Not.To.Throw();

                //---------------Test Result -----------------------
            }

            [Test]
            public void ShouldCallFirstActionOnceOnly()
            {
                //---------------Set up test pack-------------------
                var constructCalls = 0;

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                // ReSharper disable once ObjectCreationAsStatement
                new AutoResetter(() => constructCalls++, () =>
                {
                });

                //---------------Test Result -----------------------
                Expect(constructCalls).To.Equal(1);
            }

            [Test]
            public void GivenFuncWhichReturnsT_ShouldProvideTValueToSecondFuncDuringDisposal()
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
        }


        [TestFixture]
        public class Disposal
        {
            [Test]
            public void ShouldCallDisposalActionExactlyOnce()
            {
                //---------------Set up test pack-------------------
                var disposeCalls = 0;
                var sut = new AutoResetter(() =>
                {
                }, () => disposeCalls++);
                //---------------Assert Precondition----------------
                Expect(disposeCalls).To.Equal(0);

                //---------------Execute Test ----------------------
                sut.Dispose();

                //---------------Test Result -----------------------
                Expect(disposeCalls).To.Equal(1);
            }

            [Test]
            public void ShouldNotRecallDisposeActionWhenCalledAgain()
            {
                //---------------Set up test pack-------------------
                var disposeCalls = 0;
                var sut = new AutoResetter(() =>
                {
                }, () => disposeCalls++);
                //---------------Assert Precondition----------------
                Expect(disposeCalls).To.Equal(0);

                //---------------Execute Test ----------------------
                sut.Dispose();
                sut.Dispose();

                //---------------Test Result -----------------------
                Expect(disposeCalls).To.Equal(1);
            }

            [Test]
            public void ShouldOnlyCallActionOfT_Once()
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

        [TestFixture]
        public class Statics
        {
            [Test]
            public void ShouldHaveStaticForTwoActions()
            {
                // Arrange
                var firstCalled = false;
                var secondCalled = false;
                // Act
                using (var sut = AutoResetter.Create(
                           () => firstCalled = true,
                           () => secondCalled = true
                       )
                      )
                {
                    Expect(firstCalled)
                        .To.Be.True();
                    Expect(secondCalled)
                        .To.Be.False();
                }

                // Assert
                Expect(secondCalled)
                    .To.Be.True();
            }

            [Test]
            public void ShouldHaveStaticForFuncs()
            {
                // Arrange
                var firstCalled = false;
                var firstResult = GetRandomInt();
                var captured = -1;
                var secondCalled = false;
                // Act
                using (var sut = AutoResetter.Create(
                           () =>
                           {
                               firstCalled = true;
                               return firstResult;
                           },
                           i =>
                           {
                               secondCalled = true;
                               captured = i;
                           }
                       )
                      )
                {
                    Expect(firstCalled)
                        .To.Be.True();
                    Expect(secondCalled)
                        .To.Be.False();
                }

                // Assert
                Expect(secondCalled)
                    .To.Be.True();
                Expect(captured)
                    .To.Equal(firstResult);
            }
        }
    }
}