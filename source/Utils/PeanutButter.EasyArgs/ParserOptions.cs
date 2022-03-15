using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PeanutButter.EasyArgs.Attributes;
using PeanutButter.Utils;

namespace PeanutButter.EasyArgs
{
    /// <summary>
    /// Provides options for the parser
    /// </summary>
    public class ParserOptions
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
        public virtual void DisplayHelp<T>(CommandlineArgument[] options)
        {
            if (ShowedHelp)
            {
                return;
            }

            ShowedHelp = true;

            var head = GenerateHelpHead<T>();
            var headSpacer = head.Any()
                ? OneLine
                : NoLines;
            var body = GenerateArgumentHelp<T>(options.Where(
                o => !o.IsImplicit || o.Key == CommandlineArgument.HELP_FLAG_KEY
            ).ToArray());
            var footer = GenerateHelpFooter<T>();
            var footerSpacer = footer.Any()
                ? OneLine
                : NoLines;
            var negateMessage = options.Any(o => o.IsFlag && !o.IsImplicit)
                ? new[] { "", NegateMessage }
                : NoLines;
            headSpacer.And(head)
                .And(headSpacer)
                .And(body)
                .And(negateMessage)
                .And(footerSpacer)
                .And(footer)
                .And("")
                .ForEach(s => LineWriter(s.TrimEnd()));

            if (ExitOnError)
            {
                ExitAction?.Invoke(ExitCodes.SHOWED_HELP);
            }

            ShowedHelp = true;
        }

        private static readonly string[] OneLine = { "" };
        private static readonly string[] NoLines = new string[0];

        /// <summary>
        /// Generates the help header from attributes or defaults to something
        /// legible including the current app file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual string[] GenerateHelpHead<T>()
        {
            var configured = ReadTextAttributes<T, DescriptionAttribute>();
            if (configured.Any())
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
                return new[]
                {
                    $"{currentApp.GetName().Name} {{args}}"
                };
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

            if (isExe)
            {
                return new[]
                {
                    $"Usage: {currentAppFile} {{args}}"
                };
            }

            return new[]
            {
                $"Usage: dotnet {currentAppFile} {{args}}"
            };
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
                result.Add(FormatOptionHelp(
                        opt.ShortName,
                        opt.LongName,
                        opt.Type,
                        opt.IsFlag,
                        FormatDescriptionText(
                            opt.Description,
                            opt.IsFlag,
                            opt.Default,
                            opt.LongName == "help"
                        ),
                        longestLeftCol,
                        ConsoleWidth
                    )
                );
            });
            return result
                .ToArray();
        }

        /// <summary>
        /// Formats the description text for an argument to make it "fit on the right"
        /// </summary>
        /// <param name="description"></param>
        /// <param name="isFlag"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected virtual string FormatDescriptionText(
            string description,
            bool isFlag,
            object defaultValue,
            bool isHelpFlag
        )
        {
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
                    ? $"{description} (default: on)"
                    : description;
            }

            var space = string.IsNullOrWhiteSpace(description)
                ? ""
                : " ";
            return defaultValue is null
                ? description?.Trim() ?? ""
                : $"{description?.Trim()}{space}({defaultValue})";
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

                result.Add(GrabWords(left, rightWords, maxLineWidth));
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
                return new String(' ', dashes + 1 + (isFirst
                        ? 1
                        : 0)
                );
            }

            return $"{new String('-', dashes)}{name}{(isFirst ? "," : "")}";
        }

        private string GrabWords(
            string left,
            List<string> words,
            int maxLineWidth
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
            while (words.Any() && totalLength + words.First().Length + 1 < maxLineWidth)
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

            return result.JoinWith(" ");
        }

        /// <summary>
        /// Provides the window width for the console (falling back on 80
        /// when that fails)
        /// </summary>
        public virtual int ConsoleWidth => TryReadConsoleWidth();

        private int TryReadConsoleWidth()
        {
            try
            {
                return Console.WindowWidth;
            }
            catch
            {
                return 80;
            }
        }

        /// <summary>
        /// Generates the help footer from a [MoreInfo] attribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected virtual string[] GenerateHelpFooter<T>()
        {
            return ReadTextAttributes<T, MoreInfoAttribute>();
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
        public virtual void ReportMissingRequiredOption(string arg)
        {
            LineWriter(
                $"{arg} is required"
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