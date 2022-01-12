using Sixpence.Common;
using Sixpence.Common.Utils;
using Sixpence.ORM.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.EntityManager
{
    /// <summary>
    /// 持久化存储工厂类
    /// </summary>
    public class EntityManagerFactory
    {
        /// <summary>
        /// 获取Manager
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEntityManager GetManager()
        {
            var dbConfig = DBSourceConfig.Config;
            AssertUtil.CheckBoolean<SpException>(dbConfig == null, "未找到数据库配置", "AD4BC4F2-CF8D-4A4E-ACE8-F68EBD89DE42");
            AssertUtil.CheckIsNullOrEmpty<SpException>(dbConfig.ConnectionString, "数据库连接字符串为空", "AD4BC4F2-CF8D-4A4E-ACE8-F68EBD89DE42");
            AssertUtil.CheckBoolean<SpException>(!Enum.TryParse<DriverType>(dbConfig.DriverType, out var driverType), "数据库类型错误", "AD4BC4F2-CF8D-4A4E-ACE8-F68EBD89DE42");
            return new EntityManager(dbConfig.ConnectionString, Enum.Parse<DriverType>(dbConfig.DriverType));
        }
    }
}
