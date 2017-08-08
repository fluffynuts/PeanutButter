using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using NExpect;
using NExpect.Interfaces;
using NExpect.MatcherLogic;
using NUnit.Framework;
using PeanutButter.DuckTyping.Extensions;
using PeanutButter.DuckTyping.Shimming;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static PeanutButter.Utils.PyLike;

namespace PeanutButter.DuckTyping.Tests.Extensions
{
    [TestFixture]
    public class TestDictionaryWrappingConnectionStringCollection
    {
        [Test]
        public void Index_ShouldBeAbleToReadExisting()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            var input = new ConnectionStringSettingsCollection
            {
                new ConnectionStringSettings(key, value)
            };
            var sut = Create(input);
            // Pre-Assert
            // Act
            var result = sut[key];
            // Assert
            Expect(result).To.Equal(value);
        }

        [Test]
        public void Set_ShouldSetConnectionString()
        {
            // Arrange
            var key = GetRandomString();
            var existing = GetRandomString();
            var expected = GetRandomString();
            var setting = new ConnectionStringSettings(key, existing);
            var input = new ConnectionStringSettingsCollection()
            {
                setting
            };
            var sut = Create(input);
            // Pre-Assert
            // Act
            sut[key] = expected;
            // Assert
            Expect(setting.ConnectionString).To.Equal(expected);
        }

        [Test]
        public void Keys_ShouldReturnAllKeys()
        {
            // Arrange
            var setting1 = GetRandom<ConnectionStringSettings>();
            var setting2 = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection()
            {
                setting1,
                setting2
            };
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Keys;
            // Assert
            Expect(result.Count).To.Equal(2);
            Expect(result.First()).To.Equal(setting1.Name);
            Expect(result.Second()).To.Equal(setting2.Name);
        }

        [Test]
        public void Count_ShouldReturnCorrectCount()
        {
            // Arrange
            var setting1 = GetRandom<ConnectionStringSettings>();
            var setting2 = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection()
            {
                setting1,
                setting2
            };
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Count;
            // Assert
            Expect(result).To.Equal(2);
        }

