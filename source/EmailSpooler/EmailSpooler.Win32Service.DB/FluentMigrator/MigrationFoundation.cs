using System.Collections.Generic;
using FluentMigrator;

// ReSharper disable MemberCanBePrivate.Global

namespace EmailSpooler.Win32Service.DB.FluentMigrator
{
    public abstract class MigrationFoundation: Migration
    {
        protected void AddLastUpdatedTriggerFor(string tableName, string idColumn = null)
        {
            Execute.Sql(CreateLastModifiedTriggerSqlFor(tableName, idColumn));
        }

        protected string CreateLastModifiedTriggerSqlFor(string tableName, string idCol)
        {
            var lines = new List<string>();
            if (idCol == null)
            {
                var entityName = tableName;
                if (entityName.EndsWith("s"))
                    entityName = entityName.Substring(0, entityName.Length-1);
                idCol = Join(entityName, "ID");
            }
            lines.Add(Join("create trigger [dbo].[trLastUpdated_", tableName, "]"));
            lines.Add(Join("on [dbo].[",  tableName,  "]" ));
            lines.Add("for update");
            lines.Add("as");
            lines.Add("begin");
            lines.Add("set NOCOUNT ON;");
            lines.Add(Join("update [dbo].[", tableName, "] set LastModified = CURRENT_TIMESTAMP where [", 
                idCol, "] in (select [", idCol, "] from inserted);"));
            lines.Add("end");
            return string.Join("\n", lines);
        }

        protected string Join(params string[] parts)
        {
            return string.Join(string.Empty, parts);
        }
    }
}
