using System.Collections.Generic;
using FluentMigrator;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.Insert;
// ReSharper disable MemberCanBePrivate.Global

namespace EmailSpooler.Win32Service.DB.FluentMigrator
{
    public abstract class MigrationFoundation: Migration
    {
        private IExecuteExpressionRoot _executeOverride;
        public new IExecuteExpressionRoot Execute
        {
            get
            {
                lock(this)
                {
                    if (_executeOverride != null)
                        return _executeOverride;
                    return base.Execute;
                }
            }
            // ReSharper disable once UnusedMember.Global
            set
            {
                lock(this)
                {
                    _executeOverride = value;
                }
            }
        }
        private IInsertExpressionRoot _override;
        // ReSharper disable once UnusedMember.Global
        public new IInsertExpressionRoot Insert
        {
            get
            {
                lock(this)
                {
                    if (_override != null)
                        return _override;
                    return base.Insert;
                }
            }
            set
            {
                lock(this)
                {
                    _override = value;
                }
            }
        }
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

        // ReSharper disable once UnusedMember.Global
        protected void EnableIdentityInsertFor(string table)
        {
            Execute.Sql(Join("set IDENTITY_INSERT [", table, "] ON;"));
        }

        // ReSharper disable once UnusedMember.Global
        protected void DisableIdentityInsertFor(string table)
        {
            Execute.Sql(Join("set IDENTITY_INSERT [", table, "] OFF;"));
        }

        protected string Join(params string[] parts)
        {
            return string.Join(string.Empty, parts);
        }
    }
}
