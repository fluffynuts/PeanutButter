using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Parsing;
using CommandLine.Text;

namespace ServiceShell
{
    public class CommandlineOptions
    {
        public enum ExitCodes
        {
            Success,
            CommandlineArgumentError,
            Failure,
            ShowedHelp,
            InstallFailed,
            UninstallFailed
        }

        [Option('i', "install", HelpText = "Install this service")]
        public bool Install { get; set; }

        [Option('u', "uninstall", HelpText = "Uninstall this service")]
        public bool Uninstall { get; set; }

        [Option('r', "runonce", HelpText = "Run one round of this service's code and exit")]
        public bool RunOnce { get; set; }

        [Option('w', "wait", HelpText = "Wait this many seconds before actually doing the round of work")]
        public int Wait { get; set; }

        [Option('h', "help", HelpText = "Show this help")]
        public bool ShowHelp { get; set; }

        [Option('v', "version", HelpText = "Show the version of this service")]
        public bool ShowVersion { get; set; }

        [Option('s', "start", HelpText = "Start service")]
        public bool StartService { get; set; }

        [Option('x', "stop", HelpText = "Stop service")]
        public bool StopService { get; set; }

        public ExitCodes ExitCode { get; protected set; }

        private Action<string> HelpWriter { get; set; } = Console.WriteLine;
        internal IParser OptionsParser { get; set; } = new ParserFacade(Parser.Default);

        public CommandlineOptions(string[] args, string helpHeading, string copyRightInformation = null)
        {
            Init(args, helpHeading, copyRightInformation);
        }

        private void Init(string[] args, string helpHeading, string copyRightInformation)
        {
            ExitCode = ExitCodes.CommandlineArgumentError;
            if (OptionsParser.ParseArguments(args, this))
            {
                if (ShowHelp)
                {
                    ShowUsage(helpHeading, copyRightInformation);
                    return;
                }
                ExitCode = ExitCodes.Success;
            }
            else
            {
                ShowUsage(helpHeading, copyRightInformation);
            }
        }

        internal CommandlineOptions(string[] args, string helpHeading, string copyRightInformation, 
            Action<string> helpWriter, IParser parser)
        {
            HelpWriter = helpWriter;
            OptionsParser = parser;
            Init(args, helpHeading, copyRightInformation);
        }

        private void ShowUsage(string helpHeading, string copyRightInformation)
        {
            var ht = new HelpText(helpHeading) {AddDashesToOption = true};
            if (!string.IsNullOrWhiteSpace(copyRightInformation))
                ht.Copyright = copyRightInformation;
            ht.AddOptions(this);

            HelpWriter(ht.ToString());
            ExitCode = ExitCodes.ShowedHelp;
        }

    }

    internal interface IParser
    {
        bool ParseArguments(string[] arguments, object optionsObject);
    }

    internal class ParserFacade: IParser
    {
        public Parser Actual => _actual;
        private Parser _actual;

        internal ParserFacade(Parser actual)
        {
            _actual = actual;
        }

        public bool ParseArguments(string[] arguments, object optionsObject)
        {
            return _actual.ParseArguments(arguments, optionsObject);
        }
    }
}
