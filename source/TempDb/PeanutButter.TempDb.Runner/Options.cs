using CommandLine;

namespace PeanutButter.TempDb.Runner
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Options
    {
        [Option(
            'e',
            "engine",
            Default = "MySql",
            HelpText = "The flavor of engine for your TempDb. Run with 'help' to see supported flavors."
        )]
        public string Engine
        {
            get => _engine;
            set
            {
                ValidateType(value);
                _engine = value;
            }
        }

        [Option(
            'v',
            "verbose",
            Default = false,
            HelpText = "Run in verbose mode, outputting a lot more diagnostic information (applies more to mysql than other engines)"
        )]
        public bool Verbose { get; set; }

        [Option(
            'i',
            "idle-timeout-seconds",
            HelpText =
                "Set an idle timeout for engines which support it (currently: mysql). If no connections are detected within that sliding window then the server process is shut down.")]
        public int? IdleTimeoutSeconds { get; set; }

        private string _engine;

        private void ValidateType(string type)
        {
            if (type?.ToLowerInvariant() == "help")
            {
                throw new ShowSupportedEngines(TempDbFactory.AvailableEngines);
            }
        }
    }
}