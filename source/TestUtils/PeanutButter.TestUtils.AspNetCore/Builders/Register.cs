namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Place this in your global setup to ensure that GetRandom&lt;T&gt;
    /// for types in this assembly "just work"
    /// </summary>
    public static class Register
    {
        public static void RandomGenerators()
        {
            FakeHttpRequestBuilder.InstallRandomGenerators();
            FakeFormBuilder.InstallRandomGenerators();
        }
    }
}