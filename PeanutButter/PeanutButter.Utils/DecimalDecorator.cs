using System;
using System.Globalization;

namespace PeanutButter.Utils
{
    // Provides a relatively "safe" interchange between decimal and string
    //  notations for numbers, inspecting the input string to try to make the best
    //  decision based on the contents of the string instead of just relying on
    //  the current thread culture (which causes issues when communicating between
    //  machines with different ideas of what constitutes "correct" decimal notation;
    //  a prime example being transport of decimals from Javascript into a ZA .net
    //  culture -- but there are others)
    public class DecimalDecorator
    {
        static object _lock = new object();
        static NumberFormatInfo _numberFormatInfoField;
        static NumberFormatInfo _numberFormatInfo
        {
            get
            {
                lock(_lock)
                {
                    if (_numberFormatInfoField == null)
                    {
                        _numberFormatInfoField = new NumberFormatInfo()
                        {
                            CurrencyDecimalSeparator = ".",
                            NumberDecimalSeparator = ".",
                            PercentDecimalSeparator = ".",
                            CurrencyGroupSeparator = "",
                            NumberGroupSeparator = "",
                            PercentGroupSeparator = ""
                        };
                    }
                    return _numberFormatInfoField;
                }
            }
        }

        private string _stringValue;
        private decimal _decimalValue;

        public DecimalDecorator(decimal value, string format = null)
        {
            this._decimalValue = value;
            if (format != null)
                this._stringValue = value.ToString(format, _numberFormatInfo);
            else
                this._stringValue = value.ToString(_numberFormatInfo);
        }
        public DecimalDecorator(string value)
        {
            if (value == null)
                value = "0";
            if (value.Trim() == String.Empty)
                value = "0";
            value = value.Replace(" ", String.Empty);
            if (value.IndexOf(".") > -1)
            {
                value = value.Replace(",", "");
            }
            else
            {
                value = value.Replace(",", ".");
            }
            this._decimalValue = Decimal.Parse(value, _numberFormatInfo);
            this._stringValue = value;
        }

        public override string ToString()
        {
            return this._stringValue;
        }

        public decimal ToDecimal()
        {
            return this._decimalValue;
        }
    }
}
