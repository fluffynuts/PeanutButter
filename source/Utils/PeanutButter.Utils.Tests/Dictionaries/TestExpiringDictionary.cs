using System;
using System.Collections.Generic;
using System.Threading;
using PeanutButter.Utils.Dictionaries;

namespace PeanutButter.Utils.Tests.Dictionaries;

[TestFixture]
public class TestExpiringDictionary
{
    [Test]
    public void ShouldBeAbleToAddAndQueryItems()
    {
        // Arrange
        var sut = Create<string, string>();
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        sut[key] = value;
        // Assert
        Expect(sut[key])
            .To.Equal(value);
    }

    [Test]
    public void ShouldBeAbleToEnumerateItems()
    {
        // Arrange
        var sut = Create<string, string>();
        var k1 = GetRandomString();
        var v1 = GetRandomString();
        var k2 = GetAnother(k1);
        var v2 = GetRandomString();
        var collected = new List<(string, string)>();
        var expected1 = (k1, v1);
        var expected2 = (k2, v2);
        // Act
        sut[k1] = v1;
        sut[k2] = v2;
        foreach (var kvp in sut)
        {
            collected.Add((kvp.Key, kvp.Value));
        }

        // Assert
        Expect(collected)
            .To.Contain.Only(2)
            .Items();
        Expect(collected)
            .To.Contain.Exactly(1)
            .Equal.To(expected1);
        Expect(collected)
            .To.Contain.Exactly(1)
            .Equal.To(expected2);
    }

    [Test]
    public void ShouldBeAbleToAddKeyValuePairs()
    {
        // Arrange
        var sut = Create<string, int>();
        var key = GetRandomString();
        var value = GetRandomInt();
        var kvp = new KeyValuePair<string, int>(key, value);
        // Act
        sut.Add(kvp);
        // Assert
        Expect(sut[key])
            .To.Equal(value);
        Expect(sut.Count)
            .To.Equal(1);
    }

    [Test]
    public void ShouldBeAbleToAddViaKeyAndValueParameters()
    {
        // Arrange
        var sut = Create<string, int>();
        var key = GetRandomString();
        var value = GetRandomInt();
        // Act
        sut.Add(key, value);
        // Assert
        Expect(sut[key])
            .To.Equal(value);
        Expect(sut.Count)
            .To.Equal(1);
    }

    [Test]
    public void ShouldBeAbleToClear()
    {
        // Arrange
        var sut = Create<string, int>();
        var key = GetRandomString();
        var value = GetRandomInt();

        // Act
        sut[key] = value;
        Expect(sut.Count)
            .To.Equal(1);
        sut.Clear();
        // Assert
        var dict = sut as IDictionary<string, int>;
        Expect(dict)
            .To.Be.Empty();
    }

    [Test]
    public void ShouldBeAbleToTestIfItemIsContained()
    {
        // Arrange
        var sut = Create<int, int>();
        var key = GetRandomInt();
        var value = GetRandomInt();
        var invalidKey = GetAnother(key);
        var invalidValue = GetAnother(value);
        var test1 = new KeyValuePair<int, int>(key, value);
        var test2 = new KeyValuePair<int, int>(invalidKey, invalidValue);
        // Act
        sut.Add(test1);
        var result1 = sut.Contains(test1);
        var result2 = sut.Contains(test2);
        // Assert
        Expect(result1)
            .To.Be.True();
        Expect(result2)
            .To.Be.False();
    }

    [Test]
    public void ShouldBeAbleToCopyToArray()
    {
        // Arrange
        var sut = Create<string, string>();
        var k1 = GetRandomString();
        var v1 = GetRandomString();
        var k2 = GetAnother(k1);
        var v2 = GetRandomString();
        var k3 = GetAnother([k1, k2]);
        var v3 = GetRandomString();
        // Act
        sut[k1] = v1;
        sut[k2] = v2;
        sut[k3] = v3;
        var target = new KeyValuePair<string, string>[5];
        sut.CopyTo(target, 1);
        // Assert
        Expect(target)
            .To.Contain.Exactly(1)
            .Equal.To(new KeyValuePair<string, string>(k2, v2));
        Expect(target)
            .To.Contain.Exactly(1)
            .Equal.To(new KeyValuePair<string, string>(k3, v3));
        Expect(target[0])
            .To.Equal(default);
        Expect(target[4])
            .To.Equal(default);
    }

