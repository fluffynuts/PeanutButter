namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAsync
    {
        [Test]
        public void ShouldRunTheAsyncCodeSynchronously()
        {
            // Arrange
            var expected = GetRandomInt();
            // Act
            var result = Async.RunSync(() => Task.Run(() => expected));
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldRunTheAsyncCodeSynchronouslyWithAsyncAwait()
        {
            // Arrange
            var expected = GetRandomInt();
            // Act
            var result = Async.RunSync(async () => await Task.Run(() => expected));
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldNotBubbleAggregateExceptions()
        {
            // Arrange
            // Act
            Expect(() =>
                    Async.RunSync(() =>
                        Task.Run(() => throw new InvalidOperationException("Moo"))
                    )
                ).To.Throw<InvalidOperationException>()
                .With.Message("Moo");
            // Assert
        }

        [Test]
        public void ShouldNotBubbleAggregateExceptions2()
        {
            // Arrange
            // Act
            Expect(() =>
                    Async.RunSync(() =>
                        Task.Run(() =>
                        {
                            if (true)
                            {
                                throw new InvalidOperationException("Moo");
                            }
#pragma warning disable CS0162
                            return 0;
#pragma warning restore CS0162
                        })
                    )
                ).To.Throw<InvalidOperationException>()
                .With.Message("Moo");
            // Assert
        }
    }
}