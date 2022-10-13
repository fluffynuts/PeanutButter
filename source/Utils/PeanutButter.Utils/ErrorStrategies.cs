namespace PeanutButter.Utils
{
    /// <summary>
    /// What to do when one or more errors are encountered
    /// in a collection of WorkResults
    /// </summary>
    public enum ErrorStrategies
    {
        /// <summary>
        /// Throw all errors in an AggregateException
        /// </summary>
        Throw,

        /// <summary>
        /// Suppress the errors - just return the results
        /// </summary>
        Suppress
    }
}