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
        public const string SECTION = "mysqld";

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
            iniFile.AddSection(SECTION);
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
            KeyValuePair<string,string> setting)
        {
            iniFile[SECTION][setting.Key] = setting.Value;
        }

        private KeyValuePair<string, string> GetSetting(
            PropertyInfo prop,
            TempDbMySqlServerSettings tempDbMySqlSettings)
        {
            var settingAttrib = prop.GetCustomAttributes()
                .OfType<SettingAttribute>()
                .FirstOrDefault();
            if (settingAttrib == null)
                return new KeyValuePair<string, string>();
            return new KeyValuePair<string, string>(
                settingAttrib.Name,
                $"{prop.GetValue(tempDbMySqlSettings)}");
        }
    }
}