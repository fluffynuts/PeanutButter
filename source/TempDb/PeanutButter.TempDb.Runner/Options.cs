using PeanutButter.EasyArgs.Attributes;

namespace PeanutButter.TempDb.Runner
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Options
    {
        [Default("MySql")]
        [Description("The flavor of engine for your TempDb. Run with 'help' to see supported flavors.")]
        public string Engine
        {
            get => _engine;
            set
            {
                ValidateType(value);
                _engine = value;
            }
        }

        [Description("Run in verbose mode, outputting a lot more diagnostic information (applies more to mysql than other engines)")]
        public bool Verbose { get; set; }

        [Description("Set an idle timeout for engines which support it (currently: mysql). If no connections are detected within that sliding window then the server process is shut down.")]
        public int? IdleTimeoutSeconds { get; set; }

        [Description("Set an absolute timeout in seconds for tempdb usage; after this time elapses, the tempdb will be automatically disposed irrespective of any current connections.")]
        public int? AbsoluteTimeoutSeconds { get; set; }

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