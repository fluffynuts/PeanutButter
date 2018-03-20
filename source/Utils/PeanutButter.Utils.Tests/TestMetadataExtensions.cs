using System;
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
            ArrangeAndPreAssert();

            // Act
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

            // Assert
            Expect(MetadataExtensions.TrackedObjectCount()).To.Equal(0);
        }

        private static void ArrangeAndPreAssert()
        {
            // this code needs to be in a different scope to force
            //  the loss of reference to target
            // Arrange
            GC.Collect();
            var target = new {foo = "bar"};
            var key = GetRandomString(2);
            var value = GetRandomBoolean();
            target.SetMetadata(key, value);

            // Pre-Assert
            Expect(target.HasMetadata<bool>(key)).To.Be.True();
            Expect(MetadataExtensions.TrackedObjectCount()).To.Equal(1);
        }
    }
}