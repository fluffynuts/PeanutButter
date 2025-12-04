using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.Generic.Tests;

[TestFixture]
public class TestDecimalExtensions
{
    [Test]
    public void ShouldMatch_ShouldThrowWhenNumbersDontMatchAtGivenPosition()
    {
        //---------------Set up test pack-------------------
        var num1 = 1.23456M;
        var num2 = 1.23999M;

        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(() => num1.ShouldMatch(num2, 0)).Not.To.Throw();
        Expect(() => num1.ShouldMatch(num2, 1)).Not.To.Throw();
        Expect(() => num1.ShouldMatch(num2, 2)).Not.To.Throw();
        Expect(() => num1.ShouldMatch(num2, 3)).To.Throw<AssertionException>();
        Expect(() => num1.ShouldMatch(num2, 4)).To.Throw<AssertionException>();
        Expect(() => num1.ShouldMatch(num2, 5)).To.Throw<AssertionException>();

        //---------------Test Result -----------------------
    }

    [Test]
    public void ShouldMatch_GivenPoint99_and_Point998_ForTwoPlaces_ShouldNotThrow()
    {
        //--------------- Arrange -------------------
        var num1 = 9.99M;
        var num2 = 9.998M;

        //--------------- Assume ----------------

        //--------------- Act ----------------------
        Expect(() => num1.ShouldMatch(num2, 2)).Not.To.Throw();
        Expect(() => num2.ShouldMatch(num1, 2)).Not.To.Throw();


        //--------------- Assert -----------------------
    }

}