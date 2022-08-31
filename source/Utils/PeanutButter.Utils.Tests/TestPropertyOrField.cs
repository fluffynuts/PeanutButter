using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            [TestFixture]
            public class GetValueAt
            {
                [Test]
                public void ShouldBeAbleToReadArrayElement()
                {
                    // Arrange
                    var sut = Create(NumbersArrayProp);
                    var idx = GetRandomInt(0, 2);
                    var data = new SomeClass();
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(data.NumbersArrayProp[idx]);
                }

                [Test]
                public void ShouldBeAbleToReadListElement()
                {
                    // Arrange
                    var sut = Create(NumbersListProp);
                    var idx = GetRandomInt(0, 2);
                    var data = new SomeClass();
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(data.NumbersListProp[idx]);
                }

                [Test]
                public void ShouldBeAbleToReadEnumerable()
                {
                    // Arrange
                    var sut = Create(NumbersEnumerableProp);
                    var idx = GetRandomInt(0, 2);
                    var data = new SomeClass();
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(data.NumbersEnumerableProp.Skip(idx).First());
                }

                [Test]
                public void ShouldBeAbleToIndexIntoDictionary()
                {
                    // Arrange
                    var sut = Create(StringIntDictionaryProp);
                    var data = new SomeClass();
                    var idx = GetRandomFrom(data.StringIntDictionaryProp.Keys);
                    var expected = data.StringIntDictionaryProp[idx];
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class SetValueAt
            {
                [Test]
                public void ShouldBeAbleToSetArrayValue()
                {
                    // Arrange
                    var sut = Create(NumbersArrayProp);
                    var data = new SomeClass();
                    var expected = GetRandomInt(10, 20);
                    var idx = GetRandomInt(0, 2);
                    // Act
                    sut.SetValueAt(data, expected, idx);
                    // Assert
                    Expect(data.NumbersArrayProp[idx])
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldBeAbleToSetDictionaryValue()
                {
                    // Arrange
                    var sut = Create(StringIntDictionaryProp);
                    var data = new SomeClass();
                    var key = GetRandomFrom(data.StringIntDictionaryProp.Keys);
                    var expected = GetRandomInt(100, 200);
                    // Act
                    sut.SetValueAt(data, expected, key);
                    // Assert
                    Expect(data.StringIntDictionaryProp[key])
                        .To.Equal(expected);
                }
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
            
            
            [TestFixture]
            public class GetValueAt
            {
                [Test]
                public void ShouldBeAbleToReadArrayElement()
                {
                    // Arrange
                    var sut = Create(NumbersArrayField);
                    var idx = GetRandomInt(0, 2);
                    var data = new SomeClass();
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(data.NumbersArrayField[idx]);
                }

                [Test]
                public void ShouldBeAbleToReadListElement()
                {
                    // Arrange
                    var sut = Create(NumbersListField);
                    var idx = GetRandomInt(0, 2);
                    var data = new SomeClass();
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(data.NumbersListField[idx]);
                }

                [Test]
                public void ShouldBeAbleToReadEnumerable()
                {
                    // Arrange
                    var sut = Create(NumbersEnumerableField);
                    var idx = GetRandomInt(0, 2);
                    var data = new SomeClass();
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(data.NumbersEnumerableField.Skip(idx).First());
                }

                [Test]
                public void ShouldBeAbleToIndexIntoDictionary()
                {
                    // Arrange
                    var sut = Create(StringIntDictionaryField);
                    var data = new SomeClass();
                    var idx = GetRandomFrom(data.StringIntDictionaryField.Keys);
                    var expected = data.StringIntDictionaryField[idx];
                    // Act
                    var result = sut.GetValueAt(data, idx);
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }
            }
            
            [TestFixture]
            public class SetValueAt
            {
                [Test]
                public void ShouldBeAbleToSetArrayValue()
                {
                    // Arrange
                    var sut = Create(NumbersArrayField);
                    var data = new SomeClass();
                    var expected = GetRandomInt(10, 20);
                    var idx = GetRandomInt(0, 2);
                    // Act
                    sut.SetValueAt(data, expected, idx);
                    // Assert
                    Expect(data.NumbersArrayField[idx])
                        .To.Equal(expected);
                }

                [Test]
                public void ShouldBeAbleToSetDictionaryValue()
                {
                    // Arrange
                    var sut = Create(StringIntDictionaryField);
                    var data = new SomeClass();
                    var key = GetRandomFrom(data.StringIntDictionaryField.Keys);
                    var expected = GetRandomInt(100, 200);
                    // Act
                    sut.SetValueAt(data, expected, key);
                    // Assert
                    Expect(data.StringIntDictionaryField[key])
                        .To.Equal(expected);
                }
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

            public int[] NumbersArrayProp { get; set; } = { 1, 2, 3 };
            public List<int> NumbersListProp { get; set; } = new(new[] { 4, 5, 6 });

            public IEnumerable<int> NumbersEnumerableProp { get; } =
                new SomeEnumerable<int>(new[] { 7, 8, 9 });

            public Dictionary<string, int> StringIntDictionaryProp { get; }
                = new()
                {
                    ["one"] = 1,
                    ["two"] = 2,
                    ["three"] = 3
                };
            public int[] NumbersArrayField = { 1, 2, 3 };
            public List<int> NumbersListField = new(new[] { 4, 5, 6 });

            public IEnumerable<int> NumbersEnumerableField =
                new SomeEnumerable<int>(new[] { 7, 8, 9 });

            public Dictionary<string, int> StringIntDictionaryField
                = new()
                {
                    ["one"] = 1,
                    ["two"] = 2,
                    ["three"] = 3
                };
        }

        public class SomeEnumerable<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _actual;

            public SomeEnumerable(IEnumerable<T> actual)
            {
                _actual = actual;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _actual.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
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

        private static readonly PropertyInfo NumbersArrayProp = SomeClassType.GetProperty(
            nameof(SomeClass.NumbersArrayProp)
        );

        private static readonly PropertyInfo NumbersListProp = SomeClassType.GetProperty(
            nameof(SomeClass.NumbersListProp)
        );

        private static readonly PropertyInfo NumbersEnumerableProp = SomeClassType.GetProperty(
            nameof(SomeClass.NumbersEnumerableProp)
        );

        private static readonly PropertyInfo StringIntDictionaryProp = SomeClassType.GetProperty(
            nameof(SomeClass.StringIntDictionaryProp)
        );

        private static readonly FieldInfo NumbersArrayField = SomeClassType.GetField(
            nameof(SomeClass.NumbersArrayField)
        );

        private static readonly FieldInfo NumbersListField = SomeClassType.GetField(
            nameof(SomeClass.NumbersListField)
        );

        private static readonly FieldInfo NumbersEnumerableField = SomeClassType.GetField(
            nameof(SomeClass.NumbersEnumerableField)
        );

        private static readonly FieldInfo StringIntDictionaryField = SomeClassType.GetField(
            nameof(SomeClass.StringIntDictionaryField)
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