﻿using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Test
{
    [Table]
    public class TestGuidNumber : BaseEntity
    {
        [PrimaryColumn(primaryType: PrimaryType.GUIDNumber)]
        public string id { get; set; }
    }
}
