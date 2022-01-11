﻿using Sixpence.ORM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 字段特性（映射数据库字段类型）
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        internal ColumnAttribute()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="logicalName">字段逻辑名</param>
        /// <param name="type">字段类型</param>
        /// <param name="length">字段长度</param>
        /// <param name="isRequire">是否必填</param>
        public ColumnAttribute(string name, string logicalName, DataType type, int length, bool isRequire = false, object defaultValue = null)
        {
            this.Column = new Column()
            {
                Name = name,
                LogicalName = logicalName,
                Type = type,
                Length = length,
                IsRequire = isRequire,
                DefaultValue = defaultValue
            };
        }

        public ColumnAttribute(string name, string logicalName, DataType type, bool isRequire = false, object defaultValue = null)
        {
            this.Column = new Column()
            {
                Name = name,
                LogicalName = logicalName,
                Type = type,
                IsRequire = isRequire,
                DefaultValue = defaultValue
            };
        }

        public Column Column { get; set; }
    }
}