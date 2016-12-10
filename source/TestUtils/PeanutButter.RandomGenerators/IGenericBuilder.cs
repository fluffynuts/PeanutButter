namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// The most basic interface to implement for GenericBuilder instances
    /// </summary>
    public interface IGenericBuilder
    {
        /// <summary>
        /// Sets the maximum depth to traverse when randomizing properties
        /// </summary>
        /// <param name="level">How far down the rabbit-hole to go</param>
        /// <returns></returns>
        IGenericBuilder WithBuildLevel(int level);
        /// <summary>
        /// Sets up the builder to build with random property values unless the level specified
        /// is too deep, in which case it bails out
        /// </summary>
        /// <returns>The current builder instance</returns>
        IGenericBuilder GenericWithRandomProps();
        /// <summary>
        /// Builds an instance of the object this builder builds
        /// </summary>
        /// <returns>Instance of object for which this builder is designed</returns>
        object GenericBuild();
    }
}