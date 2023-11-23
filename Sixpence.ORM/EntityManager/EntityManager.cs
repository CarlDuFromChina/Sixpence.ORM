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
using static Dapper.SqlMapper;
using System.Text;

namespace Sixpence.ORM
{
    /// <summary>
    /// 实体管理
    /// </summary>
    public class EntityManager : IEntityManager, IDisposable
    {
        private DbClient _dbClient;
        private readonly IServiceProvider Provider;
        private readonly ILogger<EntityManager>? Logger;
        private readonly bool EnableLogging = SormAppBuilderExtensions.BuilderOptions.EnableLogging;

        public IDbDriver Driver => _dbClient.Driver;
        public DbClient DbClient => _dbClient;

        public EntityManager()
        {
            var dbSetting = SormServiceCollectionExtensions.Options?.DbSetting;
            _dbClient = new DbClient(dbSetting.Driver, dbSetting.ConnectionString, dbSetting.CommandTimeout);
            Provider = ServiceContainer.Provider;
            Logger = Provider.GetService<ILoggerFactory>()?.CreateLogger<EntityManager>();
        }

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
                var context = new EntityManagerPluginContext() { Entity = entity, EntityManager = this, Action = EntityAction.PreCreate, EntityName = entity.EntityMap.Table };
                ServiceContainer.Provider.GetServices<IEntityManagerBeforeCreateOrUpdate>()?
                    .Each(item => item.Execute(context));
                if (usePlugin)
                {
                    ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                        .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.EntityMap.Table))
                        .Each(item => item.Execute(context));
                }
                #endregion

                var sql = "INSERT INTO {0}({1}) Values({2})";
                var attrs = new List<string>();
                var values = new List<object>();
                var paramList = new Dictionary<string, object>();
                foreach (var attr in EntityCommon.GetDbColumns(entity))
                {
                    var attrName = attr.Key; // 列名
                    var keyValue = Driver.Dialect.HandleParameter($"{Driver.Dialect.ParameterPrefix}{attrName}", attr.Value); // 值
                    attrs.Add(attrName);
                    values.Add(keyValue.name);
                    paramList.Add(keyValue.name, keyValue.value);
                }
                sql = string.Format(sql, entity.EntityMap.Table, string.Join(",", attrs), string.Join(",", values));
                this.Execute(sql, paramList);

                #region 创建后 Plugin
                if (usePlugin)
                {
                    context.Action = EntityAction.PostCreate;
                    ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                        .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.EntityMap.Table))
                        .Each(item => item.Execute(context));
                }
                #endregion

                return entity.PrimaryColumn.Value.ToString() ?? "";
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
            var entity = ServiceContainer.Provider.GetServices<IEntity>().FirstOrDefault(item => item.EntityMap.Table == entityName) as BaseEntity;
            AssertUtil.IsNull(entity, $"未找到实体：{entityName}");
            var dataList = DbClient.Query($"SELECT * FROM {entityName} WHERE {entity.PrimaryColumn.DbPropertyMap.Name} = {Driver.Dialect.ParameterPrefix}id", new { id });

            if (dataList.Rows.Count == 0) return 0;

            var attributes = dataList.Rows[0].ToDictionary(dataList.Columns);
            attributes.Each(item => EntityCommon.SetDbColumnValue(entity, item.Key, item.Value.Equals(DBNull.Value) ? null : item.Value));

            var plugins = ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.EntityMap.Table));

            plugins?.Each(item => item.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entityName, Action = EntityAction.PreDelete }));

            var sql = $"DELETE FROM {entityName} WHERE {entity.PrimaryColumn.DbPropertyMap.Name} = {Driver.Dialect.ParameterPrefix}id";
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
                    .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, entity.EntityMap.Table));

                plugins?.Each(item => item.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.EntityMap.Table, Action = EntityAction.PreDelete }));
                
                var sql = $"DELETE FROM {entity.EntityMap.Table} WHERE {entity.PrimaryColumn.DbPropertyMap.Name} = {Driver.Dialect.ParameterPrefix}id";
                int result = this.Execute(sql, new { id = entity.PrimaryColumn?.Value });

                plugins?.Each(item => item.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.EntityMap.Table, Action = EntityAction.PostDelete }));
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
SELECT * FROM {entity.EntityMap.Table}
WHERE {entity.PrimaryColumn.DbPropertyMap.Name} = {Driver.Dialect.ParameterPrefix}id;
";
            var dataList = this.Query(sql, new { id = entity.PrimaryColumn?.Value });

            if (dataList != null && dataList.Rows.Count > 0)
                Update(entity);
            else
                Create(entity);

            return entity.PrimaryColumn?.Value?.ToString() ?? "";
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
                var tableName = entity.EntityMap.Table;
                var primaryKeyName = entity.PrimaryColumn.DbPropertyMap.Name;
                var prefix = Driver.Dialect.ParameterPrefix;

                #region 更新前 Plugin
                var context = new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = tableName, Action = EntityAction.PreUpdate };
                ServiceContainer.Provider.GetServices<IEntityManagerBeforeCreateOrUpdate>()
                    .Each(item => item.Execute(context));

                ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                    .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, tableName))
                    .Each(item => item.Execute(context));
                #endregion

                var paramList = new Dictionary<string, object>();
                var setValueSql = "";
                var whereSql = "";

                #region 处理字段SQL
                var attributes = new List<string>();
                foreach (var item in entity.Columns)
                {
                    var parameterName = $"{prefix}{item.DbPropertyMap.Name}"; // 定义参数化 @user_name

                    // 处理特殊类型
                    var parameter = Driver.Dialect.HandleParameter(parameterName, item.Value);
                    if (string.IsNullOrEmpty(parameter.name))
                        parameter.name = parameterName;
                    if (parameter.value == null)
                        parameter.value = item.Value;

                    paramList.Add(parameter.name, parameter.value); // :user_name, 'admin'
                    if (item.Name != entity.PrimaryColumn.Name)
                    {
                        attributes.Add($"{item.DbPropertyMap.Name} = {parameter.name}"); // user_name = :user_name
                    }
                    else
                    {
                        whereSql = $@"{primaryKeyName} = {parameter.name}";
                    }
                }
                setValueSql = string.Join(',', attributes);
                #endregion

                var sql = $@"UPDATE {tableName} SET {setValueSql} WHERE {primaryKeyName} = {prefix}{primaryKeyName};";
                var result = this.Execute(sql, paramList);

                #region 更新后 Plugin
                context.Action = EntityAction.PostUpdate;
                ServiceContainer.Provider.GetServices<IEntityManagerPlugin>()
                    .Where(item => EntityCommon.MatchEntityManagerPlugin(item.GetType().Name, tableName))
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
            var t = new T();
            var tableName = t.EntityMap.Table;
            var primaryKeyName = t.PrimaryColumn.DbPropertyMap.Name;
            var sql = $"SELECT * FROM {tableName} WHERE {primaryKeyName} = {Driver.Dialect.ParameterPrefix}id";
            return QueryFirst<T>(sql, new { id });
        }

        /// <summary>
        /// 执行SQL，返回查询记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public T QueryFirst<T>(string sql, object? param = null) where T : BaseEntity, new()
        {
            return DbClient.QueryFirst<T>(sql, param);
        }

        /// <summary>
        /// 根据筛选条件查询返回记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        public T QueryFirst<T>(object? param = null) where T : BaseEntity, new()
        {
            var entityMap = new T().EntityMap;
            var sql = new StringBuilder($"SELECT * FROM {entityMap.Schema}.{entityMap.Table} WHERE 1 = 1");
            param
                .ToDictionary()
                .Each(item => sql.Append($" AND {item.Key} = {Driver.Dialect.ParameterPrefix}{item.Key}"));

            return DbClient.QueryFirst<T>(sql.ToString(), param);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public DataTable Query(string sql, object? param = null)
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
        public int QueryCount(string sql, object? param = null)
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
        public IEnumerable<T> Query<T>(string sql, object? param = null)
        {
            return DbClient.Query<T>(sql, param);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(object? param = null) where T : BaseEntity, new()
        {
            var entityMap = new T().EntityMap;
            var sql = new StringBuilder($"select * from {entityMap.Schema}.{entityMap.Table} where 1 = 1");
            param
                .ToDictionary()
                .Each(item => sql.Append($" and {item.Key} = {Driver.Dialect.ParameterPrefix}{item.Key}"));

            return DbClient.Query<T>(sql.ToString(), param);
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
            var tableName = new T().EntityMap.Table;
            var primaryKey = new T().PrimaryColumn?.DbPropertyMap.Name;
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
        public object ExecuteScalar(string sql, object? param = null)
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
                            if (EnableLogging)
                                Logger?.LogError(sql + newLIne + ex.Message, ex);
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
            var tableName = t.EntityMap.Table;
            var primaryKey = t.PrimaryColumn?.DbPropertyMap.Name;
            var dt = Query($"select * from {tableName} WHERE 1 <> 1");
            dataList.ForEach(entity =>
            {
                var context = new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = tableName, Action = EntityAction.PreCreate };
                ServiceContainer.Provider.GetServices<IEntityManagerBeforeCreateOrUpdate>()
                    .Each(item => item.Execute(context));
            });
            var data = EntityCommon.ParseToDataTable(dataList, dt.Columns);
            BulkCreate(tableName, primaryKey, data);
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
            var mainKeyName = t.PrimaryColumn?.DbPropertyMap.Name; // 主键
            var tableName = t.EntityMap.Table; // 表名
            var dt = DbClient.Query($"SELECT * FROM {tableName} WHERE 1 <> 1");
            dataList.ForEach(entity =>
            {
                var context = new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = tableName, Action = EntityAction.PreUpdate };
                ServiceContainer.Provider.GetServices<IEntityManagerBeforeCreateOrUpdate>()
                    .Each(item => item.Execute(context));
            });
            BulkUpdate(tableName, mainKeyName, EntityCommon.ParseToDataTable(dataList, dt.Columns));
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

            var primaryKeyName = new TEntity().PrimaryColumn?.DbPropertyMap.Name; // 主键
            var tableName = new TEntity().EntityMap.Table; // 表名
            var dt = DbClient.Query($"SELECT * FROM {tableName} WHERE 1 <> 1");

            BulkCreateOrUpdate(tableName, primaryKeyName, EntityCommon.ParseToDataTable(dataList, dt.Columns), updateFieldList);
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
            var dialect = Driver.Dialect;
            var tableName = t.EntityMap.Table;
            var primaryKeyName = t.PrimaryColumn.DbPropertyMap.Name;
            var idList = dataList.Select(item => item.PrimaryColumn.Value.ToString()).ToArray();

            var sql = $"DELETE FROM {tableName} WHERE {primaryKeyName} {dialect.GetInClauseSql(dialect.ParameterPrefix + "ids")}";
            DbClient.Execute(sql, new { ids = idList });
        }
        #endregion
    }
}
