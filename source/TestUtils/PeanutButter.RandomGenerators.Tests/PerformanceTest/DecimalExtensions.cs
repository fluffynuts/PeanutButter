using System;
using System.Globalization;
// ReSharper disable InconsistentNaming

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public static class DecimalExtensions
    {
        public const string SimpleCurrencyFormat = "R # ### ##0.00";
        private const int CurrencyDecimalPlaces = 2;
        private const decimal Vat = 0.14m;

        private static readonly NumberFormatInfo CurrencyFormat = new NumberFormatInfo
        {
            CurrencyGroupSeparator = " ",
            CurrencyDecimalDigits = 2,
            CurrencyDecimalSeparator = ".",
            CurrencySymbol = "R "
        };

        private static readonly NumberFormatInfo GeoLocationFormat = new NumberFormatInfo
        {
            NumberGroupSeparator = "",
            NumberDecimalDigits = 4,
            NumberDecimalSeparator = "."
        };

        private static readonly NumberFormatInfo PercentageFormat = new NumberFormatInfo
        {
            NumberGroupSeparator = "",
            NumberDecimalDigits = 2,
            NumberDecimalSeparator = "."
        };

        public static decimal RoundedAsCurrency(this decimal amount)
        {
            return Math.Round(amount, CurrencyDecimalPlaces, MidpointRounding.AwayFromZero);
        }

        public static decimal WithVatAdded(this decimal amount)
        {
            return (amount + amount.VatOnAmount()).RoundedAsCurrency();
        }

        public static decimal VatOnAmount(this decimal amount)
        {
            return (amount * Vat).RoundedAsCurrency();
        }

        public static string AsCurrencyString(this decimal thisValue)
        {
            return thisValue
                .RoundedAsCurrency()
                .ToString("C", CurrencyFormat)
                .Trim();
        }

        public static string AsGeoLocation(this decimal thisValue)
        {
            return thisValue
                .ToString("N", GeoLocationFormat)
                .Trim();
        }

        public static string AsPercentage(this decimal thisValue)
        {
            return thisValue
                .ToString("N", PercentageFormat)
                .Trim();
        }
    }
}