using Npgsql;
using Sixpence.ORM.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Postgres
{
    public class PostgresBatch : IDbBatch
    {
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

                            if (dataType == typeof(bool) || dataType == typeof(bool?))
                                writer.Write(ConvertUtil.ConToBoolean(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Boolean);
                            else if (dataType == typeof(int) || dataType == typeof(int?))
                                writer.Write(ConvertUtil.ConToInt(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Integer);
                            else if (dataType == typeof(long) || dataType == typeof(long?))
                                writer.Write(ConvertUtil.ConToInt(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Bigint);
                            else if (dataType == typeof(short) || dataType == typeof(short?))
                                writer.Write(ConvertUtil.ConToInt(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Int2Vector);
                            else if (dataType == typeof(decimal) || dataType == typeof(decimal?))
                                writer.Write(ConvertUtil.ConToDecimal(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Numeric);
                            else if (dataType == typeof(DateTime) || dataType == typeof(DateTime?))
                                writer.Write(ConvertUtil.ConToDateTime(dr[columnName]), NpgsqlTypes.NpgsqlDbType.Timestamp);
                            else if (dataType == typeof(string))
                                writer.Write(dr[columnName].ToString());
                            else
                                throw new NotSupportedException($"Postgres不支持{dataType.Name}类型");
                        }
                    }
                }
                writer.Complete();
            }
        }
    }
}
