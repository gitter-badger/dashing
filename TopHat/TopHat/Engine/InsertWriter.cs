﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Engine
{
    using Dapper;

    using TopHat.Configuration;

    internal class InsertWriter : BaseWriter, IEntitySqlWriter
    {
        public InsertWriter(ISqlDialect dialect, IConfiguration config)
            : this(dialect, new WhereClauseWriter(dialect, config), config) {
        }

        public InsertWriter(ISqlDialect dialect, IWhereClauseWriter whereClauseWriter, IConfiguration config)
            : base(dialect, whereClauseWriter, config) {
        }

        public SqlWriterResult GenerateSql<T>(EntityQueryBase<T> query) {
            var map = this.Configuration.GetMap<T>();
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();
            this.GenerateColumnSpec(sql, map);
            this.GenerateValuesSpec(sql, parameters, map, query);
            return new SqlWriterResult(sql.ToString(), parameters);
        }

        private void GenerateValuesSpec<T>(StringBuilder sql, DynamicParameters parameters, IMap<T> map, EntityQueryBase<T> query) {
            var paramIdx = 0;
            foreach (var entity in query.Entities) {
                sql.Append("(");
                foreach (var column in map.Columns) {
                    if (column.Value.Relationship == RelationshipType.None || column.Value.Relationship == RelationshipType.ManyToOne) {
                        if (column.Value == map.PrimaryKey && map.PrimaryKey.IsAutoGenerated) {
                            continue;
                        }

                        var paramName = "@p_" + ++paramIdx;
                        sql.Append(paramName);
                        if (column.Value.Relationship == RelationshipType.None) {
                            parameters.Add(paramName, map.GetColumnValue(entity, column.Value));
                        }
                        else {
                            parameters.Add(paramName, this.Configuration.GetMap(column.Value.Type).GetPrimaryKeyValue(map.GetColumnValue(entity, column.Value)));
                        }

                        sql.Append(",");
                    }
                }

                sql.Remove(sql.Length - 1, 1);
                sql.Append("),");
            }

            sql.Remove(sql.Length - 1, 1);
        }

        private void GenerateColumnSpec<T>(StringBuilder sql, IMap<T> map) {
            // TODO Add caching
            sql.Append("insert into ");
            this.Dialect.AppendQuotedTableName(sql, map);
            sql.Append("(");
            foreach (var column in map.Columns) {
                if (column.Value.Relationship == RelationshipType.None || column.Value.Relationship == RelationshipType.ManyToOne) {
                    if (column.Value == map.PrimaryKey && map.PrimaryKey.IsAutoGenerated) {
                        continue;
                    }

                    this.Dialect.AppendQuotedName(sql, column.Value.DbName);
                    sql.Append(",");
                }
            }

            sql.Remove(sql.Length - 1, 1);
            sql.Append(") values");
        }
    }
}
