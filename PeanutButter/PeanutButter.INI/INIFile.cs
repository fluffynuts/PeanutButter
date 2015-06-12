using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PeanutButter.INI
{
    public interface IINIFile
    {
        Dictionary<string, string> this[string index] { get; }
        IEnumerable<string> Sections { get; }
        void Load(string path);
        void AddSection(string section);
        void Persist(string path = null);
        void SetValue(string section, string key, string value);
        string GetValue(string section, string key, string defaultValue = null);
        bool HasSection(string section);
        bool HasSetting(string section, string key);
    }

    public class INIFile : IINIFile
    {
        public IEnumerable<string> Sections
        {
            get { return Data.Keys; }
        }

        private string _path;
        private readonly char[] _sectionTrimChars;
        protected Dictionary<String, Dictionary<string, string>> Data { get; set; }
        public INIFile(string path = null)
        {
            Data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            _sectionTrimChars = new[] { '[', ']' };
            if (path != null)
                Load(path);
        }


        public void Load(string path)
        {
            _path = path;
            var lines = GetLinesFrom(path);
            Parse(lines);
        }

        public void Parse(string contents)
        {
            Parse(SplitIntoLines(contents));
        }

        private void Parse(IEnumerable<string> lines)
        {
            ClearSections();
            var currentSection = "";
            foreach (var line in lines.Select(RemoveComments).Where(l => !String.IsNullOrEmpty(l)))
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
                this[currentSection][key] = TrimOuterQuotesFrom(value);
            }
        }

        private string RemoveComments(string line)
        {
            var parts = line.Split(';');
            var toTake = 1;
            while (toTake <= parts.Length && HaveUnmatchedQuotesIn(parts.Take(toTake)))
                toTake++;
            return String.Join(";", parts.Take(toTake)).Trim();
        }

        private bool HaveUnmatchedQuotesIn(IEnumerable<string> parts)
        {
            var joined = String.Join(";", parts);
            var quoted = joined.Count(c => c == '"');
            return quoted % 2 != 0;
        }

        public Dictionary<string, string> this[string index]
        {
            get
            {
                if (!Data.ContainsKey(index))
                    AddSection(index);
                return Data[index];
            }
            set { Data[index] = value; }
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
            Data.Clear();
        }

        private static Dictionary<string, string> CreateCaseInsensitiveStringsDictionary()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> GetLinesFrom(string path)
        {
            EnsureFolderExistsFor(path);
            if (!File.Exists(path))
            {
                using (File.Create(path))
                {
                }
            }
            var fileContents = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            var lines = SplitIntoLines(fileContents);
            return lines;
        }

        private static IEnumerable<string> SplitIntoLines(string fileContents)
        {
            return fileContents
                .Split(new[] {"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim());
        }

        private static void EnsureFolderExistsFor(string path)
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        public void AddSection(string section)
        {
            if (!Data.Keys.Contains(section))
            {
                Data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
            foreach (var section in Sections)
            {
                if (section.Length > 0)
                    lines.Add(String.Join("", new[] { "[", section, "]" }));
                lines.AddRange(Data[section].Keys.Select(key => String.Join("", new[] { key.Trim(), "=", "\"", Data[section][key], "\"" })));
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
            Data[section][key] = value;
        }

        public string GetValue(string section, string key, string defaultValue = null)
        {
            if (Data.Keys.All(s => s != section)) return defaultValue;
            if (Data[section].Keys.All(k => k != key)) return defaultValue;
            return Data[section][key];
        }

        public bool HasSection(string section)
        {
            return Data.Keys.Contains(section);
        }

        public bool HasSetting(string section, string key)
        {
            return HasSection(section) && Data[section].Keys.Contains(key);
        }
    }
}
