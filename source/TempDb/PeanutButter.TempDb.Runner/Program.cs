using System;
using System.Threading;
using CommandLine;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.Runner
{
    public class Program
    {
        // really just here for testing
        public static ITempDB Instance { get; private set; }

        public static int Main(string[] args)
        {
            var running = new SemaphoreSlim(1);
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts =>
                {
                    try
                    {
                        WriteLine($"Selected engine: {opts.Engine}");
                        running.Wait();
                        Instance = TempDbFactory.Create(opts);
                        WriteLine($"Connection string: {Instance.ConnectionString}");
                        WaitForStopCommand(Instance);
                        Instance = null;
                    }
                    catch (ShowSupportedEngines ex)
                    {
                        WriteLine(ex.Message);
                    }
                    finally
                    {
                        running.Release();
                    }
                })
                .WithNotParsed(errors =>
                {
                    errors.ForEach(e => WriteLine(e.ToString()));
                });
            running.Wait();
            return 0;
        }

        public static Action<string> LineWriter { get; set; } = Console.WriteLine;

        public static void WriteLine(string line)
        {
            LineWriter?.Invoke(line);
        }

        private static void WaitForStopCommand(ITempDB instance)
        {
            var shell = new InteractiveShell(WriteLine);
            shell.RegisterCommand(
                "stop",
                cmd =>
                {
                    Console.WriteLine("Shutting down...");
                    instance?.Dispose();
                    shell.Dispose();
                });
        }
    }
}