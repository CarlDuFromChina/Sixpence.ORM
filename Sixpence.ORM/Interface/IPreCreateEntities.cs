using Sixpence.Common.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM
{
    /// <summary>
    /// 创建所有实体前
    /// </summary>
    [ServiceRegister]
    public interface IPreCreateEntities
    {
        void Execute(IEntityManager manager, IEnumerable<IEntity> entities);
    }
}
