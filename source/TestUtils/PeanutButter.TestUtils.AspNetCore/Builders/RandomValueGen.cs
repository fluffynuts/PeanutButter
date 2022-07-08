using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    // ReSharper disable once UnusedType.Global
    internal static class RandomValueGen
    {
        // ReSharper disable once UnusedMember.Global
        public static void Init()
        {
            InstallGeneratorsForAspNetTypes();
            InstallGeneratorsForFakes();
        }

        private static void InstallGeneratorsForFakes()
        {
            InstallFormCollectionGenerator();
            InstallFormFileBuilder();
            InstallHttpContextGenerator();
            InstallHttpRequestGenerator();
            InstallHttpResponseGenerator();
            InstallRequestCookieCollectionBuilder();
            InstallWebSocketBuilder();
        }

        private static void InstallHttpResponseGenerator()
        {
            InstallRandomGenerator(
                () => HttpResponseBuilder.BuildRandom() as FakeHttpResponse
            );
            InstallRandomGenerator(
                HttpResponseBuilder.BuildRandom
            );
        }

        private static void InstallWebSocketBuilder()
        {
            InstallRandomGenerator(
                WebSocketManagerBuilder.BuildRandom
            );
            InstallRandomGenerator(
                WebSocketBuilder.BuildRandom
            );
        }

        private static void InstallRequestCookieCollectionBuilder()
        {
            InstallRandomGenerator(
                RequestCookieCollectionBuilder.BuildDefault
            );
            InstallRandomGenerator(
                () => RequestCookieCollectionBuilder.BuildDefault() as FakeRequestCookieCollection
            );
        }

        private static void InstallFormFileBuilder()
        {
            InstallRandomGenerator(
                FormFileBuilder.BuildRandom
            );
            InstallRandomGenerator(
                () => FormFileBuilder.BuildRandom() as FakeFormFile
            );
        }

        private static void InstallHttpContextGenerator()
        {
            InstallRandomGenerator(
                () => HttpContextBuilder.BuildRandom() as FakeHttpContext
            );
            InstallRandomGenerator(
                HttpContextBuilder.BuildRandom
            );
        }

        private static void InstallFormCollectionGenerator()
        {
            InstallRandomGenerator(
                () => FormBuilder.BuildRandom() as FakeFormCollection
            );
            InstallRandomGenerator(
                FormBuilder.BuildRandom
            );
        }

        private static void InstallHttpRequestGenerator()
        {
            InstallRandomGenerator<HttpRequest>(
                HttpRequestBuilder.BuildRandom
            );
            InstallRandomGenerator(
                () => HttpRequestBuilder.BuildRandom() as FakeHttpRequest
            );
        }

        private static void InstallGeneratorsForAspNetTypes()
        {
            InstallRandomGenerator(
                () => new HostString(
                    GetRandomHostname(),
                    GetRandomFrom(CommonPorts)
                )
            );
            InstallRandomGenerator(
                () => new PathString(GetRandomPath())
            );
            InstallRandomGenerator(
                () => new QueryString(GetRandomUrlQuery())
            );
            InstallRandomGenerator(
                HeaderDictionaryBuilder.BuildRandom
            );
        }

        private static readonly int[] CommonPorts =
        {
            80,
            443,
            5000,
            8000,
            8080
        };
    }
}