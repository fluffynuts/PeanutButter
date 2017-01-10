using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// ReSharper disable UnusedMemberInSuper.Global

namespace PeanutButter.INIFile
{
    // ReSharper disable once InconsistentNaming
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

    // ReSharper disable once InconsistentNaming
    public class INIFile : IINIFile
    {
        public IEnumerable<string> Sections => Data.Keys;

        private string _path;
        private readonly char[] _sectionTrimChars;
        // ReSharper disable once MemberCanBePrivate.Global
        protected Dictionary<string, Dictionary<string, string>> Data { get; } = CreateCaseInsensitiveDictionary();
        private Dictionary<string, Dictionary<string, string>> Comments { get; } = CreateCaseInsensitiveDictionary();

        private const string SECTION_COMMENT_KEY = "="; // bit of a hack: this can never be a key name in a section

        public INIFile(string path = null)
        {
            _sectionTrimChars = new[] {'[', ']'};
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

        private static Dictionary<string, Dictionary<string, string>> CreateCaseInsensitiveDictionary()
        {
            return new Dictionary<string, Dictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);
        }

        private void Parse(IEnumerable<string> lines)
        {
            ClearSections();
            var currentSection = string.Empty;
            var recentComments = new List<string>();
            foreach (var line in lines.Where(l => !string.IsNullOrEmpty(l)))
            {
                var dataAndComment = SplitCommentFrom(line);
                var dataPart = dataAndComment.Item1;
                if (!string.IsNullOrWhiteSpace(dataAndComment.Item2))
                    recentComments.Add(dataAndComment.Item2);
                if (string.IsNullOrWhiteSpace(dataPart))
                    continue;
                if (IsSectionHeading(dataPart))
                {
                    currentSection = GetSectionNameFrom(dataPart);
                    AddSection(currentSection);
                    StoreCommentsForSection(currentSection, recentComments);
                    continue;
                }
                var parts = dataPart.Split('=');
                var key = parts[0].Trim();
                var value = parts.Count() > 1 ? string.Join("=", parts.Skip(1)) : null;
                this[currentSection][key] = TrimOuterQuotesFrom(value);
                StoreCommentsForItem(currentSection, key, recentComments);
            }
        }

        private void StoreCommentsForSection(string section, List<string> recentComments)
        {
            StoreCommentsForItem(section, SECTION_COMMENT_KEY, recentComments);
        }

        private void StoreCommentsForItem(string section, string key, List<string> recentComments)
        {
            if (!recentComments.Any()) return;
            var sectionComments = Comments.ContainsKey(section)
                ? Comments[section]
                : CreateCommentsForSection(section);
            sectionComments[key] = string.Join(Environment.NewLine + ";", recentComments);
            recentComments.Clear();
        }

        private Dictionary<string, string> CreateCommentsForSection(string section)
        {
            var result = new Dictionary<string, string>();
            Comments[section] = result;
            return result;
        }

        private Tuple<string, string> SplitCommentFrom(string line)
        {
            var parts = line.Split(';');
            var toTake = 1;
            while (toTake <= parts.Length && HaveUnmatchedQuotesIn(parts.Take(toTake)))
                toTake++;
            var dataPart = string.Join(";", parts.Take(toTake)).Trim();
            var commentPart = string.Join(";", parts.Skip(toTake));
            return Tuple.Create(dataPart, commentPart);
        }

        private bool HaveUnmatchedQuotesIn(IEnumerable<string> parts)
        {
            var joined = string.Join(";", parts);
            var quoted = joined.Count(c => c == '"');
            return quoted % 2 != 0;
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
                    /* intentionally left blank */
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
            if (value == null || value.Length < 2) return value;
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);
            return value.Trim();
        }

        public void Persist(string path = null)
        {
            path = CheckPersistencePath(path);
            var lines = GetLinesForCurrentData();
            File.WriteAllBytes(path, Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)));
        }

        public void Persist(Stream toStream)
        {
            var lines = GetLinesForCurrentData();
            var shouldAddNewline = false;
            var newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
            lines.ForEach(l =>
            {
                if (shouldAddNewline)
                    toStream.Write(newLine, 0, newLine.Length);
                else
                    shouldAddNewline = true;
                var lineAsBytes = Encoding.UTF8.GetBytes(l);
                toStream.Write(lineAsBytes, 0, lineAsBytes.Length);
            });
        }

        private List<string> GetLinesForCurrentData()
        {
            var lines = new List<string>();
            foreach (var section in Sections)
            {
                AddCommentsTo(lines, section, SECTION_COMMENT_KEY);
                if (section.Length > 0)
                    lines.Add(string.Join(string.Empty, "[", section, "]"));
                lines.AddRange(Data[section]
                    .Keys
                    .Select(key => LineFor(section, key)));
            }
            return lines;
        }

        private void AddCommentsTo(List<string> lines, string forSection, string forKey)
        {
            if (Comments.ContainsKey(forSection) && Comments[forSection].ContainsKey(forKey))
                lines.Add(";" + Comments[forSection][forKey]);
        }


        private string LineFor(string section, string key)
        {
            var lines = new List<string>();
            AddCommentsTo(lines, section, key);
            var dataValue = Data[section][key];
            var writeValue = dataValue == null ? "" : "=\"" + dataValue + "\"";
            lines.Add(string.Join(string.Empty, key.Trim(), writeValue));
            return string.Join(Environment.NewLine, lines);
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