using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Requires that the named property is randomized to a non-zero value
    /// </summary>
    public class RequireNonZero : RandomizerAttribute
    {
        /// <inheritdoc />
        public RequireNonZero(string propertyName)
            : base(propertyName)
        {
        }

        /// <inheritdoc />
        public override void SetRandomValue(PropertyOrField propInfo,
            ref object target)
        {
            propInfo.SetValue(target, RandomValueGen.GetRandomInt(1));
        }
    }
}