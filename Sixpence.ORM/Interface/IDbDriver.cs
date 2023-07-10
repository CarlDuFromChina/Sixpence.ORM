using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Sixpence.ORM.Models;

namespace Sixpence.ORM
{
    /// <summary>
    /// 数据库驱动接口
    /// </summary>
    public interface IDbDriver
    {
        /// <summary>
        /// 数据库驱动名，例如：SQLServer、PostgreSql、MySql、Oracle等
        /// </summary>
        string Name { get; }

        /// <summary>
        /// C# 数据类型和数据库数据类型映射
        /// </summary>
        IFieldMapping FieldMapping { get; }

        /// <summary>
        /// 数据库方言
        /// </summary>
        IDbDialect Dialect { get; }

        /// <summary>
        /// 批量操作
        /// </summary>
        IDbBatch Batch { get; }

        /// <summary>
        /// 获取数据库连接对象
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        DbConnection GetDbConnection(string connectionString);
    }
}
