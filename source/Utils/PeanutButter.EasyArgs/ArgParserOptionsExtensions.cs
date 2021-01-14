namespace PeanutButter.EasyArgs
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