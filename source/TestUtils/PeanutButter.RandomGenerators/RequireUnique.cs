using System;
using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;

namespace Imported.PeanutButter.RandomGenerators;
#else
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators;
#endif
/// <summary>
/// Abstract class to require uniqueness on a property or field by name
/// </summary>
[AttributeUsage(
    AttributeTargets.Class,
    AllowMultiple = true
)]
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    abstract class RequireUnique : RandomizerAttribute
{
    /// <summary>
    /// The type of the property which is required to be unique, should
    /// be set by inheriting class
    /// </summary>
    protected Type PropertyType { get; set; }

    private static readonly Dictionary<Tuple<Type, string>, UniqueRandomValueGenerator> Generators = new();
    private UniqueRandomValueGenerator _generator;

    /// <inheritdoc />
    public RequireUnique(string propertyName)
        : base(propertyName)
    {
    }

    /// <inheritdoc />
    public override void Init(Type entityType)
    {
        if (PropertyType is null)
        {
            throw new InvalidOperationException(
                $@"Inheritors of {
                    nameof(RequireUnique)
                } must set {
                    nameof(PropertyType)
                } and override Init()"
            );
        }

        var generatorKey = new Tuple<Type, string>(entityType, PropertyNames.Single());
        if (!Generators.ContainsKey(generatorKey))
        {
            Generators[generatorKey] = UniqueRandomValueGenerator.For(PropertyType);
        }

        _generator = Generators[generatorKey];
    }

    /// <inheritdoc />
    public override void SetRandomValue(
        PropertyOrField propInfo,
        ref object target
    )
    {
        propInfo.SetValue(target, _generator.NextObjectValue());
    }
}

/// <summary>
/// Require unique values for named properties (don't forget the type!)
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class RequireUnique<T> : RequireUnique
{
    /// <inheritdoc />
    public RequireUnique(
        string propertyName
    ) : base(propertyName)
    {
        PropertyType = typeof(T);
    }
}