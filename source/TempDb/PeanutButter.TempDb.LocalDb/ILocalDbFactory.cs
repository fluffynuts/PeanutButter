namespace PeanutButter.TempDb.LocalDb
{
    /// <summary>
    /// Interface to implement when your TempDB implementation needs to create a temporary database
    /// </summary>
    public interface ILocalDbFactory
    {
        /// <summary>
        /// Method to implement for a TempDB implementation. Will be given a database name and
        /// available temporary local file to use.
        /// </summary>
        /// <param name="databaseName">Name of the required database</param>
        /// <param name="databaseFile">A temporary file path which may be used as backing for the new database</param>
        void CreateDatabase(string databaseName, string databaseFile);
    }
}