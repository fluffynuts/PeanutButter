using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// ReSharper disable IntroduceOptionalParameters.Global

// ReSharper disable UnusedMemberInSuper.Global

namespace PeanutButter.INIFile
{
    // ReSharper disable once InconsistentNaming
    public interface IINIFile
    {
        /// <summary>
        /// Exposes the path of the loaded INIFile
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Interface to treat IINIFile like a dictionary of dictionaries
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IDictionary<string, string> this[string index] { get; }

        /// <summary>
        /// List all the currently-available sections
        /// </summary>
        IEnumerable<string> Sections { get; }

        /// <summary>
        /// Attempts to load the file at the given path, discarding any existing config
        /// </summary>
        /// <param name="path"></param>
        void Load(string path);

        /// <summary>
        /// Add a section by name
        /// </summary>
        /// <param name="section"></param>
        void AddSection(string section, params string[] comments);

        IDictionary<string, string> GetSection(string name);

        /// <summary>
        /// Sets a value by section and key
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string section, string key, string value);

        /// <summary>
        /// Get a configured value by section and key, optionally provide a fallback default value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetValue(string section, string key, string defaultValue = null);

        /// <summary>
        /// Test if a section exists by name
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        bool HasSection(string section);

        /// <summary>
        /// Test if a setting exists by section and key
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool HasSetting(string section, string key);

        /// <summary>
        /// Remove a section by name
        /// </summary>
        /// <param name="section"></param>
        void RemoveSection(string section);

        /// <summary>
        /// Merges in the ini config from the provided file path
        /// - ignores paths which don't exist
        /// </summary>
        /// <param name="iniPath">File to merge in to current config</param>
        /// <param name="mergeStrategy">Strategy to pick when merging</param>
        void Merge(string iniPath, MergeStrategies mergeStrategy);

        /// <summary>
        /// Parses the string contents, loading as ini config.    
        /// Will remove any prior config.
        /// </summary>
        /// <param name="contents"></param>
        void Parse(string contents);

        /// <summary>
        /// Persists to the last-loaded file, excluding merged configuration
        /// </summary>
        void Persist();

        /// <summary>
        /// Persists to the last-loaded file with the specified strategy
        /// </summary>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        void Persist(PersistStrategies persistStrategy);

        /// <summary>
        /// Persists to the specified path, excluding merged configuration
        /// </summary>
        /// <param name="saveToPath"></param>
        void Persist(string saveToPath);

        /// <summary>
        /// Persists to the specified path with the specified strategy
        /// </summary>
        /// <param name="saveToPath">File to save to</param>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        void Persist(string saveToPath, PersistStrategies persistStrategy);

        /// <summary>
        /// Persists to the specified stream, excluding merged configuration
        /// <param name="toStream">Stream to persist to</param>
        /// </summary>
        void Persist(Stream toStream);

        /// <summary>
        /// Persists to the specified stream, excluding merged configuration
        /// <param name="toStream">Stream to persist to</param>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        /// </summary>
        void Persist(Stream toStream, PersistStrategies persistStrategy);

