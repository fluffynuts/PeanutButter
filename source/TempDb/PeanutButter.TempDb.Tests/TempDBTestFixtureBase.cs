namespace PeanutButter.TempDb.Tests
{
    public abstract class TempDBTestFixtureBase
    {
        static TempDBTestFixtureBase()
        {
            TempDbHints.PreferredBasePath = "C:\\tmp\\tempdb\\wat";
        }
    }
}