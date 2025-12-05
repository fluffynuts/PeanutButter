using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestTaskExtensions
    {
        public class AsyncSource
        {
            public int DoStuffCalls { get; private set; }
            
            private readonly int _value;

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

            public async Task DoStuffAsync()
            {
                await Task.Run(() =>
                {
                    Thread.Sleep(100);
                    DoStuffCalls++;
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

        [Test]
        public void ShouldBeAbleToSynchronouslyWaitForVoidAsyncResult()
        {
            // Arrange
            var host = new AsyncSource(GetRandomInt());
            Expect(host.DoStuffCalls).To.Equal(0);
            // Act
            host.DoStuffAsync().WaitSync();
            // Assert
            Expect(host.DoStuffCalls).To.Equal(1);
        }
    }
}