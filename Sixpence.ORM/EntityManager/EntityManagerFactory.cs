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
    public class EntityManagerFactory
    {
        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <returns></returns>
        public static IEntityManager GetManager()
        {
            var dbConfig = DBSourceConfig.Config;
            AssertUtil.IsNull(dbConfig, "未找到数据库配置");
            AssertUtil.IsNullOrEmpty(dbConfig.ConnectionString, "数据库连接字符串为空");
            AssertUtil.IsFalse(Enum.TryParse<DriverType>(dbConfig.DriverType, out var driverType), "数据库类型错误");
            return new EntityManager(dbConfig.ConnectionString, Enum.Parse<DriverType>(dbConfig.DriverType));
        }


        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="driverType">数据库驱动类型</param>
        /// <returns></returns>
        public static IEntityManager GetManager(string connectionString, DriverType driverType)
        {
            return new EntityManager(connectionString, driverType);
        }
    }
}
