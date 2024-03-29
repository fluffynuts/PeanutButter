﻿using System.Linq;
using System.Text.RegularExpressions;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides a wrapper around handling a commandline properly
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class Commandline
    {
        /// <summary>
        /// The base command (program, batch file, etc) for this commandline
        /// </summary>
        public string Command { get; }
        
        /// <summary>
        /// The arguments passed to this command
        /// </summary>
        public string[] Args { get; }

        /// <summary>
        /// Wraps a command and arguments
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public Commandline(
            string command,
            params string[] args
        )
        {
            Command = DeQuote(command);
            Args = args;
        }

        /// <summary>
        /// Produces the full commandline for the provided command and args
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new[] { Command }
                .And(Args)
                .Select(QuoteIfRequired)
                .JoinWith(" ");
        }

        private static readonly Regex WhiteSpace = new("\\s", RegexOptions.Compiled);

        private string QuoteIfRequired(string arg)
        {
            return WhiteSpace.IsMatch(arg)
                ? $"\"{arg}\""
                : arg;
        }

        private const char QUOTE = '"';

        private static string DeQuote(string str)
        {
            if (str.Length < 2)
            {
                return str;
            }

            if (str[0] == QUOTE && str[str.Length - 1] == QUOTE)
            {
                return str.Substring(1, str.Length - 2);
            }

            return str;
        }

        /// <summary>
        /// Parse a commandline into the command and args components
        /// </summary>
        /// <param name="commandline"></param>
        /// <returns></returns>
        public static Commandline Parse(string commandline)
        {
            var parts = (commandline ?? "").SplitCommandline();
            return new Commandline(
                parts[0],
                parts.Skip(1).ToArray()
            );
        }

        /// <summary>
        /// Implicitly cast a Commandline instance to a string
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static implicit operator string(Commandline o)
        {
            return o.ToString();
        }

        /// <summary>
        /// Implicitly cast a commandline string to a Commandline instance
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static implicit operator Commandline(string s)
        {
            return Parse(s);
        }
    }
}