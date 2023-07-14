using Sixpence.ORM.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Interface
{
    public interface IDbEntityMap
    {
        public string Table { get; set; }
        public string Schema { get; set; }
        public IList<IDbPropertyMap> Properties { get; set; }
    }
}
