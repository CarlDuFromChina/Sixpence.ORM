using System;
namespace Sixpence.ORM
{
    /// <summary>
    /// ORM参数
    /// </summary>
    public class BuilderOptions
    {
        /// <summary>
        /// 实体类命名规范，默认帕斯卡命名（表名使用小写+下划线命名）
        /// </summary>
        public NameCase EntityClassNameCase { get; set; }

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
        /// 日志
        /// </summary>
        public LogOptions LogOptions { get; set; } = new LogOptions();
    }

    /// <summary>
    /// 日志
    /// </summary>
    public class LogOptions
    {
        /// <summary>
        /// 调试日志
        /// </summary>
        public Action<string> LogDebug { get; set; } = text => Console.WriteLine(text);
        
        /// <summary>
        /// 错误日志
        /// </summary>
        public Action<string, Exception?>? LogError { get; set; } = (text, exception) => Console.WriteLine(text + exception?.Message + "\r\n" + exception?.StackTrace);
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

