using System.Reflection;
using NUnit.Framework;

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestPropertyInfoContainer: AssertionHelper
    {
        [Test]
        public void Construct_GivenNoProperties_ShouldNotThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = Create();

            //--------------- Assert -----------------------
            Expect(sut.FuzzyPropertyInfos, Has.Count.EqualTo(0));
            Expect(sut.PropertyInfos, Has.Count.EqualTo(0));

        }

        interface IHasOneProperty
        {
            string Name { get; }
        }

        [Test]
        public void Construct_GivenOneProperty_ShouldIndexIt()
        {
            //--------------- Arrange -------------------
            var input = typeof(IHasOneProperty).GetProperties();

            //--------------- Assume ----------------
            Expect(input, Has.Length.EqualTo(1));

            //--------------- Act ----------------------
            var sut = Create(input);

            //--------------- Assert -----------------------
            Expect(sut.FuzzyPropertyInfos, Has.Count.EqualTo(1));
            Expect(sut.PropertyInfos, Has.Count.EqualTo(1));
            Expect(sut.FuzzyPropertyInfos.ContainsValue(input[0]), Is.True);
            Expect(sut.PropertyInfos.ContainsValue(input[0]), Is.True);
        }

        [Test]
        public void Construct_GivenDuplicatePropertyDefinitions_ShouldAddDistinct()
        {
            //--------------- Arrange -------------------
            var partialInput = typeof(IHasOneProperty).GetProperties();
            var input = new[] { partialInput[0], partialInput[0] };

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = Create(input);

            //--------------- Assert -----------------------
            Expect(sut.FuzzyPropertyInfos, Has.Count.EqualTo(1));
            Expect(sut.PropertyInfos, Has.Count.EqualTo(1));
            Expect(sut.FuzzyPropertyInfos.ContainsValue(input[0]), Is.True);
            Expect(sut.PropertyInfos.ContainsValue(input[0]), Is.True);
        }



        private PropertyInfoContainer Create(params PropertyInfo[] propertyInfos)
        {
            return new PropertyInfoContainer(propertyInfos);
        }
    }
}
