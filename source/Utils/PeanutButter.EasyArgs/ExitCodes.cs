#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
{
    /// <summary>
    /// Exit codes used when exiting early
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        static class ExitCodes
    {
        /// <summary>
        /// There was an error with the provided arguments
        /// - conflict
        /// - unknown argument
        /// </summary>
        public const int ARGUMENT_ERROR = 1;

        /// <summary>
        /// We showed the help and exited
        /// </summary>
        public const int SHOWED_HELP = 2;
    }
}