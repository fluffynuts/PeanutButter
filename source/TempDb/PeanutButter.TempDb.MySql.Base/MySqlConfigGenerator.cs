using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.TempDb.MySql.Base;

/// <summary>
/// Generates MySql configuration from TempDbMySqlServerSettings
/// </summary>
public class MySqlConfigGenerator
{
    private class ConfigLine
    {
        public string Name { get; }
        public string Value { get; }

        public ConfigLine(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    private static readonly List<ConfigLine> PerformanceSchemaOptionsForInactivityTimeout = new()
    {
        new("performance_schema_instrument", "'%=OFF'"),
        new("performance_schema_instrument", "'statement/%=ON'"),
        new("performance_schema_consumer_global_instrumentation", "ON"),
        new("performance_schema_consumer_thread_instrumentation", "ON"),
        new("performance_schema_consumer_events_statements_current", "ON")
    };

    /// <summary>
    /// Generates MySql configuration from TempDbMySqlServerSettings
    /// </summary>
    /// <param name="tempDbMySqlSettings"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GenerateFor(
        TempDbMySqlServerSettings tempDbMySqlSettings
    )
    {
        if (tempDbMySqlSettings == null)
        {
            throw new ArgumentNullException(nameof(tempDbMySqlSettings));
        }

        var iniFile = new INI.INIFile();
        iniFile.AddSection(SettingAttribute.DEFAULT_SECTION);
        var autoPerformanceSchema = false;
        if (tempDbMySqlSettings.Options.InactivityTimeout is not null)
        {
            if (tempDbMySqlSettings.PerformanceSchema == 0)
            {
                autoPerformanceSchema = true;
            }

            tempDbMySqlSettings.PerformanceSchema = 1;
        }

        tempDbMySqlSettings.GetType()
            .GetProperties()
            .Select(prop => GetSetting(prop, tempDbMySqlSettings))
            .Where(y => y.Name?.Length > 0)
            .ForEach(x => WriteSetting(iniFile, x));
        foreach (var customSetting in tempDbMySqlSettings.CustomConfiguration)
        {
            foreach (var kvp in customSetting.Value)
            {
                WriteSetting(
                    iniFile,
                    new IniSetting(
                        customSetting.Key,
                        kvp.Key,
                        kvp.Value,
                        true
                    )
                );
            }
        }


        if (Environment.GetEnvironmentVariable("TEMPDB_DUMP_CONFIG").AsBoolean())
        {
            Console.WriteLine(
                $"""
                 Starting mysqld with the following config:
                 ---
                 {iniFile}
                 ---
                 """
            );
        }

        var result = iniFile.ToString();
        if (!autoPerformanceSchema)
        {
            return result;
        }

        // when automatically enabling the performance schema 
        // for inactivity watches, keep it as light as possible
        // -> this requires first turning off all performance options
        //    and then re-enabling the ones we want, which results in
        //    a duplicated line for performance_schema_instrument, which
        //    INIFile cannot handle, by design. So we do the work "raw", here.
        var parts = new List<string>() { result };
        foreach (var item in PerformanceSchemaOptionsForInactivityTimeout)
        {
            parts.Add($"{item.Name}={item.Value}");
        }

        return parts.JoinWith(Environment.NewLine);
    }

    private void WriteSetting(
        INI.INIFile iniFile,
        IniSetting setting
    )
    {
        if (setting.Value is null && setting.IgnoreIfNull)
        {
            return;
        }

        iniFile[setting.Section][setting.Name] = setting.Value;
    }

    private class IniSetting
    {
        public string Section { get; }
        public string Name { get; }
        public string Value { get; }
        public bool IgnoreIfNull { get; }

        public IniSetting(
            string section,
            string name,
            string value,
            bool ignoreIfNull
        )
        {
            Section = section;
            Name = name;
            Value = value;
            IgnoreIfNull = ignoreIfNull;
        }

        public static IniSetting Empty()
        {
            return new(
                null,
                null,
                null,
                true
            );
        }
    }

    private IniSetting GetSetting(
        PropertyInfo prop,
        TempDbMySqlServerSettings tempDbMySqlSettings
    )
    {
        var settingAttrib = prop.GetCustomAttributes()
            .OfType<SettingAttribute>()
            .FirstOrDefault();
        if (settingAttrib is null)
        {
            return emptySetting();
        }

        var rawPropValue = prop.GetValue(tempDbMySqlSettings);
        if (prop.PropertyType != typeof(bool))
        {
            return iniSetting(
                settingAttrib.Section,
                settingAttrib.Name,
                rawPropValue is null
                    ? null
                    : $"{prop.GetValue(tempDbMySqlSettings)}",
                settingAttrib.IgnoreIfNull
            );
        }

        var propValue = (bool)rawPropValue;
        if (settingAttrib.IsBare && !propValue)
        {
            return emptySetting();
        }


        return iniSetting(
            settingAttrib.Section,
            settingAttrib.Name,
            settingAttrib.IsBare
                ? null
                : OnOffFor((bool)prop.GetValue(tempDbMySqlSettings)),
            settingAttrib.IgnoreIfNull
        );

        // ReSharper disable once InconsistentNaming
        IniSetting iniSetting(
            string section,
            string key,
            string value,
            bool ignoreIfNull
        )
        {
            return new(
                section,
                key,
                value,
                ignoreIfNull
            );
        }

        IniSetting emptySetting()
        {
            return IniSetting.Empty();
        }
    }

    private string OnOffFor(
        bool value
    )
    {
        return value
            ? "ON"
            : "OFF";
    }
}