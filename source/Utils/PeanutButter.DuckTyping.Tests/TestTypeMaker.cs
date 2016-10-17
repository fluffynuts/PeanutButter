using System;
using System.Linq;
using System.Reflection;
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

        public class Quack1
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
            var toWrap = new Quack1();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<ISample3>();
            Expect(() => CreateInstanceOf(result, toWrap), Throws.Nothing);

            //--------------- Assert -----------------------
        }

        [Test]
        [Ignore("WIP")]
        public void MakeTypeImplementing_GivenObjectToWrap_ShouldPassPropertyGetThrough()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new Quack1();
            var expected = GetRandomString(2, 5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<ISample1>();
            var instance = (ISample1)CreateInstanceOf(result, toWrap);
            instance.SetPropertyValue("Name", expected);

            //--------------- Assert -----------------------
            Expect(toWrap.Name, Is.EqualTo(expected));
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
