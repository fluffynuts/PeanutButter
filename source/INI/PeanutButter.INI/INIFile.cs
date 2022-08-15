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
        /// The default encoding to use when persisting
        /// files. You may specify an encoding at persistence
        /// time too.
        /// </summary>
        Encoding DefaultEncoding { get; set; }

        /// <summary>
        /// Separate sections with any string you like
        /// - defaults to empty, which inserts a new line
        /// - set to null for no separator at all
        /// </summary>
        string SectionSeparator { get; set; }

        /// <summary>
        /// Toggle whether an empty line is added at the bottom when persisted
        /// </summary>
        public bool AddEmptyLineAtTheBottom { get; set; }

        /// <summary>
        /// Toggle whether key values are wrapped in quote marks when persisted
        /// </summary>
        bool WrapValueInQuotes { get; set; }

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
        /// <param name="encoding"></param>
        void Load(string path, Encoding encoding);

        /// <summary>
        /// Attempts to load the file at the given path, discarding any existing config
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parseStrategy"></param>
        void Load(string path, ParseStrategies parseStrategy);

        /// <summary>
        /// Attempts to load the file at the given path, discarding any existing config
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parseStrategy"></param>
        /// <param name="encoding"></param>
        void Load(string path, ParseStrategies parseStrategy, Encoding encoding);

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
        /// Persists to the last-loaded file, excluding merged configuration,
        /// with the default encoding
        /// </summary>
        void Persist();

        /// <summary>
        /// Persists to the last-loaded file with the provided encoding
        /// </summary>
        /// <param name="encoding"></param>
        void Persist(Encoding encoding);

        /// <summary>
        /// Persists to the last-loaded file with the specified merge strategy
        /// and default encoding
        /// </summary>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        void Persist(PersistStrategies persistStrategy);

        /// <summary>
        /// Persists to the last-loaded file with the specified merge strategy and
        /// encoding
        /// </summary>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        /// <param name="encoding">The encoding to use for the written file</param>
        void Persist(PersistStrategies persistStrategy, Encoding encoding);

        /// <summary>
        /// Persists to the specified path, excluding merged configuration,
        /// with the default encoding
        /// </summary>
        /// <param name="saveToPath"></param>
        void Persist(string saveToPath);

        /// <summary>
        /// Persists to the specified path, excluding merged configuration,
        /// with the specified encoding
        /// </summary>
        /// <param name="saveToPath"></param>
        /// <param name="encoding">Encoding to use</param>
        void Persist(string saveToPath, Encoding encoding);

        /// <summary>
        /// Persists to the specified path with the specified strategy and default
        /// encoding
        /// </summary>
        /// <param name="saveToPath">File to save to</param>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        void Persist(string saveToPath, PersistStrategies persistStrategy);

        /// <summary>
        /// Persists to the specified path with the specified strategy and the specified
        /// encoding
        /// </summary>
        /// <param name="saveToPath">File to save to</param>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        /// <param name="encoding">Encoding to use when writing the file</param>
        void Persist(string saveToPath, PersistStrategies persistStrategy, Encoding encoding);

        /// <summary>
        /// Persists to the specified stream, excluding merged configuration,
        /// with the default encoding
        /// <param name="toStream">Stream to persist to</param>
        /// </summary>
        void Persist(Stream toStream);

        /// <summary>
        /// Persists to the specified stream, excluding merged configuration,
        /// with the provided encoding
        /// <param name="toStream">Stream to persist to</param>
        /// <param name="encoding">Encoding to use</param>
        /// </summary>
        void Persist(Stream toStream, Encoding encoding);

        /// <summary>
        /// Persists to the specified stream, excluding merged configuration
        /// <param name="toStream">Stream to persist to</param>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        /// </summary>
        void Persist(Stream toStream, PersistStrategies persistStrategy);

        /// <summary>
        /// Persists to the specified stream, using the provided persistence strategy
        /// and the provided encoding
        /// <param name="toStream">Stream to persist to</param>
        /// <param name="persistStrategy">Strategy to employ for merged config</param>
        /// <param name="encoding">Encoding to use</param>
        /// </summary>
        void Persist(Stream toStream, PersistStrategies persistStrategy, Encoding encoding);

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

        /// <summary>
        /// Removes a value from the section
        /// - will not remove empty sections
        /// - this is functionally equivalent to ini["section"].Remove("key")
        /// </summary>
        /// <param name="section">Section to remove the setting from</param>
        /// <param name="key">Setting to remove</param>
        /// <returns></returns>
        bool RemoveValue(
            string section,
            string key
        );

        /// <summary>
        /// Removes a value from the section
        /// - optionally removes empty sections
        /// </summary>
        /// <param name="section">Section to remove the setting from</param>
        /// <param name="key">Setting to remove</param>
        /// <param name="removeEmptySection">Whether or not to clear out empty sections</param>
        /// <returns></returns>
        bool RemoveValue(
            string section,
            string key,
            bool removeEmptySection
        );
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
        /// You must provide your own implementation of ILineParser
        /// </summary>
        Custom
    }

    // ReSharper disable once InconsistentNaming
    /// <inheritdoc />
    public class INIFile : IINIFile
    {
        /// <summary>
        /// Loads INI data from a string
        /// </summary>
        /// <param name="ini"></param>
        /// <returns></returns>
        public static INIFile FromString(string ini)
        {
            var result = new INIFile();
            result.Parse(ini);
            return result;
        }

        /// <inheritdoc />
        public ParseStrategies ParseStrategy { get; set; } = ParseStrategies.BestEffort;

        /// <inheritdoc />
        public ILineParser CustomLineParser { get; set; }

        /// <inheritdoc />
        public string SectionSeparator { get; set; } = "";

        /// <inheritdoc />
        public bool AddEmptyLineAtTheBottom { get; set; } = false;

        /// <inheritdoc />
        public bool WrapValueInQuotes { get; set; } = true;

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
        private readonly char[] _sectionTrimChars = {'[', ']'};
        private readonly List<MergedIniFile> _merged = new();

        // ReSharper disable once MemberCanBePrivate.Global

        /// <summary>
        /// Data storage for the current INI data
        /// </summary>
        protected Dictionary<string, IDictionary<string, string>> Data { get; } =
            CreateCaseInsensitiveDictionary();

        private Dictionary<string, IDictionary<string, string>> Comments { get; } =
            CreateCaseInsensitiveDictionary();

        private const string SECTION_COMMENT_KEY = "="; // bit of a hack: this can never be a key name in a section

        /// <inheritdoc />
        public Encoding DefaultEncoding
        {
            get => _encoding ?? Encoding.UTF8;
            set => _encoding = value;
        }

        private Encoding _encoding;


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
        // ReSharper disable once MemberCanBePrivate.Global
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
            ParseStrategy = parseStrategy;
            if (!string.IsNullOrWhiteSpace(path))
            {
                Load(path, parseStrategy);
            }
        }

        /// <inheritdoc />
        public void Load(string path)
        {
            Load(path, ParseStrategy);
        }

        /// <inheritdoc />
        public void Load(string path, Encoding encoding)
        {
            Load(path, ParseStrategy, encoding);
        }

        /// <inheritdoc />
        public void Load(
            string path,
            ParseStrategies parseStrategy)
        {
            Load(path, parseStrategy, DefaultEncoding);
        }

        /// <inheritdoc />
        public void Load(string path, ParseStrategies parseStrategy, Encoding encoding)
        {
            ParseStrategy = parseStrategy;
            _path = path;
            var lines = GetLinesFrom(path, encoding);
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

        private readonly Dictionary<string, IDictionary<string, string>> _sectionWrappers = new();

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
                DefaultStringComparer
            );
        }

        private static readonly Dictionary<ParseStrategies, ILineParser>
            Parsers = new()
            {
                [ParseStrategies.BestEffort] = new BestEffortLineParser(),
                [ParseStrategies.Strict] = new StrictLineParser()
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
                    currentSection = StartSection(
                        parsedLine.Key,
                        recentComments,
                        lineParser.CommentDelimiter
                    );
                    continue;
                }

                StoreWasEscaped(currentSection, parsedLine);
                this[currentSection][parsedLine.Key] = parsedLine.Value;
                StoreCommentsForItem(
                    currentSection,
                    parsedLine.Key,
                    recentComments,
                    lineParser.CommentDelimiter
                );
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
            IList<string> recentComments,
            string commentDelimiter
        )
        {
            var currentSection = GetSectionNameFrom(dataPart);
            AddSection(currentSection);
            StoreCommentsForSection(
                currentSection,
                recentComments,
                commentDelimiter
            );
            return currentSection;
        }

        private void StoreCommentsForSection(
            string section,
            IEnumerable<string> recentComments,
            string commentDelimiter
        )
        {
            StoreCommentsForItem(
                section,
                SECTION_COMMENT_KEY,
                recentComments,
                commentDelimiter
            );
        }

        private void StoreCommentsForItem(
            string section,
            string key,
            IEnumerable<string> recentComments,
            string commentDelimiter
        )
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
            sectionComments[key] = string.Join(Environment.NewLine + commentDelimiter, recentComments);
            var commentsList = recentComments as List<string>;
            commentsList?.Clear();
        }

        private Dictionary<string, string> CreateCommentsForSection(string section)
        {
            var result = new Dictionary<string, string>(DefaultStringComparer);
            Comments[section] = result;
            return result;
        }

        private static readonly StringComparer DefaultStringComparer
            = StringComparer.InvariantCultureIgnoreCase;

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

        private IEnumerable<string> GetLinesFrom(string path, Encoding encoding)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            EnsureFileExistsAt(path);
            var fileBytes = File.ReadAllBytes(path);
            fileBytes = StripByteOrderMark(fileBytes);

            var fileContents = encoding.GetString(
                fileBytes
            );
            var lines = SplitIntoLines(fileContents);
            return lines;
        }

        private byte[] StripByteOrderMark(byte[] fileBytes)
        {
            var preamble = DefaultEncoding.GetPreamble();
            return fileBytes.Take(preamble.Length).SequenceEqual(preamble)
                ? fileBytes.Skip(preamble.Length).ToArray()
                : fileBytes;
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
                .Split(new[] {"\n", "\r"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim());
        }

        private static void EnsureFolderExistsFor(string path)
        {
            var folder = System.IO.Path.GetDirectoryName(path);
            if (folder == "")
            {
                // CWD
                return;
            }

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
            var lineParser = SelectLineParser();
            if (section == null ||
                HasLocalSection(section))
            {
                StoreCommentsForSection(section, comments, lineParser.CommentDelimiter);
                return;
            }

            Data[section] = new Dictionary<string, string>(DefaultStringComparer);
            if (comments.Any())
            {
                StoreCommentsForSection(section, comments, lineParser.CommentDelimiter);
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
                DefaultStringComparer
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
        public void Reload()
        {
            if (_path is null)
            {
                // Reload is a no-op if there was no original file
                return;
            }

            Load(_path);
            _merged.ForEach(ini => ini.IniFile.Reload());
        }

        /// <inheritdoc />
        public void Persist()
        {
            Persist(DefaultEncoding);
        }

        /// <inheritdoc />
        public void Persist(Encoding encoding)
        {
            Persist(
                null as string,
                PersistStrategies.ExcludeMergedConfigurations,
                encoding
            );
        }

        /// <inheritdoc />
        public void Persist(PersistStrategies persistStrategy)
        {
            Persist(persistStrategy, DefaultEncoding);
        }

        /// <inheritdoc />
        public void Persist(PersistStrategies persistStrategy, Encoding encoding)
        {
            Persist(
                null as string,
                persistStrategy,
                encoding
            );
        }

        /// <inheritdoc />
        public void Persist(string saveToPath)
        {
            Persist(saveToPath, DefaultEncoding);
        }

        /// <inheritdoc />
        public void Persist(string saveToPath, Encoding encoding)
        {
            Persist(
                saveToPath,
                PersistStrategies.ExcludeMergedConfigurations,
                encoding
            );
        }

        /// <inheritdoc />
        public void Persist(
            string saveToPath,
            PersistStrategies persistStrategy
        )
        {
            Persist(saveToPath, persistStrategy, DefaultEncoding);
        }

        /// <inheritdoc />
        public void Persist(
            string saveToPath,
            PersistStrategies persistStrategy,
            Encoding encoding
        )
        {
            if (encoding is null)
            {
                throw new ArgumentException(nameof(encoding));
            }

            saveToPath = CheckPersistencePath(saveToPath);
            var lines = GetLinesForCurrentData(persistStrategy);
            File.WriteAllBytes(
                saveToPath,
                encoding.GetBytes(
                    string.Join(Environment.NewLine, lines)
                )
            );
        }

        /// <inheritdoc />
        public void Persist(Stream toStream)
        {
            Persist(toStream, DefaultEncoding);
        }

        /// <inheritdoc />
        public void Persist(Stream toStream, Encoding encoding)
        {
            Persist(
                toStream,
                PersistStrategies.ExcludeMergedConfigurations,
                encoding
            );
        }

        /// <inheritdoc />
        public void Persist(
            Stream toStream,
            PersistStrategies persistStrategy
        )
        {
            Persist(toStream, persistStrategy, Encoding.UTF8);
        }

        /// <inheritdoc />
        public void Persist(
            Stream toStream,
            PersistStrategies persistStrategy,
            Encoding encoding
        )
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            var lines = GetLinesForCurrentData(persistStrategy);
            var shouldAddNewline = false;
            var newLine = encoding.GetBytes(Environment.NewLine);
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

                    var lineAsBytes = encoding.GetBytes(line);
                    toStream.Write(lineAsBytes, 0, lineAsBytes.Length);
                });
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Join(
                Environment.NewLine,
                GetLinesForCurrentData(
                    PersistStrategies.IncludeMergedConfigurations
                )
            );
        }

        private List<string> GetLinesForCurrentData(
            PersistStrategies persistStrategy
        )
        {
            var lineParser = SelectLineParser();
            var sectionFetcher = PersistFetchStrategies[persistStrategy];
            var sections = persistStrategy == PersistStrategies.ExcludeMergedConfigurations
                ? Sections
                : AllSections;
            var addSeparator = SectionSeparator is not null;
            var commentDelimiter = lineParser.CommentDelimiter;
            var sep = CommentIfNecessary(SectionSeparator, commentDelimiter);
            var addEmptyLineAtTheBottom = AddEmptyLineAtTheBottom;
            var lines = new List<string>();
            foreach (var section in sections)
            {
                AddCommentsTo(lines, section, SECTION_COMMENT_KEY, lineParser.CommentDelimiter);
                if (section.Length > 0)
                {
                    lines.Add($"[{section}]");
                }

                var dictionary = sectionFetcher(section, this);
                lines.AddRange(
                    dictionary
                        .Keys
                        .Select(key => LineFor(section, key, commentDelimiter))
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

            if (addEmptyLineAtTheBottom)
            {
                lines.Add("");
            }

            return lines;
        }

        private string CommentIfNecessary(
            string sectionSeparator,
            string commentDelimiter
        )
        {
            if (string.IsNullOrWhiteSpace(sectionSeparator))
            {
                return sectionSeparator;
            }

            return string.Join("\n", sectionSeparator.Split('\n')
                .Select(line =>
                    line.StartsWith(commentDelimiter)
                        ? line
                        : $"{commentDelimiter}{line}"
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
            string forKey,
            string commentDelimiter
        )
        {
            if (Comments.ContainsKey(forSection) &&
                Comments[forSection].ContainsKey(forKey))
            {
                lines.Add(commentDelimiter + Comments[forSection][forKey]);
            }
        }

        private string LineFor(
            string section,
            string key,
            string commentDelimiter
        )
        {
            var lines = new List<string>();
            AddCommentsTo(lines, section, key, commentDelimiter);
            var dataValue = this[section][key];
            var writeValue = string.Empty;
            if (dataValue is not null)
            {
                writeValue = WrapValueInQuotes
                    ? $"=\"{EscapeEntities(dataValue, section, key)}\""
                    : $"={EscapeEntities(dataValue, section, key)}";
            }

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
        public bool RemoveValue(
            string section,
            string key
        )
        {
            return RemoveValue(section, key, false);
        }

        /// <inheritdoc />
        public bool RemoveValue(
            string section,
            string key,
            bool removeEmptySection
        )
        {
            if (!HasSection(section))
            {
                return false;
            }

            var sectionData = Data[section];
            var result = sectionData.Remove(key);
            if (removeEmptySection && !sectionData.Any())
            {
                Data.Remove(section);
            }

            return result;
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
                Data.Keys.Contains(section, DefaultStringComparer);
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
                DefaultStringComparer
            );
        }
    }
}