using DotNetService;
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
            Console.Error.WriteLine($"cli args: {string.Join(" ", args)}");
            Console.Error.WriteLine($"args name: {opts.Name}");
            DotNetServiceShell.Options = opts;
            if (opts.Install)
            {
                SaveIniValue(
                    DotNetServiceShell.SECTION_DELAY,
                    nameof(opts.StartDelay),
                    opts.StartDelay.ToString()
                );
                SaveIniValue(
                    DotNetServiceShell.SECTION_DELAY,
                    nameof(opts.PauseDelay),
                    opts.PauseDelay.ToString()
                );
                SaveIniValue(
                    DotNetServiceShell.SECTION_DELAY,
                    nameof(opts.StopDelay),
                    opts.StopDelay.ToString()
                );
            }

            DotNetServiceShell.Options = opts;

            var result = Shell.RunMain<DotNetServiceShell>(
                args
            );
            return result;
        }

        private static void SaveIniValue(
            string section,
            string setting,
            string value
        )
        {
            var ini = new INIFile(DotNetServiceShell.IniFilePath);
            ini.SetValue(section, setting, value);
            ini.Persist();
        }
    }
}