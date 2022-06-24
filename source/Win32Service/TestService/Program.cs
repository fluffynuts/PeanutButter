using PeanutButter.EasyArgs;
using PeanutButter.INI;
using PeanutButter.ServiceShell;

namespace TestService
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static int Main(string[] args)
        {
            var opts = args.ParseTo<ICliOptions>(
                out var uncollected,
                new ParserOptions()
                {
                    IgnoreUnknownSwitches = true
                });
            // hack: since we're kinda taking over args here
            // we want --help to be useful, so parse to _all_
            // options and discard
            var _ = args.ParseTo<IHackOptionsToShowAll>();


            SaveIniValue(
                TotallyNotInterestingService.SECTION_DELAY,
                nameof(opts.StartDelay),
                opts.StartDelay.ToString()
            );
            return Shell.RunMain<TotallyNotInterestingService>(
                uncollected
            );
        }

        private static void SaveIniValue(
            string section,
            string setting,
            string value
        )
        {
            var ini = new INIFile(TotallyNotInterestingService.INIFILE);
            ini.SetValue(section, setting, value);
            ini.Persist();
        }
    }
}