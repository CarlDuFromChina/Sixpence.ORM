using Sixpence.Common;
using Sixpence.Common.IoC;
using Sixpence.ORM.EntityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    [ServiceRegister]
    public interface IEntityManagerBeforeCreateOrUpdate
    {
        void Execute(EntityManagerPluginContext context);
    }
}
