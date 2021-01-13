namespace PeanutButter.Args
{
    internal interface IHasValue
    {
        string SingleValue { get; }
        string[] AllValues { get; }
        void Add(string value);
    }
}