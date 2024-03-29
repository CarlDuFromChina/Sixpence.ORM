﻿using Sixpence.ORM.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    /// <summary>
    /// 字段
    /// </summary>
    public class ColumnOptions
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 字段类型（数据库字段类型）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 字段长度
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        public bool? IsRequire { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
    }
}
