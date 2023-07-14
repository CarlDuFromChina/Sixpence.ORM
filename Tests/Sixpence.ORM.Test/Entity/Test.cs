using Newtonsoft.Json.Linq;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sixpence.ORM.Test
{
    [Entity]
    [KeyAttributes("code不能重复", "code")]
    public partial class Test : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column]
        public string name { get; set; }

        [Column]
        public string code { get; set; }

        [Column]
        public bool? is_super { get; set; }

        [Column]
        public JToken tags { get; set; }
    }

    public partial class Test
    {
        public string is_super_name { get; }
    }
}
