#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
{
    internal interface IHasValue
    {
        string SingleValue { get; }
        string[] AllValues { get; }
        void Add(string value);
    }
}