using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.ORM.EntityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
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
