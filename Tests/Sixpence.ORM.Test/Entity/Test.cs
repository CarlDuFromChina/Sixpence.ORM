using Newtonsoft.Json.Linq;
using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sixpence.ORM.Test
{
    [Table]
    public partial class Test : SormEntity
    {
        [PrimaryColumn]
        public string Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string Code { get; set; }

        [Column]
        public bool? IsSuper { get; set; }

        [Column]
        public string Tags { get; set; }
    }
}
