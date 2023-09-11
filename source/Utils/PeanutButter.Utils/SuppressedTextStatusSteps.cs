namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Provide a mechanism to swap out noisy steps for
/// fully-suppressed ones, esp from testing
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class SuppressedTextStatusSteps : TextStatusSteps
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