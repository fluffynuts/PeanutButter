namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestTimeSpanExtensions
{
    [TestFixture]
    public class Truncation
    {
        [Test]
        public void TruncateMilliseconds()
        {
            // Arrange
            var input = GetRandomTimeSpan();
            // Act
            var result = input.TruncateMilliseconds();
            // Assert
            Expect(result.Days)
                .To.Equal(input.Days);
            Expect(result.Hours)
                .To.Equal(input.Hours);
            Expect(result.Minutes)
                .To.Equal(input.Minutes);
            Expect(result.Seconds)
                .To.Equal(input.Seconds);
            Expect(result.Milliseconds)
                .To.Equal(0);
        }

        [Test]
        public void TruncateSeconds()
        {
            // Arrange
            var input = GetRandomTimeSpan();
            // Act
            var result = input.TruncateSeconds();
            // Assert
            Expect(result.Days)
                .To.Equal(input.Days);
            Expect(result.Hours)
                .To.Equal(input.Hours);
            Expect(result.Minutes)
                .To.Equal(input.Minutes);
            Expect(result.Seconds)
                .To.Equal(0);
            Expect(result.Milliseconds)
                .To.Equal(0);
        }

        [Test]
        public void TruncateMinutes()
        {
            // Arrange
            var input = GetRandomTimeSpan();
            // Act
            var result = input.TruncateMinutes();
            // Assert
            Expect(result.Days)
                .To.Equal(input.Days);
            Expect(result.Hours)
                .To.Equal(input.Hours);
            Expect(result.Minutes)
                .To.Equal(0);
            Expect(result.Seconds)
                .To.Equal(0);
            Expect(result.Milliseconds)
                .To.Equal(0);
        }

        [Test]
        public void TruncateHours()
        {
            // Arrange
            var input = GetRandomTimeSpan();
            // Act
            var result = input.TruncateHours();
            // Assert
            Expect(result.Days)
                .To.Equal(input.Days);
            Expect(result.Hours)
                .To.Equal(0);
            Expect(result.Minutes)
                .To.Equal(0);
            Expect(result.Seconds)
                .To.Equal(0);
            Expect(result.Milliseconds)
                .To.Equal(0);
        }
    }

    [TestFixture]
    public class DateTimeLikeAddingMethods
    {
        [TestFixture]
        public class AddDays
        {
            [Test]
            public void ShouldAddTheSpecifiedNumberOfDays()
            {
                // Arrange
                var timeSpan = GetRandomTimeSpan();
                var toAdd = GetRandomDouble(0, 5);
                var expected = timeSpan.Add(
                    TimeSpan.FromDays(toAdd)
                );
                // Act
                var result = timeSpan.AddDays(toAdd);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class AddHours
        {
            [Test]
            public void ShouldAddTheSpecifiedNumberOfHours()
            {
                // Arrange
                var timeSpan = GetRandomTimeSpan();
                var toAdd = GetRandomDouble(0, 5);
                var expected = timeSpan.Add(
                    TimeSpan.FromHours(toAdd)
                );
                // Act
                var result = timeSpan.AddHours(toAdd);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class AddMinutes
        {
            [Test]
            public void ShouldAddTheSpecifiedNumberOfMinutes()
            {
                // Arrange
                var timeSpan = GetRandomTimeSpan();
                var toAdd = GetRandomDouble(0, 5);
                var expected = timeSpan.Add(
                    TimeSpan.FromMinutes(toAdd)
                );
                // Act
                var result = timeSpan.AddMinutes(toAdd);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class AddSeconds
        {
            [Test]
            public void ShouldAddTheSpecifiedNumberOfSeconds()
            {
                // Arrange
                var timeSpan = GetRandomTimeSpan();
                var toAdd = GetRandomDouble(0, 5);
                var expected = timeSpan.Add(
                    TimeSpan.FromSeconds(toAdd)
                );
                // Act
                var result = timeSpan.AddSeconds(toAdd);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class AddMilliseconds
        {
            [Test]
            public void ShouldAddTheSpecifiedNumberOfMilliseconds()
            {
                // Arrange
                var timeSpan = GetRandomTimeSpan();
                var toAdd = GetRandomDouble(0, 5);
                var expected = timeSpan.Add(
                    TimeSpan.FromMilliseconds(toAdd)
                );
                // Act
                var result = timeSpan.AddMilliseconds(toAdd);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }
    }
}