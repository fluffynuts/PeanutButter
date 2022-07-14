using NExpect;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

namespace PeanutButter.SimpleHTTPServer.Testability.Tests
{
    [TestFixture]
    public class TestHttpMethodsExtensions
    {
        [Test]
        public void Matches_WhenMethodsMatch_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var left = GetRandomMethodButNotAny();
            var right = left;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = left.Matches(right);
            var result2 = right.Matches(left);

            //--------------- Assert -----------------------
            Expect(result1).To.Be.True();
            Expect(result2).To.Be.True();
        }

        [Test]
        public void Matches_WhenMethodsDoNotMatch_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var left = GetRandomMethodButNotAny();
            var right = GetRandomMethodButNotAny();
            while (right == left)
                right = GetRandomMethodButNotAny();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = left.Matches(right);
            var result2 = right.Matches(left);

            //--------------- Assert -----------------------
            Expect(result1).To.Be.False();
            Expect(result2).To.Be.False();
        }

        [Test]
        public void Matches_IfOneIsAny_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var left = GetRandomMethodButNotAny();
            var right = HttpMethods.Any;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = left.Matches(right);
            var result2 = right.Matches(left);

            //--------------- Assert -----------------------
            Expect(result1).To.Be.True();
            Expect(result2).To.Be.True();
        }

        [Test]
        public void Matches_IfBothAreAny_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var left = HttpMethods.Any;
            var right = HttpMethods.Any;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = left.Matches(right);
            var result2 = right.Matches(left);

            //--------------- Assert -----------------------
            Expect(result1).To.Be.True();
            Expect(result2).To.Be.True();
        }

        [TestCase(HttpMethods.Get, "GET")]
        [TestCase(HttpMethods.Put, "Put")]
        [TestCase(HttpMethods.Post, "poSt")]
        [TestCase(HttpMethods.Delete, "Delete")]
        [TestCase(HttpMethods.Options, "options")]
        public void Matches_GivenMatchingVerb_ShouldReturnTrue(HttpMethods method, string verb)
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = method.Matches(verb);

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        [Test]
        public void Matches_GivenAnyVerb_WhenMethodIsAny_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var method = HttpMethods.Any;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = method.Matches(RandomValueGen.GetRandomFrom(new[] { "GET", "PUT", "POST", "Options", "PATCH" } ));

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        private HttpMethods GetRandomMethodButNotAny()
        {
            var result = RandomValueGen.GetRandom<HttpMethods>();
            return result == HttpMethods.Any
                    ? GetRandomMethodButNotAny()
                    : result;
        }

    }
}
