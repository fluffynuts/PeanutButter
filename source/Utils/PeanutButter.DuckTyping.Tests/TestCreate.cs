using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestCreate: AssertionHelper
    {
        public interface ICreateMe
        {
            int Id { get; set ; }
            string Name { get; set ;}
        }
        [Test]
        public void InstanceOf_InvokedWithInterfaceType_ShouldReturnNewInstanceImplementingThatType()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = Create.InstanceOf<ICreateMe>();

            //--------------- Assert -----------------------
            Expect(result1, Is.Not.Null);
            Expect(result1, Is.InstanceOf<ICreateMe>());
        }

        [Test]
        public void InstanceOf_ShouldReturnSameTypeForMultipleCallsOnSameInterface()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result1 = Create.InstanceOf<ICreateMe>();
            var result2 = Create.InstanceOf<ICreateMe>();

            //--------------- Assert -----------------------
            Expect(result1.GetType(), Is.EqualTo(result2.GetType()));
        }

        [Test]
        public void InstanceOf_ShouldReturnInstanceWithWorkingPropertiesAsPerInterface()
        {
            //--------------- Arrange -------------------
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = Create.InstanceOf<ICreateMe>();

            //--------------- Assert -----------------------
            Expect(() =>
                result.Id = expectedId,
                Throws.Nothing);
            Expect(() =>
                result.Name = expectedName,
                Throws.Nothing);
            Expect(result.Id, Is.EqualTo(expectedId));
            Expect(result.Name, Is.EqualTo(expectedName));
        }



    }
}
