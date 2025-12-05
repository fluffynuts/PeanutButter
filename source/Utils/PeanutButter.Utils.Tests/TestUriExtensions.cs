using System;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestUriExtensions
{
    [TestFixture]
    [Parallelizable]
    public class UriRoot
    {
        [TestFixture]
        [Parallelizable]
        public class WhenUriHasDefaultPort
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnSchemeAndHost()
            {
                // Arrange
                var uri = new Uri(GetRandomHttpsUrlWithPathAndParameters());
                Expect(uri.IsDefaultPort)
                    .To.Be.True();
                // Act
                var result = uri.ToString().UriRoot();
                // Assert
                Expect(result)
                    .To.Equal($"{uri.Scheme}://{uri.Host}");
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenUriHasNonStandardPort
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnSchemeAndHostAndPort()
            {
                // Arrange
                var port = GetRandomInt(500, 700);
                var uri = new Uri(
                    $"https://{GetRandomHostname()}:{port}/{GetRandomPath()}"
                );
                Expect(uri.IsDefaultPort)
                    .To.Be.False();
                // Act
                var result = uri.ToString().UriRoot();
                // Assert
                Expect(result)
                    .To.Equal($"{uri.Scheme}://{uri.Host}:{port}");
            }
        }
    }

    [TestFixture]
    [Parallelizable]
    public class Root
    {
        [TestFixture]
        [Parallelizable]
        public class WhenUriHasDefaultPort
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnSchemeAndHost()
            {
                // Arrange
                var uri = new Uri(GetRandomHttpsUrlWithPathAndParameters());
                Expect(uri.IsDefaultPort)
                    .To.Be.True();
                // Act
                var result = uri.Root();
                // Assert
                Expect(result)
                    .To.Equal(new Uri($"{uri.Scheme}://{uri.Host}"));
            }
        }

        [TestFixture]
        [Parallelizable]
        public class WhenUriHasNonStandardPort
        {
            [Test]
            [Parallelizable]
            public void ShouldReturnSchemeAndHostAndPort()
            {
                // Arrange
                var port = GetRandomInt(500, 700);
                var uri = new Uri(
                    $"https://{GetRandomHostname()}:{port}/{GetRandomPath()}"
                );
                Expect(uri.IsDefaultPort)
                    .To.Be.False();
                // Act
                var result = uri.Root();
                // Assert
                Expect(result)
                    .To.Equal(new Uri($"{uri.Scheme}://{uri.Host}:{port}"));
            }
        }
    }
}