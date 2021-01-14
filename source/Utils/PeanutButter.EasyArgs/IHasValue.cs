namespace PeanutButter.EasyArgs
{
    internal interface IHasValue
    {
        string SingleValue { get; }
        string[] AllValues { get; }
        void Add(string value);
    }
}