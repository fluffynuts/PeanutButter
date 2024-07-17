using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace PeanutButter.RandomGenerators.Core.Tests;

[TestFixture]
public partial class TestObjectExtensions
{
    [TestFixture]
    public class Randomized
    {
        [TestFixture]
        public class WhenLikelyToSucceedWithGetRandom
        {
            [Test]
            public void ShouldRandomizeTheRandomizableProperties()
            {
                // Arrange
                var collectedIds = new List<int>();
                var collectedNames = new List<string>();
                var collectedFlags = new List<bool>();
                var collectedJoined = new List<DateTime>();
                var collectedSystemFlags = new List<bool>();
                // Act
                for (var i = 0; i < 1000; i++)
                {
                    var originalData = new PersonWithImplementation();
                    var data = originalData
                        .Randomized();
                    Expect(data)
                        .To.Be(originalData, "should return the original object for fluency");
                    collectedIds.Add(data.Id);
                    collectedNames.Add(data.Name);
                    collectedFlags.Add(data.Flag);
                    collectedJoined.Add(data.Joined);
                    collectedSystemFlags.Add(data.SystemFlag);
                }

                // Assert
                Expect(collectedIds)
                    .To.Vary();
                Expect(collectedNames)
                    .To.Vary();
                Expect(collectedFlags)
                    .To.Vary();
                Expect(collectedJoined)
                    .To.Vary();
                Expect(collectedSystemFlags)
                    .To.Contain.All
                    .Matched.By(o => o == false);
            }
        }

        [TestFixture]
        public class WhenUnlikelyToSucceedWithGetRandom
        {
            [Test]
            public void ShouldRandomizeTheRandomizableProperties1()
            {
                // Arrange
                var collectedIds = new List<int>();
                var collectedNames = new List<string>();
                var collectedFlags = new List<bool>();
                var collectedJoined = new List<DateTime>();
                var collectedSystemFlags = new List<bool>();
                // Act
                for (var i = 0; i < 1000; i++)
                {
                    var originalData = Substitute.For<IPersonWithNoImplementation>();
                    var data = originalData
                        .Randomized();
                    Expect(data)
                        .To.Be(originalData, "should return the original object for fluency");
                    collectedIds.Add(data.Id);
                    collectedNames.Add(data.Name);
                    collectedFlags.Add(data.Flag);
                    collectedJoined.Add(data.Joined);
                    collectedSystemFlags.Add(data.SystemFlag);
                }

                // Assert
                Expect(collectedIds)
                    .To.Vary();
                Expect(collectedNames)
                    .To.Vary();
                Expect(collectedFlags)
                    .To.Vary();
                Expect(collectedJoined)
                    .To.Vary();
                Expect(collectedSystemFlags)
                    .To.Contain.All
                    .Matched.By(o => o == false);
            }

            [Test]
            public void ShouldRandomizeTheRandomizableProperties2()
            {
                // Arrange
                var collectedIds = new List<int>();
                var collectedNames = new List<string>();
                var collectedFlags = new List<bool>();
                var collectedJoined = new List<DateTime>();
                var collectedSystemFlags = new List<bool>();
                // Act
                for (var i = 0; i < 1000; i++)
                {
                    var originalData = new PrivatePersonWithImplementation();
                    var data = originalData
                        .Randomized();
                    Expect(data)
                        .To.Be(originalData, "should return the original object for fluency");
                    collectedIds.Add(data.Id);
                    collectedNames.Add(data.Name);
                    collectedFlags.Add(data.Flag);
                    collectedJoined.Add(data.Joined);
                    collectedSystemFlags.Add(data.SystemFlag);
                }

                // Assert
                Expect(collectedIds)
                    .To.Vary();
                Expect(collectedNames)
                    .To.Vary();
                Expect(collectedFlags)
                    .To.Vary();
                Expect(collectedJoined)
                    .To.Vary();
                Expect(collectedSystemFlags)
                    .To.Contain.All
                    .Matched.By(o => o == false);
            }
        }

        public interface IPersonWithNoImplementation
        {
            int Id { get; set; }
            string Name { get; set; }
            bool Flag { get; set; }
            DateTime Joined { get; set; }
            bool SystemFlag { get; }
        }

        public interface IPersonWithImplementation
        {
            int Id { get; set; }
            string Name { get; set; }
            bool Flag { get; set; }
            DateTime Joined { get; set; }
            bool SystemFlag { get; }
        }

        public class PersonWithImplementation : IPersonWithImplementation
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Flag { get; set; }
            public DateTime Joined { get; set; }
            public bool SystemFlag { get; }
        }

        public interface IPrivatePersonWithImplementation
        {
            int Id { get; set; }
            string Name { get; set; }
            bool Flag { get; set; }
            DateTime Joined { get; set; }
            bool SystemFlag { get; }
        }

        public class PrivatePersonWithImplementation : IPrivatePersonWithImplementation
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Flag { get; set; }
            public DateTime Joined { get; set; }
            public bool SystemFlag { get; }
        }
    }
}