    [Test]
    public void ShouldBeAbleToRemoveAnItem()
    {
        // Arrange
        var sut = Create<string, string>();
        var k1 = GetRandomString();
        var v1 = GetRandomString();
        var k2 = GetAnother(k1);
        var v2 = GetRandomString();

        // Act
        sut[k1] = v1;
        sut[k2] = v2;

        var result = sut.Remove(new KeyValuePair<string, string>(k1, v1));
        // Assert
        Expect(result)
            .To.Be.True();
        var dict = sut as IDictionary<string, string>;
        Expect(dict)
            .Not.To.Contain.Key(k1);
        Expect(dict)
            .To.Contain.Key(k2)
            .With.Value(v2);
    }

    [Test]
    public void ShouldBeAbleToRemoveByKey()
    {
        // Arrange
        var sut = Create<string, string>();
        var k1 = GetRandomString();
        var v1 = GetRandomString();
        var k2 = GetAnother(k1);
        var v2 = GetRandomString();
        // Act
        sut[k1] = k2;
        sut[k2] = v2;
        Expect(sut.Count)
            .To.Equal(2);
        sut.Remove(k1);
        // Assert
        var dict = sut as IDictionary<string, string>;
        Expect(dict)
            .Not.To.Contain.Key(k1);
        Expect(dict)
            .To.Contain.Key(k2)
            .With.Value(v2);
    }

    [Test]
    public void ShouldNotRemoveTheItemOnKeyMatchOnly()
    {
        // Arrange
        var sut = Create<string, string>();
        var k1 = GetRandomString();
        var v1 = GetRandomString();
        var k2 = GetAnother(k1);
        var v2 = GetRandomString();
        var invalidValue = GetAnother(v2);
        var toRemove = new KeyValuePair<string, string>(k2, invalidValue);
        // Act
        sut[k1] = v1;
        sut[k2] = v2;
        var result = sut.Remove(toRemove);

        // Assert
        Expect(result)
            .To.Be.False();
        var dict = sut as IDictionary<string, string>;
        Expect(dict)
            .To.Contain.Key(k1)
            .With.Value(v1);
        Expect(dict)
            .To.Contain.Key(k2)
            .With.Value(v2);
    }

    [Test]
    public void ShouldBeAbleToQueryIfContainsKey()
    {
        // Arrange
        var sut = Create<string, string>();
        var k1 = GetRandomString();
        var v1 = GetRandomString();
        var missing = GetAnother(k1);
        // Act
        var result0 = sut.ContainsKey(k1);
        sut[k1] = v1;
        var result1 = sut.ContainsKey(k1);
        var result2 = sut.ContainsKey(missing);
        // Assert
        Expect(result0)
            .To.Be.False();
        Expect(result1)
            .To.Be.True();
        Expect(result2)
            .To.Be.False();
    }

    [Test]
    public void ShouldNotBeReadOnly()
    {
        // Arrange
        // Act
        var sut = Create<string, string>();
        // Assert
        Expect(sut.IsReadOnly)
            .To.Be.False();
    }

    [Test]
    public void ShouldExpireOldItems()
    {
        // Arrange
        var sut = Create<string, string>(
            TimeSpan.FromSeconds(0.1)
        );
        var dict = sut as IDictionary<string, string>;
        var k1 = GetRandomString();
        var v1 = GetRandomString();
        // Act
        sut[k1] = v1;
        Expect(dict)
            .To.Contain.Key(k1)
            .With.Value(v1);
        Thread.Sleep(500);
        
        // Assert
        Expect(dict)
            .Not.To.Contain.Key(k1);
    }

    private static ExpiringDictionary<TKey, TValue> Create<TKey, TValue>(
        TimeSpan? ttl = null
    )
    {
        return new ExpiringDictionary<TKey, TValue>(
            ttl ?? TimeSpan.FromMinutes(1)
        );
    }
}