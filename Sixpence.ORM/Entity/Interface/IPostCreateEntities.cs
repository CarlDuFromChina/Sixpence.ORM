using Sixpence.Common.IoC;
using Sixpence.ORM.Entity;
using Sixpence.ORM.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 创建所有实体后
    /// </summary>
    [ServiceRegister]
    public interface IPostCreateEntities
    {
        void Execute(IEntityManager manager, IEnumerable<IEntity> entities);
    }
}
