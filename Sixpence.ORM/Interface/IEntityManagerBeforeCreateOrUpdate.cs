using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public interface IEntityManagerBeforeCreateOrUpdate
    {
        void Execute(EntityManagerPluginContext context);
    }
}
