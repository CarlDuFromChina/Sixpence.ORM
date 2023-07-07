using System;
using System.Linq;
using Sixpence.Common.IoC;
using Sixpence.Common.Utils;

namespace Sixpence.ORM.EntityManager
{
    public class EntityManagerFactory
    {
        protected static DBSourceConfig DBSourceConfig = DBSourceConfig.Config;

        static EntityManagerFactory()
        {
            AssertUtil.IsNull(DBSourceConfig, "未找到数据库配置");
            AssertUtil.IsNullOrEmpty(DBSourceConfig.ConnectionString, "数据库连接字符串为空");
        }

        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <returns></returns>
        public static IEntityManager GetManager()
        {
            var driver = ServiceContainer.ResolveAll<IDbDriver>().FirstOrDefault(item => item.Name.ToUpper() == DBSourceConfig.DriverType.ToUpper());
            return new EntityManager(DBSourceConfig.ConnectionString, driver);
        }


        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="driverType">数据库驱动类型</param>
        /// <returns></returns>
        public static IEntityManager GetManager(string connectionString, string driverName)
        {
            var driver = ServiceContainer.ResolveAll<IDbDriver>().FirstOrDefault(item => item.Name.ToUpper() == driverName);
            return new EntityManager(connectionString, driver);
        }
    }
}
