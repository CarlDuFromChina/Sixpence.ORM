using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Sixpence.ORM.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sixpence.ORM.EntityManager
{
    /// <summary>
    /// 实体管理
    /// </summary>
    public class EntityManager : IEntityManager, IDisposable
    {
        private DbClient _dbClient;
        private readonly IServiceProvider Provider;
        private readonly ILogger<EntityManager> Logger;

        public IDbDriver Driver => _dbClient.Driver;
        public DbClient DbClient => _dbClient;

        public EntityManager(string connectionString, IDbDriver dbDriver, int? commandTimeout)
        {
            _dbClient = new DbClient(dbDriver, connectionString, commandTimeout);
            Provider = ServiceContainer.Provider;
            Logger = Provider.GetService<ILoggerFactory>()?.CreateLogger<EntityManager>();
        }

        #region CRUD
        /// <summary>
        /// 创建实体记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Create(BaseEntity entity, bool usePlugin = true)
        {
            return this.ExecuteTransaction(() =>
            {
                #region 创建前 Plugin
                var context = new EntityManagerPluginContext() { Entity = entity, EntityManager = this, Action = EntityAction.PreCreate, EntityName = entity.GetEntityName() };
                ServiceContainer.Provider.GetServices<IEntityManagerBeforeCreateOrUpdate>()?
                    .Each(item => item.Execute(context));
                if (usePlugin)
                {
                    ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                        .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.GetEntityName()))
                        .Each(item => item.Execute(context));
                }
                #endregion

                var sql = "INSERT INTO {0}({1}) Values({2})";
                var attrs = new List<string>();
                var values = new List<object>();
                var paramList = new Dictionary<string, object>();
                foreach (var attr in entity.GetAttributes())
                {
                    var attrName = attr.Key; // 列名
                    var keyValue = Driver.Dialect.HandleParameter($"{Driver.Dialect.ParameterPrefix}{attrName}", attr.Value); // 值
                    attrs.Add(attrName);
                    values.Add(keyValue.name);
                    paramList.Add(attrName, keyValue.value);
                }
                sql = string.Format(sql, entity.GetEntityName(), string.Join(",", attrs), string.Join(",", values));
                this.Execute(sql, paramList);

                #region 创建后 Plugin
                if (usePlugin)
                {
                    context.Action = EntityAction.PostCreate;
                    ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                        .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.GetEntityName()))
                        .Each(item => item.Execute(context));
                }
                #endregion

                return entity.GetPrimaryColumn().Value;
            });
        }

        /// <summary>
        /// 删除实体记录
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int Delete(string entityName, string id)
        {
            var entity = ServiceContainer.Provider.GetServices<IEntity>().FirstOrDefault(item => EntityCommon.CompareEntityName(nameof(item), entityName)) as BaseEntity;
            AssertUtil.IsNull(entity, $"未找到实体：{entityName}");
            var dataList = DbClient.Query($"SELECT * FROM {entityName} WHERE {entity.GetPrimaryColumn().Name} = {Driver.Dialect.ParameterPrefix}id", new { id });

            if (dataList.Rows.Count == 0) return 0;

            var attributes = dataList.Rows[0].ToDictionary(dataList.Columns);
            attributes.Each(item => entity.SetAttributeValue(item.Key, item.Value.Equals(DBNull.Value) ? null : item.Value));

            var plugins = ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.GetEntityName()));

            plugins?.Each(item => item.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entityName, Action = EntityAction.PreDelete }));

            var sql = $"DELETE FROM {entityName} WHERE {entity.GetPrimaryColumn().Name} = {Driver.Dialect.ParameterPrefix}id";
            int result = this.Execute(sql, new { id });

            plugins?.Each(item => item.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entityName, Action = EntityAction.PostDelete }));
            return result;
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int Delete(BaseEntity entity)
        {
            return this.ExecuteTransaction(() =>
            {
                var plugins = ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                    .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.GetEntityName()));

                plugins?.Each(item => item.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.GetEntityName(), Action = EntityAction.PreDelete }));
                
                var sql = $"DELETE FROM {entity.GetEntityName()} WHERE {entity.GetPrimaryColumn().Name} = {Driver.Dialect.ParameterPrefix}id";
                int result = this.Execute(sql, new { id = entity.GetPrimaryColumn().Value });

                plugins?.Each(item => item.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.GetEntityName(), Action = EntityAction.PostDelete }));
                return result;
            });
        }

        /// <summary>
        /// 批量删除实体记录
        /// </summary>
        /// <param name="objArray"></param>
        /// <returns></returns>
        public int Delete(BaseEntity[] objArray)
        {
            if (objArray == null || objArray.Length == 0) return 0;

            return objArray.Sum(Delete);
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="where"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public int DeleteByWhere(string entityName, string where, Dictionary<string, object> paramList = null)
        {
            var sql = "DELETE FROM {0} WHERE 1=1 {1}";
            sql = string.Format(sql, entityName, string.IsNullOrEmpty(where) ? "" : $" AND {where}");
            int result = this.Execute(sql, paramList);
            return result;
        }

        /// <summary>
        /// 保存实体记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Save(BaseEntity entity)
        {
            var sql = $@"
SELECT * FROM {entity.GetEntityName()}
WHERE {entity.GetPrimaryColumn().Name} = {Driver.Dialect.ParameterPrefix}id;
";
            var dataList = this.Query(sql, new { id = entity.GetPrimaryColumn().Value });

            if (dataList != null && dataList.Rows.Count > 0)
                Update(entity);
            else
                Create(entity);

            return entity.GetPrimaryColumn().Value;
        }

        /// <summary>
        /// 更新实体记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Update(BaseEntity entity)
        {
            return this.ExecuteTransaction(() =>
            {
                #region 更新前 Plugin
                var context = new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.GetEntityName(), Action = EntityAction.PreUpdate };
                ServiceContainer.Provider.GetServices<IEntityManagerBeforeCreateOrUpdate>()
                    .Each(item => item.Execute(context));

                ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                    .Where(item => EntityCommon.MatchEntityManagerPlugin(nameof(item), entity.GetEntityName()))
                    .Each(item => item.Execute(context));
                #endregion

                var sql = @"UPDATE {0} SET {1} WHERE {2} = {3}id;";
                var paramList = new Dictionary<string, object>() { { $"{Driver.Dialect.ParameterPrefix}id", entity.GetPrimaryColumn().Value } };

                #region 处理属性
                var attributes = new List<string>();
                int count = 0;
                foreach (var item in entity.GetAttributes())
                {
                    if (item.Key != "id" && item.Key != entity.GetPrimaryColumn().Name)
                    {
                        var keyValue = Driver.Dialect.HandleParameter($"{Driver.Dialect.ParameterPrefix}param{count}", item.Value);
                        paramList.Add($"{Driver.Dialect.ParameterPrefix}param{count}", keyValue.value);
                        attributes.Add($"{ item.Key} = {keyValue.name}");
                        count++;
                    }
                }
                #endregion
                sql = string.Format(sql, entity.GetEntityName(), string.Join(",", attributes), entity.GetPrimaryColumn().Name, Driver.Dialect.ParameterPrefix);
                var result = this.Execute(sql, paramList);

                #region 更新后 Plugin
                context.Action = EntityAction.PostUpdate;
                ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                    .Where(item => EntityCommon.MatchEntityManagerPlugin(nameof(item), entity.GetEntityName()))
                    .Each(item => item.Execute(context));
                #endregion
                return result;
            });
        }
        #endregion

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            (this.DbClient as IDisposable).Dispose();
        }

        #region Transcation
        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="func"></param>
        public void ExecuteTransaction(Action func)
        {
            try
            {
                DbClient.Open();
                DbClient.BeginTransaction();

                func?.Invoke();

                DbClient.CommitTransaction();
            }
            catch (Exception ex)
            {
                DbClient.Rollback();
                throw ex;
            }
            finally
            {
                DbClient.Close();
            }
        }

        /// <summary>
        /// 执行事务返回结果
        /// </summary>
        /// <param name="func"></param>
        public T ExecuteTransaction<T>(Func<T> func)
        {

            try
            {
                DbClient.Open();
                DbClient.BeginTransaction();

                var t = default(T);

                if (func != null)
                {
                    t = func();
                }

                DbClient.CommitTransaction();

                return t;
            }
            catch (Exception ex)
            {
                DbClient.Rollback();
                throw ex;
            }
            finally
            {
                DbClient.Close();
            }
        }
        #endregion

        #region Query
        /// <summary>
        /// 根据id查询记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T QueryFirst<T>(string id) where T : BaseEntity, new()
        {
            var sql = $"SELECT * FROM {new T().GetEntityName()} WHERE {new T().GetPrimaryColumn().Name} = {Driver.Dialect.ParameterPrefix}id";
            return QueryFirst<T>(sql, new { id });
        }

        /// <summary>
        /// 执行SQL，返回查询记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public T QueryFirst<T>(string sql, object param = null) where T : BaseEntity, new()
        {
            return DbClient.QueryFirst<T>(sql, param);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public DataTable Query(string sql, object param = null)
        {
            return DbClient.Query(sql, param);
        }

        /// <summary>
        /// 查询数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public int QueryCount(string sql, object param = null)
        {
            return ConvertUtil.ConToInt(this.ExecuteScalar(sql, param));
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object param = null)
        {
            return DbClient.Query<T>(sql, param);
        }

        /// <summary>
        /// 查询多条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <param name="orderby"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object param, string orderby, int pageSize, int pageIndex) where T : BaseEntity, new()
        {
            if (!string.IsNullOrEmpty(orderby))
            {
                if (!orderby.Contains("order by", StringComparison.OrdinalIgnoreCase))
                    sql += $" ORDER BY {orderby}";
                else
                    sql += $" {orderby}";
            }

            sql += $" {DbClient.Driver.Dialect.GetPageSql(pageIndex, pageSize)}";
            return Query<T>(sql, param);
        }

        /// <summary>
        /// 查询多条记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <param name="orderby"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object param, string orderby, int pageSize, int pageIndex, out int recordCount) where T : BaseEntity, new()
        {
            var recordCountSql = $"SELECT COUNT(1) FROM ({sql}) AS table1";
            recordCount = Convert.ToInt32(this.ExecuteScalar(recordCountSql, param));
            var data = Query<T>(sql, param, orderby, pageSize, pageIndex);
            return data;
        }

        /// <summary>
        /// 根据 id 批量查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(IList<string> ids) where T : BaseEntity, new()
        {
            var paramList = new Dictionary<string, object>();
            var tableName = new T().GetEntityName();
            var primaryKey = new T().GetPrimaryColumn().Name;
            var inClause = string.Join(",", ids.Select((id, index) => $"{Driver.Dialect.ParameterPrefix}id" + index));
            var sql = $"SELECT * FROM {tableName} WHERE {primaryKey} IN ({inClause})";
            var count = 0;
            ids.Each((id) => paramList.Add($"{Driver.Dialect.ParameterPrefix}id{count++}", id));
            return Query<T>(sql, paramList);
        }
        #endregion

        #region Execute
        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        public int Execute(string sql, object? param = null)
        {
            return DbClient.Execute(sql, param);
        }

        /// <summary>
        /// 执行Sql返回第一行第一列记录
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, object param = null)
        {
            return DbClient.ExecuteScalar(sql, param);
        }

        /// <summary>
        /// 执行SQL文件
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sqlFile"></param>
        /// <returns></returns>
        public int ExecuteSqlScript(string sqlFile)
        {
            int returnValue = -1;
            int sqlCount = 0, errorCount = 0;
            if (!File.Exists(sqlFile))
            {
                Logger?.LogError($"文件({sqlFile})不存在");
                return -1;
            }
            using (StreamReader sr = new StreamReader(sqlFile))
            {
                string line = string.Empty;
                char spaceChar = ' ';
                string newLIne = "\r\n", semicolon = ";";
                string sprit = "/", whiffletree = "-";
                string sql = string.Empty;
                do
                {
                    line = sr.ReadLine();
                    // 文件结束
                    if (line == null) break;
                    // 跳过注释行
                    if (line.StartsWith(sprit) || line.StartsWith(whiffletree)) continue;
                    // 去除右边空格
                    line = line.TrimEnd(spaceChar);
                    sql += line;
                    // 以分号(;)结尾，则执行SQL
                    if (sql.EndsWith(semicolon))
                    {
                        try
                        {
                            sqlCount++;
                            Execute(sql);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            Logger.LogError(sql + newLIne + ex.Message, ex);
                        }
                        sql = string.Empty;
                    }
                    else
                    {
                        // 添加换行符
                        if (sql.Length > 0) sql += newLIne;
                    }
                } while (true);
            }
            if (sqlCount > 0 && errorCount == 0)
                returnValue = 1;
            if (sqlCount == 0 && errorCount == 0)
                returnValue = 0;
            else if (sqlCount > errorCount && errorCount > 0)
                returnValue = -1;
            else if (sqlCount == errorCount)
                returnValue = -2;
            return returnValue;
        }
        #endregion

        #region Bulk CRUD
        /// <summary>
        /// 批量创建
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dataList"></param>
        public void BulkCreate<TEntity>(List<TEntity> dataList) where TEntity : BaseEntity, new()
        {
            if (dataList.IsEmpty()) return;

            var t = new TEntity();
            var tableName = t.GetEntityName();
            var primaryKey = t.GetPrimaryColumn().Name;
            var dt = Query($"select * from {tableName} WHERE 1 <> 1");

            BulkCreate(tableName, primaryKey, dataList.ToDataTable(dt.Columns));
        }

        /// <summary>
        /// 批量创建
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="primaryKeyName">主键名</param>
        /// <param name="dataTable">数据</param>
        public void BulkCreate(string tableName, string primaryKeyName, DataTable dataTable)
        {
            if (dataTable.IsEmpty())
            {
                return;
            }

            ExecuteTransaction(() =>
            {
                // 1. 创建临时表
                var tempName = DbClient.CreateTemporaryTable(tableName);

                // 2. 拷贝数据到临时表
                DbClient.BulkCopy(dataTable, tempName);

                // 3. 将临时表数据插入到目标表中
                DbClient.Execute(string.Format("INSERT INTO {0} SELECT * FROM {1} WHERE NOT EXISTS(SELECT 1 FROM {0} WHERE {0}.{2} = {1}.{2})", tableName, tempName, primaryKeyName));

                // 4. 删除临时表
                DbClient.DropTable(tempName);
            });
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dataList"></param>
        public void BulkUpdate<TEntity>(List<TEntity> dataList) where TEntity : BaseEntity, new()
        {
            if (dataList.IsEmpty()) return;

            var t = new TEntity();
            var mainKeyName = t.GetPrimaryColumn().Name; // 主键
            var tableName = t.GetEntityName(); // 表名
            var dt = DbClient.Query($"SELECT * FROM {tableName} WHERE 1 <> 1");

            BulkUpdate(tableName, mainKeyName, dataList.ToDataTable(dt.Columns));
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="primaryKeyName"></param>
        /// <param name="dataTable"></param>
        public void BulkUpdate(string tableName, string primaryKeyName, DataTable dataTable)
        {
            if (dataTable.IsEmpty())
            {
                return;
            }

            ExecuteTransaction(() =>
            {
                // 1. 创建临时表
                var tempTableName = DbClient.CreateTemporaryTable(tableName);

                // 2. 拷贝数据到临时表
                DbClient.BulkCopy(dataTable, tempTableName);

                // 3. 获取更新字段
                var updateFieldList = new List<string>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    // 主键去除
                    if (!column.ColumnName.Equals(primaryKeyName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        updateFieldList.Add(column.ColumnName);
                    }
                }

                // 4. 拼接Set语句
                var updateFieldSql = updateFieldList.Select(item => string.Format(" {1} = {0}.{1} ", tempTableName, item)).Aggregate((a, b) => a + " , " + b);

                // 5. 更新
                DbClient.Execute($@"
UPDATE {tableName}
SET {updateFieldSql} FROM {tempTableName}
WHERE {tableName}.{primaryKeyName} = {tempTableName}.{primaryKeyName}
AND {tempTableName}.{primaryKeyName} IS NOT NULL
");

                // 6. 删除临时表
                DbClient.DropTable(tempTableName);
            });
        }

        /// <summary>
        /// 批量创建或更新
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dataList"></param>
        /// <param name="updateFieldList"></param>
        public void BulkCreateOrUpdate<TEntity>(List<TEntity> dataList, List<string> updateFieldList = null) where TEntity : BaseEntity, new()
        {
            if (dataList.IsEmpty()) return;

            var primaryKeyName = new TEntity().GetPrimaryColumn().Name; // 主键
            var tableName = new TEntity().GetEntityName(); // 表名
            var dt = DbClient.Query($"SELECT * FROM {tableName} WHERE 1 <> 1");

            BulkCreateOrUpdate(tableName, primaryKeyName, dataList.ToDataTable(dt.Columns), updateFieldList);
        }

        /// <summary>
        /// 批量创建或更新
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="primaryKeyName"></param>
        /// <param name="dataTable"></param>
        /// <param name="updateFieldList"></param>
        public void BulkCreateOrUpdate(string tableName, string primaryKeyName, DataTable dataTable, List<string> updateFieldList = null)
        {
            if (dataTable.IsEmpty()) return;

            // 1. 创建临时表
            var tempTableName = DbClient.CreateTemporaryTable(tableName);

            // 2. 拷贝数据到临时表
            DbClient.BulkCopy(dataTable, tempTableName);

            // 3. 获取更新字段
            if (updateFieldList.IsEmpty())
            {
                updateFieldList = new List<string>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    // 主键去除
                    if (!column.ColumnName.Equals(primaryKeyName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        updateFieldList.Add(column.ColumnName);
                    }
                }
            }

            // 4. 拼接Set语句
            var updateFieldSql = updateFieldList.Select(item => string.Format(" {1} = {0}.{1} ", tempTableName, item)).Aggregate((a, b) => a + " , " + b);

            // 5. 更新
            DbClient.Execute($@"
UPDATE {tableName}
SET {updateFieldSql} FROM {tempTableName}
WHERE {tableName}.{primaryKeyName} = {tempTableName}.{primaryKeyName}
AND {tempTableName}.{primaryKeyName} IS NOT NULL
");
            // 6. 新增
            DbClient.Execute($@"
INSERT INTO {tableName}
SELECT * FROM {tempTableName}
WHERE NOT EXISTS(SELECT 1 FROM {tableName} WHERE {tableName}.{primaryKeyName} = {tempTableName}.{primaryKeyName})
AND {tempTableName}.{primaryKeyName} IS NOT NULL
");

            // 7. 删除临时表
            DbClient.DropTable(tempTableName);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="entities"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void BulkDelete<TEntity>(List<TEntity> dataList) where TEntity : BaseEntity, new()
        {
            if (dataList.IsEmpty())
            {
                return;
            }

            var t = new TEntity();
            var tableName = t.GetEntityName();
            var primaryKeyName = t.GetPrimaryColumn().Name;
            var ids = string.Join(",", dataList.Select(item => "'" + item.GetPrimaryColumn().Value + "'"));

            ExecuteTransaction(() =>
            {
                DbClient.Execute($"DELETE FROM {tableName} WHERE {primaryKeyName} IN ({ids})");
            });
        }
        #endregion
    }
}
