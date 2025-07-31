using System;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestDateTimeParser
{
    [Test]
    public void ShouldParseFullDateTimeString()
    {
        // Arrange
        var expected = GetRandomDate().TruncateMilliseconds();
        var sut = Create();
        // Act
        var result = sut.Parse(expected.ToString());
        // Assert
        Expect(result)
            .To.Equal(expected);
    }

    [TestCase("2025/01/20")]
    [TestCase("2025-01-02")]
    public void ShouldParseDateString(
        string input
    )
    {
        // Arrange
        var expected = DateTime.Parse(input)
            .WithKind(DateTimeKind.Local);
        var sut = Create();

        // Act
        var result = sut.Parse(input);

        // Assert
        Expect(result)
            .To.Equal(expected);
    }

    [TestCase("30 seconds ago", -30)]
    [TestCase("30 sec ago", -30)]
    [TestCase("1 second ago", -1)]
    [TestCase("123 seconds from now", 123)]
    [TestCase("-23 seconds from now", -23)]
    [TestCase("-10s", -10)]
    [TestCase("45s", 45)]
    [TestCase("11s from now", 11)]
    [TestCase("1 second from now", 1)]
    [TestCase("27 seconds from now", 27)]
    public void ShouldParseRelativeDate_Seconds(
        string input,
        int expectedSeconds
    )
    {
        // Arrange
        var expected = DateTime.Now
            .TruncateMilliseconds()
            .AddSeconds(expectedSeconds);
        var sut = Create();
        // Act
        var result = sut.Parse(input);
        // Assert
        Expect(result)
            .To.Approximately.Equal(
                expected,
                () => $"tested at: {DateTime.Now}"
            );
    }

    [TestCase("30 minutes ago", -30)]
    [TestCase("30 min ago", -30)]
    [TestCase("1 minute ago", -1)]
    [TestCase("123 minutes from now", 123)]
    [TestCase("-23 minutes from now", -23)]
    [TestCase("-10m", -10)]
    [TestCase("45m", 45)]
    [TestCase("11m from now", 11)]
    [TestCase("1 minute from now", 1)]
    [TestCase("27 minutes from now", 27)]
    public void ShouldParseRelativeDate_Minutes(
        string input,
        int expectedSeconds
    )
    {
        // Arrange
        var expected = DateTime.Now
            .TruncateMilliseconds()
            .AddMinutes(expectedSeconds);
        var sut = Create();
        // Act
        var result = sut.Parse(input);
        // Assert
        Expect(result)
            .To.Approximately.Equal(
                expected,
                () => $"tested at: {DateTime.Now}"
            );
    }

    [TestCase("30 hours ago", -30)]
    [TestCase("1 hour ago", -1)]
    [TestCase("123 hours from now", 123)]
    [TestCase("-23 hours from now", -23)]
    [TestCase("-10h", -10)]
    [TestCase("45h", 45)]
    [TestCase("11h from now", 11)]
    [TestCase("1 hour from now", 1)]
    [TestCase("27 hours from now", 27)]
    public void ShouldParseRelativeDate_Hours(
        string input,
        int expectedSeconds
    )
    {
        // Arrange
        var expected = DateTime.Now
            .TruncateMilliseconds()
            .AddHours(expectedSeconds);
        var sut = Create();
        // Act
        var result = sut.Parse(input);
        // Assert
        Expect(result)
            .To.Approximately.Equal(
                expected,
                () => $"tested at: {DateTime.Now}"
            );
    }

    [TestCase("30 days ago", -30)]
    [TestCase("1 day ago", -1)]
    [TestCase("123 days from now", 123)]
    [TestCase("-23 days from now", -23)]
    [TestCase("-10d", -10)]
    [TestCase("45d", 45)]
    [TestCase("11d from now", 11)]
    [TestCase("1 day from now", 1)]
    [TestCase("27 days from now", 27)]
    public void ShouldParseRelativeDate_Days(
        string input,
        int expectedSeconds
    )
    {
        // Arrange
        var expected = DateTime.Now
            .TruncateMilliseconds()
            .AddDays(expectedSeconds);
        var sut = Create();
        // Act
        var result = sut.Parse(input);
        // Assert
        Expect(result)
            .To.Approximately.Equal(
                expected,
                () => $"tested at: {DateTime.Now}"
            );
    }

    [TestCase("30 months ago", -30)]
    [TestCase("1 month ago", -1)]
    [TestCase("123 months from now", 123)]
    [TestCase("-23 months from now", -23)]
    [TestCase("1 month from now", 1)]
    [TestCase("27 months from now", 27)]
    public void ShouldParseRelativeDate_Months(
        string input,
        int expectedSeconds
    )
    {
        // Arrange
        var expected = DateTime.Now
            .TruncateMilliseconds()
            .AddMonths(expectedSeconds);
        var sut = Create();
        // Act
        var result = sut.Parse(input);
        // Assert
        Expect(result)
            .To.Approximately.Equal(
                expected,
                () => $"tested at: {DateTime.Now}"
            );
    }

    [TestCase("30 years ago", -30)]
    [TestCase("1 year ago", -1)]
    [TestCase("123 years from now", 123)]
    [TestCase("-23 years from now", -23)]
    [TestCase("45y", 45)]
    [TestCase("11y from now", 11)]
    [TestCase("1 year from now", 1)]
    [TestCase("27 years from now", 27)]
    public void ShouldParseRelativeDate_Years(
        string input,
        int expectedSeconds
    )
    {
        // Arrange
        var expected = DateTime.Now
            .TruncateMilliseconds()
            .AddYears(expectedSeconds);
        var sut = Create();
        // Act
        var result = sut.Parse(input);
        // Assert
        Expect(result)
            .To.Approximately.Equal(
                expected,
                () => $"tested at: {DateTime.Now}"
            );
    }

    [TestCase("2 weeks ago", -14)]
    [TestCase("1w ago", -7)]
    [TestCase("-3w", -21)]
    [TestCase("32w", 224)]
    public void ShouldParseRelativeDate_Weeks(
        string input,
        int expectedDays
    )
    {
        // Arrange
        var expected = DateTime.Now.TruncateMilliseconds()
            .AddDays(expectedDays);
        var sut = Create();
        // Act
        var result = sut.Parse(input);
        // Assert
        Expect(result)
            .To.Approximately.Equal(
                expected,
                () => $"tested at: {DateTime.Now}"
            );
    }

    private static IDateTimeParser Create()
    {
        return new DateTimeParser();
    }
}