PeanutButter.EasyArgs
---

Provides a dead-easy way to get commandline arguments
into a structured format. Can be as simple as:

```csharp
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

By default, you get:
- short option (eg `-a`) using the first character of the property name
  - an attempt at conflict resolution: first use lower-case character,
    then an upper-case one, eg `-p` for `Port` and `-P` for Password, first come,
    first served.
- long option (eg `--some-option`) using a kebab-cased version of the property name
- type conversions done for you ("123" on the cli becomes int `123`)
- help added for your arguments

With a little attribute decoration, you can add:
- `[Description("text")]` - for arguments
- `[Description("text"]` - for the class or interface your options live on
- `[MoreInfo("text")]` - help footer text for the interface your options live on
- `[Default(anyConstantValue)]` values for properties
- `[Required]` markings for required properties

eg:

```csharp
[Description(@"
This is a cool program, made to help you out

Usage: MyProgram {args} ...files
")
[MoreInfo(@"
For support, please email foo@bar.com
")]
public interface IOptions
{
    [Required]
    [Description("the port to connect to")]
    [Default(80)]
    int Port { get; }
    
    [Required]
    [Description("server ip or hostname")]
    [Default("localhost")]
    string Server { get; }
}
```

Parsed types can be interfaces or classes and don't have to have write
access on properties. Under the hood, PeanutButter.EasyArgs uses
PeanutButter.DuckTyping to generate types as necessary.