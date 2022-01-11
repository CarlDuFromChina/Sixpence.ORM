﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Entity
{
    /// <summary>
    /// 选项集
    /// </summary>
    public class SelectOption
    {
        public SelectOption() { }

        public SelectOption(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}