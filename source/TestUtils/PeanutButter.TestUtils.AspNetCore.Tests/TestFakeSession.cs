using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
    [TestFixture]
    public class TestFakeSession
    {
        [Test]
        public void ShouldBeAvailable()
        {
            // Arrange
            var sut = Create();
            // Act
            var result = sut.IsAvailable;
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        public void LoadShouldNotThrow()
        {
            // Arrange
            var sut = Create();
            // Act
            Expect(async () => await sut.LoadAsync())
                .Not.To.Throw();
            // Assert
        }

        [Test]
        public void CommitShouldNotThrow()
        {
            // Arrange
            var sut = Create();
            // Act
            Expect(async () => await sut.CommitAsync())
                .Not.To.Throw();
            // Assert
        }

        [Test]
        public void ShouldBeAbleToSetAndRetrieveAValue()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomBytes();
            var sut = Create();
            // Act
            Expect(sut.TryGetValue(key, out _))
                .To.Be.False();
            sut.Set(key, value);
            // Assert
            Expect(sut.TryGetValue(key, out var stored))
                .To.Be.True();
            Expect(stored)
                .To.Equal(value);
        }

        [Test]
        public void ShouldBeAbleToRemoveASingleItem()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomBytes();
            var sut = Create();
            sut.Set(key, value);
            // Act
            sut.Remove(key);
            // Assert
            Expect(sut.TryGetValue(key, out _))
                .To.Be.False();
        }

        [Test]
        public void RemovingAnItemWhichDoesntExistShouldNotThrow()
        {
            // Arrange
            var sut = Create();
            // Act
            Expect(() => sut.Remove(GetRandomString()))
                .Not.To.Throw();
            // Assert
        }

        [Test]
        public void ShouldBeAbleToClear()
        {
            // Arrange
            var key1 = GetRandomString();
            var value1 = GetRandomBytes();
            var key2 = GetRandomString();
            var value2 = GetRandomBytes();
            var sut = Create();
            sut.Set(key1, value1);
            sut.Set(key2, value2);
            
            Expect(sut.Keys)
                .To.Be.Equivalent.To(new[] { key1, key2 });
            
            // Act
            sut.Clear();
            
            // Assert
            Expect(sut.Keys)
                .To.Be.Empty();
            
        }

        private static ISession Create()
        {
            return new FakeSession();
        }
    }
}