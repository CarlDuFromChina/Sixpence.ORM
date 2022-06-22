using Dapper;
using Newtonsoft.Json.Linq;
using Npgsql;
using Sixpence.Common;
using Sixpence.Common.Utils;
using Sixpence.ORM.DbClient;
using Sixpence.ORM.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Driver
{
    public class PostgresDriver : IDbDriver
    {
        public string Provider => "Postgres";

        public DbConnection GetDbConnection(string connectionString)
        {
            return DbConnectionFactory.GetDbConnection(DriverType.Postgresql, connectionString);
        }

        public string CreateRole(string name)
        {
            return $"CREATE ROLE {name}";
        }

        public string CreateTemporaryTable(IDbConnection conn, string tableName)
        {
            var newTableName = tableName + Guid.NewGuid().ToString().Replace("-", "");
            conn.Execute($@"CREATE TEMP TABLE {newTableName} ON COMMIT DROP AS SELECT * FROM {tableName} WHERE 1!=1;");
            return newTableName;
        }

        public string CreateUser(string name)
        {
            return $"CREATE USER {name}";
        }

        public string DropRole(string name)
        {
            return $"DROP ROLE {name}";
        }

        public string DropUser(string name)
        {
            return $"DROP User {name}";
        }

        public string GetAddColumnSql(string tableName, List<ColumnOptions> columns)
        {
            var sql = new StringBuilder();
            var tempSql = $@"ALTER TABLE {tableName}";
            foreach (var item in columns)
            {
                var require = item.IsRequire == true ? " NOT NULL" : "";
                var length = item.Length != null ? $"({item.Length})" : "";
                var defaultValue = item.DefaultValue == null ? "" : item.DefaultValue is string ? $"DEFAULT '{item.DefaultValue}'" : $"DEFAULT {item.DefaultValue}";
                sql.Append($"{tempSql} ADD COLUMN IF NOT EXISTS {item.Name} {item.Type}{length} {require} {defaultValue};\r\n");
            }
            return sql.ToString();
        }

        public string GetDataBase(string name)
        {
            return $@"
SELECT u.datname
FROM pg_catalog.pg_database u where u.datname='{name}';
";
        }

        public string GetDropColumnSql(string tableName, List<ColumnOptions> columns)
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

        public string TableExsit(string tableName)
        {
            return $@"
SELECT COUNT(1) > 0 FROM pg_tables
WHERE schemaname = 'public' AND tablename = '{tableName}'";
        }

        public string QueryRole(string name)
        {
            return $@"
SELECT * FROM pg_roles
WHERE rolname = '{name}'";
        }

        public void BulkCopy(IDbConnection conn, DataTable dataTable, string tableName)
        {
            var columnNameList = (from DataColumn column in dataTable.Columns select column.ColumnName.ToLower())
                .ToList();
            var columnNames = columnNameList.Aggregate((l, n) => l + "," + n);
            var cn = conn as NpgsqlConnection;
            if (conn == null) return;

            using (var writer = cn.BeginBinaryImport($"COPY {tableName}({columnNames}) FROM STDIN (FORMAT BINARY)"))
            {
                foreach (DataRow dr in dataTable.Rows)
                {
                    writer.StartRow();
                    foreach (var columnName in columnNameList)
                    {
                        if (string.IsNullOrWhiteSpace(dr[columnName].ToString()))
                        {
                            writer.WriteNull();
                        }
                        else
                        {
                            var dataType = dataTable.Columns[columnName].DataType;

                            if (dataType == typeof(bool))
                                writer.Write(ConvertUtil.ConToBoolean(dr[columnName]));
                            else if (dataType == typeof(int))
                                writer.Write(ConvertUtil.ConToInt(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Integer);
                            else if (dataType == typeof(long))
                                writer.Write(ConvertUtil.ConToInt(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Bigint);
                            else if (dataType == typeof(short))
                                writer.Write(ConvertUtil.ConToInt(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Int2Vector);
                            else if (dataType == typeof(decimal))
                                writer.Write(ConvertUtil.ConToDecimal(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Numeric);
                            else if (dataTable.Columns[columnName].DataType == typeof(bool))
                                writer.Write(ConvertUtil.ConToDecimal(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Boolean);
                            else if (dataTable.Columns[columnName].DataType == typeof(DateTime))
                                writer.Write(ConvertUtil.ConToDateTime(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Timestamp);
                            else if (dataTable.Columns[columnName].DataType == typeof(JToken))
                                writer.Write(dr[columnName].ToString(), NpgsqlTypes.NpgsqlDbType.Jsonb);
                            else
                                writer.Write(dr[columnName].ToString());
                        }
                    }
                }
                writer.Complete();
            }
        }

        public void AddLimit(ref string sql, int? index, int size)
        {
            if (index.HasValue)
            {
                sql += $" LIMIT {size} OFFSET {(index - 1) * size}";
            }
            else
            {
                sql += $" LIMIT {size}";
            }
        }

        public string Convert2DbType(Type propertyType)
        {
            if (propertyType == typeof(bool) || propertyType == typeof(bool?))
                return "bool";
            else if (propertyType == typeof(int) || propertyType == typeof(int?))
                return "int4";
            else if (propertyType == typeof(long) || propertyType == typeof(long?))
                return "int8";
            else if (propertyType == typeof(short) || propertyType == typeof(short?))
                return "int2vector";
            else if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                return "numeric";
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                return "timestamp";
            else if (propertyType == typeof(JToken))
                return "jsonb";
            else if (propertyType == typeof(string))
                return "text";
            else
                throw new NotSupportedException($"Postgres不支持{propertyType.Name}类型");
        }

        public Type Convert2CSharpType(string columnType)
        {
            switch (columnType)
            {
                case "bool":
                    return typeof(bool);
                case "int4":
                    return typeof(int?);
                case "int8":
                    return typeof(long?);
                case "int2vector":
                    return typeof(short?);
                case "numeric":
                    return typeof(decimal?);
                case "timestamp":
                    return typeof(DateTime?);
                case "jsonb":
                    return typeof(JToken);
                case "text":
                    return typeof(string);
                default:
                    throw new NotSupportedException($"C#不支持{columnType}类型");
            }
        }
    }
}
