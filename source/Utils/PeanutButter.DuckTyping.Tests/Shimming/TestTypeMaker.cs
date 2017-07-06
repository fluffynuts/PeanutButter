using System;
using System.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace PeanutButter.DuckTyping.Tests.Shimming
{
    [TestFixture]
    public class TestTypeMaker : AssertionHelper
    {
        public interface ISample1
        {
            string Name { get; }
        }

        public class NotAnInterface
        {
        }

        [Test]
        public void MakeTypeImplementing_InvokedWithNonInterfaceTypeParameter_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.MakeTypeImplementing<NotAnInterface>(),
                Throws.Exception.InstanceOf<InvalidOperationException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void MakeFuzzyTypeImplementing_InvokedWithNonInterfaceTypeParameter_ShouldThrow()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            Expect(() => sut.MakeFuzzyTypeImplementing<NotAnInterface>(),
                Throws.Exception.InstanceOf<InvalidOperationException>());

            //--------------- Assert -----------------------
        }

        [Test]
        public void MakeTypeImplementing_GivenInterfaceWithOneProperty_ShouldReturnTypeImplementingThatInterface()
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
        public void MakeTypeImplementing_GivenInterfaceWithTwoReadOnlyProperties_ShouldProduceTypeWithWritableProperties()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var type = sut.MakeTypeImplementing<ISample2>();
            var instance = (ISample2)CreateInstanceOf(type);
            var expectedId = RandomValueGen.GetRandomInt();
            var expectedName = RandomValueGen.GetRandomString();

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
            var expected = RandomValueGen.GetRandomString(2, 5);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<ISample1>();
            var instance = (ISample1)CreateInstanceOf(result, new[] { new[] { toWrap } });
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
            public Sample1 Sample { get; set; }
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementWrappedNonPrimitivePropertiesForGet()
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
            var instance = CreateInstanceOf(type, new[] { new[] { toWrap } });

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
            var instance = (ISample4)CreateInstanceOf(type, new[] { new[] { toWrap } });

            //--------------- Assert -----------------------
            Expect(() => instance.Sample = expected, Throws.Exception.InstanceOf<ReadOnlyPropertyException>());
        }

        public interface IVoidVoid
        {
            void Moo();
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementSimpleVoidVoidMethod()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IVoidVoid>();
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            Expect(instance, Is.InstanceOf<IVoidVoid>());
        }

        public class VoidVoidImpl
        {
            public bool Called { get; private set; }
            public void Moo()
            {
                Called = true;
            }
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementCallThroughToSimpleVoidVoidMethod()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new VoidVoidImpl();

            //--------------- Assume ----------------
            Expect(toWrap.Called, Is.False);

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IVoidVoid>();
            var instance = (IVoidVoid)CreateInstanceOf(type, new[] { new[] { toWrap } });
            instance.Moo();

            //--------------- Assert -----------------------
            Expect(toWrap.Called, Is.True);
        }

        public interface IVoidArgs
        {
            void Moo(string pitch, int howMany);
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementVoidArgsMethod()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IVoidArgs>();
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            Expect(instance, Is.InstanceOf<IVoidArgs>());
        }

        public class VoidArgsImpl
        {
            public string Pitch { get; private set; }
            public int HowMany { get; private set; }
            public void Moo(string pitch, int howMany)
            {
                Pitch = pitch;
                HowMany = howMany;
            }
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementPassThroughToVoidArgsMethod()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new VoidArgsImpl();
            var expectedPitch = RandomValueGen.GetRandomString(2, 6);
            var expectedCount = RandomValueGen.GetRandomInt(2);

            //--------------- Assume ----------------
            Expect(toWrap.Pitch, Is.Null);
            Expect(toWrap.HowMany, Is.EqualTo(0));

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IVoidArgs>();
            var instance = (IVoidArgs)CreateInstanceOf(type, new[] { new[] { toWrap } });
            instance.Moo(expectedPitch, expectedCount);

            //--------------- Assert -----------------------
            Expect(toWrap.Pitch, Is.EqualTo(expectedPitch));
            Expect(toWrap.HowMany, Is.EqualTo(expectedCount));
        }

        public interface IArgsNonVoid
        {
            int Add(int a, int b);
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementMethodWithArgsAndReturnValue()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IArgsNonVoid>();
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            Expect(instance, Is.InstanceOf<IArgsNonVoid>());
        }

        public class ArgsNonVoidImpl
        {
            public int Add(int a, int b)
            {
                return a + b;
            }
        }

        [Test]
        public void MakeTypeImplementing_ImplementedTypeShouldBeAbleToWrapTypeWithMethodWithArgsAndReturnValue()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var type = sut.MakeTypeImplementing<IArgsNonVoid>();
            var toWrap = new ArgsNonVoidImpl();
            var first = RandomValueGen.GetRandomInt();
            var second = RandomValueGen.GetRandomInt();
            var expected = toWrap.Add(first, second);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var instance = (IArgsNonVoid)CreateInstanceOf(type, new[] { new[] { toWrap } });
            var result = instance.Add(first, second);

            //--------------- Assert -----------------------
            Expect(result, Is.EqualTo(expected));
        }


        public interface IInherited : ISample1
        {
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementAllInheritedInterfacePropertiess()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IInherited>();
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            Expect(instance, Is.InstanceOf<IInherited>());
            Expect(instance, Is.InstanceOf<ISample1>());
        }

        public interface IInheritedWithMethod : IArgsNonVoid
        {
        }

        [Test]
        public void MakeTypeImplementing_ShouldBeAbleToImplementAllInheritedInterfaceMethods()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IInheritedWithMethod>();
            var instance = CreateInstanceOf(type);

            //--------------- Assert -----------------------
            Expect(instance, Is.InstanceOf<IInheritedWithMethod>());
            Expect(instance, Is.InstanceOf<IArgsNonVoid>());
        }

        public class ToWrapSample1CaseInsensitive
        {
            public Sample1 sAmple { get; set; }
        }

        [Test]
        public void MakeTypeImplementing_GivenFuzzyIsTrue_ShouldMakeFuzzyType()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new ToWrapSample1CaseInsensitive()
            {
                sAmple = new Sample1()
            };
            var type = sut.MakeFuzzyTypeImplementing<ISample4>();
            var expected = new Sample1();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var instance = (ISample4)CreateInstanceOf(type, new[] { new[] { toWrap } });
            instance.Sample = expected;

            //--------------- Assert -----------------------
            var result = toWrap.sAmple;
            Expect(result, Is.EqualTo(expected));
        }

        public class LabelAttribute: Attribute
        {
            public string Label { get; private set; }

            public LabelAttribute(string label)
            {
                Label = label;
            }
        }
        [Label("cow!")]
        public interface IAttributeCopyTester
        {
            [Label("moo")]
            string Name { get; set; }
        }
        [Test]
        public void MakeTypeImplementing_ShouldCopyPropertyAttributes()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<IAttributeCopyTester>();

            //--------------- Assert -----------------------
            var attr = result.GetProperties().FirstOrDefault(p => p.Name == "Name")
                                ?.GetCustomAttributes(true)
                                .FirstOrDefault();
            Expect(attr, Is.Not.Null);
            Expect(attr, Is.InstanceOf<LabelAttribute>());
            Expect(((LabelAttribute)attr)?.Label, Is.EqualTo("moo"));
        }

        [Test]
        public void MakeTypeImplementing_ShouldCopyInterfaceLevelAttributes()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<IAttributeCopyTester>();

            //--------------- Assert -----------------------
            var attr = result.GetCustomAttributes(true)
                                .OfType<LabelAttribute>()
                                .FirstOrDefault();
            Expect(attr, Is.Not.Null);
            Expect(attr?.Label, Is.EqualTo("cow!"));
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
