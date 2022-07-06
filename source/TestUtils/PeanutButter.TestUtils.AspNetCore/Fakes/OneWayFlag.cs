namespace PeanutButter.TestUtils.AspNetCore.Fakes;

internal class OneWayFlag
{
    public bool WasSet
    {
        get => _flag;
        set => _flag = value || _flag;
    }

    private bool _flag;
}