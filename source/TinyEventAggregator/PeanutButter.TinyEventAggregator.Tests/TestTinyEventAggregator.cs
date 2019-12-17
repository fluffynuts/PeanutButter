using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ObjectCreationAsStatement

namespace PeanutButter.TinyEventAggregator.Tests
{
    [TestFixture]
    public class TestTinyEventAggregator
    {
        [TestFixture]
        public class Constructor
        {
            [Test]
            public void ShouldNotThrow()
            {
                // test setup

                // pre-conditions

                // execute test
                Expect(() => new EventAggregator())
                    .Not.To.Throw();

                // test result
            }
        }

        [TestFixture]
        public class StaticInstance
        {
            [Test]
            public void ShouldBeSingleton()
            {
                // test setup

                // pre-conditions

                // execute test
                var i1 = EventAggregator.Instance;
                var i2 = EventAggregator.Instance;

                // test result
                Expect(i1).Not.To.Be.Null();
                Expect(i2).Not.To.Be.Null();
                Expect(i1).To.Be(i2);
            }
        }

        [TestFixture]
        public class GetEvent
        {
            [Test]
            public void ShouldReturnTheSameInstanceOfTheSameEvent()
            {
                // test setup

                // pre-conditions

                // execute test
                var ev1 = EventAggregator.Instance.GetEvent<SomeEvent>();
                var ev2 = EventAggregator.Instance.GetEvent<SomeEvent>();

                // test result
                Expect(ev1).Not.To.Be.Null();
                Expect(ev2).Not.To.Be.Null();
                Expect(ev1).To.Be(ev2);
            }
        }

        public class SomeEvent : EventBase<object>
        {
        }
    }
}