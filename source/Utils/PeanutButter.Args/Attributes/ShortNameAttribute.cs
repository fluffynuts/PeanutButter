namespace PeanutButter.Args.Attributes
{
    public class ShortNameAttribute : StringAttribute
    {
        public ShortNameAttribute(char name) : base(name.ToString())
        {
        }
    }
}