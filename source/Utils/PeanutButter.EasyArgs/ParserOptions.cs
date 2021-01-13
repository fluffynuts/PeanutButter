using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.Args.Attributes;
using PeanutButter.Utils;

namespace PeanutButter.Args
{
    public class ParserOptions
    {
        public Action<string> LineWriter { get; set; } = Console.WriteLine;
        public Action<int> ExitAction { get; set; } = Environment.Exit;
        public bool ExitWhenShowingHelp { get; set; } = true;
        public bool ShowedHelp { get; set; } = false;
        public bool ExitOnError { get; set; } = true;

        public virtual void ReportMultipleValuesForSingleValueArgument(string arg)
        {
            LineWriter(
                $"{arg} specified more than once but only accepts one value"
            );
        }

        public virtual void ReportConflict(string left, string right)
        {
            LineWriter(
                $"{left} conflicts with {right}"
            );
        }

        public virtual void ReportUnknownArg(string arg)
        {
            LineWriter(
                $"unknown option: {arg}"
            );
        }

        protected virtual string NegateMessage =>
            "Negate any flag argument with --no-{option}";

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
            var negateMessage = options.Any(o => o.IsFlag)
                ? new[] { "", NegateMessage }
                : NoLines;
            head
                .And(headSpacer)
                .And(body)
                .And(negateMessage)
                .And(footerSpacer)
                .And(footer)
                .ForEach(s => LineWriter(s.TrimEnd()));

            if (ExitOnError)
            {
                ExitAction?.Invoke(ExitCodes.SHOWED_HELP);
            }

            ShowedHelp = true;
        }

        private static readonly string[] OneLine = { "" };
        private static readonly string[] NoLines = new string[0];

        protected virtual string[] GenerateHelpHead<T>()
        {
            return ReadTextAttributes<T, DescriptionAttribute>();
        }

        private const int SHORT_NAME_LENGTH = 1;
        private static readonly int DASH_LENGTH = "-".Length;
        private const string LEFT_BRACKET = "[";
        private const string RIGHT_BRACKET = "]";
        private static readonly int LEFT_BRACKET_LENGTH = LEFT_BRACKET.Length;
        private static readonly int RIGHT_BRACKET_LENGTH = RIGHT_BRACKET.Length;
        private static readonly int COMMA_AND_SPACE_LENGTH = ", ".Length;
        private static readonly int SINGLE_SPACE_LENGTH = " ".Length;
        private static readonly int COLUMN_PADDING_LENGTH = 1;

        protected virtual string[] GenerateArgumentHelp<T>(CommandlineArgument[] options)
        {
            var result = new List<string>();
            var longestLeftCol = options.Select(o =>
                DASH_LENGTH +
                SHORT_NAME_LENGTH +
                COMMA_AND_SPACE_LENGTH +
                DASH_LENGTH + DASH_LENGTH +
                o.LongName.Length +
                SINGLE_SPACE_LENGTH +
                LEFT_BRACKET_LENGTH +
                o.Type.Length +
                RIGHT_BRACKET_LENGTH +
                COLUMN_PADDING_LENGTH
            ).Max();
            options.ForEach(opt =>
            {
                result.Add(FormatOptionHelp(
                        opt.ShortName,
                        opt.LongName,
                        opt.Type,
                        opt.IsFlag,
                        FormatDescriptionText(opt.Description, opt.IsFlag, opt.Default),
                        longestLeftCol,
                        ConsoleWidth
                    )
                );
            });
            return result
                .ToArray();
        }

        protected virtual string FormatDescriptionText(
            string description,
            bool isFlag,
            object defaultValue
        )
        {
            if (isFlag)
            {
                return description;
            }

            var space = string.IsNullOrWhiteSpace(description)
                ? ""
                : " ";
            return defaultValue is null
                ? description?.Trim() ?? ""
                : $"{description?.Trim()}{space}({defaultValue})";
        }

        protected virtual string TypeAnnotationFor(string type, bool isFlag)
        {
            if (string.IsNullOrWhiteSpace(type) || isFlag)
            {
                return "";
            }

            return $" {LEFT_BRACKET}{type}{RIGHT_BRACKET}";
        }

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

        protected string FormatArg(string name, int dashes, bool isFirst)
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

        protected virtual string NegatedFlagLeft(
            string flagName,
            int columnWidth
        )
        {
            return $"    --no-{flagName}".PadRight(columnWidth);
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

        protected virtual string[] GenerateHelpFooter<T>()
        {
            return ReadTextAttributes<T, MoreInfoAttribute>();
        }

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

        public virtual void ReportMissingRequiredOption(string arg)
        {
            LineWriter(
                $"{arg} is required"
            );
        }
    }
}