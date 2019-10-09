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
}