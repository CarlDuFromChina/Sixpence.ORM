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
            var id = entity.PrimaryKey.Value;
            var isExist = SingleQuery(id) != null;
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
        public virtual IEnumerable<E> Query()
        {
            return Manager.RetrieveMultiple<E>($"SELECT * FROM {new E().EntityName}");
        }

        /// <summary>
        /// 查询单条记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual E SingleQuery(string id)
        {
            return Manager.Retrieve<E>(id);
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
    }
}
