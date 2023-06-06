using System;
using System.IO;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides factory methods for basic console steps
    /// </summary>
    public static partial class ConsoleSteps
    {
        /// <summary>
        /// Creates the most basic steps:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps Basic()
        {
            return Basic("");
        }

        /// <summary>
        /// Creates the most basic steps:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps Basic(string prefix)
        {
            return Basic(prefix, Console.Out);
        }

        /// <summary>
        /// Creates the most basic steps, using the provided
        /// labels and writer
        /// - no color
        /// - ascii only
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps Basic(
            string prefix,
            string startLabel,
            string okLabel,
            string failLabel,
            TextWriter target
        )
        {
            return new(
                prefix,
                startLabel,
                okLabel,
                failLabel,
                target.Write,
                target.Flush,
                e => BasicExceptionHandler(e, target)
            );
        }

        private static ErrorHandlerResult BasicExceptionHandler(
            Exception ex,
            TextWriter writer
        )
        {
            // basic handler should print out the error but still chuck it
            writer.Write($" -> {ex.Message}\n");
            writer.Flush();
            return ErrorHandlerResult.Rethrow;
        }

        /// <summary>
        /// Creates the most basic steps:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps Basic(TextWriter writer)
        {
            return Basic("", writer);
        }

        /// <summary>
        /// Creates the most basic steps, writing to the provided writer:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps Basic(
            string prefix,
            TextWriter writer
        )
        {
            return Basic(
                prefix,
                TextStatusSteps.DEFAULT_START_LABEL,
                TextStatusSteps.DEFAULT_OK_LABEL,
                TextStatusSteps.DEFAULT_FAIL_LABEL,
                writer
            );
        }

        // ------
        /// <summary>
        /// Creates the most basic steps:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps ForTesting()
        {
            return ForTesting("");
        }

        /// <summary>
        /// Creates the most basic steps:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps ForTesting(string prefix)
        {
            return ForTesting(prefix, Console.Error);
        }

        /// <summary>
        /// Creates the most basic steps, using the provided
        /// labels and writer
        /// - no color
        /// - ascii only
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps ForTesting(
            string prefix,
            string startLabel,
            string okLabel,
            string failLabel,
            TextWriter target
        )
        {
            var spacer = string.IsNullOrEmpty(prefix)
                ? ""
                : " ";
            return new(
                () => $"[{DateTime.Now:u}]{spacer}{prefix}",
                startLabel,
                okLabel,
                failLabel,
                s => WriteTestingStatusLine(s, target),
                target.Flush,
                e => BasicTestingExceptionHandler(e, target)
            );
        }

        private static void WriteTestingStatusLine(
            string s,
            TextWriter target
        )
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return;
            }

            target.WriteLine($"{s.Trim()}");
        }

        private static ErrorHandlerResult BasicTestingExceptionHandler(
            Exception ex,
            TextWriter writer
        )
        {
            if (ex is null)
            {
                return ErrorHandlerResult.Suppress;
            }

            // basic handler should print out the error but still chuck it
            writer.Write($" -> {ex.Message}\n");
            writer.Flush();
            return ErrorHandlerResult.Rethrow;
        }

        /// <summary>
        /// Creates the most basic steps:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps ForTesting(TextWriter writer)
        {
            return ForTesting("", writer);
        }

        /// <summary>
        /// Creates the most basic steps, writing to the provided writer:
        /// - no color
        /// - ascii only
        /// - success marked with [ OK ]
        /// - failure marked with [FAIL]
        /// </summary>
        /// <returns></returns>
        public static TextStatusSteps ForTesting(
            string prefix,
            TextWriter writer
        )
        {
            return ForTesting(
                prefix,
                TextStatusSteps.DEFAULT_START_LABEL,
                TextStatusSteps.DEFAULT_OK_LABEL,
                TextStatusSteps.DEFAULT_FAIL_LABEL,
                writer
            );
        }
    }
    
}