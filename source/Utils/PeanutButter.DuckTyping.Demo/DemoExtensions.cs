namespace PeanutButter.DuckTyping.Demo
{
    public static class DemoExtensions
    {
        public static bool IsString(this object ctx)
        {
            return ctx is string;
        }
    }
}