using System;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Base class to use when wishing to guide randomization on a builder via
    /// attributes.
    /// </summary>
    public abstract class RandomizerAttribute : Attribute
    {
        /// <summary>
        /// The name of the property to which this randomizer attribute pertains
        /// </summary>
        public string[] PropertyNames { get; }

        internal delegate void RefAction(PropertyOrField prop, ref object target);

        /// <summary>
        /// Constructs this attribute to act against one or more
        /// properties by name
        /// </summary>
        /// <param name="propertyName">initial property name</param>
        /// <param name="otherPropertyNames">any additional property names</param>
        protected RandomizerAttribute(
            string propertyName,
            params string[] otherPropertyNames
        )
        {
            PropertyNames = new[] { propertyName }
                .Concat(otherPropertyNames)
                .ToArray();
        }

        /// <summary>
        /// Actually invoked when attempting to set a random value on the
        /// named property
        /// </summary>
        /// <param name="propInfo"></param>
        /// <param name="target"></param>
        public abstract void SetRandomValue(
            PropertyOrField propInfo,
            ref object target);

        /// <summary>
        /// override in your implementation if you'd like to late-initialize
        /// relevant property names
        /// </summary>
        /// <param name="type"></param>
        public virtual void Init(Type type)
        {
        }
    }
}