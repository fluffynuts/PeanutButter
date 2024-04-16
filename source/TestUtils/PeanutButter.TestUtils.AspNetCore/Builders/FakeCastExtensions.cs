using PeanutButter.TestUtils.AspNetCore.Fakes;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

internal static class FakeCastExtensions
{
    public static T As<T>(this object obj) where T: class, IFake
    {
        return obj as T ?? throw new InvalidImplementationException<T>(obj);
    }
}