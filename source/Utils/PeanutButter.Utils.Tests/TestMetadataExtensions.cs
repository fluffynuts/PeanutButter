using System;
using NExpect.Implementations;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TryCastAlwaysSucceeds

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestMetadataExtensions
    {
        [Test]
        public void ShouldBeAbleToSetAndRetrieveAValue()
        {
            // Arrange
            var target = new { };
            var key = GetRandomString(2);
            var value = GetRandomInt();

            // Pre-Assert

            // Act
            target.SetMetadata(key, value);
            var result = target.GetMetadata<int>(key);

            // Assert
            Expect(result).To.Equal(value);
        }

        [Test]
        public void ShouldBeAbleToInformIfThereIsMetadata()
        {
            // Arrange
            var target = new { };
            var key = GetRandomString(2);
            var value = GetRandomString(4);
            target.SetMetadata(key, value);
            // Pre-Assert

            // Act
            var result = target.HasMetadata<string>(key);

            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void ShouldGcMetaData()
        {
            // Arrange
            var target = new { };
            var key = GetRandomString(2);
            var value = GetRandomBoolean();
            target.SetMetadata(key, value);

            // Pre-Assert
            Expect(target.HasMetadata<bool>(key)).To.Be.True();
            Expect(MetadataExtensions.TrackedObjectCount()).To.Equal(1);

            // Act
            target = null;
            GC.Collect();

            // Assert
            Expect(() =>
            {
                target.HasMetadata<bool>(key);
            }).To.Throw();
            Expect(MetadataExtensions.TrackedObjectCount()).To.Equal(0);
        }
    }
}