using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test.Entity
{
    [Entity]
    public class UserInfo : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column("code", "编码", DataType.Varchar, 100, false)]
        public string code { get; set; }
    }
}
