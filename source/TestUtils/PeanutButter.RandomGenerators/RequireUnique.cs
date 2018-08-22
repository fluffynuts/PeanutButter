using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Abstract class to require uniqueness on a property or field by name
    /// </summary>
    public abstract class RequireUnique : RandomizerAttribute
    {
        /// <summary>
        /// The type of the property which is required to be unique, should
        /// be set by inheriting class
        /// </summary>
        protected Type PropertyType { get; set; }
        private static Dictionary<Tuple<Type, string>, UniqueRandomValueGenerator> _generators
            = new Dictionary<Tuple<Type, string>, UniqueRandomValueGenerator>();
        private UniqueRandomValueGenerator _generator;

        /// <inheritdoc />
        public RequireUnique(string propertyName)
            : base(propertyName)
        {
        }

        /// <inheritdoc />
        public override void Init(Type entityType)
        {
            if (PropertyType == null)
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
            if (!_generators.ContainsKey(generatorKey))
                _generators[generatorKey] = UniqueRandomValueGenerator.For(PropertyType);
            _generator = _generators[generatorKey];
        }

        /// <inheritdoc />
        public override void SetRandomValue(PropertyOrField propInfo,
            ref object target)
        {
            propInfo.SetValue(target, _generator.NextObjectValue());
        }
    }
}