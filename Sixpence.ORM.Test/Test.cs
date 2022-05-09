using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sixpence.ORM.Test
{
    [Entity("test", "测试")]
    [KeyAttributes("code不能重复", "code")]
    public class Test : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column("name", "名称", DataType.Varchar, 100)]
        public string name { get; set; }

        [Column("code", "编码", DataType.Varchar, 100)]
        public string code { get; set; }
    }

    [Entity("test_guid_number", "测试GIUD Number")]
    public class test_guid_number : BaseEntity
    {
        [PrimaryColumn(primaryType: PrimaryType.GUIDNumber)]
        public string id { get; set; }
    }
}
