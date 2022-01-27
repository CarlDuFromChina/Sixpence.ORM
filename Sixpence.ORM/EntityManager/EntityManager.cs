using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.Common.Utils;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.DbClient;
using Sixpence.ORM.Driver;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Sixpence.Common.Logging;

namespace Sixpence.ORM.EntityManager
{
    /// <summary>
    /// 实体管理
    /// </summary>
    internal class EntityManager : IEntityManager, IDisposable
    {
        internal EntityManager(string connectionString, DriverType driverType = DriverType.Postgresql)
        {
            _dbClient = new DbClientProxy();
            _dbClient.Initialize(connectionString, driverType);
        }

        public IDbDriver Driver => DbClient.Driver;

        IDbClient _dbClient;
        public IDbClient DbClient => _dbClient;

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
                var context = new EntityManagerPluginContext() { Entity = entity, EntityManager = this, Action = EntityAction.PreCreate, EntityName = entity.EntityName };
                ServiceContainer.ResolveAll<IEntityManagerBeforeCreateOrUpdate>()?
                    .Each(item => item.Execute(context));
                if (usePlugin)
                {
                    ServiceContainer.ResolveAll<IEntityManagerPlugin>(item => item.StartsWith(entity.EntityName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
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
                    var keyValue = ParseSqlUtil.GetSpecialValue($"@{attrName}", attr.Value); // 值
                    attrs.Add(attrName);
                    values.Add(keyValue.name);
                    paramList.Add(attrName, keyValue.value);
                }
                sql = string.Format(sql, entity.EntityName, string.Join(",", attrs), string.Join(",", values));
                this.Execute(sql, paramList);

                #region 创建后 Plugin
                if (usePlugin)
                {
                    context.Action = EntityAction.PostCreate;
                    ServiceContainer.ResolveAll<IEntityManagerPlugin>(item => item.StartsWith(entity.EntityName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                        .Each(item => item.Execute(context));
                }
                #endregion

                return entity.PrimaryKey.Value;
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
            var entity = ServiceContainer.Resolve<IEntity>(key => key.ToLower().Equals(entityName.Replace("_", "").ToLower())) as BaseEntity;
            AssertUtil.CheckNull<SpException>(entity, $"未找到实体：{entityName}", "FB2369B2-6B3E-471D-986A-7719330DBF5E");
            var dataList = DbClient.Query($"SELECT * FROM {entityName} WHERE {entity.PrimaryKey.Name} = @id", new Dictionary<string, object>() { { "@id", id } });

            if (dataList.Rows.Count == 0) return 0;

            var attributes = dataList.Rows[0].ToDictionary(dataList.Columns);
            attributes.Each(item => entity.SetAttributeValue(item.Key, item.Value.Equals(DBNull.Value) ? null : item.Value));
            var plugin = ServiceContainer.Resolve<IEntityManagerPlugin>(item => item.StartsWith(entityName.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
            plugin?.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entityName, Action = EntityAction.PreDelete });

            var sql = "DELETE FROM {0} WHERE {1} = @id";
            sql = string.Format(sql, entityName, entity.PrimaryKey.Name);
            int result = this.Execute(sql, new Dictionary<string, object>() { { "@id", id } });

            plugin?.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entityName, Action = EntityAction.PostDelete });
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
                var plugin = ServiceContainer.Resolve<IEntityManagerPlugin>(item => item.StartsWith(entity.EntityName.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
                plugin?.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.EntityName, Action = EntityAction.PreDelete });
                var sql = "DELETE FROM {0} WHERE {1} = @id";
                sql = string.Format(sql, entity.EntityName, entity.PrimaryKey.Name);
                int result = this.Execute(sql, new Dictionary<string, object>() { { "@id", entity.PrimaryKey.Value } });
                plugin?.Execute(new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.EntityName, Action = EntityAction.PostDelete });
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
SELECT * FROM {entity.EntityName}
WHERE {entity.PrimaryKey.Name} = @id;
";
            var dataList = this.Query(sql, new Dictionary<string, object>() { { "@id", entity.PrimaryKey.Value } });

            if (dataList != null && dataList.Rows.Count > 0)
                Update(entity);
            else
                Create(entity);

            return entity.PrimaryKey.Value;
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
                var context = new EntityManagerPluginContext() { EntityManager = this, Entity = entity, EntityName = entity.EntityName, Action = EntityAction.PreUpdate };
                ServiceContainer.ResolveAll<IEntityManagerBeforeCreateOrUpdate>()?
                    .Each(item => item.Execute(context));

                ServiceContainer.ResolveAll<IEntityManagerPlugin>(item => item.StartsWith(entity.EntityName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                    .Each(item => item.Execute(context));
                #endregion

                var sql = @"
UPDATE {0} SET {1} WHERE {2} = @id;
";
                var paramList = new Dictionary<string, object>() { { "@id", entity.PrimaryKey.Value } };

                #region 处理属性
                var attributes = new List<string>();
                int count = 0;
                foreach (var item in entity.GetAttributes())
                {
                    if (item.Key != "id" && item.Key != entity.PrimaryKey.Name)
                    {
                        var keyValue = ParseSqlUtil.GetSpecialValue($"@param{count}", item.Value);
                        paramList.Add($"@param{count}", keyValue.value);
                        attributes.Add($"{ item.Key} = {keyValue.name}");
                        count++;
                    }
                }
                #endregion
                sql = string.Format(sql, entity.EntityName, string.Join(",", attributes), entity.PrimaryKey.Name);
                var result = this.Execute(sql, paramList);

                #region 更新后 Plugin
                context.Action = EntityAction.PostUpdate;
                ServiceContainer.ResolveAll<IEntityManagerPlugin>(item => item.StartsWith(entity.EntityName.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
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
            var sql = $"SELECT * FROM {new T().EntityName} WHERE {new T().PrimaryKey.Name} =@id";
            return QueryFirst<T>(sql, new Dictionary<string, object>() { { "@id", id } });
        }

        /// <summary>
        /// 执行SQL，返回查询记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public T QueryFirst<T>(string sql, IDictionary<string, object> paramList) where T : BaseEntity, new()
        {
            return DbClient.QueryFirst<T>(sql, paramList);
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public DataTable Query(string sql, IDictionary<string, object> paramList = null)
        {
            return DbClient.Query(sql, paramList);
        }

        /// <summary>
        /// 查询数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public int QueryCount(string sql, IDictionary<string, object> paramList = null)
        {
            return ConvertUtil.ConToInt(this.ExecuteScalar(sql, paramList));
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, IDictionary<string, object> paramList = null)
        {
            return DbClient.Query<T>(sql, paramList);
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
        public IEnumerable<T> Query<T>(string sql, IDictionary<string, object> paramList, string orderby, int pageSize, int pageIndex) where T : BaseEntity, new()
        {
            if (!string.IsNullOrEmpty(orderby))
            {
                if (!orderby.Contains("order by", StringComparison.OrdinalIgnoreCase))
                    sql += $" ORDER BY {orderby}";
                else
                    sql += $" {orderby}";
            }

            DbClient.Driver.AddLimit(ref sql, pageIndex, pageSize);
            return Query<T>(sql, paramList);
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
        public IEnumerable<T> Query<T>(string sql, IDictionary<string, object> paramList, string orderby, int pageSize, int pageIndex, out int recordCount) where T : BaseEntity, new()
        {
            var recordCountSql = $"SELECT COUNT(1) FROM ({sql}) AS table1";
            recordCount = Convert.ToInt32(this.ExecuteScalar(recordCountSql, paramList));
            var data = Query<T>(sql, paramList, orderby, pageSize, pageIndex);
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
            var sql = $@"
SELECT
	*
FROM
	{new T().EntityName}
WHERE 
	{new T().PrimaryKey.Name} IN (@ids)";
            var parmas = new Dictionary<string, object>();
            var count = 0;
            ids.ToList().ForEach(item =>
            {
                parmas.Add($"@id{++count}", ids[count - 1]);
            });
            sql = sql.Replace("@ids", string.Join(",", parmas.Keys));
            var data = Query<T>(sql, parmas);
            return data;
        }
        #endregion

        #region Execute
        /// <summary>
        /// 执行Sql
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        public int Execute(string sql, IDictionary<string, object> paramList = null)
        {
            return DbClient.Execute(sql, paramList);
        }

        /// <summary>
        /// 执行Sql返回第一行第一列记录
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, IDictionary<string, object> paramList = null)
        {
            return DbClient.ExecuteScalar(sql, paramList);
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
                LogUtils.Error($"文件({sqlFile})不存在");
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
                            Execute(sql, null);
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            LogUtils.Error(sql + newLIne + ex.Message, ex);
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
    }
}
