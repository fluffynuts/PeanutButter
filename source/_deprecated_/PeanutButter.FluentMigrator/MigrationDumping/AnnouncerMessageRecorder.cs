using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.FluentMigrator.MigrationDumping
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    internal class AnnouncerMessageRecorder
    {
        public IEnumerable<string> Statements => PrettifyStatements();

        private readonly List<string> _statements = new List<string>();
        private readonly IMigrationDumperOptions _options;

        internal AnnouncerMessageRecorder(IMigrationDumperOptions options)
        {
            _options = options;
        }

        public virtual void Record(string statement)
        {
            var trimmed = statement.Trim();
            if (!_options.IncludeComments && LooksLikeComment(trimmed))
                return;
            if (!_options.IncludeFluentMigratorStructures &&
                LooksLikeContainsVersionInfoStatement(trimmed))
                return;
            _statements.Add(statement);
        }

        protected virtual bool LooksLikeContainsVersionInfoStatement(string trimmed)
        {
            var lowered = trimmed.ToLower(CultureInfo.InvariantCulture);
            return lowered.Contains("versioninfo") &&
                    (lowered.ContainsOneOf("create table", "alter table", "insert into") ||
                     lowered.ContainsAllOf("create", "index"));
        }

        protected virtual bool LooksLikeComment(string trimmed)
        {
            return trimmed.StartsWith("/*");    // these are text-announcer comments
        }

        protected virtual string[] PrettifyStatements()
        {
            return _statements.Aggregate(new List<string>(), (acc, cur) =>
            {
                var isWhitespace = string.IsNullOrWhiteSpace(cur);
                var lastWasWhitespace = acc.Count > 0 && string.IsNullOrWhiteSpace(acc.Last());
                if (isWhitespace && lastWasWhitespace)
                    return acc;
                acc.Add(cur);
                return acc;
            }).ToArray();
        }
    }
}