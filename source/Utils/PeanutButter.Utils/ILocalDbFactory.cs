namespace PeanutButter.Utils
{
    public interface  ILocalDbFactory
    {
        void CreateDatabase(string databaseName, string databaseFile);
    }
}