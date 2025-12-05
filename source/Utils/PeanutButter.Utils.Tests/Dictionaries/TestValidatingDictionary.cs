using System;
using System.Collections.Generic;
using PeanutButter.Utils.Dictionaries;

namespace PeanutButter.Utils.Tests.Dictionaries;

[TestFixture]
public class TestValidatingDictionary
{
    [Test]
    public void ShouldImplementIDictionary()
    {
        // Arrange
        var sut = typeof(ValidatingDictionary<string, string>);
        // Act
        Expect(sut)
            .To.Implement<IDictionary<string, string>>();
        // Assert
    }

    [TestFixture]
    public class Construction
    {
        [Test]
        public void ShouldRequireValidationAction()
        {
            // Arrange
            // Act
            Expect(() => Create(null))
                .To.Throw<ArgumentNullException>()
                .For("validator");
            // Assert
        }
    }

    [TestFixture]
    public class Enumeration
    {
        [Test]
        public void ShouldBeAbleToEnumerateEntries()
        {
            // Arrange
            var sut = Create(NoOp);
            // Act
            var keys = GetRandomArray<string>(5, 5);
            var values = GetRandomArray<string>(5, 5);
            var reference = new Dictionary<string, string>();
            foreach (var pair in keys.StrictZip(values))
            {
                sut[pair.Item1] = pair.Item2;
                reference[pair.Item1] = pair.Item2;
            }

            // Assert
            foreach (var kvp in sut)
            {
                Expect(reference)
                    .To.Contain.Key(kvp.Key)
                    .With.Value(kvp.Value);
            }

            Expect(sut)
                .To.Equal(reference);
        }
    }

    [TestFixture]
    public class Add
    {
        [Test]
        public void ShouldBeAbleToAddKeyValuePair()
        {
            // Arrange
            var sut = Create(NoOp);
            var kvp = new KeyValuePair<string, string>(GetRandomString(), GetRandomString());
            // Act
            sut.Add(kvp);
            // Assert
            Expect(sut)
                .To.Contain.Key(kvp.Key)
                .With.Value(kvp.Value);
        }

        [Test]
        public void ShouldBeAbleToAddKeyAndValue()
        {
            // Arrange
            var sut = Create(NoOp);
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            sut.Add(key, value);
            // Assert
            Expect(sut)
                .To.Contain.Key(key)
                .With.Value(value);
        }
    }

    [TestFixture]
    public class Clear
    {
        [Test]
        public void ShouldRemoveAllItems()
        {
            // Arrange
            var sut = Create(NoOp);
            sut[GetRandomString()] = GetRandomString();
            Expect(sut)
                .Not.To.Be.Empty();
            // Act
            sut.Clear();
            // Assert
            Expect(sut)
                .To.Be.Empty();
        }
    }

