namespace PeanutButter.INI
{
    /// <summary>
    /// Represents a line parser for parsing a data line into:
    /// - key
    /// - value
    /// - comment
    /// </summary>
    public interface ILineParser
    {
        /// <summary>
        /// The delimiter to use for comments (should
        /// be semi-colon in properly-formatted ini files,
        /// but INIFile allows for custom comment delimiters)
        /// </summary>
        public string CommentDelimiter { get; }

        /// <summary>
        /// Should parse a text line out into the parts of a parsed line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        IParsedLine Parse(string line);
    }
    
    /// <summary>
    /// Represents a parsed data line
    /// </summary>
    public interface IParsedLine
    {
        /// <summary>
        /// Key for this line
        /// - will be a section name if this is a section line
        /// - can be empty if this is a pure comment line
        /// - should never be null
        /// </summary>
        string Key { get; }
        /// <summary>
        /// Value for this line
        /// - may be null, eg for the case where a key is specified on a line without an =
        /// </summary>
        string Value { get; }
        /// <summary>
        /// Comment for this line
        /// - must include any whitespace surrounding the comment to insure
        ///   that rewrite of the file doesn't incur unnecessary changes
        /// - may be the whole line: then ensure that the Key is empty
        /// </summary>
        string Comment { get; }
        /// <summary>
        /// Helper flag: did the value originally contain escaped entities?
        /// - used on rewrite to determine if entities should be re-escaped
        ///   when in best-effort mode
        /// </summary>
        bool ContainedEscapedEntities { get; }
    }

}