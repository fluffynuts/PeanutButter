using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils.Dictionaries;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestOrderedDictionary
    {
        [Test]
        public void ShouldImplementIDictionaryWithTypedParameters()
        {
            // Arrange
#pragma warning disable 618
            var sut = new OrderedDictionary<string, int>();
#pragma warning restore 618
            // Act
            Expect(sut)
                .To.Be.An.Instance.Of<IDictionary<string, int>>();
            // Assert
        }

        [Test]
        public void ShouldAddKeyValuePair()
        {
            // Arrange
            var sut = Create<string, int>();
            var key = GetRandomString();
            var value = GetRandomInt();
            // Act
            sut.Add(new KeyValuePair<string, int>(key, value));
            // Assert
            Expect(sut.Count)
                .To.Equal(1);
            Expect(sut.ContainsKey(key))
                .To.Be.True();
            Expect(sut[key])
                .To.Equal(value);
            Expect(sut.ToArray())
                .To.Deep.Equal(new[]
                {
                    new KeyValuePair<string, int>(key, value)
                });
        }

        [Test]
        public void ShouldAddKeyAndValue()
        {
            // Arrange
            var sut = Create<string, int>();
            var key = GetRandomString();
            var value = GetRandomInt();
            // Act
            sut.Add(key, value);
            // Assert
            Expect(sut.Count)
                .To.Equal(1);
            Expect(sut.ContainsKey(key))
                .To.Be.True();
            Expect(sut[key])
                .To.Equal(value);
            Expect(sut.ToArray())
                .To.Deep.Equal(new[]
                {
                    new KeyValuePair<string, int>(key, value)
                });
        }

        [TestFixture]
        public class Remove
        {
            [Test]
            public void ShouldRemoveKeyValuePairWhenKeyAndValueMatch()
            {
                // Arrange
                var sut = Create<string, int>();
                var key = GetRandomString();
                var value = GetRandomInt();
                sut[key] = value;

                // Act
                var result = sut.Remove(new KeyValuePair<string, int>(key, value));
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(sut.Count)
                    .To.Equal(0);
            }

            [Test]
            public void ShouldNotRemoveWhenKeyOfKeyValuePairDoesNotMatch()
            {
                // Arrange
                var sut = Create<string, int>();
                var key = GetRandomString();
                var value = GetRandomInt();
                sut[key] = value;

                // Act
                var result = sut.Remove(new KeyValuePair<string, int>(GetAnother(key), value));
                // Assert
                Expect(result)
                    .To.Be.False();
                Expect(sut.Count)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldNotRemoveWhenValueOfKeyValuePairDoesNotMatch()
            {
                // Arrange
                var sut = Create<string, int>();
                var key = GetRandomString();
                var value = GetRandomInt();
                sut[key] = value;

                // Act
                var result = sut.Remove(new KeyValuePair<string, int>(key, GetAnother(value)));
                // Assert
                Expect(result)
                    .To.Be.False();
                Expect(sut.Count)
                    .To.Equal(1);
            }

            [Test]
            public void ShouldRemoveByMatchingKey()
            {
                // Arrange
                var sut = Create<string, int>();
                var key = GetRandomString();
                var value = GetRandomInt();
                sut[key] = value;
                // Act
                var result = sut.Remove(key);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(sut.Count)
                    .To.Equal(0);
            }

            [Test]
            public void ShouldNotRemoveWhenKeyMismatches()
            {
                // Arrange
                var sut = Create<string, int>();
                var key = GetRandomString();
                var value = GetRandomInt();
                var otherKey = GetAnother(key);
                sut[key] = value;
                // Act
                var result = sut.Remove(otherKey);
                // Assert
                Expect(result)
                    .To.Be.False();
                Expect(sut.Count)
                    .To.Equal(1);
            }
        }

        [TestFixture]
        public class Ordering
        {
            [Test]
            public void ShouldProvideKeysInInsertedOrder_2Keys()
            {
                // Arrange
                var keys = GetRandomArray(
                    () => GetRandomInt(1, 1000000),
                    100000
                ).Distinct().ToArray();
                var sut = Create<int, int>();
                keys.ForEach(k => sut[k] = GetRandomInt());

                // Act
                // Assert
                Expect(sut.Select(kvp => kvp.Key))
                    .To.Equal(keys);
                Expect(sut.Keys)
                    .To.Equal(keys);
            }
        }

        private static IOrderedDictionary<TKey, TValue> Create<TKey, TValue>()
        {
#pragma warning disable 618
            return new OrderedDictionary<TKey, TValue>();
#pragma warning restore 618
        }
    }
}