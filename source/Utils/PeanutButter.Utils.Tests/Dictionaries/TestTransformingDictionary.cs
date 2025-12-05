using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils.Dictionaries;

namespace PeanutButter.Utils.Tests.Dictionaries;

[TestFixture]
public class TestTransformingDictionary
{
    [TestFixture]
    public class Construction
    {
        [Test]
        public void ShouldRequireMutator()
        {
            // Arrange
            // Act
            Expect(() => Create<string, string>(null, null))
                .To.Throw<ArgumentNullException>()
                .For("mutator");
            // Assert
        }

        [Test]
        public void ShouldRequireAnUnderlyingDictionary()
        {
            // Arrange
            // Act
            Expect(() => Create<string, string>(s => s.Value, null))
                .To.Throw<ArgumentNullException>()
                .For("underlyingData");
            // Assert
        }
    }

    [TestFixture]
    public class GetEnumerator
    {
        [Test]
        public void ShouldReturnEnumeratorThatReturnsTransformedValues()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var sut = Create(o => $"_{o.Value}_", data);
            // Act
            using var dataEnumerator = data.GetEnumerator();
            using var sutEnumerator = sut.GetEnumerator();
            while (dataEnumerator.MoveNext())
            {
                sutEnumerator.MoveNext();
                Expect(sutEnumerator.Current.Key)
                    .To.Equal(dataEnumerator.Current.Key);
                Expect(sutEnumerator.Current.Value)
                    .To.Equal($"_{dataEnumerator.Current.Value}_");
            }

            Expect(sutEnumerator.MoveNext())
                .To.Be.False();

