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
        string Create(E entity);
        string Save(E entity);
        void Delete(string id);
        void Delete(IEnumerable<string> ids);
        IEnumerable<E> Find(IDictionary<string, object> conditions = null);
        IEnumerable<E> FindByIds(string ids);
        E FindOne(string id);
        E FindOne(IDictionary<string, object> conditions = null);
        IEnumerable<E> Query(string sql, IDictionary<string, object> paramList = null);
        void Update(E entity);
        void Clear();
    }
}
