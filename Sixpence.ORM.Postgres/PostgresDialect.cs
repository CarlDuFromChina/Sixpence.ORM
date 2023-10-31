using Dapper;
using Sixpence.ORM.Mappers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sixpence.ORM.Postgres
{
    public class PostgresDialect : IDbDialect
    {
        public char ParameterPrefix => '@';

        public string Schema => "public";

        /// <summary>
        /// 获取临时表创建语句
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetCreateTemporaryTableSql(string tableName, string tempTableName)
        {
            return $@"CREATE TEMP TABLE {tempTableName}
ON COMMIT DROP AS SELECT * FROM {tableName}
WHERE 1!=1;";
        }

        /// <summary>
        /// 获取列新增获取语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public string GetAddColumnSql(string tableName, IList<IDbPropertyMap> columns)
        {
            var sql = new StringBuilder();
            var tempSql = $@"ALTER TABLE {tableName}";
            foreach (var item in columns)
            {
                var require = item.IsRequired == true ? " NOT NULL" : "";
                var length = item.Length != null ? $"({item.Length})" : "";
                var defaultValue = item.DefaultValue == null ? "" : item.DefaultValue is string ? $"DEFAULT '{item.DefaultValue}'" : $"DEFAULT {item.DefaultValue}";
                sql.Append($"{tempSql} ADD COLUMN IF NOT EXISTS {item.Name} {item.Type}{length} {require} {defaultValue};\r\n");
            }
            return sql.ToString();
        }

        /// <summary>
        /// 获取列删除语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public string GetDropColumnSql(string tableName, IList<IDbPropertyMap> columns)
        {
            var sql = $@"
ALTER TABLE {tableName}
";
            var count = 0;
            foreach (var item in columns)
            {
                var itemSql = $"DROP COLUMN IF EXISTS {item.Name} {(++count == columns.Count ? ";" : ",")}";
                sql += itemSql;
            }
            return sql;
        }

        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetTableExsitSql(string tableName)
        {
            return $@"
SELECT COUNT(1) > 0 FROM pg_tables
WHERE schemaname = 'public' AND tablename = '{tableName}'";
        }

        /// <summary>
        /// 添加查询数量限制
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        public string GetPageSql(int? index, int size)
        {
            if (index.HasValue)
            {
                return $" LIMIT {size} OFFSET {(index - 1) * size}";
            }
            else
            {
                return $" LIMIT {size}";
            }
        }

        /// <summary>
        /// 查询表的字段
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetTableColumnsSql(string tableName)
        {
            var sql = @"
SELECT 
	A.attname AS Name,
	A.attnotnull AS IsRequired,
	format_type ( A.atttypid, A.atttypmod ) AS Type
FROM
	pg_class AS C,
	pg_attribute AS A 
WHERE
	C.relname = 'test' 
	AND A.attrelid = C.oid 
	AND A.attnum > 0
	AND A.atttypid <> 0";
            return sql;
        }

        /// <summary>
        /// 创建或更新SQL
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="updatedColumns"></param>
        /// <param name="values"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="updatedValues"></param>
        /// <returns></returns>
        public string GetCreateOrUpdateSQL(string tableName, string updatedColumns, string values, string primaryKeys, string updatedValues)
        {
            var templateSQL = $@"INSERT INTO {tableName} ({updatedColumns}) VALUES ({values})
ON CONFLICT ({primaryKeys}) DO UPDATE SET {updatedValues};";
            return templateSQL;
        }

        /// <summary>
        /// 删除表SQL
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetDropTableSql(string tableName)
        {
            return $"DROP TABLE IF EXISTS {tableName}";
        }

        /// <summary>
        /// 处理字段名和值的前缀和后缀
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public (string name, object value) HandleParameter(string name, object value)
        {
            return (name, value);
        }
    }
}
