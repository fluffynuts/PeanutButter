using System;
using CommandLine;
using CommandLine.Text;
using PeanutButter.EasyArgs;
using PeanutButter.EasyArgs.Attributes;
using PeanutButter.Utils;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.ServiceShell
{
    public interface IServiceCommandlineOptions
    {
        [Description("install this service")]
        bool Install { get; set; }

        [Description("uninstall this service")]
        bool Uninstall { get; set; }

        [Description("run one round of this service's code and exit")]
        bool RunOnce { get; set; }

        [Description("run continually, with logging set to ALL")]
        bool Debug { get; set; }

        [Description("wait this many seconds before actually doing the round of work, for run-once")]
        int Wait { get; set; }

        [Description("show the version of this service")]
        [ShortName('v')]
        [LongName("--version")]
        bool ShowVersion { get; set; }

        [Description("start service")]
        [LongName("start")]
        bool StartService { get; set; }

        [Description("stop service")]
        [ShortName('x')]
        [LongName("stop")]
        bool StopService { get; set; }

        [Description("install with manual startup")]
        bool ManualStart { get; set; }

        [Description("install as a disabled service")]
        [ShortName('z')]
        bool Disabled { get; set; }
    }

    // TODO: port to EasyArgs & drop the extra package
    public class ServiceCommandlineOptions
        : IServiceCommandlineOptions
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

        public bool Install { get; set; }
        public bool Uninstall { get; set; }
        public bool RunOnce { get; set; }
        public bool Debug { get; set; }
        public int Wait { get; set; }
        public bool ShowVersion { get; set; }
        public bool StartService { get; set; }
        public bool StopService { get; set; }
        public bool ManualStart { get; set; }
        public bool Disabled { get; set; }

        private Action<string> HelpWriter { get; set; } = Console.WriteLine;

        public ServiceCommandlineOptions(
            string[] args,
            string helpHeading,
            string copyRightInformation = null
        )
        {
            Init(args, helpHeading, copyRightInformation);
        }

        private void Init(
            string[] args,
            string helpHeading,
            string copyRightInformation,
            bool preventParserExit = false
        )
        {
            var opts = new ParserOptions()
            {
                Description = new[] { helpHeading },
                MoreInfo = new[] { copyRightInformation },
                LineWriter = HelpWriter,
                IgnoreUnknownSwitches = true
            };
            if (preventParserExit)
            {
                opts.ExitOnError = false;
                opts.ExitWhenShowingHelp = false;
                opts.ShowHelpOnArgumentError = true;
            }

            var parsed = args.ParseTo<IServiceCommandlineOptions>(
                out _,
                opts
            );
            foreach (var prop in typeof(IServiceCommandlineOptions).GetProperties())
            {
                var val = prop.GetValue(parsed);
                prop.SetValue(this, val);
            }
        }

        internal ServiceCommandlineOptions(
            string[] args,
            string helpHeading,
            string copyRightInformation,
            Action<string> helpWriter
        )
        {
            HelpWriter = helpWriter;
            Init(
                args,
                helpHeading,
                copyRightInformation,
                preventParserExit: true
            );
        }
    }
}