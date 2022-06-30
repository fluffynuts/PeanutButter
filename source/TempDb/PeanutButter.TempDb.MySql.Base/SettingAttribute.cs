using System;

namespace PeanutButter.TempDb.MySql.Base
{
    /// <summary>
    /// Decorates a TitleCase setting with the correct name for the setting within an ini file
    /// </summary>
    public class SettingAttribute : Attribute
    {
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

        /// <inheritdoc />
        public SettingAttribute(string name)
        {
            Name = name;
        }

        public SettingAttribute(
            string name,
            bool isBare
        )
        {
            Name = name;
            IsBare = isBare;
        }
    }
}