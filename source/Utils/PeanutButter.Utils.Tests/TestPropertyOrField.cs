using System;
using System.Reflection;
using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestPropertyOrField
    {
        [TestFixture]
        public class WhenConstructedWithPropertyInfo
        {
            [Test]
            public void ShouldStoreDeclaringType()
            {
                // Arrange
                var sut = Create(GetRandomFrom(Props));
                // Act
                var result = sut.DeclaringType;
                // Assert
                Expect(result).To.Equal(SomeClassType);
            }

            [Test]
            public void ShouldStoreName()
            {
                // Arrange
                var prop = GetRandomFrom(Props);
                var sut = Create(prop);
                // Act
                var result = sut.Name;
                // Assert
                Expect(result)
                    .To.Equal(prop.Name);
            }

            [Test]
            public void ShouldStorePropertyType()
            {
                // Arrange
                var prop = GetRandomFrom(Props);
                var sut = Create(prop);
                // Act
                var result = sut.Type;
                // Assert
                Expect(result)
                    .To.Equal(prop.PropertyType);
            }

            [Test]
            public void ShouldStoreMemberType()
            {
                // Arrange
                // Act
                var sut = Create(GetRandomFrom(Props));
                // Assert
                Expect(sut.MemberType)
                    .To.Equal(PropertyOrFieldTypes.Property);
            }

            [Test]
            public void ShouldStoreReadWrite()
            {
                // Arrange
                // Act
                var sut = Create(ReadWriteIntProp);
                // Assert
                Expect(sut.CanRead)
                    .To.Be.True();
                Expect(sut.CanWrite)
                    .To.Be.True();
            }

            [Test]
            public void ShouldStoreReadOnly()
            {
                // Arrange
                // Act
                var sut = Create(ReadOnlyIntProp);
                // Assert
                Expect(sut.CanRead)
                    .To.Be.True();
                Expect(sut.CanWrite)
                    .To.Be.False();
            }

            [Test]
            public void ShouldStoreWriteOnly()
            {
                // Arrange
                // Act
                var sut = Create(WriteOnlyIntProp);
                // Assert
                Expect(sut.CanRead)
                    .To.Be.False();
                Expect(sut.CanWrite)
                    .To.Be.True();
            }
        }
        
        [TestFixture]
        public class WhenConstructedWithFieldInfo
        {
            [Test]
            public void ShouldStoreDeclaringType()
            {
                // Arrange
                var sut = Create(IntField);
                // Act
                var result = sut.DeclaringType;
                // Assert
                Expect(result).To.Equal(SomeClassType);
            }

            [Test]
            public void ShouldStoreName()
            {
                // Arrange
                var sut = Create(IntField);
                // Act
                var result = sut.Name;
                // Assert
                Expect(result)
                    .To.Equal(IntField.Name);
            }

            [Test]
            public void ShouldStorePropertyType()
            {
                // Arrange
                var sut = Create(IntField);
                // Act
                var result = sut.Type;
                // Assert
                Expect(result)
                    .To.Equal(IntField.FieldType);
            }

            [Test]
            public void ShouldStoreMemberType()
            {
                // Arrange
                // Act
                var sut = Create(IntField);
                // Assert
                Expect(sut.MemberType)
                    .To.Equal(PropertyOrFieldTypes.Field);
            }

            [Test]
            public void ShouldStoreReadWrite()
            {
                // Arrange
                // Act
                var sut = Create(IntField);
                // Assert
                Expect(sut.CanRead)
                    .To.Be.True();
                Expect(sut.CanWrite)
                    .To.Be.True();
            }
        }

        public class SomeClass
        {
            public int ReadWriteIntProperty { get; set; }

            // ReSharper disable once UnassignedGetOnlyAutoProperty
            public int ReadOnlyIntProperty { get; }

            public int WriteOnlyProperty
            {
                // ReSharper disable once ValueParameterNotUsed
                set { }
            }

            public int IntField;
        }

        private static readonly Type SomeClassType = typeof(SomeClass);

        private static readonly PropertyInfo ReadWriteIntProp = SomeClassType.GetProperty(
            nameof(SomeClass.ReadWriteIntProperty)
        );

        private static readonly PropertyInfo ReadOnlyIntProp = SomeClassType.GetProperty(
            nameof(SomeClass.ReadOnlyIntProperty)
        );

        private static readonly PropertyInfo WriteOnlyIntProp = SomeClassType.GetProperty(
            nameof(SomeClass.WriteOnlyProperty)
        );

        private static readonly PropertyInfo[] Props = 
            { ReadWriteIntProp, ReadOnlyIntProp, WriteOnlyIntProp };

        private static readonly FieldInfo IntField = SomeClassType.GetField(
            nameof(SomeClass.IntField)
        );

        private static PropertyOrField Create(
            PropertyInfo pi)
        {
            return new PropertyOrField(pi);
        }

        private static PropertyOrField Create(
            FieldInfo fi)
        {
            return new PropertyOrField(fi);
        }
    }
}