namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Most basic interface to implement to be considered a builder
    /// </summary>
    /// <typeparam name="TSubject">Type of the entity this builder should build</typeparam>
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IBuilder<TSubject>
    {
        /// <summary>
        /// Builds a new instance of TSubbject
        /// </summary>
        /// <returns>New instance of TSubject</returns>
        TSubject Build();
    }

    /// <summary>
    /// Most basic base builder class; deprecated in favour of GenericBuilder, but
    /// may be useful if you wish to roll most of your builder yourself.
    /// </summary>
    /// <typeparam name="TConcrete">Concrete implementation of this builder (ie, the class inheriting from this)</typeparam>
    /// <typeparam name="TSubject">Subject type which will be built by this builder</typeparam>
    public abstract class BuilderBase<TConcrete, TSubject> where TConcrete: IBuilder<TSubject>, new()
    {
        /// <summary>
        /// Fluency method to creates a new instance of the builder
        /// </summary>
        /// <returns>New instance of this builder</returns>
        public static TConcrete Create()
        {
            return new TConcrete();
        }

        /// <summary>
        /// Builds a default version of TSubject
        /// </summary>
        /// <returns>New instance of TSubject with default property values</returns>
        public static TSubject BuildDefault()
        {
            return Create().Build();
        }
    }
}
