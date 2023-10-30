using Sixpence.ORM.Entity;
using Sixpence.ORM.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    internal class SormPrimaryColumn : ISormPrimaryColumn
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public PrimaryType PrimaryType { get; set; }
        public IDbPropertyMap DbPropertyMap { get; set; }
    }
}
