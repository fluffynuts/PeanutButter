using System;
using PeanutButter.Args;

namespace PeanutButter.EasyArgs.Consumer
{
    class Program
    {
        static int Main(string[] args)
        {
            var opts = args.ParseTo<IBareArgs>();
            return 0;
        }
    }

    public interface IBareArgs
    {
        int Port { get; }
    }
}