            // Assert
        }
    }

    [TestFixture]
    public class IsReadonly
    {
        [Test]
        public void ShouldReflectUnderlyingDataState()
        {
            // Arrange
            var rw = new Dictionary<string, string>() as IDictionary<string, string>;
            var ro = new MergeDictionary<string, string>(
                new Dictionary<string, string>()
            );
            Expect(ro.IsReadOnly)
                .To.Be.True();
            Expect(rw.IsReadOnly)
                .To.Be.False();
            var sut1 = Create(NoOp, rw);
            var sut2 = Create(NoOp, ro);
            // Act

            // Assert
            Expect(sut1.IsReadOnly)
                .To.Be.False();
            Expect(sut2.IsReadOnly)
                .To.Be.True();
        }
    }

    [TestFixture]
    public class Count
    {
        [Test]
        public void ShouldReflectUnderlyingDataCount()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var sut = Create(NoOp, data);
            // Act
            var result = sut.Count;
            // Assert
            Expect(result)
                .To.Equal(data.Count);
        }
    }

    [TestFixture]
    public class ContainsKey
    {
        [Test]
        public void ShouldReturnResultFromUnderlyingData()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var shouldFind = GetRandomFrom(data.Keys.ToArray());
            var shouldMiss = GetAnother<string>(data.Keys.ToArray());
            var sut = Create(NoOp, data);
            // Act
            var findResult = sut.ContainsKey(shouldFind);
            var missResult = sut.ContainsKey(shouldMiss);
            // Assert
            Expect(findResult)
                .To.Be.True();
            Expect(missResult)
                .To.Be.False();
        }
    }

    [TestFixture]
    public class Contains
    {
        [Test]
        public void ShouldReturnResultFromUnderlyingData()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var shouldFind = GetRandomFrom(data.ToArray());
            var shouldMiss = GetAnother<KeyValuePair<string, string>>(data.ToArray());
            var sut = Create(NoOp, data);
            // Act
            var findResult = sut.Contains(shouldFind);
            var missResult = sut.Contains(shouldMiss);
            // Assert
            Expect(findResult)
                .To.Be.True();
            Expect(missResult)
                .To.Be.False();
        }
    }

    [TestFixture]
    public class CopyTo
    {
        [Test]
        public void ShouldCopyMutated()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var sut = Create(Prefix("*"), data);
            var target = new KeyValuePair<string, string>[data.Count];
            // Act
            sut.CopyTo(target, 0);
            // Assert
            foreach (var item in target)
            {
                Expect(item.Value)
                    .To.Equal(sut[item.Key]);
            }
        }
    }

    [TestFixture]
    public class TryGetValue
    {
        [Test]
        public void ShouldProvideMutatedValueWhenPossible()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var shouldFind = GetRandomFrom(data.Keys.ToArray());
            var shouldMiss = GetAnother<string>(data.Keys.ToArray());
            var sut = Create(Prefix("~"), data);
            // Act
            var result1 = sut.TryGetValue(shouldFind, out var foundResult);
            var result2 = sut.TryGetValue(shouldMiss, out var missedResult);
            // Assert
            Expect(result1)
                .To.Be.True();
            Expect(foundResult)
                .To.Equal($"~{data[shouldFind]}");
            Expect(result2)
                .To.Be.False();
            Expect(missedResult)
                .To.Be.Null();
        }
    }

    [TestFixture]
    public class Keys
    {
        [Test]
        public void ShouldReflectUnderlyingKeys()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var sut = Create(Prefix("^"), data);
            // Act
            var result = sut.Keys;
            // Assert
            Expect(result)
                .To.Equal(data.Keys);
        }
    }

    [TestFixture]
    public class Values
    {
        [Test]
        public void ShouldReturnMutatedValues()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            var sut = Create(Prefix("$"), data);
            // Act
            var result = sut.Values;
            // Assert
            Expect(result)
                .To.Equal(
                    data.Values.Select(v => $"${v}")
                );
        }
    }

    [TestFixture]
    public class CollectionMutation
    {
        [Test]
        public void ShouldBeAbleToAddToUnderlyingData1()
        {
            // Arrange
            var k1 = GetRandomString();
            var v1 = GetRandomString();
            var data = new Dictionary<string, string>();
            var sut = Create(Prefix("_"), data);
            // Act
            sut.Add(k1, v1);
            var result = sut[k1];
            // Assert
            Expect(result)
                .To.Equal($"_{v1}");
            Expect(data)
                .To.Contain.Key(k1)
                .With.Value(v1);
        }

        [Test]
        public void ShouldBeAbleToAddToUnderlyingData2()
        {
            // Arrange
            var k1 = GetRandomString();
            var v1 = GetRandomString();
            var data = new Dictionary<string, string>();
            var sut = Create(Prefix("_"), data);
            // Act
            sut.Add(new KeyValuePair<string, string>(k1, v1));
            var result = sut[k1];
            // Assert
            Expect(result)
                .To.Equal($"_{v1}");
            Expect(data)
                .To.Contain.Key(k1)
                .With.Value(v1);
        }

        [Test]
        public void ShouldBeAbleToRemove1()
        {
            // Arrange
            var k1 = GetRandomString();
            var v1 = GetRandomString();
            var data = new Dictionary<string, string>()
            {
                [k1] = v1
            };
            var sut = Create(NoOp, data);
            // Act
            sut.Remove(k1);
            // Assert
            Expect(data)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldBeAbleToRemove2()
        {
            // Arrange
            var k1 = GetRandomString();
            var v1 = GetRandomString();
            var data = new Dictionary<string, string>()
            {
                [k1] = v1
            };
            var sut = Create(NoOp, data);
            // Act
            sut.Remove(new KeyValuePair<string, string>(k1, v1));
            // Assert
            Expect(data)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldBeAbleToClear()
        {
            // Arrange
            var data = GetRandom<Dictionary<string, string>>();
            Expect(data)
                .Not.To.Be.Empty();
            var sut = Create(NoOp, data);
            // Act
            sut.Clear();
            // Assert
            Expect(data)
                .To.Be.Empty();
        }
    }

    [Test]
    public void ShouldBeConstructableWithoutExternalDataStore()
    {
        // Arrange
        var sut = new TransformingDictionary<string, int?>(
            kvp => kvp.Value ?? 0
        ) as IDictionary<string, int?>;
        // Act
        sut["Zero"] = null;
        // Assert
        Expect(sut)
            .To.Contain.Key("Zero")
            .With.Value(0);
    }

    private static Func<KeyValuePair<string, string>, string> Prefix(
        string prefix
    )
    {
        return (kvp => $"{prefix}{kvp.Value}");
    }

    private static string NoOp(KeyValuePair<string, string> kvp)
    {
        return kvp.Value;
    }


    private static IDictionary<TKey, TValue> Create<TKey, TValue>(
        Func<KeyValuePair<TKey, TValue>, TValue> mutator,
        IDictionary<TKey, TValue> underlyingData
    )
    {
        return new TransformingDictionary<TKey, TValue>(
            mutator,
            underlyingData
        );
    }
}