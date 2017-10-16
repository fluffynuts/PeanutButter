using System.Reflection;
using NUnit.Framework;
using PeanutButter.DuckTyping.Shimming;
using NExpect;
using static NExpect.Expectations;

namespace PeanutButter.DuckTyping.Tests.Shimming
{
    [TestFixture]
    public class TestPropertyInfoContainer
    {
        [Test]
        public void Construct_GivenNoProperties_ShouldNotThrow()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = Create();

            //--------------- Assert -----------------------
            Expect(sut.FuzzyPropertyInfos).To.Be.Empty();
            Expect(sut.PropertyInfos).To.Be.Empty();

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
            Expect(input).To.Contain.Only(1).Item();

            //--------------- Act ----------------------
            var sut = Create(input);

            //--------------- Assert -----------------------
            Expect(sut.FuzzyPropertyInfos).To.Contain.Only(1).Item();
            Expect(sut.PropertyInfos).To.Contain.Only(1).Item();
            Expect(sut.FuzzyPropertyInfos)
                .To.Contain.Key(nameof(IHasOneProperty.Name))
                .With.Value(input[0]);
            Expect(sut.PropertyInfos)
                .To.Contain.Key(nameof(IHasOneProperty.Name))
                .With.Value(input[0]);
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
            Expect(sut.FuzzyPropertyInfos).To.Contain.Only(1).Item();
            Expect(sut.PropertyInfos).To.Contain.Only(1).Item();
            Expect(sut.FuzzyPropertyInfos)
                .To.Contain.Key(nameof(IHasOneProperty.Name))
                .With.Value(input[0]);
            Expect(sut.PropertyInfos)
                .To.Contain.Key(nameof(IHasOneProperty.Name))
                .With.Value(input[0]);
        }

        private PropertyInfoContainer Create(params PropertyInfo[] propertyInfos)
        {
            return new PropertyInfoContainer(propertyInfos);
        }
    }
}
