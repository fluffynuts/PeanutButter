using System;
using System.Globalization;
// ReSharper disable UnusedMember.Global

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public static class DateTimeExtensions
{
    public const string DATE_FORMAT = "yyyy-MM-dd";
    public const string DATE_TIME_FORMAT = "yyyy-MM-dd HH.mm.ss";

    public static string AsFormattedDate(this DateTime? dateTime)
    {
        return !dateTime.HasValue ? "" : dateTime.Value.AsFormattedDate();
    }

    public static string AsFormattedDate(this DateTime dateTime)
    {
        return dateTime.ToString(DATE_FORMAT, CultureInfo.InvariantCulture);
    }


    public static string AsFormattedDateTime(this DateTime? date)
    {
        return date?.ToString(DATE_TIME_FORMAT, CultureInfo.InvariantCulture) ?? "";
    }

    public static string AsFormattedDateTime(this DateTime date)
    {
        return date.ToString(DATE_TIME_FORMAT, CultureInfo.InvariantCulture);
    }

    public static DateTime AsFirstDayOfTheMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    public static DateTime AsLastDayOfTheMonth(this DateTime date)
    {
        var year = date.Year;
        var month = date.Month;
        return new DateTime(year, month, DateTime.DaysInMonth(year, month));
    }

    public static DateTime AsEndOfTheDay(this DateTime? dateTime)
    {
        if (dateTime == null) return new DateTime(1, 1, 1, 0, 0, 0);
        var date = dateTime.Value;
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
    }

    public static DateTime AsEndOfTheDay(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
    }
}