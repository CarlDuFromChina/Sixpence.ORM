using Sixpence.ORM.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    internal class SormColumn : ISormColumn
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public IDbPropertyMap DbPropertyMap { get; set; }
    }
}
