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
            InstallHttpContextGenerator();
            InstallHttpRequestGenerator();
            InstallFormCollectionGenerator();
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