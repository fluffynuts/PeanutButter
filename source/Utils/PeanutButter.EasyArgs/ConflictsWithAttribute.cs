using PeanutButter.Args.Attributes;

namespace PeanutButter.Args
{
    public class ConflictsWithAttribute : StringAttribute
    {
        public ConflictsWithAttribute(string value) : base(value)
        {
        }
    }
}