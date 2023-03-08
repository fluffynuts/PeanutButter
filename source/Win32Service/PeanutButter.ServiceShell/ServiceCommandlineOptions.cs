using System;
using PeanutButter.EasyArgs;
using PeanutButter.EasyArgs.Attributes;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.ServiceShell
{
    /// <summary>
    /// Commandline options for services which are implemented via ServiceShell
    /// </summary>
    public interface IServiceCommandlineOptions
    {
        /// <summary>
        /// Instructs the service to install itself
        /// </summary>
        [Description("install this service")]
        bool Install { get; set; }

        /// <summary>
        /// Instructs the service to uninstall itself
        /// </summary>
        [Description("uninstall this service")]
        bool Uninstall { get; set; }

        /// <summary>
        /// Instructs the service to run one round of operations and then exit
        /// </summary>
        [Description("run one round of this service's code and exit")]
        bool RunOnce { get; set; }

        /// <summary>
        /// Instructs the service to run continually, with logging set to ALL
        /// </summary>
        [Description("run continually, with logging set to ALL")]
        bool Debug { get; set; }

        /// <summary>
        /// Instructs the service to wait this many seconds before actually
        /// doing a round of work for RunOnce
        /// </summary>
        [Description("wait this many seconds before actually doing the round of work, for run-once")]
        int Wait { get; set; }

        /// <summary>
        /// Instructs the service to display its current version
        /// </summary>
        [Description("show the version of this service")]
        [ShortName('v')]
        [LongName("--version")]
        bool ShowVersion { get; set; }

        /// <summary>
        /// Instructs the service to start itself, if it is registered
        /// </summary>
        [Description("start service")]
        [LongName("start")]
        bool StartService { get; set; }

        /// <summary>
        /// Instructs the service to stop itself, if it is registered
        /// </summary>
        [Description("stop service")]
        [ShortName('x')]
        [LongName("stop")]
        bool StopService { get; set; }

        /// <summary>
        /// Instructs the service to install with manual startup, when
        /// invoked to install itself
        /// </summary>
        [Description("install with manual startup")]
        bool ManualStart { get; set; }

        /// <summary>
        /// Instructs the service to install itself disabled,
        /// when invoked to install itself
        /// </summary>
        [Description("install as a disabled service")]
        [ShortName('z')]
        bool Disabled { get; set; }
    }

    /// <inheritdoc />
    public class ServiceCommandlineOptions
        : IServiceCommandlineOptions
    {
        /// <summary>
        /// Codes issued at exit
        /// </summary>
        public enum ExitCodes
        {
            /// <summary>
            /// Operation was successful (0)
            /// 
            /// </summary>
            Success = 0,
            /// <summary>
            /// An invalid commandline was specified
            /// </summary>
            CommandlineArgumentError,
            /// <summary>
            /// The requested operation (eg start/stop) could not be completed
            /// </summary>
            Failure,
            /// <summary>
            /// The service showed help and exited
            /// </summary>
            ShowedHelp,
            /// <summary>
            /// Installation failed
            /// </summary>
            InstallFailed,
            /// <summary>
            /// Uninstallation failed
            /// </summary>
            UninstallFailed
        }

        /// <inheritdoc />
        public bool Install { get; set; }

        /// <inheritdoc />
        public bool Uninstall { get; set; }

        /// <inheritdoc />
        public bool RunOnce { get; set; }

        /// <inheritdoc />
        public bool Debug { get; set; }

        /// <inheritdoc />
        public int Wait { get; set; }

        /// <inheritdoc />
        public bool ShowVersion { get; set; }

        /// <inheritdoc />
        public bool StartService { get; set; }

        /// <inheritdoc />
        public bool StopService { get; set; }

        /// <inheritdoc />
        public bool ManualStart { get; set; }

        /// <inheritdoc />
        public bool Disabled { get; set; }

        private Action<string> HelpWriter { get; set; } = Console.WriteLine;

        /// <summary>
        /// parameterless constructor to be used with something else that parses
        /// arguments, eg EasyArgs
        /// </summary>
        public ServiceCommandlineOptions()
        {
            // to be used with the EasyArgs parser
        }

        /// <summary>
        /// Construct the commandline arguments, using the string[] args
        /// provided to the program
        /// </summary>
        /// <param name="args"></param>
        /// <param name="helpHeading"></param>
        /// <param name="copyRightInformation"></param>
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
                opts.IgnoreUnknownSwitches = false;
            }

            var parsed = args.ParseTo<ServiceCommandlineOptions>(
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