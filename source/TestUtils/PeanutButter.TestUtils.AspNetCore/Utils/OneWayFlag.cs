#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

internal class OneWayFlag
{
    public bool WasSet
    {
        get => _flag;
        set => _flag = value || _flag;
    }

    private bool _flag;
}