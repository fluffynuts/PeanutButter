using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static NExpect.Expectations;

[TestFixture]
public class TestFakeResponseCookies
{
    [Test]
    public void ShouldCreateSubstituteWherePossible()
    {
        // Arrange
        var result = FakeResponseCookies.Create();
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        result.Append(key, value);
        // Assert
        // FIXME: the result _is_ a sub, but this assertion fails within NSubstitute
        // unless NSubstitute has been invoked earlier, eg via
        // var foo = SubstitutionContext.Current;
        // or
        // var foo = Substitute.For<Something>();
        // Expect(result)
        //     .To.Have.Received(1)
        //     .Append(key, value);
        Expect((result as FakeResponseCookies).Store)
            .To.Contain.Key(key)
            .With.Value.Matched.By(cookie => cookie.Value == value);
    }
}