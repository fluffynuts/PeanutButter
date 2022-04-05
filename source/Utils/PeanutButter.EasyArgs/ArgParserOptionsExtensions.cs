#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
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