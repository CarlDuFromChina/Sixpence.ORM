using Sixpence.ORM.Common;
using Sixpence.ORM.Common.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.EntityManager
{
    [ServiceRegister]
    public interface IEntityManagerBeforeCreateOrUpdate
    {
        void Execute(EntityManagerPluginContext context);
    }
}
