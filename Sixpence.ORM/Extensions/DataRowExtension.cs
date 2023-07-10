using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Sixpence.ORM
{
    public static class DataRowExtension
    {
        public static Dictionary<string, object> ToDictionary(this DataRow row)
        {
            var dict = row.Table.Columns
              .Cast<DataColumn>()
              .ToDictionary(c => c.ColumnName, c => row[c]);
            return dict;
        }

        public static Dictionary<string, object> ToDictionary(this DataRow dataRow, DataColumnCollection columnCollection)
        {
            var columns = new List<string>();
            foreach (DataColumn column in columnCollection)
            {
                columns.Add(column.ColumnName);
            }
            var dic = new Dictionary<string, object>();
            for (int i = 0; i < columns.Count; i++)
            {
                dic.Add(columns[i], dataRow.ItemArray[i]);
            }
            return dic;
        }
    }
}
