using NUnit.Framework;

namespace PeanutButter.SimpleHTTPServer.Testability.Tests
{
    [TestFixture]
    public class TestHttpMethodExtensions
    {
        [TestCase(HttpMethods.Get, "GET", true)]
        [TestCase(HttpMethods.Get, "GeT", true)]
        [TestCase(HttpMethods.Post, "Post", true)]
        [TestCase(HttpMethods.Post, "POST", true)]
        [TestCase(HttpMethods.Post, "GET", false)]
        [TestCase(HttpMethods.Post, "GeT", false)]
        [TestCase(HttpMethods.Get, "Post", false)]
        [TestCase(HttpMethods.Get, "POST", false)]
        public void Matches_GivenMatch_ShouldReturnExpected_(HttpMethods method, string match, bool expected)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = method.Matches(match);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }

    }
}