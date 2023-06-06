// ReSharper disable UnusedType.Global
namespace PeanutButter.Utils
{
    /// <summary>
    /// Provide a mechanism to swap out noisy steps for
    /// fully-suppressed ones, esp from testing
    /// </summary>
    public class SuppressedTextStatusSteps : TextStatusSteps
    {
        /// <summary>
        /// Create the suppressed status steps - ie
        /// to easily conditionally suppress all step
        /// output, swap in this one
        /// </summary>
        public SuppressedTextStatusSteps()
            : base(
                () => "",
                "",
                "",
                "",
                _ =>
                {
                },
                () =>
                {
                },
                _ => ErrorHandlerResult.Rethrow
            )
        {
        }
    }
}