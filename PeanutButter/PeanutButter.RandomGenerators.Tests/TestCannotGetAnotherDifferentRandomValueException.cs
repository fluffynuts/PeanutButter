using System;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestCannotGetAnotherDifferentRandomValueException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (CannotGetAnotherDifferentRandomValueException<>);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenIntValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = RandomValueGen.GetRandomInt();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Assert.AreEqual(
                $"Unable to get a value different from ${value} after ${RandomValueGen.MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':",
                sut.Message);

        }

        [Test]
        public void Construct_GivenStringValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Assert.AreEqual(
                $"Unable to get a value different from ${value} after ${RandomValueGen.MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':",
                sut.Message);

        }

        [Test]
        public void Construct_GivenDecimalValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Assert.AreEqual(
                $"Unable to get a value different from ${value} after ${RandomValueGen.MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':",
                sut.Message);

        }

        [Test]
        public void Construct_GivenBooleanValue_ShouldProduceExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var value = RandomValueGen.GetRandomBoolean();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(value);

            //---------------Test Result -----------------------
            Assert.AreEqual(
                $"Unable to get a value different from ${value} after ${RandomValueGen.MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':",
                sut.Message);

        }

        private CannotGetAnotherDifferentRandomValueException<T> Create<T>(T value)
        {
            return new CannotGetAnotherDifferentRandomValueException<T>(value);
        }
    }
}