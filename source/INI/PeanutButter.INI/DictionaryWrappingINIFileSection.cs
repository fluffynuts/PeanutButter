using System.Collections;
using System.Collections.Generic;

namespace PeanutButter.INI
{
    internal class DictionaryWrappingIniFileSection : IDictionary<string, string>
    {
        private readonly IINIFile _iniFile;
        private readonly string _section;

        public DictionaryWrappingIniFileSection(
            IINIFile iniFile,
            string section)
        {
            _iniFile = iniFile;
            _section = section;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Section?.GetEnumerator() ?? new EmptyEnumerator<KeyValuePair<string, string>>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, string> item)
        {
            if (Section == null)
                _iniFile.AddSection(_section);
            // ReSharper disable once PossibleNullReferenceException
            Section.Add(item);
        }

        public void Clear()
        {
            Section?.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return Section?.Contains(item) ?? false;
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            Section?.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return Section?.Remove(item) ?? true;
        }

        public int Count => Section?.Count ?? 0;
        public bool IsReadOnly => Section?.IsReadOnly ?? false;

        public bool ContainsKey(string key)
        {
            return _iniFile.HasSetting(_section, key);
        }

        public void Add(string key, string value)
        {
            _iniFile.SetValue(_section, key, value);
        }

        public bool Remove(string key)
        {
            // TODO: what about merged values?
            //    -> does the original inifile need a way to mask existing values with
            //        null values?
            _iniFile.RemoveValue(_section, key);
            return true;
        }

        public bool TryGetValue(string key, out string value)
        {
            var hasSetting = _iniFile.HasSetting(_section, key);
            value = hasSetting
                ? _iniFile.GetValue(_section, key)
                : null;
            return hasSetting;
        }

        public string this[string key]
        {
            get => _iniFile.GetValue(_section, key) ?? 
                (_iniFile.HasSetting(_section, key) ? null as string : throw new KeyNotFoundException(key));
            set => _iniFile.SetValue(_section, key, value);
        }
        
        private IDictionary<string, string> Section =>
            _iniFile.HasSection(_section) ? _iniFile.GetSection(_section) : null;

        private static readonly ICollection<string> EmptyCollection = new string[0];

        public ICollection<string> Keys => Section?.Keys ?? EmptyCollection;

        public ICollection<string> Values => Section?.Values ?? EmptyCollection;
    }
}