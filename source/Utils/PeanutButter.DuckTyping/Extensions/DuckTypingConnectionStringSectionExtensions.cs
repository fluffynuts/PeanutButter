using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.DuckTyping.Shimming;

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides Duck-Typing extensions for ConnectionStringSettingsCollection objects
    /// (ie your connection strings from app.config / web.config
    /// </summary>
    public static class DuckTypingConnectionStringSectionExtensions
    {
        /// <summary>
        /// Attempts to fuzzy-duck connection strings to the provided interface
        /// -> returns null when cannot duck
        /// </summary>
        /// <param name="connectionStringSettings">ConnectionStrings section from your (app|web).config</param>
        /// <typeparam name="T">Type to duck to</typeparam>
        /// <returns></returns>
        public static T FuzzyDuckAs<T>(
            this ConnectionStringSettingsCollection connectionStringSettings
        ) where T : class
        {
            return connectionStringSettings.FuzzyDuckAs<T>(false);
        }

        /// <summary>
        /// Attempts to fuzzy-duck connection strings to the provided interface
        /// throws when cannot duck if throwOnError is true
        /// </summary>
        /// <param name="connectionStringSettings"></param>
        /// <param name="throwOnError"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T FuzzyDuckAs<T>(
            this ConnectionStringSettingsCollection connectionStringSettings,
            bool throwOnError
        ) where T : class
        {
            return new DictionaryWrappingConnectionStringSettingCollection(
                connectionStringSettings
            ).FuzzyDuckAs<T>(throwOnError);
        }

        /// <summary>
        /// Attempts to duck connection strings to the provided interface
        /// -> returns null when cannot duck
        /// </summary>
        /// <param name="connectionStringSettings">ConnectionStrings section from your (app|web).config</param>
        /// <typeparam name="T">Type to duck to</typeparam>
        /// <returns></returns>
        public static T DuckAs<T>(
            this ConnectionStringSettingsCollection connectionStringSettings
        ) where T : class
        {
            return connectionStringSettings.DuckAs<T>(false);
        }

        /// <summary>
        /// Attempts to duck connection strings to the provided interface
        /// throws when cannot duck if throwOnError is true
        /// </summary>
        /// <param name="connectionStringSettings"></param>
        /// <param name="throwOnError"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DuckAs<T>(
            this ConnectionStringSettingsCollection connectionStringSettings,
            bool throwOnError
        ) where T : class
        {
            return new DictionaryWrappingConnectionStringSettingCollection(
                connectionStringSettings
            ).DuckAs<T>(throwOnError);
        }
    }
}