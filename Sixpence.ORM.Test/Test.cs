using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sixpence.ORM.Test
{
    [Entity("test", "测试")]
    public class Test : BaseEntity
    {
        /// <summary>
        /// 实体id
        /// </summary>
        [Column("testid", "实体id", DataType.Varchar, 100)]
        public string testid
        {
            get
            {
                return this.Id;
            }
            set
            {
                this.Id = value;
            }
        }

        [Column("code", "编码", DataType.Varchar)]
        public string code { get; set; }
    }
}
