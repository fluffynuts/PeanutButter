namespace PeanutButter.Args
{
    internal static class ArgParserOptionsExtensions
    {
        internal static void ExitIfRequired(
            this ParserOptions opts,
            int exitCode
        )
        {
            if (opts.ExitOnError)
            {
                opts.ExitAction?.Invoke(exitCode);
            }
        }
    }
}