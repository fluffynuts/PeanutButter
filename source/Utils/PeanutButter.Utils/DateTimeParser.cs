using System;
using System.Text.RegularExpressions;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif

/// <summary>
/// Versatile DateTime parser
/// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
internal
#else
public
#endif
    interface IDateTimeParser
{
    /// <summary>
    /// Attempts to parse the given date string into a DateTime
    /// object
    /// - direct parsing
    /// - relative parsing, eg `1 day ago`, `3 weeks ago`
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    DateTime Parse(
        string date
    );
}

/// <inheritdoc />
public class DateTimeParser
    : IDateTimeParser
{
    private static readonly Regex RelativeDateTimeMatcher = new(
        "^\\s*((?<count>([+-]?[0-9]+[.]?[0-9]*))\\s*(?<interval>(seconds|minutes|hours|days|weeks|months|years|second|minute|hour|day|week|month|year|min|sec|s|h|d|w|m|y))\\s*(?<direction>.*)?)\\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Attempts to parse the given date string into a DateTime
    /// object
    /// - direct parsing
    /// - relative parsing, eg `1 day ago`, `3 weeks ago`
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public DateTime Parse(
        string date
    )
    {
        var match = RelativeDateTimeMatcher.Match(date);
        if (match.Success)
        {
            return DetermineRelativeDateTimeFrom(match);
        }

        var result = DateTime.Parse(date);

        return result.Kind == DateTimeKind.Unspecified
            ? result.WithKind(DateTimeKind.Local)
            : result;
    }

    private DateTime DetermineRelativeDateTimeFrom(
        Match match
    )
    {
        var decorator = new DecimalDecorator(match.Groups["count"].Value);
        var count = decorator.IsValidDecimal
            ? decorator.ToDecimal()
            : throw new ArgumentException("count must be numeric");

        if (match.Groups["direction"].Value.Equals("ago", StringComparison.OrdinalIgnoreCase))
        {
            count *= -1;
        }

        var interval = DetermineIntervalFor(match.Groups["interval"].Value, count);
        return DateTime.Now.Add(interval);
    }

    private TimeSpan DetermineIntervalFor(
        string value,
        decimal count
    )
    {
        switch (value)
        {
            case "s":
            case "sec":
            case "second":
            case "seconds":
                return TimeSpan.FromSeconds((double)count);
            case "m":
            case "min":
            case "minute":
            case "minutes":
                return TimeSpan.FromMinutes((double)count);
            case "h":
            case "hour":
            case "hours":
                return TimeSpan.FromHours((double)count);
            case "d":
            case "day":
            case "days":
                return TimeSpan.FromDays((double)count);
            case "w":
            case "weeks":
            case "week":
                return DateTimeFromWeeksDelta(count);
            case "M":
            case "months":
            case "month":
                return DateTimeFromMonthsDelta(count);
            case "y":
            case "year":
            case "years":
                return DateTimeFromYearsDelta(count);
            default:
                throw new ArgumentException(
                    $"Unhandled interval type: '{value}'",
                    nameof(value)
                );
        }
    }

    private TimeSpan DateTimeFromYearsDelta(
        decimal count
    )
    {
        var now = DateTime.Now;
        var lower = now.AddYears((int)count);
        var upper = now.AddYears((int)(count + 1));
        var delta = upper - lower;
        var fraction = count - (int)count;
        var deltaTimeSpan = TimeSpan.FromSeconds(
            delta.TotalSeconds * (double)fraction
        );
        return lower.Add(deltaTimeSpan) - now;
    }

    private TimeSpan DateTimeFromMonthsDelta(
        decimal count
    )
    {
        var now = DateTime.Now;
        var lower = now.AddMonths((int)count);
        var upper = now.AddMonths((int)(count + 1));
        var delta = upper - lower;
        var fraction = count - (int)count;
        var deltaTimeSpan = TimeSpan.FromSeconds(
            delta.TotalSeconds * (double)fraction
        );
        return lower.Add(deltaTimeSpan) - now;
    }

    private TimeSpan DateTimeFromWeeksDelta(
        decimal count
    )
    {
        var dayCount = count * 7;
        var now = DateTime.Now;
        var lower = now.AddDays((int)dayCount);
        var upper = now.AddDays((int)(dayCount + 1));
        var delta = upper - lower;
        var fraction = dayCount - (int)dayCount;
        var deltaTimeSpan = TimeSpan.FromSeconds(
            delta.TotalSeconds * (double)fraction
        );
        return lower.Add(deltaTimeSpan) - now;
    }
}