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

        public CommandlineOptions(string[] args, string helpHeading, string copyRightInformation = null)
        {
            this.ExitCode = ExitCodes.CommandlineArgumentError;
            if (Parser.Default.ParseArguments(args, this))
            {
                if (this.ShowHelp)
                {
                    ShowUsage(helpHeading, copyRightInformation);
                    return;
                }
                this.ExitCode = ExitCodes.Success;
            }
            else
            {
                ShowUsage(helpHeading, copyRightInformation);
            }
        }

        private void ShowUsage(string helpHeading, string copyRightInformation)
        {
            var ht = new HelpText(helpHeading);
            ht.AddDashesToOption = true;
            if (!String.IsNullOrWhiteSpace(copyRightInformation))
                ht.Copyright = copyRightInformation;
            ht.AddOptions(this);

            Console.WriteLine(ht.ToString());
            this.ExitCode = ExitCodes.ShowedHelp;
        }

    }
}
