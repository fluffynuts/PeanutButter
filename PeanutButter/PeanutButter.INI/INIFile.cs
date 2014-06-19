using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PeanutButter.INI
{
    public interface IINIFile
    {
        Dictionary<String, Dictionary<string, string>> Sections { get; }
        void Parse(string path);
        void AddSection(string section);
        void Persist(string path = null);
        void SetValue(string section, string key, string value);
        string GetValue(string section, string key, string defaultValue = null);
        bool HasSection(string section);
        bool HasSetting(string section, string key);
    }

    public class INIFile : IINIFile
    {
        private readonly List<string> _sections;
        private string _path;
        private readonly char[] _sectionTrimChars;
        public Dictionary<String, Dictionary<string, string>> Sections { get; private set; }
        public INIFile(string path = null)
        {
            Sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _sections = new List<string>();
            _sectionTrimChars = new[] { '[', ']' };
            if (path != null)
                Parse(path);
        }

        public void Parse(string path)
        {
            _path = path;
            ClearSections();
            var lines = GetLinesFrom(path);
            var currentSection = "";
            foreach (var line in lines)
            {
                if (IsSectionHeading(line))
                {
                    currentSection = GetSectionNameFrom(line);
                    AddSection(currentSection);
                    continue;
                }
                var parts = line.Split('=');
                var key = parts[0].Trim();
                var value = String.Join("=", parts.Skip(1));
                Sections[currentSection][key] = TrimOuterQuotesFrom(value);
            }
        }

        private string GetSectionNameFrom(string line)
        {
            return line.Trim(_sectionTrimChars);
        }

        private bool IsSectionHeading(string line)
        {
            return line.StartsWith(_sectionTrimChars[0].ToString()) && line.EndsWith(_sectionTrimChars[1].ToString());
        }

        private void ClearSections()
        {
            Sections.Clear();
            AddSection("");
        }

        private static Dictionary<string, string> CreateCaseInsensitiveStringsDictionary()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> GetLinesFrom(string path)
        {
            if (!File.Exists(path))
                return new string[] { };
            var lines = Encoding.UTF8.GetString(File.ReadAllBytes(path))
                                .Split(new[] {"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries)
                                .Select(line => line.Trim());
            return lines;
        }

        public void AddSection(string section)
        {
            if (!Sections.Keys.Contains(section))
            {
                Sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _sections.Add(section);
            }
        }

        private string TrimOuterQuotesFrom(string value)
        {
            if (value.Length < 2) return value;
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);
            return value.Trim();
        }

        public void Persist(string path = null)
        {
            path = CheckPersistencePath(path);
            var lines = new List<string>();
            foreach (var section in _sections)
            {
                if (section.Length > 0)
                    lines.Add(String.Join("", new[] { "[", section, "]" }));
                lines.AddRange(Sections[section].Keys.Select(key => String.Join("", new[] { key.Trim(), "=", "\"", Sections[section][key], "\"" })));
            }
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(String.Join(Environment.NewLine, lines)));
        }

        private string CheckPersistencePath(string path)
        {
            if (path == null)
                path = _path;
            if (path == null)
                throw new ArgumentException("No path specified to persist to and INIFile instantiated without an auto-path", "path");
            return path;
        }

        public void SetValue(string section, string key, string value)
        {
            AddSection(section);
            Sections[section][key] = value;
        }

        public string GetValue(string section, string key, string defaultValue = null)
        {
            if (Sections.Keys.All(s => s != section)) return defaultValue;
            if (Sections[section].Keys.All(k => k != key)) return defaultValue;
            return Sections[section][key];
        }

        public bool HasSection(string section)
        {
            return Sections.Keys.Contains(section);
        }

        public bool HasSetting(string section, string key)
        {
            return HasSection(section) && Sections[section].Keys.Contains(key);
        }
    }
}
