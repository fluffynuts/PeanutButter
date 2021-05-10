using System;
using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable UnusedParameter.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestTypeExtensions
    {
        [Test]
        public void Ancestry_WorkingOnTypeOfObject_ShouldReturnOnlyObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(object);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.Ancestry();

            //---------------Test Result -----------------------
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(typeof(object), result[0]);
        }

        public class SimpleType
        {
        }

        [Test]
        public void Ancestry_WorkingOnSimpleType_ShouldReturnItAndObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(SimpleType);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.Ancestry();

            //---------------Test Result -----------------------
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(typeof(object), result[0]);
            Assert.AreEqual(typeof(SimpleType), result[1]);
        }

        public class InheritedType: SimpleType
        {
        }

        [Test]
        public void Ancestry_WorkingOnInheritedType_ShouldReturnItAndObject()
        {
            //---------------Set up test pack-------------------
            var type = typeof(InheritedType);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.Ancestry();

            //---------------Test Result -----------------------
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(typeof(object), result[0]);
            Assert.AreEqual(typeof(SimpleType), result[1]);
            Assert.AreEqual(typeof(InheritedType), result[2]);
        }

        public class ClassWithConstants
        {
            public const string CONSTANT1 = "Constant1";
            public const string CONSTANT2 = "Constant2";
            public const int CONSTANT3 = 1;
            public const bool CONSTANT4 = false;
        }

        [Test]
        public void GetAllConstants_OperatingOnType_ShouldReturnDictionaryOfConstants()
        {
            //---------------Set up test pack-------------------
            var type = typeof(ClassWithConstants);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.GetAllConstants();

            //---------------Test Result -----------------------
            Assert.AreEqual(result["CONSTANT1"], ClassWithConstants.CONSTANT1);
            Assert.AreEqual(result["CONSTANT2"], ClassWithConstants.CONSTANT2);
            Assert.AreEqual(result["CONSTANT3"], ClassWithConstants.CONSTANT3);
            Assert.AreEqual(result["CONSTANT4"], ClassWithConstants.CONSTANT4);
        }

        [Test]
        public void GetAllConstants_OperatingOnType_WithGenericParameter_ShouldReturnDictionaryOfConstantsOfThatType()
        {
            //---------------Set up test pack-------------------
            var type = typeof(ClassWithConstants);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.GetAllConstants<string>();

            //---------------Test Result -----------------------
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(result["CONSTANT1"], ClassWithConstants.CONSTANT1);
            Assert.AreEqual(result["CONSTANT2"], ClassWithConstants.CONSTANT2);
        }

        [Test]
        public void GetAllConstantValues_ShouldListAllConstantsOfThatType()
        {
            //---------------Set up test pack-------------------
            var type = typeof(ClassWithConstants);
            var expected = new object[]
            {
                "Constant1",
                "Constant2",
                1,
                false
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.GetAllConstantValues();

            //---------------Test Result -----------------------
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void GetAllConstantValues_OfGenericType_ShouldListAllConstantsOfThatType()
        {
            //---------------Set up test pack-------------------
            var type = typeof(ClassWithConstants);
            var expected = new[]
            {
                "Constant1",
                "Constant2"
            };

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.GetAllConstantValues<string>();

            //---------------Test Result -----------------------
            CollectionAssert.AreEquivalent(expected, result);
        }

        public enum SomeEnums
        {
        }
        public struct SomeStruct
        {
        }
        [TestCase(typeof(int))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(SomeEnums))]
        [TestCase(typeof(SomeStruct))]
        public void CanBeAssignedNull_OperatingOnNonNullableType_ShouldReturnFalse(Type t)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.CanBeAssignedNull();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        [TestCase(typeof(ClassWithConstants))]
        [TestCase(typeof(int[]))]
        [TestCase(typeof(int?))]
        [TestCase(typeof(string))]
        public void CanBeAssignedNull_OperatingOnNullableType_ShouldReturnTrue(Type t)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = t.CanBeAssignedNull();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        public class TypeWithDefaultConstructor
        {
        }

        [Test]
        public void HasDefaultConstructor_OperatingOnTypeWithDefaultConstructorOnly_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var type = typeof(TypeWithDefaultConstructor);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.HasDefaultConstructor();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        public class TypeWithoutDefaultConstructor
        {
            public TypeWithoutDefaultConstructor(int id)
            {
            }
        }

        [Test]
        public void HasDefaultConstructor_OperatingOnTypeWithoutDefaultConstructor_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var type = typeof(TypeWithoutDefaultConstructor);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.HasDefaultConstructor();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }

        public class TypeWithDefaultAndParameteredConstructor
        {
            public TypeWithDefaultAndParameteredConstructor()
            {
            }
            public TypeWithDefaultAndParameteredConstructor(int id)
            {
            }
        }

        [Test]
        public void HasDefaultConstructor_OperatingOnTypeWithDefaultConstructorAndParameteredConstructor_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var type = typeof(TypeWithDefaultAndParameteredConstructor);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = type.HasDefaultConstructor();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        public interface ILevel4Again
        {
        }
        public interface ILevel4
        {
        }
        public interface ILevel3: ILevel4, ILevel4Again
        {
        }
        public interface ILevel2: ILevel3
        {
        }
        public interface ILevel1: ILevel2
        {
        }

        public class Level1: ILevel1
        {
        }

        [Test]
        public void GetAllImplementedInterfaces_ShouldReturnAllInterfacesAllTheWayDown()
        {
            //--------------- Arrange -------------------

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = typeof(Level1).GetAllImplementedInterfaces();

            //--------------- Assert -----------------------
            Expect(result).To.Contain.Exactly(1).Equal.To(typeof(ILevel1));
            Expect(result).To.Contain.Exactly(1).Equal.To(typeof(ILevel2));
            Expect(result).To.Contain.Exactly(1).Equal.To(typeof(ILevel3));
            Expect(result).To.Contain.Exactly(1).Equal.To(typeof(ILevel4));
            Expect(result).To.Contain.Exactly(1).Equal.To(typeof(ILevel4Again));

        }

        public class NotDisposable
        {
            public void Dispose()
            {
                /* just to prevent mickey-mousery */
            }
        }

        public class DisposableTeen: IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void IsDisposable_OperatingOnNonDisposableType_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var sut = typeof(NotDisposable);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.IsDisposable();

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void IsDisposable_OperatingOnDisposableType_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var sut = typeof(DisposableTeen);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = sut.IsDisposable();

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        [TestCase(typeof(byte), typeof(long))]
        [TestCase(typeof(byte), typeof(ushort))]
        [TestCase(typeof(byte), typeof(ulong))]
        [TestCase(typeof(short), typeof(long))]
        [TestCase(typeof(int), typeof(long))]
        [TestCase(typeof(float), typeof(double))]
        public void CanImplicitlyUpcastTo_ShouldReturnTrueFor_(Type src, Type target)
        {
            // Arrange
            // Pre-Assert
            // Act
            var result = src.CanImplicitlyCastTo(target);
            // Assert
            Expect(result).To.Be.True();
        }

        [TestCase(typeof(object), null)]
        [TestCase(typeof(string), null)]
        [TestCase(typeof(bool), false)]
        [TestCase(typeof(int), 0)]
        public void DefaultValue_ShouldReturnDefaultValueForType(
            Type t, object expected)
        {
            // Arrange
            // Pre-assert
            // Act
            var result = t.DefaultValue();
            // Assert
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToGetEnumerableTypeOfArray()
        {
            // Arrange
            var arr = new int[0];
            // Pre-assert
            // Act
            var result = arr.GetType().TryGetEnumerableItemType();
            // Assert
            Expect(result).To.Equal(typeof(int));
        }

        [TestFixture]
        public class Implements
        {
            public interface IInterface
            {
            }

            public class NotImplemented
            {
            }

            public class Implemented : IInterface
            {
            }

            public class DerivedImplemented : Implemented
            {
            }

            [Test]
            public void ShouldReturnFalseIfInterfaceNotImplemented()
            {
                // Arrange
                // Act
                var result = typeof(NotImplemented).Implements<IInterface>();
                // Assert
                Expect(result).To.Be.False();
            }

            [Test]
            public void ShouldReturnTrueIfInterfaceImmediatelyImplemented()
            {
                // Arrange
                // Act
                var result = typeof(Implemented).Implements<IInterface>();
                // Assert
                Expect(result).To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueIfInterfaceImplementedByAncestor()
            {
                // Arrange
                // Act
                var result = typeof(DerivedImplemented).Implements<IInterface>();
                // Assert
                Expect(result).To.Be.True();
            }
        }

        [TestFixture]
        public class SetStatic
        {
            [Test]
            public void ShouldSetStaticProperty()
            {
                // Arrange
                var t = typeof(HasStatics);
                var expected = GetRandomInt();
                // Act
                t.SetStatic(nameof(HasStatics.Id), expected);
                // Assert
                Expect(HasStatics.Id)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldSetStaticField()
            {
                // Arrange
                var t = typeof(HasStatics);
                var expected = GetRandomString();
                // Act
                t.SetStatic("_name", expected);
                // Assert
                Expect(new HasStatics().Name)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldImplicitlyUpCastValue()
            {
                // Arrange
                var t = typeof(HasStatics);
                var expected = (byte)GetRandomInt(1, 10);
                // Act
                t.SetStatic("Id", expected);
                // Assert
                Expect(HasStatics.Id)
                    .To.Equal(expected);
            }

            public class HasStatics
            {
                public string Name => _name;
                private static string _name;

                public static int Id { get; set; }
            }
        }

        [TestFixture]
        public class GetStatic
        {
            [Test]
            public void ShouldRetrieveStaticProperty()
            {
                // Arrange
                var t = typeof(HasStatics);
                var expected = GetRandomInt();
                // Act
                t.SetStatic("Id", expected);
                var result = t.GetStatic<int>("Id");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldRetrieveStaticField()
            {
                // Arrange
                var t = typeof(HasStatics);
                var expected = GetRandomString();
                // Act
                t.SetStatic("_name", expected);
                var result = t.GetStatic<string>("_name");
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
            
            public class HasStatics
            {
                public string Name => _name;
                private static string _name;

                private static int Id { get; set; }
            }
        }
    }
}
