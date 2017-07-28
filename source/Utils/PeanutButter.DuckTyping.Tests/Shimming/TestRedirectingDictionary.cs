using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using PeanutButter.DatabaseHelpers;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Tests.Shimming
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
                () => new RedirectingDictionary<object>(null, null, null),
                Throws
                    .Exception.InstanceOf<ArgumentNullException>()
                    .With.Message.Contains("data")
            );

            // Assert
        }

        [Test]
        public void Construct_GivenNullToNativeTransform_ShouldThrow_ANE()
        {
            // Arrange

            // Pre-Assert

            // Act
            Expect(
                () => new RedirectingDictionary<object>(MakeData(), null, null),
                Throws
                    .Exception.InstanceOf<ArgumentNullException>()
                    .With.Message.Contains("toNativeTransform")
            );

            // Assert
        }

        [Test]
        public void Construct_GivenNullFromNativeTransform_ShouldThrow_ANE()
        {
            // Arrange

            // Pre-Assert

            // Act
            Expect(
                () => new RedirectingDictionary<object>(MakeData(), s => s, null),
                Throws
                    .Exception.InstanceOf<ArgumentNullException>()
                    .With.Message.Contains("fromNativeTransform")
            );

            // Assert
        }

        [Test]
        public void Index_Read_ShouldReturnValueofMappedKey()
        {
            // Arrange
            var data = MakeData();
            var prefix = RandomValueGen.GetRandomString(2);
            var key = RandomValueGen.GetRandomString(2);
            var expected = RandomValueGen.GetRandomString(4);
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
            var prefix = RandomValueGen.GetRandomString(2);
            var key = RandomValueGen.GetRandomString(2);
            var expected = RandomValueGen.GetRandomString(4);
            innerData[$"{prefix}.{key}"] = expected;
            // TEST ME: getting this from another project since net40 doesn't have one
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
            var prefix = RandomValueGen.GetRandomString(2);
            var key = RandomValueGen.GetRandomString(2);
            var expected = RandomValueGen.GetRandomString(4);
            var nativeKey = $"{prefix}.{key}";
            data[nativeKey] = RandomValueGen.GetRandom<string>(o => o != expected);
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
            data.Add(RandomValueGen.GetRandomString(5), RandomValueGen.GetRandomString(5));
            result = sut.Count;
            Expect(result, Is.EqualTo(data.Count));
        }

        [Test]
        public void Add_GivenKeyAndValue_ShouldAddItem()
        {
            // Arrange
            var data = MakeData();
            var prefix = RandomValueGen.GetRandomString(3);
            var sut = Create(data, s => prefix + s);
            var key = RandomValueGen.GetRandomString(3);
            var nativeKey = $"{prefix}{key}";
            var expected = RandomValueGen.GetRandomString();

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
            var prefix = RandomValueGen.GetRandomString(3);
            var sut = Create(data, s => prefix + s);
            var key = RandomValueGen.GetRandomString(3);
            var nativeKey = $"{prefix}{key}";
            var expected = RandomValueGen.GetRandomString();

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
            var suffix = RandomValueGen.GetRandomString(2);
            var key = RandomValueGen.GetRandomString(2);
            var nativeKey = $"{key}{suffix}";
            var sut = Create(data, s => s + suffix, s => s.RegexReplace($"{suffix}$", ""));
            var expected = RandomValueGen.GetRandomString(5);
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
            var prefix = RandomValueGen.GetRandomString(3);
            var data = MakeRandomData(prefix);
            var seek = new KeyValuePair<string, object>(prefix + RandomValueGen.GetRandomString(11), RandomValueGen.GetRandomString(11));
            var sut = Create(data, s => prefix + s);

            // Pre-Assert
            Expect(data, Is.Not.Empty);

            // Act
            var result = sut.Contains(seek);

            // Assert
            Expect(result, Is.False);
        }

        [Test]
        public void Contains_GivenKnownKeyValuePair_ShouldReturnTrue()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(3);
            var data = MakeRandomData(prefix);
            var seek = new KeyValuePair<string, object>(RandomValueGen.GetRandomString(11), RandomValueGen.GetRandomString(11));
            var actual = new KeyValuePair<string, object>(prefix + seek.Key, seek.Value);
            data.Add(actual);
            var sut = Create(data, s => prefix + s);

            // Pre-Assert
            Expect(data, Is.Not.Empty);

            // Act
            var result = sut.Contains(seek);

            // Assert
            Expect(result, Is.True);
        }

        [Test]
        public void Keys_ShouldReturnAllKeys()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(3);
            var data = MakeRandomData(prefix);
            var expected = data.Keys.Select(k => k.RegexReplace($"^{prefix}", "")).ToArray();
            var sut = Create(data, s => prefix + s, s => s.RegexReplace($"^{prefix}", ""));

            // Pre-Assert

            // Act
            var result = sut.Keys;

            // Assert
            Expect(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void CopyTo_ShouldCopyItemsToArray()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(3) + ":";
            var data = MakeRandomData(prefix);
            var offset = RandomValueGen.GetRandomInt();
            var target = new KeyValuePair<string, object>[data.Count + offset];
            var sut = Create(data, s => prefix + s, s => s.RegexReplace($"^{prefix}", ""));

            // Pre-Assert

            // Act
            sut.CopyTo(target, offset);

            // Assert
            target.Skip(offset).ForEach(kvp => 
            {
                Expect(sut.Contains(kvp));
            });
        }

        [Test]
        public void Remove_GivenKnownItem_ShouldRemoveIt()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(2);
            var data = MakeRandomData(prefix);
            var sut = Create(data, s => prefix + s, s => s.RegexReplace($"^{prefix}", ""));
            var randomItem = RandomValueGen.GetRandomFrom(data);
            var toRemove = new KeyValuePair<string, object>(randomItem.Key.RegexReplace($"^{prefix}", ""), randomItem.Value);

            // Pre-Assert

            // Act
            var result = sut.Remove(toRemove);

            // Assert
            Expect(result, Is.True);
            Expect(sut, Does.Not.Contains(randomItem));
        }

        [Test]
        public void Remove_GivenUnknownItem_ShouldReturnFalse()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(2);
            var data = MakeRandomData(prefix);
            var sut = Create(data, s => prefix + s);
            var randomItem = RandomValueGen.GetRandomFrom(data);
            var toRemove = new KeyValuePair<string, object>(
                randomItem.Key.RegexReplace($"^{prefix}", "") + "moo", 
                randomItem.Value
            );

            // Pre-Assert

            // Act
            var result = sut.Remove(toRemove);

            // Assert
            Expect(result, Is.False);
            Expect(sut[randomItem.Key.RegexReplace($"^{prefix}", "")], Is.EqualTo(randomItem.Value));
        }

        [Test]
        public void ContainsKey_WhenNativeKeyIsNotFound_ShouldReturnFalse()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(4);
            var data = MakeRandomData(prefix);
            var randomItem = RandomValueGen.GetRandomFrom(data);
            var searchKey = RandomValueGen.GetRandom<string>(s => s != randomItem.Key.RegexReplace($"^{prefix}", ""));
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            var result = sut.ContainsKey(searchKey);

            // Assert
            Expect(result, Is.False);
        }

        [Test]
        public void ContainsKey_WhenNativeKeyIsFound_ShouldReturnFalse()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(4);
            var data = MakeRandomData(prefix);
            var randomItem = RandomValueGen.GetRandomFrom(data);
            var searchKey = randomItem.Key.RegexReplace($"^{prefix}", "");
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            var result = sut.ContainsKey(searchKey);

            // Assert
            Expect(result, Is.True);
        }

        [Test]
        public void TryGetValue_WhenKeyNotFound_ShouldReturnFalseAndSetValueToNull()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(3);
            var data = MakeRandomData(prefix);
            var searchKey = RandomValueGen.GetRandomString(15);
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            object found = new object();
            var result = sut.TryGetValue(searchKey, out found);

            // Assert
            Expect(result, Is.False);
            Expect(found, Is.Null);
        }

        [Test]
        public void TryGetValue_WhenKeyIsFound_ShouldReturnTrueAndSetValue()
        {
            // Arrange
            var prefix = RandomValueGen.GetRandomString(3);
            var data = MakeRandomData(prefix);
            var randomItem = RandomValueGen.GetRandomFrom(data);
            var searchKey = randomItem.Key.RegexReplace($"^{prefix}", "");
            var expected = randomItem.Value;
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            object found;
            var result = sut.TryGetValue(searchKey, out found);

            // Assert
            Expect(result, Is.True);
            Expect(found, Is.EqualTo(expected));
        }

        [Test]
        public void Values_ShouldReturnAllValues()
        {
            // Arrange
            var data = MakeRandomData();
            var sut = Create(data, s => s);
            var expected = data.Values.ToArray();

            // Pre-Assert

            // Act
            var result = sut.Values;

            // Assert
            Expect(result, Is.EquivalentTo(expected));
        }

        private IDictionary<string, object> MakeRandomData(string prefix = "")
        {
            var result = MakeData();
            var howMany = RandomValueGen.GetRandomInt(3);
            howMany.TimesDo(i => result[prefix + RandomValueGen.GetRandomString(5)] = RandomValueGen.GetRandomString(5));
            return result;
        }


        private IDictionary<string, object> MakeData()
        {
            return new Dictionary<string, object>();
        }

        private IDictionary<string, object> Create(
            IDictionary<string, object> data,
            Func<string, string> toNativeTransform,
            Func<string, string> fromNativeTransform = null
        )
        {
            return new RedirectingDictionary<object>(
                data, 
                toNativeTransform, 
                fromNativeTransform ?? (s => 
                {
                    throw new NotImplementedException("SUT created without fromNativeTransform");
                })
            );
        }

    }
}