        /// <summary>
        /// Reload config (and all merged config) from disk
        /// </summary>
        void Reload();
    }

    public enum MergeStrategies
    {
        AddIfMissing,
        Override
    }

    public enum PersistStrategies
    {
        ExcludeMergedConfigurations,
        IncludeMergedConfigurations
    }

    // ReSharper disable once InconsistentNaming

    public class EmptyEnumerator<T>
        : IEnumerator<T>
    {
        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }

        public T Current => default(T);

        object IEnumerator.Current => Current;
    }

    // ReSharper disable once InconsistentNaming
    public class INIFile : IINIFile
    {
        public IEnumerable<string> Sections => Data.Keys;
        public IEnumerable<string> MergedSections => GetMergedSections();
        public IEnumerable<string> AllSections => Data.Keys.Concat(GetMergedSections()).Distinct();

        private IEnumerable<string> GetMergedSections()
        {
            return _merged.Select(m => m.IniFile.Sections)
                .SelectMany(s => s)
                .Distinct();
        }

        public string Path => _path;

        private string _path;
        private readonly char[] _sectionTrimChars;
        private readonly List<MergedIniFile> _merged = new List<MergedIniFile>();


        // ReSharper disable once MemberCanBePrivate.Global

        protected Dictionary<string, IDictionary<string, string>> Data { get; } =
            CreateCaseInsensitiveDictionary();

        private Dictionary<string, IDictionary<string, string>> Comments { get; } =
            CreateCaseInsensitiveDictionary();

        private const string SECTION_COMMENT_KEY = "="; // bit of a hack: this can never be a key name in a section


        public INIFile(string path = null)
        {
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

        public IDictionary<string, string> this[string index]
        {
            get
            {
                lock (sectionWrappers)
                {
                    if (sectionWrappers.TryGetValue(index, out var wrapper))
                        return wrapper;
                    wrapper = new DictionaryWrappingIniFileSection(this, index);
                    sectionWrappers.Add(index, wrapper);
                    return wrapper;
                }
            }
            set => Data[index] = value;
        }

        private Dictionary<string, IDictionary<string, string>> sectionWrappers
            = new Dictionary<string, IDictionary<string, string>>();

        public IDictionary<string, string> GetSection(string section)
        {
            var result = HasLocalSection(section)
                ? Data[section]
                : _merged.FirstOrDefault(m => m.IniFile.HasSection(section))
                    ?.IniFile[section];
            return result ?? throw new KeyNotFoundException(section);
        }

        private static Dictionary<string, IDictionary<string, string>> CreateCaseInsensitiveDictionary()
        {
            return new Dictionary<string, IDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }

        private void Parse(IEnumerable<string> lines)
        {
            ClearSections();
            var currentSection = string.Empty;
            var recentComments = new List<string>();
            foreach (var line in lines.Where(l => l != null))
            {
                var dataAndComment = SplitCommentFrom(line);
                var dataPart = dataAndComment.Item1;
                if (!string.IsNullOrWhiteSpace(dataAndComment.Item2))
                    recentComments.Add(dataAndComment.Item2);
                if (string.IsNullOrWhiteSpace(dataPart))
                    continue;
                if (IsSectionHeading(dataPart))
                {
                    currentSection = StartSection(dataPart, recentComments);
                    continue;
                }

                StoreSetting(currentSection, dataPart, recentComments);
            }
        }

        private string StartSection(
            string dataPart,
            List<string> recentComments
        )
        {
            var currentSection = GetSectionNameFrom(dataPart);
            AddSection(currentSection);
            StoreCommentsForSection(currentSection, recentComments);
            return currentSection;
        }

        private void StoreSetting(
            string currentSection,
            string dataPart,
            List<string> recentComments
        )
        {
            var parts = dataPart.Split('=');
            var key = parts[0].Trim();
            var value = parts.Count() > 1
                ? string.Join("=", parts.Skip(1))
                : null;
            this[currentSection][key] = TrimOuterQuotesFrom(value);
            StoreCommentsForItem(currentSection, key, recentComments);
        }

        private void StoreCommentsForSection(
            string section,
            IEnumerable<string> recentComments)
        {
            StoreCommentsForItem(section, SECTION_COMMENT_KEY, recentComments);
        }

        private void StoreCommentsForItem(
            string section,
            string key,
            IEnumerable<string> recentComments)
        {
            if (!recentComments.Any())
            {
                return;
            }

            var sectionComments = Comments.ContainsKey(section)
                ? Comments[section]
                : CreateCommentsForSection(section);
            sectionComments[key] = string.Join(Environment.NewLine + ";", recentComments);
            var commentsList = recentComments as List<string>;
            commentsList?.Clear();
        }

        private Dictionary<string, string> CreateCommentsForSection(string section)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
            return line.StartsWith(_sectionTrimChars[0].ToString()) &&
                line.EndsWith(_sectionTrimChars[1].ToString());
        }

        private void ClearSections()
        {
            Data.Clear();
        }

        private static IEnumerable<string> GetLinesFrom(string path)
        {
            EnsureFileExistsAt(path);
            var fileContents = Encoding.UTF8.GetString(File.ReadAllBytes(path));
            var lines = SplitIntoLines(fileContents);
            return lines;
        }

        private static void EnsureFileExistsAt(string path)
        {
            if (File.Exists(path))
                return;
            EnsureFolderExistsFor(path);
            using (File.Create(path))
            {
                /* intentionally left blank */
            }
        }

        private static IEnumerable<string> SplitIntoLines(string fileContents)
        {
            return fileContents
                .Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim());
        }

        private static void EnsureFolderExistsFor(string path)
        {
            var folder = System.IO.Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(
                    folder ?? throw new InvalidOperationException(
                        $"{nameof(EnsureFolderExistsFor)} must be called with a non-null folder name"
                    ));
        }

        public void AddSection(
            string section, 
            params string[] comments)
        {
            if (section == null ||
                HasLocalSection(section))
            {
                StoreCommentsForSection(section, comments);
                return;
            }

            Data[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (comments.Any())
            {
                StoreCommentsForSection(section, comments);
            }
        }

        public void RemoveSection(string section)
        {
            if (section == null)
                return;
            Data.Remove(section);
        }

        public void Merge(string iniPath, MergeStrategies mergeStrategy)
        {
            if (!File.Exists(iniPath))
                return;
            var toMerge = new INIFile(iniPath);
            Merge(toMerge, mergeStrategy);
        }

        public void Merge(IINIFile other, MergeStrategies mergeStrategy)
        {
            _merged.Add(new MergedIniFile(other, mergeStrategy));
        }

        private string TrimOuterQuotesFrom(string value)
        {
            if (value == null || value.Length < 2) return value;
            if (value.StartsWith("\"") && value.EndsWith("\""))
                return value.Substring(1, value.Length - 2);
            return value.Trim();
        }

        public void Persist()
        {
            Persist(null as string, PersistStrategies.ExcludeMergedConfigurations);
        }

        public void Reload()
        {
            if (_path == null)
                return; // TODO: throw? or just ignore like as is?
            Load(_path);
            _merged.ForEach(ini => ini.IniFile.Reload());
        }

        public void Persist(PersistStrategies persistStrategy)
        {
            Persist(null as string, persistStrategy);
        }

        public void Persist(string saveToPath)
        {
            Persist(saveToPath, PersistStrategies.ExcludeMergedConfigurations);
        }

        public void Persist(string saveToPath, PersistStrategies persistStrategy)
        {
            saveToPath = CheckPersistencePath(saveToPath);
            var lines = GetLinesForCurrentData(persistStrategy);
            File.WriteAllBytes(
                saveToPath,
                Encoding.UTF8.GetBytes(
                    string.Join(Environment.NewLine, lines)
                )
            );
        }

        public void Persist(Stream toStream)
        {
            Persist(toStream, PersistStrategies.ExcludeMergedConfigurations);
        }

        public void Persist(Stream toStream, PersistStrategies persistStrategy)
        {
            var lines = GetLinesForCurrentData(persistStrategy);
            var shouldAddNewline = false;
            var newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
            lines.ForEach(
                l =>
                {
                    if (shouldAddNewline)
                        toStream.Write(newLine, 0, newLine.Length);
                    else
                        shouldAddNewline = true;
                    var lineAsBytes = Encoding.UTF8.GetBytes(l);
                    toStream.Write(lineAsBytes, 0, lineAsBytes.Length);
                });
        }

        public override string ToString()
        {
            return string.Join(
                Environment.NewLine,
                GetLinesForCurrentData(PersistStrategies.IncludeMergedConfigurations)
            );
        }

        private List<string> GetLinesForCurrentData(PersistStrategies persistStrategy)
        {
            var sectionFetcher = PersistFetchStrategies[persistStrategy];
            var sections = persistStrategy == PersistStrategies.ExcludeMergedConfigurations
                ? Sections
                : AllSections;
            var lines = new List<string>();
            foreach (var section in sections)
            {
                AddCommentsTo(lines, section, SECTION_COMMENT_KEY);
                if (section.Length > 0)
                    lines.Add(string.Join(string.Empty, "[", section, "]"));
                var dictionary = sectionFetcher(section, this);
                lines.AddRange(
                    dictionary
                        .Keys
                        .Select(key => LineFor(section, key)));
                lines.Add("-----");
            }

            return lines;
        }

        private static Dictionary<PersistStrategies, Func<string, INIFile, IDictionary<string, string>>>
            PersistFetchStrategies =
                new Dictionary<PersistStrategies, Func<string, INIFile, IDictionary<string, string>>>()
                {
                    [PersistStrategies.ExcludeMergedConfigurations] = FetchSectionFromData,
                    [PersistStrategies.IncludeMergedConfigurations] = FetchSectionFromThis
                };

        private static IDictionary<string, string> FetchSectionFromThis(
            string section,
            INIFile ini)
        {
            return ini[section];
        }

        private static IDictionary<string, string> FetchSectionFromData(
            string section,
            INIFile ini)
        {
            return ini.Data[section];
        }


        private void AddCommentsTo(List<string> lines, string forSection, string forKey)
        {
            if (Comments.ContainsKey(forSection) &&
                Comments[forSection].ContainsKey(forKey))
            {
                lines.Add(";" + Comments[forSection][forKey]);
            }
        }


        private string LineFor(string section, string key)
        {
            var lines = new List<string>();
            AddCommentsTo(lines, section, key);
            var dataValue = this[section][key];
            var writeValue = dataValue == null
                ? ""
                : "=\"" + dataValue + "\"";
            lines.Add(string.Join(string.Empty, key.Trim(), writeValue));
            return string.Join(Environment.NewLine, lines);
        }

        private string CheckPersistencePath(string path)
        {
            path = path ?? _path;
            return path ?? throw new ArgumentException(
                "No path specified to persist to and INIFile instantiated without an auto-path",
                nameof(path)
            );
        }

        public void SetValue(
            string section,
            string key,
            string value)
        {
            AddSection(section);
            Data[section][key] = value;
        }

        public string GetValue(
            string section,
            string key,
            string defaultValue = null)
        {
            if (!HasSetting(section, key))
                return defaultValue;
            var start = HasLocalKey(Data, section, key)
                ? Data[section][key]
                : null;
            return GetMergedValue(section, key, defaultValue, start);
        }

        private bool HasLocalKey(
            IDictionary<string, IDictionary<string, string>> data,
            string section,
            string key)
        {
            return HasKey(data, section) &&
                HasKey(data[section], key);
        }

        private string GetMergedValue(
            string section,
            string key,
            string defaultValue,
            string startingValue)
        {
            return _merged.Aggregate(
                startingValue,
                (acc, cur) =>
                {
                    var mergeValue = cur.IniFile.GetValue(section, key);
                    if (mergeValue != null &&
                        cur.MergeStrategy == MergeStrategies.Override)
                        return mergeValue;
                    return acc ?? mergeValue;
                }
            ) ?? defaultValue;
        }

        public bool HasSection(string section)
        {
            return HasLocalSection(section) ||
                HaveMergedSection(section);
        }

        private bool HasLocalSection(string section)
        {
            return section != null &&
                Data.Keys.Contains(section, StringComparer.OrdinalIgnoreCase);
        }

        private bool HaveMergedSection(string section)
        {
            return _merged.Aggregate(
                false,
                (acc, cur) => acc || cur.IniFile.HasSection(section));
        }

        public bool HasSetting(string section, string key)
        {
            return key != null &&
                (HasLocalSection(section) &&
                    HasKey(Data[section], key) ||
                    HasMergedSetting(section, key));
        }

        private bool HasMergedSetting(string section, string key)
        {
            return _merged.Aggregate(
                false,
                (acc, cur) => acc || cur.IniFile.HasSetting(section, key));
        }

        private bool HasKey<T>(IDictionary<string, T> dict, string key)
        {
            return dict.Keys.Contains(key, StringComparer.OrdinalIgnoreCase);
        }
    }

    internal class MergedIniFile
    {
        public IINIFile IniFile { get; }
        public MergeStrategies MergeStrategy { get; }

        internal MergedIniFile(
            IINIFile iniFile,
            MergeStrategies mergeStrategy)
        {
            IniFile = iniFile;
            MergeStrategy = mergeStrategy;
        }
    }
}