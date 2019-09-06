using System;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Demo
{
    // demo names and ids taken from:
    // https://www.ranker.com/list/famous-female-programmers/reference
    class Program
    {
        public static void Main(string[] args)
        {
            var allDemos = typeof(Program).Assembly
                .GetTypes()
                .Where(t => t.Implements<IDemo>())
                .Where(t => !t.IsAbstract)
                .Select(t => new RunnableDemo(t))
                .OrderBy(d => d.Order)
                .ToArray();
            var total = allDemos.Length;
            var runLast = args.Contains("last");
            if (runLast)
            {
                allDemos = allDemos.Skip(allDemos.Length - 1)
                    .ToArray();
            }

            var generic = typeof(Program)
                .GetMethod(nameof(Run), BindingFlags.Public | BindingFlags.Static);
            var ctx = null as object;
            Console.Clear();
            Console.WriteLine("*** Welcome to the PeanutButter.DuckTyping Demo! **\n\n");
            allDemos.ForEach((runnableDemo, idx) =>
            {
                
                var specific = generic.MakeGenericMethod(runnableDemo.Type);
                var demo = Activator.CreateInstance(runnableDemo.Type);
                specific.Invoke(ctx, new[]
                {
                    demo,
                    runnableDemo.Name,
                    runnableDemo.Description,
                    runLast ? total : idx + 1,
                    total
                });
            });

            Console.WriteLine("\n\nThanks for attending this demo (:\n\n");
        }

        public static void Run<T>(
            T instance,
            string name,
            string description,
            int idx,
            int total) where T : IDemo
        {
            Console.WriteLine($"Demo [{idx}/{total}]: {name}");
            if (!string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine($"\n{description}\n\n");
            }

            WaitForKey();
            Console.WriteLine("===============\n");
            instance.Run();
            Console.WriteLine("\n===============\n\n");
            WaitForKey();
            Console.Clear();
        }

        static void WaitForKey()
        {
            Console.WriteLine("  (press any key to continue)");
            Console.ReadKey();
        }
    }
}