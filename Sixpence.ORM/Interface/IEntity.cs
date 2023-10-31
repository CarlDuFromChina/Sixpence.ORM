using Sixpence.ORM.Entity;
using Sixpence.ORM.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public interface IEntity
    {
        IDbEntityMap? EntityMap { get; }
        ISormPrimaryColumn PrimaryColumn { get; }
        IList<ISormColumn> Columns { get; }
    }
}
