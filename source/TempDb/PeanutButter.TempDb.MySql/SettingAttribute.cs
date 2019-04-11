using System;

namespace PeanutButter.TempDb.MySql
{
    /// <summary>
    /// Decorates a TitleCase setting with the correct name for the setting within an ini file
    /// </summary>
    public class SettingAttribute: Attribute
    {
        /// <summary>
        /// The name of the setting to use when writing out the file
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public SettingAttribute(string name)
        {
            Name = name;
        }
    }
}