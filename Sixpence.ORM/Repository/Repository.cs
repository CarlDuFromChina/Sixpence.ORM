using Sixpence.Common;
using Sixpence.ORM.Broker;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public class Repository<E>
        where E : BaseEntity, new()
    {
        #region 构造函数
        public Repository()
        {
            Broker = PersistBrokerFactory.GetPersistBroker();
        }

        public Repository(IPersistBroker broker)
        {
            Broker = broker;
        }
        #endregion

        public IPersistBroker Broker { get; set; }

        /// <summary>
        /// 创建实体记录
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual string Create(E entity)
        {
            if (string.IsNullOrEmpty(entity.PrimaryKey.Value))
            {
                return "";
            }
            var id = Broker.Create(entity);
            return id;
        }

        /// <summary>
        /// 创建或更新历史记录
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual string CreateOrUpdateData(E entity)
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
            Broker.Delete(new E().EntityName, id);
        }

        /// <summary>
        /// 删除历史记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ids"></param>
        public virtual void Delete(IEnumerable<string> ids)
        {
            Broker.ExecuteTransaction(() => ids.Each(id => Delete(id)));
        }


        /// <summary>
        /// 获取实体记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IEnumerable<E> Query()
        {
            return Broker.RetrieveMultiple<E>($"SELECT * FROM {new E().EntityName}");
        }

        /// <summary>
        /// 查询单条记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual E SingleQuery(string id)
        {
            return Broker.Retrieve<E>(id);
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

            Broker.Update(entity);
        }
    }
}
