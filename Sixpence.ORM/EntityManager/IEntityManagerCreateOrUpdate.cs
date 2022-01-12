using Sixpence.Common;
using Sixpence.Common.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.EntityManager
{
    [ServiceRegister]
    public interface IEntityManagerCreateOrUpdate
    {
        void Execute(EntityManagerPluginContext context);
    }
}
