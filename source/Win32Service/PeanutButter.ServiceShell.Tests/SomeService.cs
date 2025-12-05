namespace PeanutButter.ServiceShell.Tests;

public class SomeService: Shell
{
    public SomeService()
    {
        ServiceName = "SomeService";
        DisplayName = "SomeService";
    }

    protected override void RunOnce()
    {
        throw new System.NotImplementedException();
    }
}