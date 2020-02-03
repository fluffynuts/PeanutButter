using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.DuckTyping.Demo
{
    interface IDemo
    {
        void Run();
    }
    
    public abstract class Demo: IDemo
    {
        protected void Log(params object[] toLog)
        {
            var last = "";
            var parts = toLog.Select(o => o.IsString() 
                    ? o as string 
                    : o.Stringify())
                .Aggregate(new List<string>(),
                    (acc, cur) =>
                    {
                        acc.Add(last.EndsWith("\n")
                            ? cur
                            : $" {cur}");
                        last = cur;
                        return acc;
                    });
            
            Console.WriteLine(parts.JoinWith("").Trim());
        }

        protected void EmptyLine()
        {
            Console.WriteLine("");
        }
        
        protected void WaitForKey(string message = null)
        {
            Console.WriteLine(message ?? "  (press any key to continue)");
            Console.ReadKey();
        }

        public abstract void Run();
    }
}