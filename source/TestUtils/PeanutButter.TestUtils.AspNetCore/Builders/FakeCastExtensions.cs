
#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes;
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

internal static class FakeCastExtensions
{
    public static T As<T>(this object obj) where T: class, IFake
    {
        return obj as T ?? throw new InvalidImplementationException<T>(obj);
    }
}