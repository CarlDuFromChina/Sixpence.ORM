using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test
{
    [Entity("test", "测试")]
    public class Test : BaseEntity
    {
        [Column("code", "编码", DataType.Varchar)]
        public string code { get; set; }
    }
}
