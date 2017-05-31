namespace PeanutButter.DuckTyping.Shimming
{
    /// <summary>
    /// Interface to implement for shimming ducked-types. Used for the backing
    /// field within the ducked type. Could potentially be used as a starting point
    /// for new ways to duck.
    /// </summary>
    public interface IShimSham
    {
        /// <summary>
        /// Gets a property value from the underlying object
        /// </summary>
        /// <param name="propertyName">Name of the property to get the value for</param>
        /// <returns>Value of the underlying property</returns>
        object GetPropertyValue(string propertyName);

        /// <summary>
        /// Sets a property value on the underlying object
        /// </summary>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="newValue">Value to set</param>
        void SetPropertyValue(string propertyName, object newValue);

        /// <summary>
        /// Calls through to an underlying void-returning method
        /// </summary>
        /// <param name="methodName">Name of the method to call through to</param>
        /// <param name="parameters">Parameters to pass through to the method</param>
        void CallThroughVoid(string methodName, params object[] parameters);

        /// <summary>
        /// Calls through to an underlying method and returns the result
        /// </summary>
        /// <param name="methodName">Name of the method to call through to</param>
        /// <param name="parameters">Parameters to pass through to the method</param>
        /// <returns>Value returned from the underlying method</returns>
        object CallThrough(string methodName, object[] parameters);
    }
}