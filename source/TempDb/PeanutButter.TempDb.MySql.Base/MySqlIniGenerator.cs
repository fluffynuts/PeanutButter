using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql.Base
{
    /// <summary>
    /// Generates MySql configuration from TempDbMySqlServerSettings
    /// </summary>
    public class MySqlConfigGenerator
    {
        public const string MAIN_CONFIG_SECTION = "mysqld";

        /// <summary>
        /// Generates MySql configuration from TempDbMySqlServerSettings
        /// </summary>
        /// <param name="tempDbMySqlSettings"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GenerateFor(TempDbMySqlServerSettings tempDbMySqlSettings)
        {
            if (tempDbMySqlSettings == null) throw new ArgumentNullException(nameof(tempDbMySqlSettings));
            var iniFile = new INI.INIFile();
            iniFile.AddSection(MAIN_CONFIG_SECTION);
            tempDbMySqlSettings.GetType()
                .GetProperties()
                .Select(prop => GetSetting(prop, tempDbMySqlSettings))
                .Where(y => y.Key?.Length > 0)
                .ForEach(x => WriteSetting(iniFile, x));
            tempDbMySqlSettings
                .CustomConfiguration
                ?.Select(x => new KeyValuePair<string, string>(x.Key, x.Value))
                .ForEach(x => WriteSetting(iniFile, x));
            return iniFile.ToString();
        }

        private void WriteSetting(
            INI.INIFile iniFile,
            KeyValuePair<string, string> setting)
        {
            iniFile[MAIN_CONFIG_SECTION][setting.Key] = setting.Value;
        }

        private KeyValuePair<string, string> GetSetting(
            PropertyInfo prop,
            TempDbMySqlServerSettings tempDbMySqlSettings)
        {
            var settingAttrib = prop.GetCustomAttributes()
                .OfType<SettingAttribute>()
                .FirstOrDefault();
            if (settingAttrib is null)
            {
                return new KeyValuePair<string, string>();
            }

            if (prop.PropertyType == typeof(bool))
            {
                var propValue = (bool)prop.GetValue(tempDbMySqlSettings);
                if (settingAttrib.IsBare && !propValue)
                {
                    return empty();
                }

                return kvp(
                    settingAttrib.Name,
                    settingAttrib.IsBare
                        ? null
                        : OnOffFor((bool) prop.GetValue(tempDbMySqlSettings))
                );
            }

            return kvp(
                settingAttrib.Name,
                $"{prop.GetValue(tempDbMySqlSettings)}"
            );

            // ReSharper disable once InconsistentNaming
            KeyValuePair<string, string> kvp(
                string key,
                string value
            )
            {
                return new KeyValuePair<string, string>(key, value);
            }

            KeyValuePair<string, string> empty()
            {
                return new KeyValuePair<string, string>();
            }
        }

        private string OnOffFor(bool value)
        {
            return value
                ? "ON"
                : "OFF";
        }
    }
}