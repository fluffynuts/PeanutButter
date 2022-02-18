using System;

namespace PeanutButter.EasyArgs.Attributes
{
    /// <summary>
    /// Attribute specifying a required numeric value
    /// </summary>
    public abstract class NumericAttribute : Attribute
    {
        /// <summary>
        /// The required value
        /// </summary>
        public decimal Value { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        public NumericAttribute(decimal min)
        {
            Value = min;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        public NumericAttribute(long min)
        {
            Value = min;
        }
    }
}