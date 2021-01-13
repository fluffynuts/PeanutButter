namespace PeanutButter.Args
{
    /// <summary>
    /// Exit codes used when exiting early
    /// </summary>
    public static class ExitCodes
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