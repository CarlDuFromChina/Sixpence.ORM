using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    /// <summary>
    /// 数据库实例
    /// </summary>
    public class DbClient : IDisposable
    {
        private IDbDriver driver;
        private int? commandTimeout = 20;
        private readonly ILogger<DbClient> Logger;
        private readonly bool EnableLogging = SormAppBuilderExtensions.BuilderOptions.EnableLogging;

        public int? CommandTimeout => commandTimeout;
        public IDbConnection DbConnection { get; private set; }
        public IDbDriver Driver => driver; // 数据库驱动
        public IDbDialect Dialect => driver.Dialect; // 数据库方言
        public IDbBatch Batch => driver.Batch; // 数据库批量操作

        internal DbClient(IDbDriver dbDriver, string connectionString, int? commandTimeout)
        {
            driver = dbDriver;
            DbConnection = dbDriver.GetDbConnection(connectionString);
            if (commandTimeout != null)
            {
                this.commandTimeout = commandTimeout;
            }
            Logger = ServiceContainer.Provider.GetService<ILogger<DbClient>>();
        }

        /// <summary>
        ///获取数据库连接状态 
        /// </summary>
        /// <returns></returns>
        public ConnectionState ConnectionState => DbConnection.State;

        #region 开启数据库连接
        //数据库打开、关闭的计数器
        private int _dbOpenCounter;

        /// <summary>
        ///打开数据库的连接（如果已经Open，则忽略）
        /// </summary>
        public void Open()
        {
            //counter = 0代表没有打开过，否则说明已经打开过了，不需要再打开
            if (_dbOpenCounter++ == 0)
            {
                if (DbConnection.State != ConnectionState.Open)
                    DbConnection.Open();
            }

        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Close()
        {
            //counter先自减1，然后判断是否=0，是的话代表是最后一次关闭
            if (--_dbOpenCounter == 0)
            {
                if (DbConnection.State != ConnectionState.Closed)
                {
                    DbConnection?.Close();
                }
            }
        }
        #endregion

        #region 事务

        private IDbTransaction _trans;
        private int _transCounter;

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public IDbTransaction BeginTransaction()
        {
            if (_transCounter++ == 0)
            {
                _trans = DbConnection.BeginTransaction();
            }
            return _trans;
        }

        /// <summary>
        /// 提交数据库的事务
        /// </summary>
        public void CommitTransaction()
        {
            if (--_transCounter == 0)
            {
                _trans?.Commit();
                _trans?.Dispose();
                _trans = null;
            }
        }

        /// <summary>
        /// 回滚数据库的事务
        /// </summary>
        public void Rollback()
        {
            try
            {
                if (--_transCounter == 0)
                {
                    _trans?.Rollback();
                    _trans?.Dispose();
                    _trans = null;
                }
            }
            finally
            {
                if (_transCounter == 0)
                    _trans = null;
            }
        }
        #endregion

        #region Execute
        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public int Execute(string sql, object? param = null)
        {
            var paramList = param.ToDictionary();

            if (EnableLogging)
                Logger.LogDebug(sql + paramList.ToLogString());

            return DbConnection.Execute(sql, param, commandTimeout: CommandTimeout);
        }

        /// <summary>
        /// 执行SQL语句，并返回第一行第一列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, object? param = null)
        {
            var paramList = param.ToDictionary();

            if (EnableLogging)
                Logger.LogDebug(sql + paramList.ToLogString());

            return DbConnection.ExecuteScalar(sql, param, commandTimeout: CommandTimeout);
        }
        #endregion

        #region Query
        /// <summary>
        /// 执行SQL语句，并返回查询结果集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object? param = null)
        {
            var paramList = param.ToDictionary();

            if (EnableLogging)
                Logger.LogDebug(sql + paramList.ToLogString());

            return DbConnection.Query<T>(sql, param, commandTimeout: CommandTimeout);
        }

        /// <summary>
        /// 执行SQL语句，并返回查询结果
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public T QueryFirst<T>(string sql, object? param = null)
        {
            var paramList = param.ToDictionary();

            if (EnableLogging)
                Logger.LogDebug(sql + paramList.ToLogString());

            return DbConnection.QueryFirstOrDefault<T>(sql, param, commandTimeout: CommandTimeout);
        }
        #endregion

        #region DataTable
        /// <summary>
        /// 执行SQL语句，并返回查询结果集
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public DataTable Query(string sql, object? param = null)
        {
            var paramList = param.ToDictionary();

            if (EnableLogging)
                Logger.LogDebug(sql + paramList.ToLogString());

            DataTable dt = new DataTable();
            var reader = DbConnection.ExecuteReader(sql, param, commandTimeout: CommandTimeout);
            dt.Load(reader);
            return dt;
        }
        #endregion

        /// <summary>
        /// 创建临时表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string CreateTemporaryTable(string tableName)
        {
            var tempTableName = $"{tableName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            var sql = Driver.Dialect.GetCreateTemporaryTableSql(tableName, tempTableName);

            if (EnableLogging)
                Logger.LogDebug(sql);

            DbConnection.Execute(sql);
            return tempTableName;
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName"></param>
        public void DropTable(string tableName)
        {
            var sql = Dialect.GetDropTableSql(tableName);

            if (EnableLogging)
                Logger.LogDebug(sql);

            DbConnection.Execute(sql, commandTimeout: CommandTimeout);
        }

        /// <summary>
        /// 释放连接
        /// </summary>
        public void Dispose()
            => DbConnection.Dispose();

        /// <summary>
        /// 拷贝数据
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        public void BulkCopy(DataTable dataTable, string tableName)
            => Batch.BulkCopy(DbConnection, dataTable, tableName);
    }
}
