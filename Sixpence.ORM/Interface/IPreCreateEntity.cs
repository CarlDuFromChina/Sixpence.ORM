using Sixpence.Common.IoC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM
{
    /// <summary>
    /// 创建实体前
    /// </summary>
    [ServiceRegister]
    public interface IPreCreateEntity
    {
        void Execute(IEntityManager manager, IEntity entity);
    }
}
