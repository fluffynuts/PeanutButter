using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestTaskExtensions
    {
        public class AsyncSource
        {
            private int _value;

            public AsyncSource(
                int value)
            {
                _value = value;
            }

            public async Task<int> GetValueAsync()
            {
                return await GetValueInternal();
            }

            private async Task<int> GetValueInternal()
            {
                // make sure we've gone through at least one await
                return await Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    return _value;
                });
            }
        }


        [Test]
        public void ShouldBeAbleToSynchronouslyRetrieveAsyncResult()
        {
            // Arrange
            var expected = GetRandomInt();
            var host = new AsyncSource(expected);
            // Pre-assert
            // Act
            var result = host.GetValueAsync().GetResultSync();
            // Assert
            Expect(result).To.Equal(expected);
        }
    }
}