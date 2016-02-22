namespace PeanutButter.Utils
{
    public interface  ILocalDbFactory
    {
        void CreateDatabase(string dbName, string dbFile);
    }
}