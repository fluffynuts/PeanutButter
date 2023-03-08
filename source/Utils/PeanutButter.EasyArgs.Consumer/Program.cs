using System;
using PeanutButter.EasyArgs;

namespace PeanutButter.EasyArgs.Consumer
{
    class Program
    {
        static int Main(string[] args)
        {
            var opts = args.ParseTo<BareArgs>();
            return 0;
        }
    }

    public class BareArgs
    {
        int Port { get; }
    }
}