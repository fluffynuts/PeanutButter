using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes;
#else
namespace PeanutButter.EasyArgs.Attributes;
#endif
/// <summary>
/// Attribute specifying a required numeric value
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
internal
#else
public
#endif
    abstract class NumericAttribute : Attribute
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