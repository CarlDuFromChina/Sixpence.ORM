using Microsoft.Extensions.DependencyInjection;
using Sixpence.ORM.Interface;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sixpence.ORM
{
    /// <summary>
    /// ORM参数
    /// </summary>
    public class ServiceCollectionOptions
    {
        /// <summary>
        /// 数据库配置
        /// </summary>
        public DbSetting? DbSetting { get; set; }

        /// <summary>
        /// 实体映射
        /// </summary>
        internal IDictionary<string, IDbEntityMap> EntityMaps { get; set; } = new Dictionary<string, IDbEntityMap>();
    }

    /// <summary>
    /// 数据库配置
    /// </summary>
    public class DbSetting
    {
        /// <summary>
        /// 数据库驱动
        /// </summary>
        public IDbDriver? Driver { get; set; }

        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public int? CommandTimeout { get; set; }
    }

    /// <summary>
    /// 类命名规范
    /// </summary>
    public enum NameCase
    {
        /// <summary>
        /// 帕斯卡命名（UserInfo）
        /// </summary>
        Pascal,
        /// <summary>
        /// 下划线命名（user_info）
        /// </summary>
        UnderScore
    }
}

