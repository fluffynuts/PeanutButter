using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.SimpleHTTPServer.Testability
{
    [TestFixture]
    public class TestHttpMethodsExtensions: AssertionHelper
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
            Expect(result1, Is.True);
            Expect(result2, Is.True);
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
            Expect(result1, Is.False);
            Expect(result2, Is.False);
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
            Expect(result1, Is.True);
            Expect(result2, Is.True);
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
            Expect(result1, Is.True);
            Expect(result2, Is.True);
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
            Expect(result, Is.True);
        }

        [Test]
        public void Matches_GivenAnyVerb_WhenMethodIsAny_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var method = HttpMethods.Any;

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = method.Matches(GetRandomFrom(new[] { "GET", "PUT", "POST", "Options", "PATCH" } ));

            //--------------- Assert -----------------------
            Expect(result, Is.True);
        }

        private HttpMethods GetRandomMethodButNotAny()
        {
            var result = GetRandom<HttpMethods>();
            return result == HttpMethods.Any 
                    ? GetRandomMethodButNotAny() 
                    : result;
        }

    }
}
