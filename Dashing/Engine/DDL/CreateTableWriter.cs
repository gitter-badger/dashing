namespace Dashing.Engine.DDL {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    public class CreateTableWriter : ICreateTableWriter {
        private readonly ISqlDialect dialect;

        public CreateTableWriter(ISqlDialect dialect) {
            if (dialect == null) {
                throw new ArgumentNullException("dialect");
            }

            this.dialect = dialect;
        }

        public string CreateTable(IMap map) {
            var sql = new StringBuilder();

            sql.Append("create table ");
            this.dialect.AppendQuotedTableName(sql, map);
            sql.Append(" (");

            this.dialect.AppendColumnSpecification(sql, map.PrimaryKey);

            foreach (var column in map.OwnedColumns(true).Where(c => !c.IsPrimaryKey)) {
                sql.Append(", ");
                this.dialect.AppendColumnSpecification(sql, column);
            }

            sql.Append(")");
            return sql.ToString();
        }

        public IEnumerable<string> CreateForeignKeys(IMap map) {
            return this.CreateForeignKeys(map.ForeignKeys);
        }

        private string CreateForeignKey(ForeignKey foreignKey) {
            var sql = new StringBuilder();
            sql.Append("alter table ");
            this.dialect.AppendQuotedTableName(sql, foreignKey.ChildColumn.Map);
            sql.Append(" add constraint ").Append(foreignKey.Name).Append(" foreign key (");
            this.dialect.AppendQuotedName(sql, foreignKey.ChildColumn.DbName);
            sql.Append(") references ");
            this.dialect.AppendQuotedTableName(sql, foreignKey.ParentMap);
            sql.Append("(");
            this.dialect.AppendQuotedName(sql, foreignKey.ParentMap.PrimaryKey.DbName);
            sql.Append(")");
            return sql.ToString();
        }

        public IEnumerable<string> CreateForeignKeys(IEnumerable<ForeignKey> foreignKeys) {
            return foreignKeys.Select(f => this.CreateForeignKey(f));
        }

        public IEnumerable<string> CreateIndexes(IMap map) {
            return this.CreateIndexes(map.Indexes);
        }

        public IEnumerable<string> CreateIndexes(IEnumerable<Index> indexes) {
            foreach (var index in indexes) {
                yield return this.dialect.CreateIndex(index);
            }
        }
    }
}