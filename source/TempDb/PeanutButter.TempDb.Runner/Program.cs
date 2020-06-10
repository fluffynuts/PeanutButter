using System;
using System.Threading;
using CommandLine;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.Runner
{
    public class Program
    {
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
                        WaitForStopCommand();
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
            AppDomain.CurrentDomain.ProcessExit += OnAppExit;
            Console.CancelKeyPress += OnCancelKeyPress;
            running.Wait();
            return 0;
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            ShutdownTempDbIfNecessary();
        }

        private static void OnAppExit(object sender, EventArgs e)
        {
            ShutdownTempDbIfNecessary();
        }

        private static void ShutdownTempDbIfNecessary()
        {
            if (Instance is null)
            {
                return;
            }

            Console.Write("Shutting down TempDb instance... ");
            Console.Out.Flush();
            Instance?.Dispose();
            Console.Write("done.");
        }

        // really just here for testing
        public static void DestroyInstance()
        {
            Instance?.Dispose();
            Instance = null;
        }

        public static ITempDB Instance { get; private set; }

        public static Action<string> LineWriter { get; set; } = Console.WriteLine;

        public static void WriteLine(string line)
        {
            LineWriter?.Invoke(line);
        }

        private static void WaitForStopCommand()
        {
            var shell = new InteractiveShell(WriteLine);
            shell.RegisterCommand(
                "stop",
                cmd =>
                {
                    Console.WriteLine("Shutting down...");
                    Instance?.Dispose();
                    Instance = null;
                    shell.Dispose();
                });
        }
    }
}