    [TestFixture]
    public class ContainsKey
    {
        [TestFixture]
        public class WhenContainsExactKey
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var sut = Create(NoOp);
                sut[key] = value;
                // Act
                var result = sut.ContainsKey(key);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenContainsEqualKey
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var seek = GetAnother(key, () => key.ToRandomCase());
                Expect(seek)
                    .Not.To.Equal(key);
                Expect(seek.ToLower())
                    .To.Equal(key.ToLower());
                var sut = Create(NoOp, StringComparer.OrdinalIgnoreCase);
                sut[key] = value;
                // Act
                var result = sut.ContainsKey(seek);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenDoesNotContainEqualKey
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var key = GetRandomString();
                var seek = GetAnother(key);
                var value = GetRandomString();
                var sut = Create(NoOp);
                sut[key] = value;
                // Act
                var result = sut.ContainsKey(seek);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }
    }

    [TestFixture]
    public class Contains
    {
        [TestFixture]
        public class WhenKeyValuePairIsMatched
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var seek = new KeyValuePair<string, string>(key, value);
                var sut = Create(NoOp);
                sut[seek.Key] = seek.Value;
                // Act
                var result = sut.Contains(seek);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenKeyValuePairIsNotMatched
        {
            [TestFixture]
            public class AndKeyDifferent
            {
                [Test]
                public void ShouldReturnFalse()
                {
                    // Arrange
                    var key = GetRandomString();
                    var value = GetRandomString();
                    var seek = new KeyValuePair<string, string>(GetAnother(key), value);
                    var sut = Create(NoOp);
                    // Act
                    var result = sut.Contains(seek);
                    // Assert
                    Expect(result)
                        .To.Be.False();
                }
            }

            [TestFixture]
            public class AndValueDifferent
            {
                [Test]
                public void ShouldReturnFalse()
                {
                    // Arrange
                    var key = GetRandomString();
                    var value = GetRandomString();
                    var seek = new KeyValuePair<string, string>(key, GetAnother(value));
                    var sut = Create(NoOp);
                    // Act
                    var result = sut.Contains(seek);
                    // Assert
                    Expect(result)
                        .To.Be.False();
                }
            }

            [TestFixture]
            public class AndEntireItemDifferent
            {
                [Test]
                public void ShouldReturnFalse()
                {
                    // Arrange
                    var key = GetRandomString();
                    var value = GetRandomString();
                    var seek = new KeyValuePair<string, string>(
                        GetAnother(key),
                        GetAnother(value)
                    );
                    var sut = Create(NoOp);
                    // Act
                    var result = sut.Contains(seek);
                    // Assert
                    Expect(result)
                        .To.Be.False();
                }
            }
        }
    }

    [TestFixture]
    public class Remove
    {
        [TestFixture]
        public class WhenHaveMatchingItem
        {
            [Test]
            public void ShouldRemoveByKeyAndValue()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var toRemove = new KeyValuePair<string, string>(key, value);
                var sut = Create(NoOp);
                sut[key] = value;
                // Act
                var result = sut.Remove(toRemove);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(sut)
                    .Not.To.Contain.Key(key);
            }
        }

        [TestFixture]
        public class WhenHaveMatchingKey
        {
            [Test]
            public void ShouldRemoveByKey()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var sut = Create(NoOp);
                sut[key] = value;
                // Act
                var result = sut.Remove(key);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(sut)
                    .Not.To.Contain.Key(key);
            }
        }

        [TestFixture]
        public class WhenNoMatchingItem
        {
            [Test]
            public void ShouldNotRemoveAnything()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var toRemove1 = new KeyValuePair<string, string>(key, GetAnother(value));
                var toRemove2 = new KeyValuePair<string, string>(GetAnother(key), value);
                var toRemove3 = new KeyValuePair<string, string>(GetAnother(key), GetAnother(value));
                var sut = Create(NoOp);
                sut[key] = value;
                // Act
                var result1 = sut.Remove(toRemove1);
                var result2 = sut.Remove(toRemove2);
                var result3 = sut.Remove(toRemove3);
                // Assert
                Expect(result1)
                    .To.Be.False();
                Expect(result2)
                    .To.Be.False();
                Expect(result3)
                    .To.Be.False();
                Expect(sut)
                    .To.Contain.Key(key)
                    .With.Value(value);
                ;
            }
        }

        [TestFixture]
        public class WhenNoMatchingKey
        {
            [Test]
            public void ShouldNotRemoveAnything()
            {
                // Arrange
                var key = GetRandomString();
                var value = GetRandomString();
                var sut = Create(NoOp);
                sut[key] = value;
                // Act
                var result = sut.Remove(GetAnother(key));
                // Assert
                Expect(result)
                    .To.Be.False();
                Expect(sut)
                    .To.Contain.Key(key);
            }
        }
    }

    [TestFixture]
    public class TryGetValue
    {
        [TestFixture]
        public class WhenHaveMatchingKey
        {
            [Test]
            public void ShouldReturnTrueAndSetValue()
            {
                // Arrange
                var sut = Create(NoOp);
                var key = GetRandomString();
                var value = GetRandomString();
                sut[key] = value;
                // Act
                var result = sut.TryGetValue(key, out var stored);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(stored)
                    .To.Equal(value);
            }

            [Test]
            public void ShouldReturnTrueAndSetValue2()
            {
                // Arrange
                var sut = Create(NoOp, StringComparer.OrdinalIgnoreCase);
                var key = GetRandomString();
                var value = GetRandomString();
                sut[key] = value;
                // Act
                var result = sut.TryGetValue(key.ToRandomCase(), out var stored);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(stored)
                    .To.Equal(value);
            }
        }

        [TestFixture]
        public class WhenNoMatchingKey
        {
            [Test]
            public void ShouldReturnFalseAndSetValueNull()
            {
                // Arrange
                var sut = Create(NoOp);
                var key = GetRandomString();
                var value = GetRandomString();
                // ReSharper disable once InlineOutVariableDeclaration
                // ReSharper disable once RedundantAssignment
                var stored = GetAnother(value);
                sut[key] = value;

                // Act
                var result = sut.TryGetValue(GetAnother(key), out stored);
                // Assert
                Expect(result)
                    .To.Be.False();
                Expect(stored)
                    .To.Be.Null();
            }
        }
    }

    [TestFixture]
    public class Keys
    {
        [Test]
        public void ShouldReturnCurrentKeys()
        {
            // Arrange
            var expected = GetRandomArray<string>(100, 200).AsHashSet();
            var sut = Create(NoOp);
            foreach (var key in expected)
            {
                sut[key] = GetRandomString();
            }

            var extraKey = GetAnother<string>(expected);
            var extraValue = GetRandomString();
            // Act
            Expect(sut.Keys)
                .To.Be.Equivalent.To(expected);
            sut[extraKey] = extraValue;
            expected.Add(extraKey);
            var result = sut.Keys;
            // Assert
            Expect(result)
                .To.Be.Equivalent.To(expected);
        }
    }

    [TestFixture]
    public class Values
    {
        [Test]
        public void ShouldReturnCurrentValues()
        {
            // Arrange
            var keys = GetRandomArray<string>(100, 200).AsHashSet();
            var expected = new List<string>();
            var sut = Create(NoOp);
            foreach (var key in keys)
            {
                var value = GetRandomString();
                expected.Add(value);
                sut[key] = value;
            }

            var extraKey = GetAnother<string>(keys);
            var extraValue = GetRandomString();
            // Act
            var result1 = sut.Values;
            Expect(result1)
                .To.Be.Equivalent.To(expected);
            sut[extraKey] = extraValue;
            var result2 = sut.Values;
            // Assert
            expected.Add(extraValue);
            Expect(result2)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class Count
    {
        [Test]
        public void ShouldReturnCurrentItemCount()
        {
            // Arrange
            var sut = Create(NoOp);
            Expect(sut.Count)
                .To.Equal(0);
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            sut[key] = value;
            var result1 = sut.Count;
            sut.Remove(key);
            var result2 = sut.Count;
            // Assert
            Expect(result1)
                .To.Equal(1);
            Expect(result2)
                .To.Equal(0);
        }
    }

    [TestFixture]
    public class EnforcingValidation
    {
        [Test]
        public void ShouldValidateWhenAddingNewItem()
        {
            // Arrange
            var mutations = new List<Mutation>();
            var sut = Create(
                (dict, key, value, mutation) =>
                {
                    mutations.Add(mutation);
                    if (key == "aaa" && mutation == Mutation.Create)
                    {
                        throw new ArgumentException("aaa is not allowed as a key", nameof(key));
                    }
                }
            );
            sut["abc"] = "123";
            // Act
            Expect(() => sut["aaa"] = GetRandomString())
                .To.Throw<ArgumentException>()
                .For("key")
                .With.Message.Containing("is not allowed");
            Expect(() => sut.Add("aaa", GetRandomString()))
                .To.Throw<ArgumentException>()
                .For("key")
                .With.Message.Containing("is not allowed");
            // Assert
            Expect(sut)
                .To.Contain.Only(1).Item();
            Expect(mutations)
                .To.Equal(
                    [
                        Mutation.Create, // from setup
                        Mutation.Create, // first set attempt
                        Mutation.Create // Add attempt
                    ]
                );
        }

        [Test]
        public void ShouldValidateWhenRemovingItem()
        {
            // Arrange
            var mutations = new List<Mutation>();
            IDictionary<string, string> sut = null;
            sut = Create(
                (dict, key, value, mutation) =>
                {
                    Expect(dict)
                        .To.Be(sut);
                    if (dict.TryGetValue(key, out var stored))
                    {
                        Expect(value)
                            .To.Equal(stored);
                    }

                    mutations.Add(mutation);
                    if (key == "aaa" && mutation == Mutation.Create)
                    {
                        throw new ArgumentException("aaa is not allowed as a key", nameof(key));
                    }
                }
            );
            sut["abc"] = "123";
            // Act
            Expect(() => sut.Remove("abc"))
                .Not.To.Throw();
            Expect(sut.Remove("aaa"))
                .To.Be.False("should fail to remove what's not there, but not throw");
            // Assert
            Expect(sut)
                .To.Be.Empty();
            Expect(mutations)
                .To.Equal(
                    [
                        Mutation.Create, // from setup
                        Mutation.Remove // blocked removal
                    ]
                );
        }

        [Test]
        public void ShouldValidateOnClearing()
        {
            // Arrange
            var sut = Create(
                (dict, key, value, mutation) =>
                {
                    if (mutation != Mutation.Clear)
                    {
                        return;
                    }

                    Expect(key)
                        .To.Equal(default);
                    Expect(value)
                        .To.Equal(default);

                    if (dict.ContainsKey("precious"))
                    {
                        throw new InvalidOperationException(
                            "may not remove precious items!"
                        );
                    }
                }
            );
            // Act
            sut["junk"] = GetRandomWords();
            Expect(sut)
                .To.Contain.Key("junk");
            Expect(() => sut.Clear())
                .Not.To.Throw();
            sut["precious"] = GetRandomWords();
            Expect(() => sut.Clear())
                .To.Throw<InvalidOperationException>()
                .With.Message.Containing("may not remove precious items");
            // Assert
        }

        [Test]
        public void ShouldBeAbleToValidateValuesOnUpdate()
        {
            // Arrange
            var sut = Create(
                (dict, key, value, mutation) =>
                {
                    if (key != "lower_case")
                    {
                        return;
                    }

                    if (value != value.ToLower())
                    {
                        throw new ArgumentException(
                            "lower_case value must be lower case",
                            nameof(key)
                        );
                    }
                }
            );
            // Act
            Expect(() => sut["lower_case"] = "abc123")
                .Not.To.Throw();
            Expect(() => sut["lower_case"] = "ABC123")
                .To.Throw<ArgumentException>()
                .For("key")
                .With.Message.Containing("must be lower case");
            // Assert
        }
    }

    private static void NoOp<TKey, TValue>(
        IDictionary<TKey, TValue> arg1,
        TKey arg2,
        TValue arg3,
        Mutation mutation
    )
    {
        // does nothing, intentionally
    }

    private static IDictionary<string, string> Create(
        Action<IDictionary<string, string>, string, string, Mutation> validator,
        IEqualityComparer<string> equalityComparer = null
    )
    {
        return Create<string, string>(validator, equalityComparer);
    }

    private static IDictionary<TKey, TValue> Create<TKey, TValue>(
        Action<IDictionary<TKey, TValue>, TKey, TValue, Mutation> validator,
        IEqualityComparer<TKey> equalityComparer = null
    )
    {
        return equalityComparer is null
            ? new ValidatingDictionary<TKey, TValue>(
                validator
            )
            : new ValidatingDictionary<TKey, TValue>(
                validator,
                equalityComparer
            );
    }
}