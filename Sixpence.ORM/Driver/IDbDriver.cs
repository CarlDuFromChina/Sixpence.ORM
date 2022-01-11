﻿using Microsoft.Extensions.DependencyInjection;
using Sixpence.Common;
using Sixpence.Common.IoC;
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
    [ServiceRegister]
    public interface IDbDriver
    {
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
        string GetAddColumnSql(string tableName, List<Column> columns);

        /// <summary>
        /// 获取列删除语句
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        string GetDropColumnSql(string tableName, List<Column> columns);

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
    }
}
