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
            // hack: since we're kinda taking over args here
            // we want --help to be useful, so parse to _all_
            // options and discard
            var opts = args.ParseTo<IServiceOptions>(
                out _,
                new ParserOptions()
                {
                    IgnoreUnknownSwitches = true
                });
            if (opts.Install)
            {
                SaveIniValue(
                    TotallyNotInterestingService.SECTION_DELAY,
                    nameof(opts.StartDelay),
                    opts.StartDelay.ToString()
                );
            }
            TotallyNotInterestingService.Options = opts;

            return Shell.RunMain<TotallyNotInterestingService>(
                args
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