using System;
using System.Linq;
using System.Reflection;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql
{
    public class MySqlConfigGenerator
    {
        private const string SECTION = "mysqld";

        public string GenerateFor(MySqlSettings mySqlSettings)
        {
            if (mySqlSettings == null) throw new ArgumentNullException(nameof(mySqlSettings));

            var iniFile = new INIFile.INIFile();
            iniFile.AddSection(SECTION);
            mySqlSettings
                .GetType()
                .GetProperties()
                .ForEach(prop => AddSetting(iniFile, prop, mySqlSettings));
            return iniFile.ToString();
        }

        private void AddSetting(
            INIFile.INIFile iniFile,
            PropertyInfo prop, 
            MySqlSettings mySqlSettings
        )
        {
            var settingAttrib = prop.GetCustomAttributes()
                .OfType<SettingAttribute>()
                .FirstOrDefault();
            if (settingAttrib == null)
                return;
            iniFile[SECTION][settingAttrib.Name] = $"{prop.GetValue(mySqlSettings)}";
        }
    }
}