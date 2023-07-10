using Microsoft.Extensions.DependencyInjection;
using System;
namespace Sixpence.ORM
{
    /// <summary>
    /// ORM参数
    /// </summary>
    public class ServiceCollectionOptions
    {
        /// <summary>
        /// 实体类命名规范，默认帕斯卡命名（表名使用小写+下划线命名）
        /// </summary>
        public NameCase EntityClassNameCase { get; set; }

        /// <summary>
        /// 数据库配置
        /// </summary>
        public DbSetting? DbSetting { get; set; }
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

        /// <summary>
        /// 自动迁移
        /// </summary>
        public bool Migration { get; set; }
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

