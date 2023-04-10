using Sixpence.ORM.Common.IoC;
using Sixpence.ORM.Entity;
using Sixpence.ORM.EntityManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 创建实体后
    /// </summary>
    [ServiceRegister]
    public interface IPostCreateEntity
    {
        void Execute(IEntityManager manager, IEntity entity);
    }
}
