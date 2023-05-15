using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Sixpence.Common.IoC;
using Sixpence.ORM.Models;

namespace Sixpence.ORM.Driver
{
    [ServiceRegister]
    public interface IDbDriver
    {
        string Provider { get; }

        /// <summary>
        /// 获取数据库连接对象
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        DbConnection GetDbConnection(string connectionString);

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string CreateRole(string name);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string DropRole(string name);

        /// <summary>
        /// 创建用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string CreateUser(string name);

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string DropUser(string name);

        /// <summary>
        /// 查询角色
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string QueryRole(string name);

        /// <summary>
        /// 获取数据库记录查询语句
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetDataBase(string name);

        /// <summary>
        /// 获取列新增获取语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        string GetAddColumnSql(string tableName, List<ColumnOptions> columns);

        /// <summary>
        /// 获取列删除语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        string GetDropColumnSql(string tableName, List<ColumnOptions> columns);

        /// <summary>
        /// 获取临时表创建语句
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string CreateTemporaryTable(IDbConnection conn, string tableName);

        /// <summary>
        /// 批量创建
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        void BulkCopy(IDbConnection conn, DataTable dataTable, string tableName);

        /// <summary>
        /// 添加查询数量限制
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        void AddLimit(ref string sql, int? index, int size);

        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        string TableExsit(string tableName);

        /// <summary>
        /// 转换C#数据类型为数据库字段类型
        /// </summary>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        string Convert2DbType(Type propertyType);

        /// <summary>
        /// 转换数据库数据类型为C#数据类型
        /// </summary>
        /// <param name="columnType"></param>
        /// <returns></returns>
        Type Convert2CSharpType(string columnType);

        /// <summary>
        /// 处理数据库写入字段名称和值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        (string name, object value) HandleNameValue(string name, object value);

        /// <summary>
        /// 查询表的字段
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        List<EntityAttr> GetEntityAttributes(IDbConnection conn, string tableName);

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
    }
}
