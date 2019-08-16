using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.Runner
{
    public class InteractiveShell : IDisposable
    {
        private readonly Action<string> _writeLine;
        public static Func<string> ReadLine { get; set; } = Console.ReadLine;

        private readonly Dictionary<string, Action<string>> _commands
            = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase);

        private bool _active;

        public InteractiveShell(Action<string> writeLine)
        {
            _writeLine = writeLine;
        }

        public void RegisterCommand(
            string command,
            Action<string> callback)
        {
            _commands[command] = callback;
            _active = true;
            Start();
        }

        private void Start()
        {
            while (_active)
            {
                var input = ReadLine()?.Trim();
                if (input == null)
                {
                    continue;
                }

                if (_commands.TryGetValue(input.Split(new[] { " ", "\t" }, StringSplitOptions.None).First(),
                    out var action))
                {
                    action(input);
                    continue;
                }

                _writeLine($"Command not understood: {input}");
                _writeLine($"Supported command{(_commands.Count == 1 ? "": "s")}: {_commands.Keys.JoinWith(",")}");
            }
        }

        public void Dispose()
        {
            _active = false;
        }
    }
}