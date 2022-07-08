using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

internal static class FakeCastExtensions
{
    public static T As<T>(this object obj) where T: class, IFake
    {
        return obj as T ?? throw new InvalidImplementationException<T>(obj);
    }
}