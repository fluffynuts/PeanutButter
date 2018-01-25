using System;

namespace PeanutButter.TempDb.MySql
{
    public class SettingAttribute: Attribute
    {
        public string Name { get; set; }

        public SettingAttribute(string name)
        {
            Name = name;
        }
    }
}