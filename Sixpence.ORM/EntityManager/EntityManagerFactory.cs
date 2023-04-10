using System;
using Sixpence.ORM.Common.Utils;
using Sixpence.ORM.Driver;

namespace Sixpence.ORM.EntityManager
{
    public class EntityManagerFactory
    {
        protected static DBSourceConfig DBSourceConfig = DBSourceConfig.Config;

        static EntityManagerFactory()
        {
            AssertUtil.IsNull(DBSourceConfig, "未找到数据库配置");
            AssertUtil.IsNullOrEmpty(DBSourceConfig.ConnectionString, "数据库连接字符串为空");
            AssertUtil.IsFalse(Enum.TryParse<DriverType>(DBSourceConfig.DriverType, out var driverType), "数据库类型错误");
        }

        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <returns></returns>
        public static IEntityManager GetManager()
        {
            return new EntityManager(DBSourceConfig.ConnectionString, Enum.Parse<DriverType>(DBSourceConfig.DriverType));
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
