using Sixpence.Common;
using Sixpence.Common.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.EntityManager
{
    /// <summary>
    /// 持久化插件
    /// </summary>
    [ServiceRegister]
    public interface IEntityManagerPlugin
    {
        /// <summary>
        /// 执行
        /// </summary>
        void Execute(EntityManagerPluginContext context);
    }
}
