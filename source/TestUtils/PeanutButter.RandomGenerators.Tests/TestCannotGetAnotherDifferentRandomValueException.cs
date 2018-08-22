using System;
using NUnit.Framework;
using static NExpect.Expectations;
using NExpect;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestCannotGetAnotherDifferentRandomValueException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(CannotGetAnotherDifferentRandomValueException<>);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut).To.Inherit<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenIntValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = GetRandomInt();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Expect(sut.Message).To.Equal(
                $"Unable to get a value different from {value} after {MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':"
            );
            Expect(sut.Value).To.Equal(value);

        }

        [Test]
        public void Construct_GivenStringValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Expect(sut.Message).To.Equal(
                $"Unable to get a value different from {value} after {MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':"
            );
            Expect(sut.Value).To.Equal(value);
        }

        [Test]
        public void Construct_GivenDecimalValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Expect(sut.Message).To.Equal(
                $"Unable to get a value different from {value} after {MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':"
            );
            Expect(sut.Value).To.Equal(value);

        }

        [Test]
        public void Construct_GivenBooleanValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = GetRandomBoolean();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Expect(sut.Message).To.Equal(
                $"Unable to get a value different from {value} after {MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':"
            );
            Expect(sut.Value).To.Equal(value);

        }

        private CannotGetAnotherDifferentRandomValueException<T> Create<T>(T value)
        {
            return new CannotGetAnotherDifferentRandomValueException<T>(value);
        }
    }
}