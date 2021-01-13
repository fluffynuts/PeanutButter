PeanutButter.EasyArgs
---

Provides a dead-easy way to get commandline arguments
into a structured format. Can be as simple as:

```

public static class Program
{
    public interface IOptions
    {
        int Port { get; }
        string Server { get; }
    }
    
    public static int Main(string[] args)
    {
        var opts = args.ParseTo<IOptions>();
        // bam! if you had the commandline:
        // --port 123 --server localhost
        // then you'd have
        // { Port: 123, Server: "localhost" }
    }
}

```