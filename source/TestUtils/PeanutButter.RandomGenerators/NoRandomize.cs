using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Specifies one or more properties, by name, to ignore
    ///    when attempting .WithRandomProps on an entity
    /// </summary>
    public class NoRandomize : RandomizerAttribute
    {
        /// <inheritdoc />
        public NoRandomize(
            string propertyName,
            params string[] otherProperties
        ) : base(propertyName,
            otherProperties)
        {
        }

        /// <inheritdoc />
        public override void SetRandomValue(PropertyOrField propInfo, ref object target)
        {
            /* intentionally left blank */
        }
    }
}