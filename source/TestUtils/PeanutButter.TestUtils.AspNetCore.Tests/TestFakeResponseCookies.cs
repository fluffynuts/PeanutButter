using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
using static NExpect.Expectations;

[TestFixture]
public class TestFakeResponseCookies
{
    [Test]
    [Explicit("Only works if NSubstitute is used before-hand - have to figure out the secret sauce")]
    public void ShouldCreateSubstituteWherePossible()
    {
        // Arrange
        var sut = FakeResponseCookies.CreateSubstitutedIfPossible();
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        sut.Append(key, value);
        // Assert
        // FIXME: the result _is_ a sub, but this assertion fails within NSubstitute
        // unless NSubstitute has been invoked earlier, eg via
        // var foo = SubstitutionContext.Current;
        // or
        // var foo = Substitute.For<Something>();
        Expect(sut)
            .To.Have.Received(1)
            .Append(key, value);
    }

    [Test]
    public void ShouldStoreCookieWithDefaultSettings()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        sut.Append(key, value);
        // Assert
        Expect(sut.Store)
            .To.Contain.Key(key)
            .With.Value.Matched.By(
                o => o.Value == value &&
                    o.Options.DeepEquals(new CookieOptions())
            );
    }

    [Test]
    public void ShouldStoreWithProvidedOptions()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        var options = GetRandom<CookieOptions>();
        // Act
        sut.Append(key, value, options);
        // Assert
        Expect(sut.Store)
            .To.Contain.Key(key)
            .With.Value.Matched.By(
                o => o.Value == value &&
                    o.Options.DeepEquals(options)
            );
    }

    [Test]
    public void ShouldDeleteByKey()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        sut.Append(key, value);

        // Act
        sut.Delete(key);
        
        // Assert
        Expect(sut.Store)
            .Not.To.Contain.Key(key);
    }

    [Test]
    public void ShouldDeleteByKeyAndOptionsWhenDomainAndPathMatch()
    {
        // Arrange
        var sut = Create();
        var key = GetRandomString();
        var value = GetRandomString();
        var options = GetRandom<CookieOptions>();
        Expect(options.Domain)
            .Not.To.Be.Null.Or.Empty();
        Expect(options.Path)
            .Not.To.Be.Null.Or.Empty();
        sut.Append(key, value, options);
        var otherOptions = GetRandom<CookieOptions>();
        
        // Act
        sut.Delete(key, otherOptions);
        Expect(sut.Store)
            .To.Contain.Key(key);
        sut.Delete(key, options);
        // Assert
        Expect(sut.Store)
            .Not.To.Contain.Key(key);
    }

    private static FakeResponseCookies Create()
    {
        return FakeResponseCookies.CreateSubstitutedIfPossible() as FakeResponseCookies;
        ;
    }
}