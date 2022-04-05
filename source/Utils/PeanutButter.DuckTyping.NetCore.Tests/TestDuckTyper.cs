using System;
using NUnit.Framework;
using NExpect;
using PeanutButter.DuckTyping.Extensions;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.DuckTyping.NetCore.Tests
{
    [TestFixture]
    public class TestDuckTyper
    {
        [Test]
        public void ShouldBeAbleToDuckInNetCore()
        {
            // Arrange
            var data = new HasAnId() { Id = 42 };
            // Act
            var ducked = data.DuckAs<IHasAnId>();
            // Assert
            Expect(ducked.Id)
                .To.Equal(42);
        }

        [Test]
        public void TryConvertStringToDecimal()
        {
            // Arrange
            var s = "1.23";
            // Act
            var converted = s.TryConvertTo<decimal>(out var result);
            // Assert
            Expect(converted)
                .To.Be.True();
            Expect(result)
                .To.Equal(1.23M);
        }

        [Test]
        public void TryConvertStringToDecimal2()
        {
            // Arrange
            var s = "1.23";
            // Act
            var converted = s.TryConvertTo(typeof(decimal), out var result);
            // Assert
            Expect(converted)
                .To.Be.True();
            Expect(result)
                .To.Equal(1.23M);
        }

        [Test]
        public void TryConvertStringToString()
        {
            // Arrange
            var s = "abc123";
            // Act
            var converted = s.TryConvertTo<string>(out var result);
            // Assert
            Expect(converted)
                .To.Be.True();
            Expect(result)
                .To.Equal(s);
        }

        [TestFixture]
        public class Issues
        {
            [Test]
            public void ShouldNotBreakOnWrapping()
            {
                // Arrange
                var data = GetRandom<ChangeRequest>();
                // Act
                var result = data.DuckAs<IWrapped>();
                // Assert
                Expect(result.Id)
                    .To.Equal(data.Id);
                Expect(result.ChangeRequestNumber)
                    .To.Equal(data.ChangeRequestNumber);
            }
            
            public interface IWrapped
            {
                int Id { get; set; }
                int ChangeRequestNumber { get; set; }
            }
            
            public class ChangeRequest
            {
                public int Id { get; set; }
                public int ChangeRequestNumber { get; set; }
                public Guid Uuid { get; set; }
                public string Title { get; set; }
                public string Detail { get; set; }
                public string Motivation { get; set; }
                public int CategoryId { get; set; }
                public int CountryId { get; set; }
                public int DistributionCenterId { get; set; }
                public int StatusId { get; set; }
                public int NumberOfStoresId { get; set; }
                public string SubmittedBy { get; set; }
                public int SubmittedByUserId { get; set; }
                public DateTime SubmittedDate { get; set; }
                public string AssignedTo { get; set; }
                public int AssignedToUserId { get; set; }
                public string ReleaseNumber { get; set; }
                public int UpdatedDate { get; set; }
                public int CompletedDate { get; set; }
                public string AttachmentGuidsCsv { get; set; }
            }

        }
    }

    public interface IHasAnId
    {
        public int Id { get; set; }
    }

    public class HasAnId
    {
        public int Id { get; set; }
    }
}