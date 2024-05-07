using System.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;

// ReSharper disable PossibleNullReferenceException

namespace PeanutButter.DuckTyping.Tests.Exceptions
{
    [TestFixture]
    public class TestUnresolvedParameterOrderMismatchException
    {
        [Test]
        public void Construct_ShouldSetMessage()
        {
            //--------------- Arrange -------------------
            var methodParameterTypes = new[] { new { }.GetType(), new { }.GetType() };
            var methodInfo = GetType().GetMethod("Construct_ShouldSetMessage");
            var expected =
                $"Attempt to {methodInfo.DeclaringType.Name}.{methodInfo.Name} with arguments out of sequence and target method has repeated argument types, making a re-order anyone's guess (parameters are of types: {string.Join(",", methodParameterTypes.Select(t => t.Name))})";
            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = new UnresolveableParameterOrderMismatchException(
                methodParameterTypes, methodInfo
            );

            //--------------- Assert -----------------------
            Expect(sut.Message).To.Equal(expected);
        }

        [Test]
        public void Construct_GivenNullMethodInfo_ShouldSetAppropriateMessage()
        {
            //--------------- Arrange -------------------
            var methodParameterTypes = new[] { new { }.GetType(), new { }.GetType() };
            var expected =
                $"Attempt to (unknown declaring type).(anonymous method) with arguments out of sequence and target method has repeated argument types, making a re-order anyone's guess (parameters are of types: {string.Join(",", methodParameterTypes.Select(t => t.Name))})";

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var sut = new UnresolveableParameterOrderMismatchException(
                methodParameterTypes,
                null
            );

            //--------------- Assert -----------------------
            Expect(sut.Message).To.Equal(expected);
        }


    }
}
