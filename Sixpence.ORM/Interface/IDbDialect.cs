using Sixpence.ORM.Mappers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public interface IDbDialect
    {
        /// <summary>
        /// 参数化前缀
        /// </summary>
        char ParameterPrefix { get; }

        /// <summary>
        /// 命名空间
        /// </summary>
        string Schema { get; }

        /// <summary>
        /// 获取列新增获取语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        string GetAddColumnSql(string tableName, IList<IDbPropertyMap> columns);

        /// <summary>
        /// 获取列删除语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        string GetDropColumnSql(string tableName, IList<IDbPropertyMap> columns);

        /// <summary>
        /// 获取临时表创建语句
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetCreateTemporaryTableSql(string tableName, string tempTableName);

        /// <summary>
        /// 添加查询数量限制
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        string GetPageSql(int? index, int size);

        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetTableExsitSql(string tableName);

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetDropTableSql(string tableName);

        /// <summary>
        /// 查询表的字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string GetTableColumnsSql(string tableName);

        /// <summary>
        /// 创建或更新SQL
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="updatedColumns"></param>
        /// <param name="values"></param>
        /// <param name="primaryKeys"></param>
        /// <param name="updatedValues"></param>
        /// <returns></returns>
        string GetCreateOrUpdateSQL(string tableName, string updatedColumns, string values, string primaryKeys, string updatedValues);

        /// <summary>
        /// 处理字段名和值的前缀和后缀
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        (string name, object value) HandleParameter(string name, object value);

        /// <summary>
        /// 获取不同数据库的 IN 语法，例如：PG 里使用 Any 代替 IN
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        string GetInClauseSql(string parameter);
    }
}
