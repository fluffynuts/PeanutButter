using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests
{
    [TestFixture]
    public class TestRedirectingDictionary : AssertionHelper
    {
        [TestCase(typeof(object))]
        [TestCase(typeof(int))]
        public void Type_ShouldImplement_IDictionary_of_String_and_T(Type valueType)
        {
            // Arrange
            var genericSutType = typeof(RedirectingDictionary<>);
            var sut = genericSutType.MakeGenericType(valueType);
            var genericDictionaryType = typeof(IDictionary<,>);
            var expected = genericDictionaryType.MakeGenericType(typeof(string), valueType);
            // Pre-Assert

            // Act
            sut.ShouldImplement(expected);

            // Assert
        }

        [Test]
        public void Construct_GivenNullData_ShouldThrow_ANE()
        {
            // Arrange

            // Pre-Assert

            // Act
            Expect(
                () => new RedirectingDictionary<object>(null, null),
                Throws
                    .Exception.InstanceOf<ArgumentNullException>()
                    .With.Message.Contains("data")
            );

            // Assert
        }

        [Test]
        public void Construct_GivenNullGetterTransform_ShouldThrow_ANE()
        {
            // Arrange

            // Pre-Assert

            // Act
            Expect(
                () => new RedirectingDictionary<object>(MakeData(), null),
                Throws
                    .Exception.InstanceOf<ArgumentNullException>()
                    .With.Message.Contains("keyTransform")
            );

            // Assert
        }

        [Test]
        public void Index_Read_ShouldReturnValueofMappedKey()
        {
            // Arrange
            var data = MakeData();
            var prefix = GetRandomString(2);
            var key = GetRandomString(2);
            var expected = GetRandomString(4);
            data[$"{prefix}.{key}"] = expected;
            var sut = Create(data, s => prefix + "." + s);
            // Pre-Assert

            // Act
            var result = sut[key];

            // Assert
            Expect(result, Is.EqualTo(expected));
        }

        [Test]
        public void Index_Write_WhenDataIsReadonly_ShouldThrow()
        {
            // Arrange
            var innerData = MakeData();
            var prefix = GetRandomString(2);
            var key = GetRandomString(2);
            var expected = GetRandomString(4);
            innerData[$"{prefix}.{key}"] = expected;
            var data = new ReadOnlyDictionary<string, object>(innerData);
            var sut = Create(data, s => prefix + "." + s);
            // Pre-Assert

            // Act
            Expect(
                () => sut[key] = expected,
                Throws
                    .Exception.InstanceOf<InvalidOperationException>()
                    .With.Message.Contains("Collection is read-only")
            );

            // Assert
        }

        
        [Test]
        public void Index_Write_WhenSutIsNotReadonly_ShouldWriteThrough()
        {
            // Arrange
            var data = MakeData();
            var prefix = GetRandomString(2);
            var key = GetRandomString(2);
            var expected = GetRandomString(4);
            var nativeKey = $"{prefix}.{key}";
            data[nativeKey] = GetRandom<string>(o => o != expected);
            var sut = Create(data, s => prefix + "." + s);
            // Pre-Assert

            // Act
            Expect(
                () => sut[key] = expected,
                Throws.Nothing
            );

            // Assert
            Expect(data[nativeKey], Is.EqualTo(expected));
        }

        [Test]
        public void Count_ShouldReturnData_Count()
        {
            // Arrange
            var data = MakeRandomData();
            var sut = Create(data, s => s);
            // Pre-Assert

            // Act
            var result = sut.Count;

            // Assert
            Expect(result, Is.EqualTo(data.Count));
            data.Add(GetRandomString(5), GetRandomString(5));
            result = sut.Count;
            Expect(result, Is.EqualTo(data.Count));
        }

        [Test]
        public void Add_GivenKeyAndValue_ShouldAddItem()
        {
            // Arrange
            var data = MakeData();
            var prefix = GetRandomString(3);
            var sut = Create(data, s => prefix + s);
            var key = GetRandomString(3);
            var nativeKey = $"{prefix}{key}";
            var expected = GetRandomString();

            // Pre-Assert
            Expect(data, Has.Count.EqualTo(0));

            // Act
            sut.Add(key, expected);

            // Assert
            Expect(data, Has.Count.EqualTo(1));
            Expect(data[nativeKey], Is.EqualTo(expected));
        }

        [Test]
        public void Add_KeyValuePair_ShouldAddItem()
        {
            // Arrange
            var data = MakeData();
            var prefix = GetRandomString(3);
            var sut = Create(data, s => prefix + s);
            var key = GetRandomString(3);
            var nativeKey = $"{prefix}{key}";
            var expected = GetRandomString();

            // Pre-Assert
            Expect(data, Has.Count.EqualTo(0));

            // Act
            sut.Add(new KeyValuePair<string, object>(key, expected));

            // Assert
            Expect(data, Has.Count.EqualTo(1));
            Expect(data[nativeKey], Is.EqualTo(expected));
        }

        [Test]
        public void Clear_ShouldClear()
        {
            // Arrange
            var data = MakeRandomData();
            var sut = Create(data, s => s);

            // Pre-Assert
            Expect(data, Has.Count.GreaterThan(0));

            // Act
            sut.Clear();

            // Assert
            Expect(sut, Has.Count.EqualTo(0));
            Expect(data, Has.Count.EqualTo(0));
        }

        [Test]
        public void CanEnumerateWithTransformedKeys()
        {
            // Arrange
            var data = MakeData();
            var suffix = GetRandomString(2);
            var key = GetRandomString(2);
            var nativeKey = $"{key}{suffix}";
            var sut = Create(data, s => s + suffix);
            var expected = GetRandomString(5);
            sut[key] = expected;

            // Pre-Assert
            Expect(data[nativeKey], Is.EqualTo(expected));

            // Act
            foreach (var kvp in sut)
            {
                Expect(kvp.Key, Is.EqualTo(key));
                Expect(kvp.Value, Is.EqualTo(expected));
            }

            // Assert
        }

        [Test]
        public void Contains_GivenUnknownKeyValuePair_ShouldReturnFalse()
        {
            // Arrange
            var prefix = GetRandomString(3);
            var data = MakeRandomData(prefix);
            var seek = new KeyValuePair<string, object>(prefix + GetRandomString(11), GetRandomString(11));
            var sut = Create(data, s => prefix + s);

            // Pre-Assert
            Expect(data, Is.Not.Empty);

            // Act
            var result = sut.Contains(seek);

            // Assert
            Expect(result, Is.False);
        }

        private IDictionary<string, object> MakeRandomData(string prefix = "")
        {
            var result = MakeData();
            var howMany = GetRandomInt(3);
            howMany.TimesDo(i => result[prefix + GetRandomString(5)] = GetRandomString(5));
            return result;
        }
        

        private IDictionary<string, object> MakeData()
        {
            return new Dictionary<string, object>();
        }

        private IDictionary<string, object> Create(
            IDictionary<string, object> data,
            Func<string, string> keyTransform
        )
        {
            return new RedirectingDictionary<object>(data, keyTransform);
        }
    }
}