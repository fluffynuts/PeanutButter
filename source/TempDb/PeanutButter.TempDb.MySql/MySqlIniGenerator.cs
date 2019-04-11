using System;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql
{
    /// <summary>
    /// Generates MySql configuiration from TempDbMySqlServerSettings
    /// </summary>
    public class MySqlConfigGenerator
    {
        private const string SECTION = "mysqld";

        /// <summary>
        /// Generates MySql configuiration from TempDbMySqlServerSettings
        /// </summary>
        /// <param name="tempDbMySqlSettings"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GenerateFor(TempDbMySqlServerSettings tempDbMySqlSettings)
        {
            if (tempDbMySqlSettings == null) throw new ArgumentNullException(nameof(tempDbMySqlSettings));

            var iniFile = new INIFile.INIFile();
            iniFile.AddSection(SECTION);
            tempDbMySqlSettings
                .GetType()
                .GetProperties()
                .ForEach(prop => AddSetting(iniFile, prop, tempDbMySqlSettings));
            return iniFile.ToString();
        }

        private void AddSetting(
            INIFile.INIFile iniFile,
            PropertyInfo prop, 
            TempDbMySqlServerSettings tempDbMySqlSettings
        )
        {
            var settingAttrib = prop.GetCustomAttributes()
                .OfType<SettingAttribute>()
                .FirstOrDefault();
            if (settingAttrib == null)
                return;
            iniFile[SECTION][settingAttrib.Name] = $"{prop.GetValue(tempDbMySqlSettings)}";
        }
    }
}