using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable UnusedMemberInSuper.Global
namespace PeanutButter.INI
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Contract provided by the PeanutButter INI parser
    /// </summary>
    public interface IINIFile
    {
        /// <summary>
        /// Separate sections with any string you like
        /// - defaults to empty, which inserts a new line
        /// - set to null for no separator at all
        /// </summary>
        string SectionSeparator { get; set; }

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
        /// Whether or not to handle escape characters in ini values
        /// When enabled (default), then the following sequences in values
        /// are supported:
        /// \\ -> backslash
        /// \" -> quote
        /// </summary>
        ParseStrategies ParseStrategy { get; set; }

        /// <summary>
        /// Provides an enumeration over all section names: whether
        /// from merging or the initial load
        /// </summary>
        IEnumerable<string> AllSections { get; }

        /// <summary>
        /// Provides an enumeration over all merged section names
        /// </summary>
        IEnumerable<string> MergedSections { get; }

        /// <summary>
        /// Provide a custom line parser if you like
        /// - make sure to set ParseStrategies.Custom
        /// </summary>
        ILineParser CustomLineParser { get; set; }

        /// <summary>
        /// Attempts to load the file at the given path, discarding any existing config
        /// </summary>
        /// <param name="path"></param>
        void Load(string path);

        /// <summary>
        /// Attempts to load the file at the given path, discarding any existing config
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parseStrategy"></param>
        void Load(string path, ParseStrategies parseStrategy);

        /// <summary>
        /// Add a section by name
        /// </summary>
        /// <param name="section">Name of the section to add</param>
        /// <param name="comments">(Optional) comments for the section</param>
        void AddSection(string section, params string[] comments);

        /// <summary>
        /// Retrieve the collection of settings for a section by section name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
        /// Rename a section
        /// </summary>
        /// <param name="existingName">Name of the existing section</param>
        /// <param name="newName">New name of the section that is to be renamed</param>
        void RenameSection(string existingName, string newName);


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

        /// <summary>
        /// Merges another ini file into this one
        /// </summary>
        /// <param name="other">other ini file</param>
        /// <param name="mergeStrategy">strategy to use when encountering conflicts</param>
        void Merge(IINIFile other, MergeStrategies mergeStrategy);
    }

    /// <summary>
    /// Strategies which may be employed when merging INI data
    /// </summary>
    public enum MergeStrategies
    {
        /// <summary>
        /// Only add merged-in data when it's not already found
        /// </summary>
        OnlyAddIfMissing,

        /// <summary>
        /// Override existing data with subsequent merges
        /// </summary>
        Override
    }

    /// <summary>
    /// Strategies which may be employed when persisting INI files
    /// </summary>
    public enum PersistStrategies
    {
        /// <summary>
        /// Exclude merged configurations when persisting
        /// </summary>
        ExcludeMergedConfigurations,

        /// <summary>
        /// Include merged configurations when persisting
        /// </summary>
        IncludeMergedConfigurations
    }

    /// <summary>
    /// Strategies which may be employed for parsing INI data
    /// </summary>
    public enum ParseStrategies
    {
        /// <summary>
        /// Use the Best Effort line parser which may give unpredicted
        /// results, especially if you have inline comments with quotes in them
        /// </summary>
        BestEffort,

        /// <summary>
        /// Use the Strict line parser which expects that backslashes
        /// and quotes within values are _always_ escaped by another backslash,
        /// eg: key="value \"in quotes\" \\slash\\"
        /// </summary>
        Strict,
        
        /// <summary>
        /// Use the Plain line parser which expects no escaped characters
        /// and all comments to start on a new line 
        /// (no comments on same line as entry or section header)
        /// </summary>
        Plain,

        /// <summary>
        /// You must provide your own implementation of ILineParser
        /// </summary>
        Custom
    }

    // ReSharper disable once InconsistentNaming
    /// <inheritdoc />
    public class INIFile : IINIFile
    {
        /// <inheritdoc />
        public ParseStrategies ParseStrategy { get; set; } = ParseStrategies.BestEffort;

        /// <inheritdoc />
        public ILineParser CustomLineParser { get; set; }

        /// <inheritdoc />
        public string SectionSeparator { get; set; } = "";

        /// <inheritdoc />
        public IEnumerable<string> Sections => Data.Keys;

        /// <inheritdoc />
        public IEnumerable<string> MergedSections => GetMergedSections();

        /// <inheritdoc />
        public IEnumerable<string> AllSections => Data.Keys.Concat(GetMergedSections()).Distinct();

        private IEnumerable<string> GetMergedSections()
        {
            return _merged.Select(m => m.IniFile.Sections)
                .SelectMany(s => s)
                .Distinct();
        }

        /// <inheritdoc />
        public string Path => _path;

        private string _path;
        private readonly char[] _sectionTrimChars = { '[', ']' };
        private readonly List<MergedIniFile> _merged = new List<MergedIniFile>();

        // ReSharper disable once MemberCanBePrivate.Global

        /// <summary>
        /// Data storage for the current INI data
        /// </summary>
        protected Dictionary<string, IDictionary<string, string>> Data { get; } =
            CreateCaseInsensitiveDictionary();

        private Dictionary<string, IDictionary<string, string>> Comments { get; } =
            CreateCaseInsensitiveDictionary();

        private const string SECTION_COMMENT_KEY = "="; // bit of a hack: this can never be a key name in a section


        /// <summary>
        /// Constructs an instance of INIFile without parsing any files,
        /// defaulting to best effort parser
        /// </summary>
        public INIFile() : this(null, ParseStrategies.BestEffort)
        {
        }

        /// <summary>
        /// Constructs an instance of INIFile, parsing the provided
        /// path if found, with the best-effort parser
        /// </summary>
        /// <param name="path">Path to an existing ini file.
        /// Will not error if not found, but will be used as the default path
        /// to persist to.</param>
        // ReSharper disable once MemberCanBePrivate.Global
        public INIFile(string path) : this(path, ParseStrategies.BestEffort)
        {
        }

        /// <summary>
        /// Constructs an instance of INIFile with a custom line parser
        /// </summary>
        /// <param name="lineParser"></param>
        public INIFile(ILineParser lineParser)
            : this(null, lineParser)
        {
        }

        /// <summary>
        /// Constructs an instance of INIFile with a custom line parser
        /// </summary>
        /// <param name="path"></param>
        /// <param name="customLineParser"></param>
        public INIFile(
            string path,
            ILineParser customLineParser)
        {
            CustomLineParser = customLineParser;
            Init(path, ParseStrategies.Custom);
        }

        /// <summary>
        /// Constructs an instance of INIFile, parsing the provided
        /// path if found, with escaped characters enabled if
        /// enableEscapeCharacters is true
        /// </summary>
        /// <param name="path">Path to an existing ini file.
        /// Will not error if not found, but will be used as the default path
        /// to persist to.</param>
        /// <param name="parseStrategy"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public INIFile(
            string path,
            ParseStrategies parseStrategy)
        {
            Init(path, parseStrategy);
        }

        private void Init(string path, ParseStrategies parseStrategy)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Load(path, parseStrategy);
            }
        }

        /// <inheritdoc />
        public void Load(string path)
        {
            Load(path, ParseStrategies.BestEffort);
        }

        /// <inheritdoc />
        public void Load(
            string path,
            ParseStrategies parseStrategy)
        {
            ParseStrategy = parseStrategy;
            _path = path;
            var lines = GetLinesFrom(path);
            Parse(lines);
        }

        /// <inheritdoc />
        public void Parse(string contents)
        {
            Parse(SplitIntoLines(contents));
        }

        /// <inheritdoc />
        public IDictionary<string, string> this[string index]
        {
            get
            {
                lock (_sectionWrappers)
                {
                    if (_sectionWrappers.TryGetValue(index, out var wrapper))
                    {
                        return wrapper;
                    }

                    wrapper = new DictionaryWrappingIniFileSection(this, index);
                    _sectionWrappers.Add(index, wrapper);
                    return wrapper;
                }
            }
            set => Data[index] = value;
        }

        private readonly Dictionary<string, IDictionary<string, string>> _sectionWrappers
            = new Dictionary<string, IDictionary<string, string>>();

        /// <inheritdoc />
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
            return new Dictionary<string, IDictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase
            );
        }

        private static readonly Dictionary<ParseStrategies, ILineParser>
            Parsers = new Dictionary<ParseStrategies, ILineParser>()
            {
                [ParseStrategies.BestEffort] = new BestEffortLineParser(),
                [ParseStrategies.Strict] = new StrictLineParser(),
                [ParseStrategies.Plain] = new PlainLineParser(),                
            };

        private void Parse(IEnumerable<string> lines)
        {
            ClearSections();
            var currentSection = string.Empty;
            var recentComments = new List<string>();
            var lineParser = SelectLineParser();
            _wasEscaped.Clear();

            foreach (var line in lines.Where(l => l != null))
            {
                var parsedLine = lineParser.Parse(line);

                if (!string.IsNullOrWhiteSpace(parsedLine.Comment))
                {
                    recentComments.Add(parsedLine.Comment);
                }

                if (string.IsNullOrWhiteSpace(parsedLine.Key) &&
                    string.IsNullOrWhiteSpace(parsedLine.Value))
                {
                    continue;
                }

                if (IsSectionHeading(parsedLine.Key))
                {
                    currentSection = StartSection(parsedLine.Key, recentComments);
                    continue;
                }

                StoreWasEscaped(currentSection, parsedLine);
                this[currentSection][parsedLine.Key] = parsedLine.Value;
                StoreCommentsForItem(currentSection, parsedLine.Key, recentComments);
            }
        }

        private ILineParser SelectLineParser()
        {
            if (ParseStrategy == ParseStrategies.Custom)
            {
                return CustomLineParser
                    ?? throw new InvalidOperationException(
                        $"A custom implementation of {nameof(ILineParser)} must be provided if the ParseStrategy is set to {nameof(ParseStrategies.Custom)}"
                    );
            }

            if (!Parsers.TryGetValue(ParseStrategy, out var lineParser))
            {
                throw new NotSupportedException(
                    $"Parse strategy {ParseStrategy} is not supported"
                );
            }

            return lineParser;
        }

        private readonly HashSet<string> _wasEscaped = new HashSet<string>();

        private void StoreWasEscaped(string section, IParsedLine parsedLine)
        {
            if (!parsedLine.ContainedEscapedEntities)
            {
                return;
            }

            _wasEscaped.Add($"{section}.{parsedLine.Key}");
        }

        private string StartSection(
            string dataPart,
            IList<string> recentComments
        )
        {
            var currentSection = GetSectionNameFrom(dataPart);
            AddSection(currentSection);
            StoreCommentsForSection(currentSection, recentComments);
            return currentSection;
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
            // ReSharper disable once PossibleMultipleEnumeration
            if (!recentComments.Any())
            {
                return;
            }

            var sectionComments = Comments.ContainsKey(section)
                ? Comments[section]
                : CreateCommentsForSection(section);
            // ReSharper disable once PossibleMultipleEnumeration
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

        private string GetSectionNameFrom(string line)
        {
            return line.Trim(_sectionTrimChars);
        }

        private bool IsSectionHeading(string line)
        {
            return (line is not null) &&
                line.StartsWith(_sectionTrimChars[0].ToString()) &&
                line.EndsWith(_sectionTrimChars[1].ToString());
        }

        private void ClearSections()
        {
            Data.Clear();
        }

        private static IEnumerable<string> GetLinesFrom(string path)
        {
            EnsureFileExistsAt(path);
            var fileContents = Encoding.UTF8.GetString(
                File.ReadAllBytes(path)
            );
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
            {
                Directory.CreateDirectory(
                    folder ?? throw new InvalidOperationException(
                        $"{nameof(EnsureFolderExistsFor)} must be called with a non-null folder name"
                    ));
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void RemoveSection(string section)
        {
            if (section is null)
            {
                return;
            }

            Data.Remove(section);
        }

        /// <inheritdoc />
        public void RenameSection(string existingName, string newName)
        {
            if (existingName is null || newName is null)
            {
                return;
            }

            if (existingName == newName)
            {
                return;
            }

            Data[newName] = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase
            );

            var oldSection = Data[existingName];
            foreach (var keyValuePair in oldSection)
            {
                Data[newName].Add(keyValuePair.Key, keyValuePair.Value);
            }

            Data.Remove(existingName);
        }

        /// <inheritdoc />
        public void Merge(string iniPath, MergeStrategies mergeStrategy)
        {
            if (!File.Exists(iniPath))
            {
                return;
            }

            var toMerge = new INIFile(iniPath);
            Merge(toMerge, mergeStrategy);
        }

        /// <inheritdoc />
        public void Merge(IINIFile other, MergeStrategies mergeStrategy)
        {
            _merged.Add(
                new MergedIniFile(
                    other,
                    mergeStrategy
                )
            );
        }

        /// <inheritdoc />
        public void Persist()
        {
            Persist(
                null as string,
                PersistStrategies.ExcludeMergedConfigurations
            );
        }

        /// <inheritdoc />
        public void Reload()
        {
            if (_path is null)
            {
                return; // TODO: throw? or just ignore like as is?
            }

            Load(_path);
            _merged.ForEach(ini => ini.IniFile.Reload());
        }

        /// <inheritdoc />
        public void Persist(PersistStrategies persistStrategy)
        {
            Persist(
                null as string,
                persistStrategy
            );
        }

        /// <inheritdoc />
        public void Persist(string saveToPath)
        {
            Persist(
                saveToPath,
                PersistStrategies.ExcludeMergedConfigurations
            );
        }

        /// <inheritdoc />
        public void Persist(
            string saveToPath,
            PersistStrategies persistStrategy
        )
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

        /// <inheritdoc />
        public void Persist(Stream toStream)
        {
            Persist(
                toStream,
                PersistStrategies.ExcludeMergedConfigurations
            );
        }

        /// <inheritdoc />
        public void Persist(Stream toStream, PersistStrategies persistStrategy)
        {
            var lines = GetLinesForCurrentData(persistStrategy);
            var shouldAddNewline = false;
            var newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
            lines.ForEach(
                line =>
                {
                    if (shouldAddNewline)
                    {
                        toStream.Write(newLine, 0, newLine.Length);
                    }
                    else
                    {
                        shouldAddNewline = true;
                    }

                    var lineAsBytes = Encoding.UTF8.GetBytes(line);
                    toStream.Write(lineAsBytes, 0, lineAsBytes.Length);
                });
        }

        /// <inheritdoc />
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
            var addSeparator = SectionSeparator is not null;
            var sep = CommentIfNecessary(SectionSeparator);
            var lines = new List<string>();
            foreach (var section in sections)
            {
                AddCommentsTo(lines, section, SECTION_COMMENT_KEY);
                if (section.Length > 0)
                {
                    lines.Add(string.Join(string.Empty, "[", section, "]"));
                }

                var dictionary = sectionFetcher(section, this);
                lines.AddRange(
                    dictionary
                        .Keys
                        .Select(key => LineFor(section, key))
                );
                if (addSeparator)
                {
                    lines.Add(sep);
                }
            }

            if (addSeparator)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            return lines;
        }

        private string CommentIfNecessary(string sectionSeparator)
        {
            if (string.IsNullOrWhiteSpace(sectionSeparator))
            {
                return sectionSeparator;
            }

            return string.Join("\n", sectionSeparator.Split('\n')
                .Select(line =>
                    line.StartsWith(";")
                        ? line
                        : $";{line}"
                )
            );
        }

        private static readonly Dictionary<PersistStrategies, Func<string, INIFile, IDictionary<string, string>>>
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


        private void AddCommentsTo(
            List<string> lines,
            string forSection,
            string forKey)
        {
            if (Comments.ContainsKey(forSection) &&
                Comments[forSection].ContainsKey(forKey))
            {
                lines.Add(";" + Comments[forSection][forKey]);
            }
        }

        private string LineFor(
            string section,
            string key)
        {
            var lines = new List<string>();
            AddCommentsTo(lines, section, key);
            var dataValue = this[section][key];
            var writeValue = dataValue == null
                ? ""
                : $"=\"{EscapeEntities(dataValue, section, key)}\"";
            lines.Add(string.Join(string.Empty, key.Trim(), writeValue));
            return string.Join(Environment.NewLine, lines);
        }

        private string EscapeEntities(string data, string section, string key)
        {
            var shouldEscape = ParseStrategy == ParseStrategies.Strict ||
                _wasEscaped.Contains($"{section}.{key}");
            return shouldEscape
                ? data
                    ?.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                : data;
        }

        private string CheckPersistencePath(string path)
        {
            path = path ?? _path;
            return path ?? throw new ArgumentException(
                "No path specified to persist to and INIFile instantiated without an auto-path",
                nameof(path)
            );
        }

        /// <inheritdoc />
        public void SetValue(
            string section,
            string key,
            string value)
        {
            AddSection(section);
            Data[section][key] = value;
        }

        /// <inheritdoc />
        public string GetValue(
            string section,
            string key,
            string defaultValue = null)
        {
            if (!HasSetting(section, key))
            {
                return defaultValue;
            }

            var start = HasLocalKey(Data, section, key)
                ? Data[section][key]
                : null;
            return GetMergedValue(
                section,
                key,
                defaultValue,
                start
            );
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
                    if (mergeValue is not null &&
                        cur.MergeStrategy == MergeStrategies.Override)
                    {
                        return mergeValue;
                    }

                    return acc ?? mergeValue;
                }
            ) ?? defaultValue;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public bool HasSetting(string section, string key)
        {
            return key is not null &&
                (HasLocalSection(section) &&
                    HasKey(Data[section], key) ||
                    HasMergedSetting(section, key));
        }

        private bool HasMergedSetting(string section, string key)
        {
            return _merged.Aggregate(
                false,
                (acc, cur) => acc || cur.IniFile.HasSetting(section, key)
            );
        }

        private bool HasKey<T>(IDictionary<string, T> dict, string key)
        {
            return dict.Keys.Contains(
                key,
                StringComparer.OrdinalIgnoreCase
            );
        }
    }
}
