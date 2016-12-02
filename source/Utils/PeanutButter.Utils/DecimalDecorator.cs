using System;
using System.Globalization;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a relatively "safe" interchange between decimal and string
    ///  notations for numbers, inspecting the input string to try to make the best
    ///  decision based on the contents of the string instead of just relying on
    ///  the current thread culture (which causes issues when communicating between
    ///  machines with different ideas of what constitutes "correct" decimal notation
    ///  a prime example being transport of decimals from Javascript into a ZA .net
    ///  culture -- but there are others)
    /// </summary>
    public class DecimalDecorator
    {
        private static readonly object _lock = new object();
        private static NumberFormatInfo _numberFormatInfoField;
        private static NumberFormatInfo NumberFormatInfo
        {
            get
            {
                lock(_lock)
                {
                    return _numberFormatInfoField ?? (_numberFormatInfoField = CreateNumberFormatInfo());
                }
            }
        }

        private static NumberFormatInfo CreateNumberFormatInfo()
        {
            return new NumberFormatInfo()
            {
                CurrencyDecimalSeparator = ".",
                NumberDecimalSeparator = ".",
                PercentDecimalSeparator = ".",
                CurrencyGroupSeparator = string.Empty,
                NumberGroupSeparator = string.Empty,
                PercentGroupSeparator = string.Empty
            };
        }

        private readonly string _stringValue;
        private readonly decimal _decimalValue;

        /// <summary>
        /// Constructs a new DecimalDecorator with the provided decimal value an an
        /// optional string format
        /// </summary>
        /// <param name="value">Decimal value to decorate</param>
        /// <param name="format">Optional string format to use</param>
        public DecimalDecorator(decimal value, string format = null)
        {
            _decimalValue = value;
            _stringValue = format != null 
                                ? value.ToString(format, NumberFormatInfo) 
                                : value.ToString(NumberFormatInfo);
        }

        /// <summary>
        /// Constructs a new DecimalDecorator with the provided string value to
        /// parse as a decimal
        /// </summary>
        /// <param name="value">String value to parse as Decimal</param>
        public DecimalDecorator(string value)
        {
            if (value == null)
                value = "0";
            if (value.Trim() == string.Empty)
                value = "0";
            value = value.Replace(" ", string.Empty);
            value = value.Replace(",", value.IndexOf(".", StringComparison.Ordinal) > -1 ? string.Empty : ".");
            _decimalValue = decimal.Parse(value, NumberFormatInfo);
            _stringValue = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _stringValue;
        }

        /// <summary>
        /// Returns the decimal version of the underlying data
        /// </summary>
        /// <returns>The decimal version of the underlying data</returns>
        public decimal ToDecimal()
        {
            return _decimalValue;
        }
    }
}
