using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;
#else
using PeanutButter.Utils;
#endif

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
using Imported.PeanutButter.EasyArgs.Attributes;
namespace Imported.PeanutButter.EasyArgs
#else
using PeanutButter.EasyArgs.Attributes;

namespace PeanutButter.EasyArgs
#endif
{
    /// <summary>
    /// Provides options for the parser
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        class ParserOptions
    {
        /// <summary>
        /// Writes a line to the output (default is Console.WriteLine)
        /// </summary>
        public Action<string> LineWriter { get; set; } = Console.WriteLine;

        /// <summary>
        /// Used to exit the app when necessary (default Environment.Exit)
        /// </summary>
        public Action<int> ExitAction { get; set; } = Environment.Exit;

        /// <summary>
        /// (flag) exit when showing help? default true
        /// </summary>
        public bool ExitWhenShowingHelp { get; set; } = true;

        /// <summary>
        /// (flag) did we show help? useful if you choose not to exit when showing
        /// help
        /// </summary>
        public bool ShowedHelp { get; set; } = false;

        /// <summary>
        /// (flag) should we exit when args have an error? default true
        /// </summary>
        public bool ExitOnError { get; set; } = true;

        /// <summary>
        /// (flag) ignore unknown switches - only include them in the collected
        /// </summary>
        public bool IgnoreUnknownSwitches { get; set; }

        /// <summary>
        /// When enabled, some condensed versions of argument
        /// handling are enabled, including running together
        /// short flag arguments, eg
        ///   -n -a -b => -nab
        /// specifying numeric values without spaces, eg
        ///   -l 1234 => -l1234
        /// </summary>
        public bool EnableExtendedParsing { get; set; } = true;

        /// <summary>
        /// When unable to determine the number of columns available
        /// to the console, fall back on this value
        /// </summary>
        public int ConsoleColumns
        {
            get => _consoleColumns;
            set => _consoleColumns = Math.Max(value, 50);
        }

        private int _consoleColumns = 100;

        /// <summary>
        /// Reports that multiple values were found for a single-value argument
        /// </summary>
        /// <param name="arg"></param>
        public virtual void ReportMultipleValuesForSingleValueArgument(string arg)
        {
            LineWriter(
                $"{arg} specified more than once but only accepts one value"
            );
        }

        /// <summary>
        /// Reports when an expected file is missing
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="value"></param>
        public virtual void ReportMissingFile(
            string arg,
            string value
        )
        {
            LineWriter(
                $"{arg} should specify the path to an existing file, but '{value}' was not found"
            );
        }

        /// <summary>
        /// Reports that two conflicting arguments were specified
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public virtual void ReportConflict(string left, string right)
        {
            LineWriter(
                $"{left} conflicts with {right}"
            );
        }

        /// <summary>
        /// Reports that an unknown switch was encountered
        /// </summary>
        /// <param name="arg"></param>
        public virtual void ReportUnknownSwitch(string arg)
        {
            if (IgnoreUnknownSwitches)
            {
                return;
            }

            LineWriter(
                $"unknown option: {arg}"
            );
        }


        /// <summary>
        /// Message to display about the auto --no-{arg} functionality for flags
        /// </summary>
        protected virtual string NegateMessage =>
            "Negate any flag argument with --no-{option}";

        /// <summary>
        /// Displays help via the LineWriter action
        /// </summary>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void DisplayHelp<T>(
            CommandlineArgument[] options
        )
        {
            if (ShowedHelp)
            {
                return;
            }

            ShowedHelp = true;

            var head = GenerateHelpHead<T>(this);
            var headSpacer = head.Any()
                ? OneLine
                : NoLines;
            var body = GenerateArgumentHelp<T>(
                options.Where(o => !o.IsImplicit || o.Key == CommandlineArgument.HELP_FLAG_KEY
                ).ToArray()
            );
            var footer = GenerateHelpFooter<T>(this);
            var footerSpacer = footer.Any()
                ? OneLine
                : NoLines;
            var negateMessage = options.Any(o => o.IsFlag && !o.IsImplicit)
                ? ["", NegateMessage]
                : NoLines;
            headSpacer.And(head)
                .And(headSpacer)
                .And(body)
                .And(negateMessage)
                .And(footerSpacer)
                .And(footer)
                .And("")
                .ForEach(s => LineWriter(s.TrimEnd()));

            if (ExitWhenShowingHelp)
            {
                ExitAction?.Invoke(ExitCodes.SHOWED_HELP);
            }

            ShowedHelp = true;
        }

        private static readonly string[] OneLine =
        {
            ""
        };

        private static readonly string[] NoLines = [];

        /// <summary>
        /// Generates the help header from attributes or defaults to something
        /// legible including the current app file
        /// </summary>
        /// <param name="parserOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual string[] GenerateHelpHead<T>(
            ParserOptions parserOptions
        )
        {
            var configured = FirstSpecified(
                parserOptions.Description,
                ReadTextAttributes<T, DescriptionAttribute>()
            );
            if (configured.Any() && !parserOptions.IncludeDefaultDescription)
            {
                return configured;
            }

            var currentApp = Assembly.GetEntryAssembly();
            if (currentApp is null)
            {
                return configured;
            }

            if (string.IsNullOrWhiteSpace(currentApp.Location))
            {
                // packed, published app - just take the assembly name
                return
                [
                    $"{currentApp.GetName().Name} {{args}}"
                ];
            }

            var currentAppPath = new Uri(currentApp.Location).LocalPath;
            var currentAppFile = Path.GetFileName(currentAppPath);
            var currentAppExtension = Path.GetExtension(currentAppFile).ToLowerInvariant();
            var isExe = currentAppExtension == ".exe";
            if (currentAppExtension == ".dll")
            {
                var searchExe = currentAppFile.RegexReplace("dll$", "exe");
                if (File.Exists(searchExe))
                {
                    currentAppFile = searchExe;
                    isExe = true;
                }
            }

            string[] DefaultDescription()
            {
                return isExe
                    ? [$"Usage: {currentAppFile} {{args}}"]
                    : [$"Usage: dotnet {currentAppFile} {{args}}"];
            }

            return configured
                .Concat(DefaultDescription())
                .ToArray();
        }

        private static string[] FirstSpecified(
            string[] left,
            string[] right
        )
        {
            return left?.Any() ?? false
                ? left
                : right;
        }

        private const int SHORT_NAME_LENGTH = 1;
        private static readonly int DashLength = "-".Length;
        private const string LEFT_BRACKET = "[";
        private const string RIGHT_BRACKET = "]";
        private static readonly int LeftBracketLength = LEFT_BRACKET.Length;
        private static readonly int RightBracketLength = RIGHT_BRACKET.Length;
        private static readonly int CommaAndSpaceLength = ", ".Length;
        private static readonly int SingleSpaceLength = " ".Length;
        private static readonly int COLUMN_PADDING_LENGTH = 1;

        /// <summary>
        /// Generates the help for the arguments section
        /// </summary>
        /// <param name="options"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual string[] GenerateArgumentHelp<T>(CommandlineArgument[] options)
        {
            var result = new List<string>();
            if (options.Any(o => o.EnvironmentDefaultVariable is not null))
            {
                result.Add(
                    """
                    One or more options can have their default value
                      set from an environment variable. Where this is
                      the case, the variable will be mentioned in
                      [SQUARE_BRACKETS] after the option.
                    """
                );
            }

            var longestLeftCol = options.Select(o =>
                DashLength +
                SHORT_NAME_LENGTH +
                CommaAndSpaceLength +
                DashLength + DashLength +
                o.LongName.Length +
                SingleSpaceLength +
                LeftBracketLength +
                o.Type.Length +
                RightBracketLength +
                COLUMN_PADDING_LENGTH
            ).Max();
            options.ForEach(opt =>
                {
                    result.Add(
                        FormatOptionHelp(
                            opt.ShortName,
                            opt.LongName,
                            opt.Type,
                            opt.IsFlag,
                            FormatDescriptionText(
                                opt.Description,
                                opt.IsFlag,
                                opt.Default,
                                opt.EnvironmentDefaultVariable,
                                opt.LongName == "help"
                            ),
                            longestLeftCol,
                            ConsoleWidth
                        )
                    );
                }
            );
            return result
                .ToArray();
        }

        /// <summary>
        /// Formats the description text for an argument to make it "fit on the right"
        /// </summary>
        /// <param name="description"></param>
        /// <param name="isFlag"></param>
        /// <param name="defaultValue"></param>
        /// <param name="envVarForDefault"></param>
        /// <param name="isHelpFlag"></param>
        /// <returns></returns>
        protected virtual string FormatDescriptionText(
            string description,
            bool isFlag,
            object defaultValue,
            string envVarForDefault,
            bool isHelpFlag
        )
        {
            var postfix = envVarForDefault is null
                ? ""
                : $" [{envVarForDefault}]";
            if (isFlag)
            {
                if (isHelpFlag)
                {
                    return description;
                }

                var defaultFlag = false;
                try
                {
                    defaultFlag = (bool)defaultValue;
                }
                catch
                {
                    // suppress
                }

                return defaultFlag
                    ? $"{description} (default: on){postfix}"
                    : $"{description}{postfix}";
            }

            var space = string.IsNullOrWhiteSpace(description)
                ? ""
                : " ";
            return defaultValue is null
                ? $"{description?.Trim() ?? ""}{postfix}"
                : $"{description?.Trim()}{space}({defaultValue}){postfix}";
        }

        /// <summary>
        /// Produces a formatted type annotation for an argument
        /// </summary>
        /// <param name="type"></param>
        /// <param name="isFlag"></param>
        /// <returns></returns>
        protected virtual string TypeAnnotationFor(
            string type,
            bool isFlag
        )
        {
            if (string.IsNullOrWhiteSpace(type) || isFlag)
            {
                return "";
            }

            return $" {LEFT_BRACKET}{type}{RIGHT_BRACKET}";
        }

        /// <summary>
        /// Formats the help entry for an option
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="longName"></param>
        /// <param name="type"></param>
        /// <param name="isFlag"></param>
        /// <param name="helpText"></param>
        /// <param name="leftColWidth"></param>
        /// <param name="maxLineWidth"></param>
        /// <returns></returns>
        protected string FormatOptionHelp(
            string shortName,
            string longName,
            string type,
            bool isFlag,
            string helpText,
            int leftColWidth,
            int maxLineWidth
        )
        {
            var left = $@"{
                FormatArg(shortName, 1, true)
            } {
                FormatArg(longName, 2, false)
            }{
                TypeAnnotationFor(type, isFlag)
            }".PadRight(leftColWidth);
            var rightWords = (helpText ?? "").Trim().Split(' ').ToList();
            if (!rightWords.Any())
            {
                return left;
            }

            var result = new List<string>();
            while (rightWords.Any())
            {
                if (result.Count == 1)
                {
                    left = new String(' ', left.Length);
                }

                var prefix = result.Any()
                    ? "  "
                    : "";
                result.Add(GrabWords(left, rightWords, maxLineWidth, prefix));
            }

            return result.JoinWith("\n");
        }

        /// <summary>
        /// Formats the argument part of argument help (eg '-h, --help')
        /// Should be called twice per argument, with a flag as to whether
        /// it's the first call (for the short part) or the second (for the
        /// long part)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dashes"></param>
        /// <param name="isFirst"></param>
        /// <returns></returns>
        protected string FormatArg(
            string name,
            int dashes,
            bool isFirst
        )
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new String(
                    ' ',
                    dashes + 1 + (isFirst
                        ? 1
                        : 0)
                );
            }

            return $"{new String('-', dashes)}{name}{(isFirst ? "," : "")}";
        }

        private string GrabWords(
            string left,
            List<string> words,
            int maxLineWidth,
            string prefix
        )
        {
            if (!words.Any())
            {
                return left;
            }

            var result = new List<string>()
            {
                left
            };
            var haveAddedWord = false;
            var totalLength = result.JoinWith(" ").Length;
            while (words.Any() && totalLength + words.First().Length + 1 < maxLineWidth - prefix.Length)
            {
                haveAddedWord = true;
                result.Add(words[0]);
                words.RemoveAt(0);
                totalLength = result.JoinWith(" ").Length;
            }

            if (!haveAddedWord)
            {
                result.Add(words.Shift());
            }

            return $"{prefix}{result.JoinWith(" ")}";
        }

        /// <summary>
        /// Provides the window width for the console (falling back on
        /// the value of ConsoleColumns (default: 100) when that fails)
        /// </summary>
        public virtual int ConsoleWidth => TryReadConsoleWidth();

        /// <summary>
        /// Override any overall help description
        /// </summary>
        public string[] Description { get; set; }

        /// <summary>
        /// Provide the help footer via parser argument rather than
        /// from the decorated [MoreInfo]
        /// </summary>
        public string[] MoreInfo { get; set; }

        /// <summary>
        /// When specifying a description for your options, whether
        /// or not the default {programname.exe} {args} should be included
        /// </summary>
        public bool IncludeDefaultDescription { get; set; }

        /// <summary>
        /// Automatically show help text on argument error
        /// </summary>
        public bool ShowHelpOnArgumentError { get; set; }

        /// <summary>
        /// When enabled, environment variables will be observed as fallback values
        /// for arguments. For example, if you have a RemoteHost property, then any
        /// of the following environment variables should configure it if it is not
        /// explicitly set from the commandline:
        /// REMOTEHOST
        /// REMOTE_HOST
        /// ReMOteHosT
        /// </summary>
        public bool FallbackOnEnvironmentVariables { get; set; }

        private int TryReadConsoleWidth()
        {
            try
            {
                return Console.WindowWidth;
            }
            catch
            {
                var envVar = Environment.GetEnvironmentVariable("COLUMNS");
                if (envVar is not null && int.TryParse(envVar, out var cols))
                {
                    return cols;
                }

                return ConsoleColumns;
            }
        }
        
        /// <summary>
        /// Generates the help footer from a [MoreInfo] attribute
        /// </summary>
        /// <param name="parserOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual string[] GenerateHelpFooter<T>(ParserOptions parserOptions)
        {
            return FirstSpecified(
                parserOptions.MoreInfo,
                ReadTextAttributes<T, MoreInfoAttribute>()
            );
        }

        /// <summary>
        /// Reads all values from text-based attributes of type
        /// TAttribute which decorate THost
        /// </summary>
        /// <typeparam name="THost"></typeparam>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        protected string[] ReadTextAttributes<THost, TAttribute>()
            where TAttribute : StringAttribute
        {
            return typeof(THost)
                .GetCustomAttributes()
                .OfType<TAttribute>()
                .Select(a => a.Value?.Trim() ?? "")
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToArray();
        }

        /// <summary>
        /// Reports a missing, required option
        /// </summary>
        /// <param name="arg"></param>
        public virtual void ReportMissingRequiredOption(CommandlineArgument arg)
        {
            var suffix = FallbackOnEnvironmentVariables
                ? $", or set the {arg.Key.ToSnakeCase().ToUpper()} environment variable appropriately"
                : "";
            LineWriter(
                $"--{arg.LongName} is required{suffix}"
            );
        }

        /// <summary>
        /// Reports a minimum value constraint violation
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="minValue"></param>
        /// <param name="value"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ReportMinimumViolation(
            string arg,
            decimal? minValue,
            decimal value
        )
        {
            LineWriter(
                $"{arg} should be at least {minValue} (received: {value})"
            );
        }

        /// <summary>
        /// Reports a minimum value constraint violation
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="maxValue"></param>
        /// <param name="value"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ReportMaximumViolation(
            string arg,
            decimal? maxValue,
            decimal value
        )
        {
            LineWriter(
                $"{arg} should be at most {maxValue} (received: {value})"
            );
        }
    }
}