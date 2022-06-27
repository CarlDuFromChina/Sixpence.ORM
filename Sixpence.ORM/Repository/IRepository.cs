using Sixpence.ORM.Entity;
using Sixpence.ORM.EntityManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Repository
{
    public interface IRepository<E> where E : BaseEntity, new()
    {
        IEntityManager Manager { get; set; }

        /// <summary>
        /// 创建实体，返回实体ID
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        string Create(E entity);

        /// <summary>
        /// 创建或更新实体，返回实体ID
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        string Save(E entity);

        /// <summary>
        /// 根据实体ID删除实体
        /// </summary>
        /// <param name="id"></param>
        void Delete(string id);

        /// <summary>
        /// 根据实体ID批量删除实体
        /// </summary>
        /// <param name="ids"></param>
        void Delete(IEnumerable<string> ids);

        /// <summary>
        /// 查找实体
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        IEnumerable<E> Find(object conditions = null);

        /// <summary>
        /// 根据实体ID批量查询实体
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        IEnumerable<E> FindByIds(string ids);

        /// <summary>
        /// 根据实体ID查询实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        E FindOne(string id);

        /// <summary>
        /// 根据传入条件查询实体，返回匹配的第一条记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        E FindOne(object conditions = null);

        /// <summary>
        /// 原生SQL查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        IEnumerable<E> Query(string sql, object param = null);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="entity"></param>
        void Update(E entity);

        /// <summary>
        /// 清除表
        /// </summary>
        void Clear();
    }
}
