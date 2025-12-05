namespace PeanutButter.ServiceShell.Tests;

public static class Program
{
    public static int Main(string[] args)
    {
        return Shell.RunMain<SomeService>(args);
    }
}