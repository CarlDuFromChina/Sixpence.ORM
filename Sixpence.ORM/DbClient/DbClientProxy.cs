using Sixpence.Common;
using Sixpence.ORM.Driver;
using Sixpence.Common.Logging;
using Sixpence.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Sixpence.ORM.DbClient
{
    /// <summary>
    /// DbClient 代理类（记录日志、Sql本地化）
    /// </summary>
    public class DbClientProxy : IDbClient, IDisposable
    {
        #region 构造函数
        public DbClientProxy()
        {
            this.dbClient = new DbClient();
        }

        public DbClientProxy(IDbClient client)
        {
            this.dbClient = client;
        }
        #endregion

        private IDbClient dbClient;

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public IDbConnection DbConnection => dbClient.DbConnection;

        /// <summary>
        /// 数据库链接状态
        /// </summary>
        public ConnectionState ConnectionState => dbClient.ConnectionState;

        public IDBProvider Driver => dbClient.Driver;

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public IDbTransaction BeginTransaction()
        {
            return dbClient.BeginTransaction();
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void Close()
        {
            dbClient.Close();
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            dbClient.CommitTransaction();
        }

        /// <summary>
        /// 创建临时表
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string CreateTemporaryTable(string tableName)
        {
            return dbClient.CreateTemporaryTable(tableName);
        }

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tableName"></param>
        public void DropTable(string tableName)
        {
            dbClient.DropTable(tableName);
        }

        /// <summary>
        /// 执行SQL，返回受影响行数（记录 log）
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public int Execute(string sqlText, object param  = null)
        {
            var paramList = param?.ToDictionary();
            sqlText = ConvertSqlToDialectSql(sqlText, paramList);
            LogUtil.Debug(sqlText + paramList.ToLogString());
            return dbClient.Execute(sqlText, paramList);
        }

        /// <summary>
        /// 执行SQL，返回第一行第一列（记录 log）
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, object param  = null)
        {
            var paramList = param?.ToDictionary();
            sql = ConvertSqlToDialectSql(sql, paramList);
            LogUtil.Debug(sql + paramList.ToLogString());
            return dbClient.ExecuteScalar(sql, paramList);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="connectinString"></param>
        public void Initialize(string connectinString, DriverType driverType)
        {
            dbClient.Initialize(connectinString, driverType);
        }

        /// <summary>
        /// 打开连接
        /// </summary>
        public void Open()
        {
            dbClient.Open();
        }

        /// <summary>
        /// 根据SQL查询，返回传入类型集合（记录 log）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            var paramList = param?.ToDictionary();
            sql = ConvertSqlToDialectSql(sql, paramList);
            LogUtil.Debug(sql + paramList.ToLogString());
            return dbClient.Query<T>(sql, param);
        }

        /// <summary>
        /// 根据SQL查询，返回DataTable（记录 log）
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public DataTable Query(string sql, object param = null)
        {
            var paramList = param?.ToDictionary();
            sql = ConvertSqlToDialectSql(sql, paramList);
            LogUtil.Debug(sql + paramList.ToLogString());
            return dbClient.Query(sql, paramList);
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public void Rollback()
        {
            dbClient.Rollback();
        }

        /// <summary>
        /// 将SQL转换为本地化SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramsList"></param>
        /// <returns></returns>
        public string ConvertSqlToDialectSql(string sql, IDictionary<string, object> paramsList)
        {
            if (paramsList == null || paramsList.Count == 0)
            {
                return sql;
            }
            if (sql.Contains("in@"))
            {
                var toRemovedParamNameList = new Dictionary<string, Dictionary<string, object>>();
                var paramValueNullList = new List<string>(); // 记录传入的InList参数的Value如果为空或者没有值的特殊情况

                foreach (var paramName in paramsList.Keys)
                {
                    if (!paramName.ToLower().StartsWith("in")) continue;
                    var paramValue = paramsList[paramName]?.ToString();
                    if (string.IsNullOrWhiteSpace(paramValue))
                    {
                        paramValueNullList.Add(paramName);
                        continue;
                    }

                    toRemovedParamNameList.Add(paramName, new Dictionary<string, object>());
                    var inListValues = paramValue.Split(',');
                    for (var i = 0; i < inListValues.Length; i++)
                    {
                        toRemovedParamNameList[paramName].Add(paramName.Substring(2, paramName.Length - 2) + i, inListValues[i]);
                    }
                }

                foreach (var paramNameRemoved in toRemovedParamNameList.Keys)
                {
                    paramsList.Remove(paramNameRemoved);
                    foreach (var paramNameAdd in toRemovedParamNameList[paramNameRemoved].Keys)
                    {
                        paramsList.Add(paramNameAdd, toRemovedParamNameList[paramNameRemoved][paramNameAdd]);
                    }

                    var newParamNames = toRemovedParamNameList[paramNameRemoved].Keys.Aggregate((l, n) => l + "," + n);
                    sql = sql.Replace(paramNameRemoved, newParamNames);
                }

                foreach (var paramValueNullName in paramValueNullList)
                {
                    paramsList.Remove(paramValueNullName);
                    sql = sql.Replace(paramValueNullName, "null");
                }

                return sql;
            }
            return sql;
        }

        /// <summary>
        /// 将SQL转换为本地化SQL
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string ConvertSqlToDialectSql(string sql, object param = null)
        {
            return ConvertSqlToDialectSql(sql, param.ToDictionary());
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            (dbClient as IDisposable).Dispose();
        }

        /// <summary>
        /// 批量拷贝
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        public void BulkCopy(DataTable dataTable, string tableName)
        {
            dbClient.BulkCopy(dataTable, tableName);
        }

        public T QueryFirst<T>(string sql, object param  = null)
        {
            var paramList = param?.ToDictionary();
            sql = ConvertSqlToDialectSql(sql, paramList);
            LogUtil.Debug(sql + paramList.ToLogString());
            return dbClient.QueryFirst<T>(sql, paramList);
        }
    }
}
