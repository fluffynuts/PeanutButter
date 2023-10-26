using NExpect;
using NUnit.Framework;
using PeanutButter.Utils;
using static NExpect.Expectations;

namespace PeanutButter.TempRedis.Tests
{
    [TestFixture]
    public class TestTempRedisFactory
    {
        [Test]
        public void ShouldProvideAndReUseServers()
        {
            // Arrange
            // Act
            ITempRedis server1;
            ITempRedis server2;
            ITempRedis server3;

            using (var sut = Create())
            {
                using (var lease1 = sut.Borrow())
                {
                    server1 = lease1.Instance;
                    using var lease2 = sut.Borrow();
                    server2 = lease2.Instance;
                }

                using (var lease3 = sut.Borrow())
                {
                    server3 = lease3.Instance;
                }

                // Assert
                Expect(server1)
                    .To.Be.An.Instance.Of<TempRedis>()
                    .And
                    .Not.To.Be(server2);
                Expect(server3)
                    .To.Be.An.Instance.Of<TempRedis>()
                    .And
                    .To.Be(server1);
            }

            Expect(server1.IsDisposed)
                .To.Be.True();
            Expect(server2.IsDisposed)
                .To.Be.True();
            Expect(server3.IsDisposed)
                .To.Be.True();
        }

        [Test]
        public void ShouldFlushAllBeforeLeasing()
        {
            // Arrange
            using var sut = Create();
            // Act
            using (var lease1 = sut.Borrow())
            {
                var redis = lease1.Instance;
                redis.Store("foo", "bar");
                var set = redis.Fetch("foo");
                Expect(set)
                    .To.Equal("bar");
                Expect(redis.FetchKeys())
                    .Not.To.Be.Empty();
            }

            using (var lease = sut.Borrow())
            {
                var redis = lease.Instance;
                var keys = redis.FetchKeys();
                Expect(keys)
                    .To.Be.Empty();
            }
            // Assert
        }

        private static TempRedisFactory Create()
        {
            return new TempRedisFactory();
        }
    }
}