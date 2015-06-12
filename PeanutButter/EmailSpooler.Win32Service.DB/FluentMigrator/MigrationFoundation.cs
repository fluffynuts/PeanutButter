using System;
using System.Collections.Generic;
using FluentMigrator;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Builders.Insert;

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
            set
            {
                lock(this)
                {
                    _executeOverride = value;
                }
            }
        }
        private IInsertExpressionRoot _override;
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
                    this._override = value;
                }
            }
        }
        protected void AddLastUpdatedTriggerFor(string tableName, string idColumn = null)
        {
            Execute.Sql(this.CreateLastModifiedTriggerSqlFor(tableName, idColumn));
        }

        protected string CreateLastModifiedTriggerSqlFor(string tableName, string idCol)
        {
            var lines = new List<string>();
            if (idCol == null)
            {
                var entityName = tableName;
                if (entityName.EndsWith("s"))
                    entityName = entityName.Substring(0, entityName.Length-1);
                idCol = String.Join("", new[] { entityName, "ID" });
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
            return String.Join("\n", lines);
        }

        protected void EnableIdentityInsertFor(string table)
        {
            Execute.Sql(String.Join("", new[] { "set IDENTITY_INSERT [", table, "] ON;" }));
        }

        protected void DisableIdentityInsertFor(string table)
        {
            Execute.Sql(String.Join("", new[] { "set IDENTITY_INSERT [", table, "] OFF;" }));
        }

        protected string Join(params string[] parts)
        {
            return String.Join("", parts);
        }
    }
}
