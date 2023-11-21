using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 实体转 DataTable
    /// </summary>
    public static partial class EntityCommon
    {
        public static DataTable ParseToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(PascalToUnderline(prop.Name), Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[PascalToUnderline(prop.Name)] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static DataTable ParseToDataTable<T>(IList<T> data, DataColumnCollection columns)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (DataColumn item in columns)
            {
                var prop = properties.Find(item.ColumnName, true);
                table.Columns.Add(new DataColumn(PascalToUnderline(item.ColumnName), item.DataType));
            }

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (DataColumn c in columns)
                {
                    row[c.ColumnName] = DBNull.Value;

                    var prop = properties.Find(UnderlineToPascal(c.ColumnName), true);
                    var propValue = prop?.GetValue(item);
                    if (propValue != null)
                    {
                        row[c.ColumnName] = Convert.ChangeType(propValue, c.DataType);
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
