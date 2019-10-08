using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Requires that the field named "Id" be non-zero
    /// </summary>
    public class RequireNonZeroId : RequireNonZero
    {
        /// <inheritdoc />
        public RequireNonZeroId()
            : base("Id")
        {
        }
    }

    public class RandomizerIgnore : RandomizerAttribute
    {
        public RandomizerIgnore(string propertyName) 
            : base(propertyName)
        {
        }

        public override void SetRandomValue(PropertyOrField propInfo, ref object target)
        {
            /* intentionally left blank */
        }
    }
}