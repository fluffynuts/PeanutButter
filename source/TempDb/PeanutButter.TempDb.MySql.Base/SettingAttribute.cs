using System;

namespace PeanutButter.TempDb.MySql.Base
{
    /// <summary>
    /// Decorates a TitleCase setting with the correct name for the setting within an ini file
    /// </summary>
    public class SettingAttribute : Attribute
    {
        /// <summary>
        /// The default section for applying settings, if not specified
        /// </summary>
        public const string DEFAULT_SECTION = "mysqld";

        /// <summary>
        /// The name of the setting to use when writing out the file
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether or not this is a "bare" setting. Bare settings
        /// are controlled by boolean values and either appear or not - they
        /// don't have a value set to them, eg skip-name-resolve
        /// </summary>
        public bool IsBare { get; }

        /// <summary>
        /// The section in my.ini to which this setting belongs (default: mysqld)
        /// </summary>
        public string Section { get; }

        /// <summary>
        /// If the value is null and this flag is set, then
        /// no extra output for this setting will be done, meaning
        /// you'll get the default values for mysql, whatever they are
        /// </summary>
        public bool IgnoreIfNull { get; }
        
        /// <inheritdoc />
        public SettingAttribute(
            string name
        ) : this(
            name,
            DEFAULT_SECTION
        )
        {
        }

        /// <summary>
        /// Define a setting in the provided section
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        public SettingAttribute(
            string name,
            string section
        ) : this(name, section, isBare: false, ignoreIfNull: true)
        {
        }

        /// <summary>
        /// Define a setting in the provided section.
        /// 
        /// If isBare is true, then the setting will only be set if the flag it
        /// decorates is true and the setting will have no value (ie, the setting
        /// name alone will be on one line in the requested section)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="section"></param>
        /// <param name="isBare"></param>
        /// <param name="ignoreIfNull"></param>
        /// <exception cref="ArgumentException"></exception>
        public SettingAttribute(
            string name,
            string section,
            bool isBare,
            bool ignoreIfNull
        )
        {
            Name = name ?? throw new ArgumentException("setting name may not be null");
            Section = section ?? DEFAULT_SECTION;
            IsBare = isBare;
            IgnoreIfNull = ignoreIfNull;
        }

        public SettingAttribute(
            string name,
            bool isBare
        ): this(name, DEFAULT_SECTION, isBare, ignoreIfNull: true)
        {
        }
    }
}