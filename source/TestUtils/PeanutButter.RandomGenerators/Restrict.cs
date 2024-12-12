#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;

namespace Imported.PeanutButter.RandomGenerators;
#else
using System;
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Restricts the possible values for a property
/// to the provided list
/// </summary>
[AttributeUsage(
    validOn: AttributeTargets.Class,
    AllowMultiple = true
)]
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class Restrict : RandomizerAttribute
{
    /// <summary>
    /// The restricted values for this property
    /// </summary>
    public object[] Values { get; }

    /// <inheritdoc />
    public Restrict(
        string propertyName,
        object value,
        params object[] moreValues
    ) : base(propertyName)
    {
        Values = new[]
        {
            value
        }.And(moreValues);
    }

    /// <inheritdoc />
    public override void SetRandomValue(
        PropertyOrField propInfo,
        ref object target
    )
    {
        var toSet = RandomValueGen.GetRandomFrom(Values);
        if (toSet is null)
        {
            propInfo.SetValue(
                target,
                propInfo.Type.IsNullableType()
                    ? null
                    : propInfo.Type.DefaultValue()
            );

            return;
        }

        var valueType = toSet.GetType();
        if (valueType == propInfo.Type)
        {
            propInfo.SetValue(target, toSet);
            return;
        }

        var converter = ConverterLocator.TryFindConverter(
            valueType,
            propInfo.Type
        );
        if (converter is null)
        {
            throw new NotImplementedException(
                $"""
                 There is no known converter to convert between {valueType} and {propInfo.Type}.
                 You may implement IConverter<{valueType}, {propInfo.Type}> to resolve this:
                   ConverterLocator should pick it up.
                 """
            );
        }

        var converted = converter.Convert(toSet);
        propInfo.SetValue(target, converted);
    }
}