using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using PeanutButter.Utils.Dictionaries;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace PeanutButter.Utils.Tests.Dictionaries
{
    [TestFixture]
    public class TestCaseWarpingDictionaryWrapper
    {
        [Test]
        public void WhenCaseSensitive_ShouldWrapAroundADictionary()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandom<Tuple<string, bool, int>>();
            var other = GetAnother(value);
            var src = new Dictionary<string, object>()
            {
                [key] = value
            };
            var sut = Create(src, false);
            // Pre-Assert
            // Act
            sut[key] = other;
            // Assert
            Expect(src[key]).To.Equal(other);
        }

        [Test]
        public void WhenCaseInSensitive_ShouldWrapAroundADictionary()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandom<Tuple<string, bool, int>>();
            var other = GetAnother(value);
            var src = new Dictionary<string, object>()
            {
                [key] = value
            };
            var sut = Create(src, true);
            // Pre-Assert
            // Act
            sut[key.ToRandomCase()] = other;
            // Assert
            Expect(src[key]).To.Equal(other);
            Expect(sut[key.ToRandomCase()]).To.Equal(other);
        }

        [TestFixture]
        public class MethodsWhichShouldPassThrough
        {
            [Test]
            public void Enumeration()
            {
                // Arrange
                var actual = Substitute.For<IDictionary<string, object>>();
                var expected = Substitute.For<IEnumerator<KeyValuePair<string, object>>>();
                actual.GetEnumerator().Returns(expected);
                var sut = Create(actual, GetRandomBoolean());
                // Pre-Assert
                // Act
                var result = sut.GetEnumerator();
                // Assert
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void Add()
            {
                // Arrange
                var actual = Substitute.For<IDictionary<string, int>>();
                var key = GetRandomString();
                var value = GetRandomInt();
                var sut = Create(actual, GetRandomBoolean());
                // Pre-Assert
                // Act
                sut.Add(key, value);
                actual.Received(1).Add(key, value);
                var kvp = new KeyValuePair<string, int>(key, value);
                sut.Add(kvp);
                actual.Received(1).Add(kvp);
                // Assert
            }

            [Test]
            public void Clear()
            {
                // Arrange
                var actual = Substitute.For<IDictionary<string, int>>();
                var sut = Create(actual, GetRandomBoolean());
                // Pre-Assert
                // Act
                sut.Clear();
                // Assert
                actual.Received(1).Clear();
            }

            [Test]
            public void CopyTo()
            {
                // Arrange
                var actual = Substitute.For<IDictionary<string, bool>>();
                var target = new KeyValuePair<string, bool>[GetRandomInt(10, 20)];
                var offset = GetRandomInt(1, 5);
                var sut = Create(actual, GetRandomBoolean());
                // Pre-Assert
                // Act
                sut.CopyTo(target, offset);
                // Assert
                actual.Received(1).CopyTo(target, offset);
            }

            [Test]
            public void Count()
            {
                // Arrange
                var actual = Substitute.For<IDictionary<string, bool>>();
                var expected = GetRandomInt();
                actual.Count.Returns(expected);
                var sut = Create(actual, GetRandomBoolean());
                // Pre-Assert
                // Act
                var result = sut.Count;
                // Assert
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void IsReadOnly()
            {
                // Arrange
                var actual = Substitute.For<IDictionary<string, bool>>();
                var expected = GetRandomBoolean();
                actual.IsReadOnly.Returns(expected);
                var sut = Create(actual, GetRandomBoolean());
                // Pre-Assert
                // Act
                var result = sut.IsReadOnly;
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        [Test]
        public void Contains_WhenCaseSensitive_ShouldReturnForExactMatch()
        {
            // Arrange
            var key = GetRandomString(10);
            var value = GetRandomInt();
            var actual = new Dictionary<string, object>()
            {
                [key] = value
            };
            var exactMatch = new KeyValuePair<string, object>(key, value);
            var fuzzedKey = key.ToRandomCase();
            while (fuzzedKey == key)
            {
                fuzzedKey = key.ToRandomCase();
            }

            var fuzzyMatch = new KeyValuePair<string, object>(fuzzedKey, value);
            var sut = Create(actual, false);

            // Pre-Assert
            // Act
            var exactResult = sut.Contains(exactMatch);
            var fuzzyResult = sut.Contains(fuzzyMatch);

            // Assert
            Expect(exactResult).To.Be.True();
            Expect(fuzzyResult).To.Be.False();
        }

        [Test]
        public void Contains_WhenCaseInsensitive_ShouldReturnForExactMatch()
        {
            // Arrange
            var key = GetRandomString(10);
            var value = GetRandomInt();
            var actual = new Dictionary<string, object>()
            {
                [key] = value
            };
            var exactMatch = new KeyValuePair<string, object>(key, value);
            var fuzzyMatch = new KeyValuePair<string, object>(key.ToRandomCase(), value);
            var sut = Create(actual, true);

            // Pre-Assert
            // Act
            var exactResult = sut.Contains(exactMatch);
            var fuzzyResult = sut.Contains(fuzzyMatch);

            // Assert
            Expect(exactResult).To.Be.True();
            Expect(fuzzyResult).To.Be.True();
        }

        [TestFixture]
        public class Remove
        {
            [Test]
            public void GivenKVP_WhenCaseSensitive()
            {
                // Arrange
                var key = GetRandomAlphaString(10);
                var value = GetRandomInt();
                var actual = new Dictionary<string, object>()
                {
                    [key] = value
                };
                var sut = Create(actual, false);
                // Pre-Assert
                // Act
                var mismatchedValueResult = sut.Remove(new KeyValuePair<string, object>(key, GetAnother(value)));
                Expect(actual).To.Contain.Exactly(1).Item();

                var mismatchedKeyResult = sut.Remove(
                    new KeyValuePair<string, object>(key.ToRandomCase(), value));
                Expect(actual).To.Contain.Exactly(1).Item();
                var exactMatchResult = sut.Remove(new KeyValuePair<string, object>(key, value));
                Expect(actual).To.Contain.No.Items();
                // Assert
                Expect(mismatchedValueResult).To.Be.False();
                Expect(mismatchedKeyResult).To.Be.False();
                Expect(exactMatchResult).To.Be.True();
            }

            [Test]
            public void GivenKVP_WhenCaseInsensitive()
            {
                // Arrange
                var key = GetRandomString(10);
                var value = GetRandomInt();
                var actual = new Dictionary<string, object>()
                {
                    [key] = value
                };
                var sut = Create(actual, true);
                // Pre-Assert
                // Act
                var mismatchedKeyResult = sut.Remove(new KeyValuePair<string, object>(key.ToRandomCase(), value));
                Expect(actual).To.Contain.No.Items();
                // Assert
                Expect(mismatchedKeyResult).To.Be.True();
            }

            [Test]
            public void GivenKey_WhenCaseSensitive()
            {
                // Arrange
                var key = GetRandomString(10);
                var value = GetRandomInt();
                var actual = new Dictionary<string, object>()
                {
                    [key] = value
                };
                var sut = Create(actual, false);
                // Pre-Assert
                // Act
                var mismatchedValueResult = sut.Remove(GetAnother(key));
                Expect(actual).To.Contain.Exactly(1).Item();
                var mismatchedKeyResult = sut.Remove(key.ToRandomCase());
                Expect(actual).To.Contain.Exactly(1).Item();
                var exactMatchResult = sut.Remove(key);
                Expect(actual).To.Contain.No.Items();
                // Assert
                Expect(mismatchedValueResult).To.Be.False();
                Expect(mismatchedKeyResult).To.Be.False();
                Expect(exactMatchResult).To.Be.True();
            }

            [Test]
            public void GivenKey_WhenCaseInsensitive()
            {
                // Arrange
                var key = GetRandomString(10);
                var value = GetRandomInt();
                var actual = new Dictionary<string, object>()
                {
                    [key] = value
                };
                var sut = Create(actual, true);
                // Pre-Assert
                // Act
                var mismatchedKeyResult = sut.Remove(key.ToRandomCase());
                Expect(actual).To.Contain.No.Items();
                // Assert
                Expect(mismatchedKeyResult).To.Be.True();
            }
        }

        [Test]
        public void ContainsKey_CaseSensitive()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandom<Tuple<int, bool, string>>();
            var actual = new Dictionary<string, object>()
            {
                [key] = value
            };
            var sut = Create(actual, false);
            // Pre-Assert
            // Act
            var matchResult = sut.ContainsKey(key);
            var noMatchResult = sut.ContainsKey(GetAnother(key));
            // Assert
            Expect(matchResult).To.Be.True();
            Expect(noMatchResult).To.Be.False();
        }

        [Test]
        public void ContainsKey_CaseInsensitive()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandom<Tuple<int, bool, string>>();
            var actual = new Dictionary<string, object>()
            {
                [key] = value
            };
            var sut = Create(actual, true);
            // Pre-Assert
            // Act
            var matchResult = sut.ContainsKey(key.ToRandomCase());
            var noMatchResult = sut.ContainsKey(GetAnother(key));
            // Assert
            Expect(matchResult).To.Be.True();
            Expect(noMatchResult).To.Be.False();
        }

        [Test]
        public void Enumeration()
        {
            // Arrange
            var actual = new Dictionary<string, int>()
            {
                ["one"] = 1,
                ["two"] = 2
            };
            var sut = Create(actual, true);
            // Pre-assert
            // Act
            var result = sut.ToArray();
            // Assert
            Expect(result).To.Contain.Exactly(1).Deep.Equal.To(
                new KeyValuePair<string, int>("one", 1)
            );
            Expect(result).To.Contain.Exactly(1).Deep.Equal.To(
                new KeyValuePair<string, int>("two", 2)
            );
        }

        [Test]
        public void EnumerationAsIEnumerable()
        {
            // Arrange
            var actual = new Dictionary<string, int>()
            {
                ["one"] = 1,
                ["two"] = 2
            };
            var sut = Create(actual, true) as IEnumerable;
            // Pre-assert
            // Act
            var result = new List<KeyValuePair<string, int>>();
            foreach (KeyValuePair<string, int> kvp in sut)
            {
                result.Add(kvp);
            }

            // Assert
            Expect(result).To.Contain.Exactly(1).Deep.Equal.To(
                new KeyValuePair<string, int>("one", 1)
            );
            Expect(result).To.Contain.Exactly(1).Deep.Equal.To(
                new KeyValuePair<string, int>("two", 2)
            );
        }

        [Test]
        public void SetValue_WhenNewValue()
        {
            // Arrange
            var actual = new Dictionary<string, int>();
            var key = GetRandomString(10);
            var value = GetRandomInt();
            var sut = Create(actual, true);
            // Pre-assert
            // Act
            sut[key] = value;
            // Assert
            Expect(actual[key]).To.Equal(value);
        }

        [Test]
        public void SetValue_WhenUpdatedValue()
        {
            // Arrange
            var actual = new Dictionary<string, int>();
            var key = GetRandomString(10);
            var original = GetRandomInt();
            var updated = GetAnother(original);
            var sut = Create(actual, true);
            // Pre-assert
            // Act
            sut[key] = original;
            sut[key] = updated;
            // Assert
            Expect(actual[key]).To.Equal(updated);
        }

        [Test]
        public void TryingToRetrieveFromUnkownKey()
        {
            // Arrange
            var known = GetRandomString(2);
            var unknown = GetAnother(known, () => GetRandomString(2));
            var sut = Create(
                new Dictionary<string, int>()
                {
                    [known] = GetRandomInt()
                },
                true);
            // Pre-assert
            // Act
            Expect(() => sut[unknown])
                .To.Throw<KeyNotFoundException>();
            // Assert
        }

        [Test]
        public void Keys_ShouldReturnUnderlyingKeys()
        {
            // Arrange
            var k1 = GetRandomString(2).ToPascalCase();
            var k2 = GetAnother(k1, () => GetRandomString(2).ToPascalCase());
            var actual = new Dictionary<string, bool>()
            {
                [k1] = GetRandomBoolean(),
                [k2] = GetRandomBoolean()
            };
            var sut = Create(actual, true);
            // Pre-assert
            // Act
            var keys = sut.Keys.ToArray();
            // Assert
            Expect(keys).To.Be.Equivalent.To(
            [
                k1,
                    k2
            ]
            );
        }

        [Test]
        public void Values_ShouldReturnAllValues()
        {
            // Arrange
            var k1 = GetRandomString(2).ToPascalCase();
            var k2 = GetAnother(k1, () => GetRandomString(2).ToPascalCase());
            var v1 = GetRandomInt();
            var v2 = GetRandomInt();
            var actual = new Dictionary<string, int>()
            {
                [k1] = v1,
                [k2] = v2
            };
            var sut = Create(actual, true);
            // Pre-assert
            // Act
            var values = sut.Values.ToArray();
            // Assert
            Expect(values).To.Be.Equivalent.To(
            [
                v1,
                    v2
            ]
            );
        }

        [Test]
        public void MergeDictionaryWithFallback()
        {
            // Arrange
            var expected = GetRandomString();
            var src = new MergeDictionary<string, object>(
                new DefaultDictionary<string, object>(() => expected)
            );
            
            Expect(src["BaseUrl"])
                .To.Equal(expected);
            
            // Act
            var result = new CaseWarpingDictionaryWrapper<object>(src, true);
            // Assert
            Expect(result["BaseUrl"])
                .To.Equal(expected);
        }

        private static IDictionary<string, TValue> Create<TValue>(
            IDictionary<string, TValue> src,
            bool caseInsensitive
        )
        {
            return new CaseWarpingDictionaryWrapper<TValue>(
                src,
                caseInsensitive
            );
        }
    }
}