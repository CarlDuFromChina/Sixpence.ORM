using Sixpence.Common;
using Sixpence.ORM.EntityManager;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Repository
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
            if (string.IsNullOrEmpty(entity.PrimaryKey.Value))
            {
                entity.SetAttributeValue(entity.PrimaryKey.Name, Guid.NewGuid().ToString());
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
            var id = entity.PrimaryKey.Value ?? Guid.NewGuid().ToString();
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
            Manager.Delete(new E().EntityName, id);
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
            var result = ParseConditions(conditions?.ToDictionary());
            return Manager.Query<E>($"SELECT * FROM {new E().EntityName} WHERE 1 = 1 {result.WhereSQL}", result.ParamList);
        }

        /// <summary>
        /// 根据id查询
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public virtual IEnumerable<E> FindByIds(string ids)
        {
            var paramList = new Dictionary<string, object>();
            var sql = $"SELECT * FROM {new E().EntityName} WHERE 1 = 1";
            if (!string.IsNullOrEmpty(ids))
            {
                sql += $" AND {new E().PrimaryKey.Name} IN (in@ids)";
                paramList.Add("in@ids", ids);
            }
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
            return Manager.Query<E>(sql, param?.ToDictionary());
        }

        /// <summary>
        /// 更新实体记录
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entity"></param>
        public virtual void Update(E entity)
        {
            if (string.IsNullOrEmpty(entity?.PrimaryKey.Value))
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
            var sql = $"SELECT * FROM {new E().EntityName} WHERE 1 = 1 {result.WhereSQL}";
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
            if (!conditions.IsEmpty())
            {
                conditions.Distinct().Each(item =>
                {
                    whereSQL += $" AND {item.Key} = @{item.Key}";
                    paramList.Add($"@{item.Key}", item.Value);
                });
            }
            return (whereSQL, paramList);
        }

        /// <summary>
        /// 清空表
        /// </summary>
        public void Clear()
        {
            Manager.Execute($"TRUNCATE TABLE {new E().EntityName}");
        }
    }
}
