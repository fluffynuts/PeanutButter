using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.Tests.Shimming
{
    [TestFixture]
    public class TestRedirectingDictionary
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
                    () => new RedirectingDictionary<object>(null, null, null)
                )
                .To.Throw<ArgumentException>()
                .With.Message.Containing("data");

            // Assert
        }

        [Test]
        public void Construct_GivenNullToNativeTransform_ShouldThrow_ANE()
        {
            // Arrange

            // Pre-Assert

            // Act
            Expect(
                    () => new RedirectingDictionary<object>(MakeData(), null, null)
                )
                .To.Throw<ArgumentNullException>()
                .With.Message.Containing("toNativeTransform");

            // Assert
        }

        [Test]
        public void Construct_GivenNullFromNativeTransform_ShouldThrow_ANE()
        {
            // Arrange

            // Pre-Assert

            // Act
            Expect(
                    () => new RedirectingDictionary<object>(MakeData(), s => s, null)
                )
                .To.Throw<ArgumentNullException>()
                .With.Message.Containing("fromNativeTransform");

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
            Expect(result).To.Equal(expected);
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
                    () => sut[key] = expected
                )
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("Collection is read-only");

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
                    () => sut[key] = expected
                )
                .Not.To.Throw();

            // Assert
            Expect(data)
                .To.Contain.Key(nativeKey)
                .With.Value(expected);
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
            Expect(result).To.Equal(sut.Count);
            data.Add(GetRandomString(5), GetRandomString(5));
            result = sut.Count;
            Expect(result).To.Equal(data.Count);
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
            Expect(data).To.Be.Empty();

            // Act
            sut.Add(key, expected);

            // Assert
            Expect(data).To.Contain.Only(1).Item();
            Expect(data)
                .To.Contain.Key(nativeKey)
                .With.Value(expected);
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
            Expect(data).To.Be.Empty();

            // Act
            sut.Add(new KeyValuePair<string, object>(key, expected));

            // Assert
            Expect(data).To.Contain.Only(1).Item();
            Expect(data)
                .To.Contain.Key(nativeKey)
                .With.Value(expected);
        }

        [Test]
        public void Clear_ShouldClear()
        {
            // Arrange
            var data = MakeRandomData();
            var sut = Create(data, s => s);

            // Pre-Assert
            Expect(data).Not.To.Be.Empty();

            // Act
            sut.Clear();

            // Assert
            Expect(sut).To.Be.Empty();
            Expect(data).To.Be.Empty();
        }

        [Test]
        public void CanEnumerateWithTransformedKeys()
        {
            // Arrange
            var data = MakeData();
            var suffix = GetRandomString(2);
            var key = GetRandomString(2);
            var nativeKey = $"{key}{suffix}";
            var sut = Create(data, s => s + suffix, s => s.RegexReplace($"{suffix}$", ""));
            var expected = GetRandomString(5);
            sut[key] = expected;

            // Pre-Assert
            Expect(data)
                .To.Contain.Key(nativeKey)
                .With.Value(expected);

            // Act
            foreach (var kvp in sut)
            {
                var seek = new KeyValuePair<string, object>(
                    kvp.Key + suffix,
                    kvp.Value
                );
                Expect(data)
                    .To.Contain(seek);
            }

            // Assert
        }

        [Test]
        public void Contains_GivenUnknownKeyValuePair_ShouldReturnFalse()
        {
            // Arrange
            var prefix = GetRandomString(3);
            var data = MakeRandomData(prefix);
            var seek = new KeyValuePair<string, object>(prefix + GetRandomString(11),
                GetRandomString(11));
            var sut = Create(data, s => prefix + s);

            // Pre-Assert
            Expect(data).Not.To.Be.Empty();

            // Act
            var result = sut.Contains(seek);

            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Contains_GivenKnownKeyValuePair_ShouldReturnTrue()
        {
            // Arrange
            var prefix = GetRandomString(3);
            var data = MakeRandomData(prefix);
            var seek = new KeyValuePair<string, object>(GetRandomString(11),
                GetRandomString(11));
            var actual = new KeyValuePair<string, object>(prefix + seek.Key, seek.Value);
            data.Add(actual);
            var sut = Create(data, s => prefix + s);

            // Pre-Assert
            Expect(data).Not.To.Be.Empty();

            // Act
            var result = sut.Contains(seek);

            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void Keys_ShouldReturnAllKeys()
        {
            // Arrange
            var prefix = GetRandomString(3);
            var data = MakeRandomData(prefix);
            var expected = data.Keys.Select(k => k.RegexReplace($"^{prefix}", "")).ToArray();
            var sut = Create(data, s => prefix + s, s => s.RegexReplace($"^{prefix}", ""));

            // Pre-Assert

            // Act
            var result = sut.Keys;

            // Assert
            Expect(result).To.Be.Equivalent.To(expected);
        }

        [Test]
        public void CopyTo_ShouldCopyItemsToArray()
        {
            // Arrange
            var prefix = GetRandomString(3) + ":";
            var data = MakeRandomData(prefix);
            var offset = GetRandomInt();
            var target = new KeyValuePair<string, object>[data.Count + offset];
            var sut = Create(data, s => prefix + s, s => s.RegexReplace($"^{prefix}", ""));

            // Pre-Assert

            // Act
            sut.CopyTo(target, offset);

            // Assert
            target.Skip(offset)
                .ForEach(kvp =>
                {
                    Expect(sut).To.Contain(kvp);
                });
        }

        [Test]
        public void Remove_GivenKnownItem_ShouldRemoveIt()
        {
            // Arrange
            var prefix = GetRandomString(2);
            var data = MakeRandomData(prefix);
            var sut = Create(data, s => prefix + s, s => s.RegexReplace($"^{prefix}", ""));
            var randomItem = GetRandomFrom(data);
            var toRemove =
                new KeyValuePair<string, object>(randomItem.Key.RegexReplace($"^{prefix}", ""), randomItem.Value);

            // Pre-Assert

            // Act
            var result = sut.Remove(toRemove);

            // Assert
            Expect(result).To.Be.True();
            Expect(sut).Not.To.Contain(randomItem);
        }

        [Test]
        public void Remove_GivenUnknownItem_ShouldReturnFalse()
        {
            // Arrange
            var prefix = GetRandomString(2);
            var data = MakeRandomData(prefix);
            var sut = Create(data, s => prefix + s);
            var randomItem = GetRandomFrom(data);
            var toRemove = new KeyValuePair<string, object>(
                randomItem.Key.RegexReplace($"^{prefix}", "") + "moo",
                randomItem.Value
            );

            // Pre-Assert

            // Act
            var result = sut.Remove(toRemove);

            // Assert
            Expect(result).To.Be.False();
            Expect(sut[randomItem.Key.RegexReplace($"^{prefix}", "")])
                .To.Equal(randomItem.Value);
        }

        [Test]
        public void ContainsKey_WhenNativeKeyIsNotFound_ShouldReturnFalse()
        {
            // Arrange
            var prefix = GetRandomString(4);
            var data = MakeRandomData(prefix);
            var randomItem = GetRandomFrom(data);
            var searchKey = GetRandom<string>(s => s != randomItem.Key.RegexReplace($"^{prefix}", ""));
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            var result = sut.ContainsKey(searchKey);

            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void ContainsKey_WhenNativeKeyIsFound_ShouldReturnFalse()
        {
            // Arrange
            var prefix = GetRandomString(4);
            var data = MakeRandomData(prefix);
            var randomItem = GetRandomFrom(data);
            var searchKey = randomItem.Key.RegexReplace($"^{prefix}", "");
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            var result = sut.ContainsKey(searchKey);

            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void TryGetValue_WhenKeyNotFound_ShouldReturnFalseAndSetValueToNull()
        {
            // Arrange
            var prefix = GetRandomString(3);
            var data = MakeRandomData(prefix);
            var searchKey = GetRandomString(15);
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            var result = sut.TryGetValue(searchKey, out var found);

            // Assert
            Expect(result).To.Be.False();
            Expect(found).To.Be.Null();
        }

        [Test]
        public void TryGetValue_WhenKeyIsFound_ShouldReturnTrueAndSetValue()
        {
            // Arrange
            var prefix = GetRandomString(3);
            var data = MakeRandomData(prefix);
            var randomItem = GetRandomFrom(data);
            var searchKey = randomItem.Key.RegexReplace($"^{prefix}", "");
            var expected = randomItem.Value;
            var sut = Create(data, s => prefix + s);

            // Pre-Assert

            // Act
            var result = sut.TryGetValue(searchKey, out var found);

            // Assert
            Expect(result).To.Be.True();
            Expect(found).To.Equal(expected);
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
            Expect(result).To.Be.Equivalent.To(expected);
        }

        private IDictionary<string, object> MakeRandomData(string prefix = "")
        {
            var result = MakeData();
            var howMany = GetRandomInt(3);
            howMany.TimesDo(i =>
                result[prefix + GetRandomString(5)] = GetRandomString(5));
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
                fromNativeTransform ??
                (s =>
                {
                    throw new NotImplementedException("SUT created without fromNativeTransform");
                })
            );
        }
    }
}