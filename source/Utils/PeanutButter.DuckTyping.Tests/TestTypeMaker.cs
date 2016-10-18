using System;
using NUnit.Framework;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestTypeMaker: AssertionHelper
    {
        public interface ISample1
        {
            string Name { get; }
        }

        [Test]
        public void BuildFor_GivenInterfaceWithOneProperty_ShouldReturnTypeImplementingThatInterface()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<ISample1>();
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            Expect(instance, Is.Not.Null);
            Expect(instance, Is.InstanceOf<ISample1>());
        }

        public interface ISample2
        {
            int Id { get; }
            string Name { get; }
        }

        [Test]
        public void BuildFor_GivenInterfaceWithTwoReadOnlyProperties_ShouldProduceTypeWithWritableProperties()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var type = sut.MakeTypeImplementing<ISample2>();
            var instance = (ISample2)CreateInstanceOf(type);
            var expectedId = GetRandomInt();
            var expectedName = GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            instance.SetPropertyValue("Id", expectedId);
            instance.SetPropertyValue("Name", expectedName);

            //--------------- Assert -----------------------
            Expect(instance.Id, Is.EqualTo(expectedId));
            Expect(instance.Name, Is.EqualTo(expectedName));
        }

        public class Sample1 // "implements" ISample1
        {
            public string Name { get; set; }
        }

        public interface ISample3    // because there are no guards against creating the same-named dynamic type again (yet)
        {
            string Name { get; }
        }
        // TODO: implement type from all interfaces, including inherited ones

        [Test]
        public void MakeTypeImplementing_ShouldCreateConstructorRequiringObjectToWrap()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new Sample1();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<ISample3>();
            Expect(() => CreateInstanceOf(result, toWrap), Throws.Nothing);

            //--------------- Assert -----------------------
        }

        [Test]
        public void MakeTypeImplementing_GivenObjectToWrap_ShouldPassPropertyGetThrough()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new Sample1();
            var expected = GetRandomString(2, 5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<ISample1>();
            var instance = (ISample1)CreateInstanceOf(result, toWrap);
            instance.SetPropertyValue("Name", expected);

            //--------------- Assert -----------------------
            Expect(toWrap.Name, Is.EqualTo(expected));
        }

        public interface ISample4
        {
            Sample1 Sample { get; set; }
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementNonPrimitiveProperties()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var type = sut.MakeTypeImplementing<ISample4>();
            var expected = new Sample1();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            instance.SetPropertyValue("Sample", expected);
            var result = instance.GetPropertyValue("Sample");
            Expect(result, Is.EqualTo(expected));
        }

        public class ToWrapSample1
        {
            public Sample1 Sample { get; set ; }
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementWrappedNonPrimitiveProperties()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new ToWrapSample1()
            {
                Sample = new Sample1()
            };
            var type = sut.MakeTypeImplementing<ISample4>();
            var expected = new Sample1();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var instance = CreateInstanceOf(type, toWrap);

            //--------------- Assert -----------------------
            instance.SetPropertyValue("Sample", expected);
            var result = toWrap.Sample;
            Expect(result, Is.EqualTo(expected));
        }

        public class ToWrapSample1ReadOnly
        {
            public Sample1 Sample { get; }
        }

        [Test]
        public void MakeTypeImplementing_WhenAttemptingToSetReadOnlyPropertyOnWrappedObject_ShouldThrow_ReadOnlyPropertyException()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new ToWrapSample1ReadOnly();
            var type = sut.MakeTypeImplementing<ISample4>();
            var expected = new Sample1();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var instance = (ISample4)CreateInstanceOf(type, toWrap);

            //--------------- Assert -----------------------
            Expect(() => instance.Sample = expected, Throws.Exception.InstanceOf<ReadOnlyPropertyException>());
        }



        private object CreateInstanceOf(Type type, params object[] constructorArgs)
        {
            return Activator.CreateInstance(type, constructorArgs);
        }

        private ITypeMaker Create()
        {
            return new TypeMaker();
        }

    }
}
