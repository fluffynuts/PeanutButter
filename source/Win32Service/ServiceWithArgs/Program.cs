using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;

namespace ServiceWithArgs
{
    class Program
    {
        static void Main(string[] args)
        {
            Validate(args);
            // first arg is the file to log to
            // other args should be appended to that file every 500 ms
            var someService = new SomeService()
            {
                Args = args
            };
            ServiceBase.Run(someService);
        }

        private static void Validate(string[] args)
        {
            if (args.Length == 0)
            {
                throw new InvalidOperationException("Log file path required");
            }

            if (args.Length < 2)
            {
                throw new InvalidOperationException("Specify _something_ to log to the file!");
            }

            var logDir = Path.GetDirectoryName(args[0]);
            EnsureDirExists(logDir);
        }

        private static void EnsureDirExists(string dir)
        {
            var parts = new Queue<string>(dir.Split(new[] { "\\", "/" }, StringSplitOptions.None));
            var current = new List<string>();
            do
            {
                current.Add(parts.Dequeue());
                var asPath = string.Join(Path.PathSeparator.ToString(), current);
                if (!Directory.Exists(asPath))
                {
                    Directory.CreateDirectory(asPath);
                }
            } while (parts.Count > 0);
        }
    }

    public class SomeService : ServiceBase
    {
        public string[] Args { get; set; }
        private bool _running;
        private Thread _runningThread;

        protected override void OnStart(string[] args)
        {
            var logFile = Args.First();
            var logArgs = string.Join(" ", Args.Skip(1));
            _running = true;
            _runningThread = new Thread(() =>
            {
                while (_running)
                {
                    File.AppendAllLines(
                        logFile,
                        new[]
                        {
                            $"[{DateTime.Now}]: {string.Join(" ", logArgs)}"
                        });
                    Thread.Sleep(500);
                }
            });
            _runningThread.Start();
        }

        protected override void OnStop()
        {
            _running = false;
            _runningThread.Join();
        }
    }
}