        [Test]
        public void Values_ShouldReturnAllConnectionStrings()
        {
            // Arrange
            var setting1 = GetRandom<ConnectionStringSettings>();
            var setting2 = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection()
            {
                setting1,
                setting2
            };
            var sut = Create(collection);
            var expected = new[] {setting1, setting2}.Select(o => o.ConnectionString);
            // Pre-Assert
            // Act
            var result = sut.Values;
            // Assert
            Expect(result.Count).To.Equal(2);
            // TODO: update when NExpect has Equlivalence
            Assert.That(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void IsReadOnly_ShouldReturnFalse()
        {
            // Arrange
            var collection = new ConnectionStringSettingsCollection();
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.IsReadOnly;
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void TryGetValue_WhenValueExists_ShouldReturnTrueAndOutValue()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.TryGetValue(setting.Name, out var connectionString);
            // Assert
            Expect(result).To.Be.True();
            Expect(connectionString).To.Equal(setting.ConnectionString);
        }

        [Test]
        public void TryGetValue_WhenValueDoesNotExist_ShouldReturnFalse()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.TryGetValue(GetAnother(setting.Name), out var connectionString);
            // Assert
            Expect(result).To.Be.False();
            Expect(connectionString).To.Be.Null();
        }

        [Test]
        public void Remove_GivenKey_WhenExists_ShouldRemove()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Remove(setting.Name);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void Remove_GivenKey_WhenDoesNotExist_ShouldNotRemove()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Remove(GetAnother(setting.Name));
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Remove_GivenKeyValuePair_WhenExistsExactly_ShouldRemove()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var item = new KeyValuePair<string, object>(setting.Name, setting.ConnectionString);
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Remove(item);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void Remove_GivenKeyValuePair_WhenMatchingKeyDoesNotMatchConnectionString_ShouldNotRemove()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var item = new KeyValuePair<string, object>(setting.Name, GetAnother(setting.ConnectionString));
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Remove(item);
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Remove_GivenKeyValueItem_WhenDoesNotExist_ShouldNotRemove()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var item = new KeyValuePair<string, object>(GetAnother(setting.Name), setting.ConnectionString);
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Remove(item);
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Add_GivenNameAndConnectionString_ShouldAdd()
        {
            // Arrange
            var collection = new ConnectionStringSettingsCollection();
            var item = GetRandom<ConnectionStringSettings>();
            var sut = Create(collection);
            // Pre-Assert
            // Act
            sut.Add(item.Name, item.ConnectionString);
            var result = collection[item.Name];
            // Assert
            Expect(result.ConnectionString).To.Equal(item.ConnectionString);
        }

        [Test]
        public void Add_GivenKeyValuePair_ShouldAdd()
        {
            // Arrange
            var collection = new ConnectionStringSettingsCollection();
            var item = GetRandom<KeyValuePair<string, string>>();
            var sut = Create(collection);
            // Pre-Assert
            // Act
            sut.Add(item.Key, item.Value);
            var result = collection[item.Key];
            // Assert
            Expect(result.ConnectionString).To.Equal(item.Value);
        }

        [Test]
        public void ContainsKey_WhenHaveKey_ShouldReturnTrue()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.ContainsKey(setting.Name);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void ContainsKey_WhenDoNotHaveKey_ShouldReturnFalse()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.ContainsKey(GetAnother(setting.Name));
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenHaveExactMatch_ShouldReturnTrue()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var item = new KeyValuePair<string, object>(setting.Name, setting.ConnectionString);
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Contains(item);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenHaveKeyMatchOnly_ShouldReturnFalse()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var item = new KeyValuePair<string, object>(
                setting.Name, GetAnother(setting.ConnectionString)
            );
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Contains(item);
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void Contains_GivenKeyValuePair_WhenHaveValueMatchOnly_ShouldReturnFalse()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var item = new KeyValuePair<string, object>(
                GetAnother(setting.Name), setting.ConnectionString
            );
            var collection = new ConnectionStringSettingsCollection() {setting};
            var sut = Create(collection);
            // Pre-Assert
            // Act
            var result = sut.Contains(item);
            // Assert
            Expect(result).To.Be.False();
        }

        [Test]
        public void CopyTo_ShouldCopyToTargetArray()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() {setting};
            var target = new KeyValuePair<string, object>[10];
            var sut = Create(collection);
            // Pre-Assert
            // Act
            sut.CopyTo(target, 2);
            // Assert
            var defaultValue = default(KeyValuePair<string, object>);
            Range(2).ForEach(i => Expect(target[i]).To.Equal(defaultValue));
            Expect(target[2].Key).To.Equal(setting.Name);
            Expect(target[2].Value).To.Equal(setting.ConnectionString);
            Range(3, 10).ForEach(i => Expect(target[i]).To.Equal(defaultValue));
        }

        [Test]
        public void Enumeration()
        {
            // Arrange
            var setting1 = GetRandom<ConnectionStringSettings>();
            var setting2 = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection()
            {
                setting1,
                setting2
            };
            var collector = new List<KeyValuePair<string, object>>();
            var sut = Create(collection);
            // Pre-Assert
            // Act
            foreach (var item in sut)
                collector.Add(item);

            // Assert
            Expect(collector.Count).To.Equal(2);
            Expect(collector.ToArray()).To
                .Contain.Exactly(1).Equal.To(new KeyValuePair<string, object>(setting1.Name, setting1.ConnectionString));
            Expect(collector.ToArray()).To
                .Contain.Exactly(1).Equal.To(new KeyValuePair<string, object>(setting2.Name, setting2.ConnectionString));
        }

        [Test]
        public void Clear_ShouldClear()
        {
            // Arrange
            var setting = GetRandom<ConnectionStringSettings>();
            var collection = new ConnectionStringSettingsCollection() { setting };
            var sut = Create(collection);
            // Pre-Assert
            // Act
            sut.Clear();
            // Assert
            Expect(sut.Keys.Count).To.Equal(0);
        }

        private IDictionary<string, object> Create(ConnectionStringSettingsCollection wrap)
        {
            return new DictionaryWrappingConnectionStringSettingCollection(wrap);
        }
    }

}