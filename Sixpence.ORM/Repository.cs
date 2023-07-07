using Sixpence.Common;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sixpence.ORM.Interface;

namespace Sixpence.ORM
{
    public class Repository<E> : IRepository<E>
        where E : BaseEntity, new()
    {
        #region 构造函数
        public Repository()
        {
            Manager = EntityManagerFactory.GetManager();
        }

        public Repository(IEntityManager manager)
        {
            Manager = manager;
        }
        #endregion

        public IEntityManager Manager { get; set; }

        /// <summary>
        /// 创建实体记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual string Create(E entity)
        {
            if (string.IsNullOrEmpty(entity.GetPrimaryColumn().Value))
            {
                entity.SetAttributeValue(entity.GetPrimaryColumn().Name, entity.NewId());
            }
            var id = Manager.Create(entity);
            return id;
        }

        /// <summary>
        /// 创建或更新历史记录
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual string Save(E entity)
        {
            var id = entity.GetPrimaryColumn().Value ?? entity.NewId();
            var isExist = FindOne(id) != null;
            if (isExist)
            {
                Update(entity);
            }
            else
            {
                id = Create(entity);
            }
            return id;
        }

        /// <summary>
        /// 根据id删除实体
        /// </summary>
        /// <param name="id"></param>
        public virtual void Delete(string id)
        {
            Manager.Delete(new E().GetEntityName(), id);
        }

        /// <summary>
        /// 删除历史记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        public virtual void Delete(IEnumerable<string> ids)
        {
            Manager.ExecuteTransaction(() => ids.Each(id => Delete(id)));
        }

        /// <summary>
        /// 获取实体记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IEnumerable<E> Find(object conditions = null)
        {
            var sql = $"SELECT * FROM {new E().GetEntityName()}";

            if (conditions == null)
                return Manager.Query<E>(sql);

            var result = ParseConditions(conditions?.ToDictionary());
            return Manager.Query<E>($"{sql} WHERE 1 = 1 {result.WhereSQL}", result.ParamList);
        }

        /// <summary>
        /// 根据id查询，多个id用逗号分隔
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public virtual IEnumerable<E> FindByIds(string ids)
        {
            var paramList = new Dictionary<string, object>();
            var tableName = new E().GetEntityName();
            var primaryKey = new E().GetPrimaryColumn().Name;
            var inClause = string.Join(",", ids.Select((id, index) => $"{Manager.Driver.Dialect.ParameterPrefix}id" + index));
            var sql = $"SELECT * FROM {tableName} WHERE {primaryKey} IN ({inClause})";
            var count = 0;
            ids.Split(',')
                .Each((id) =>
                {
                    paramList.Add($"{Manager.Driver.Dialect.ParameterPrefix}id{count++}", id);
                });
            return Query(sql, paramList);
        }

        /// <summary>
        /// 执行原生SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public virtual IEnumerable<E> Query(string sql, object param = null)
        {
            return Manager.Query<E>(sql, param);
        }

        /// <summary>
        /// 更新实体记录
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entity"></param>
        public virtual void Update(E entity)
        {
            if (string.IsNullOrEmpty(entity?.GetPrimaryColumn().Value))
            {
                return;
            }

            Manager.Update(entity);
        }

        /// <summary>
        /// 根据id查找实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public E FindOne(string id)
        {
            return Manager.QueryFirst<E>(id);
        }

        /// <summary>
        /// 根据条件查询实体
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public E FindOne(object conditions = null)
        {
            var result = ParseConditions(conditions?.ToDictionary());
            var sql = $"SELECT * FROM {new E().GetEntityName()} WHERE 1 = 1 {result.WhereSQL}";
            return Manager.QueryFirst<E>(sql, result.ParamList);
        }

        /// <summary>
        /// 转换条件为SQL原生where语句
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        internal (string WhereSQL, Dictionary<string, object> ParamList) ParseConditions(IDictionary<string, object> conditions)
        {
            string whereSQL = string.Empty;
            Dictionary<string, object> paramList = new Dictionary<string, object>();
            var dialect = Manager.Driver.Dialect;
            if (!conditions.IsEmpty())
            {
                conditions.Distinct().Each(item =>
                {
                    whereSQL += $" AND {item.Key} = {dialect.ParameterPrefix}{item.Key}";
                    paramList.Add($"{dialect.ParameterPrefix}{item.Key}", item.Value);
                });
            }
            return (whereSQL, paramList);
        }

        /// <summary>
        /// 清空表
        /// </summary>
        public void Clear()
        {
            Manager.Execute($"TRUNCATE TABLE {new E().GetEntityName()}");
        }
    }
}
