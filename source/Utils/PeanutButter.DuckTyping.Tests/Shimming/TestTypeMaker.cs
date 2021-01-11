using System;
using System.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty
// ReSharper disable InconsistentNaming

namespace PeanutButter.DuckTyping.Tests.Shimming
{
    [TestFixture]
    public class TestTypeMaker
    {
        public interface ISample1
        {
            string Name { get; }
        }

        public class NotAnInterface
        {
        }

        [Test]
        public void MakeTypeImplementing_InvokedWithNonInterfaceTypeParameter_ShouldWork()
        {
            //--------------- Arrange -------------------
            var sut = Create();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.MakeTypeImplementing<NotAnInterface>();

            //--------------- Assert -----------------------
            Expect(result)
                .Not.To.Be.Null();
            Expect(result)
                .Not.To.Be(typeof(NotAnInterface));
            var instance = Activator.CreateInstance(result);
            Expect(instance)
                .To.Be.An.Instance.Of<NotAnInterface>();
        }

        public class SimplePoco
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
        }

        [Test]
        public void ShouldBeAbleToMakeReadWritablePoco()
        {
            // Arrange
            var sut = Create();
            var id = GetRandomInt();
            var name = GetRandomWords();
            // Act
            var result = Activator.CreateInstance(
                sut.MakeTypeImplementing<SimplePoco>()
            ) as SimplePoco;

            // Assert
            Expect(result)
                .Not.To.Be.Null();
            result.Id = id;
            result.Name = name;
            Expect(result.Id)
                .To.Equal(id);
            Expect(result.Name)
                .To.Equal(name);
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
            Expect(instance).Not.To.Be.Null();
            Expect(instance).To.Be.An.Instance.Of<ISample1>();
        }

        public interface ISample2
        {
            int Id { get; }
            string Name { get; }
        }

        [Test]
        public void
            MakeTypeImplementing_GivenInterfaceWithTwoReadOnlyProperties_ShouldProduceTypeWithWritableProperties()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var type = sut.MakeTypeImplementing<ISample2>();
            var instance = (ISample2) CreateInstanceOf(type);
            var expectedId = RandomValueGen.GetRandomInt();
            var expectedName = RandomValueGen.GetRandomString();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            instance.SetPropertyValue("Id", expectedId);
            instance.SetPropertyValue("Name", expectedName);

            //--------------- Assert -----------------------
            Expect(instance.Id).To.Equal(expectedId);
            Expect(instance.Name).To.Equal(expectedName);
        }

        public class Sample1 // "implements" ISample1
        {
            public string Name { get; set; }
        }

        public interface
            ISample3 // because there are no guards against creating the same-named dynamic type again (yet)
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
            Expect(() => CreateInstanceOf(result, toWrap))
                .Not.To.Throw();

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
            var instance = (ISample1) CreateInstanceOf(result, new object[] { new[] { toWrap } });
            instance.SetPropertyValue("Name", expected);

            //--------------- Assert -----------------------
            Expect(toWrap.Name).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            var instance = CreateInstanceOf(type, new object[] { new[] { toWrap } });

            //--------------- Assert -----------------------
            instance.SetPropertyValue("Sample", expected);
            var result = toWrap.Sample;
            Expect(result).To.Equal(expected);
        }

        public class ToWrapSample1ReadOnly
        {
            public Sample1 Sample { get; }
        }

        [Test]
        public void
            MakeTypeImplementing_WhenAttemptingToSetReadOnlyPropertyOnWrappedObject_ShouldThrow_ReadOnlyPropertyException()
        {
            //--------------- Arrange -------------------
            var sut = Create();
            var toWrap = new ToWrapSample1ReadOnly();
            var type = sut.MakeTypeImplementing<ISample4>();
            var expected = new Sample1();

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var instance = (ISample4) CreateInstanceOf(type, new object[] { new[] { toWrap } });

            //--------------- Assert -----------------------
            Expect(() => instance.Sample = expected)
                .To.Throw<ReadOnlyPropertyException>();
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
            Expect(instance).To.Be.An.Instance.Of<IVoidVoid>();
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
            Expect(toWrap.Called).To.Be.False();

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IVoidVoid>();
            var instance = (IVoidVoid) CreateInstanceOf(type, new object[] { new[] { toWrap } });
            instance.Moo();

            //--------------- Assert -----------------------
            Expect(toWrap.Called).To.Be.True();
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
            Expect(instance).To.Be.An.Instance.Of<IVoidArgs>();
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
            Expect(toWrap.Pitch).To.Be.Null();
            Expect(toWrap.HowMany).To.Equal(0);

            //--------------- Act ----------------------
            var type = sut.MakeTypeImplementing<IVoidArgs>();
            var instance = (IVoidArgs) CreateInstanceOf(type, new object[] { new[] { toWrap } });
            instance.Moo(expectedPitch, expectedCount);

            //--------------- Assert -----------------------
            Expect(toWrap.Pitch).To.Equal(expectedPitch);
            Expect(toWrap.HowMany).To.Equal((expectedCount));
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
            Expect(instance).To.Be.An.Instance.Of<IArgsNonVoid>();
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
            var instance = (IArgsNonVoid) CreateInstanceOf(type, new object[] { new[] { toWrap } });
            var result = instance.Add(first, second);

            //--------------- Assert -----------------------
            Expect(result).To.Equal((expected));
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
            Expect(instance).To.Be.An.Instance.Of<IInherited>();
            Expect(instance).To.Be.An.Instance.Of<ISample1>();
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
            Expect(instance).To.Be.An.Instance.Of<IInheritedWithMethod>();
            Expect(instance).To.Be.An.Instance.Of<IArgsNonVoid>();
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
            var instance = (ISample4) CreateInstanceOf(type, new object[] { new[] { toWrap } });
            instance.Sample = expected;

            //--------------- Assert -----------------------
            var result = toWrap.sAmple;
            Expect(result).To.Equal((expected));
        }

        public class LabelAttribute : Attribute
        {
            public string Label { get; }

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
            Expect(attr).Not.To.Be.Null();
            Expect(attr).To.Be.An.Instance.Of<LabelAttribute>();
            Expect(((LabelAttribute) attr)?.Label).To.Equal("moo");
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
            Expect(attr).Not.To.Be.Null();
            Expect(attr?.Label).To.Equal("cow!");
        }

        [TestFixture]
        public class WhenInterfaceHasMethodWithDuplicateName
        {
            public interface IConfusing
            {
                int GetProductId();
                int GetProductId(string name);
            }

            [Test]
            public void ShouldStillBeAbleToMakeType()
            {
                // Arrange
                var sut = Create();
                // Act
                Expect(() => sut.MakeTypeImplementing<IConfusing>())
                    .Not.To.Throw();
                // Assert
            }
        }


        private object CreateInstanceOf(Type type, params object[] constructorArgs)
        {
            return Activator.CreateInstance(type, constructorArgs);
        }

        private static ITypeMaker Create()
        {
            return new TypeMaker();
        }
    }
}