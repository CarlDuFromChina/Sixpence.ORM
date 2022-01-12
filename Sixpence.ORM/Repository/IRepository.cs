using Sixpence.ORM.EntityManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Repository
{
    public interface IRepository<E>
    {
        IEntityManager Manager { get; set; }
        string Create(E entity);
        string Save(E entity);
        void Delete(string id);
        void Delete(IEnumerable<string> ids);
        IEnumerable<E> Query();
        E Query(string id);
        void Update(E entity);
    }
}
