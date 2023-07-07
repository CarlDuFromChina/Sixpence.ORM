using System;
using System.Linq;
using Sixpence.Common.IoC;
using Sixpence.Common.Utils;

namespace Sixpence.ORM.EntityManager
{
    public static class EntityManagerFactory
    {
        private static IDbDriver Driver;
        private static string ConnectionString;
        private static int CommandTimeout;

        static EntityManagerFactory()
        {
            Driver = SixpenceORMBuilderExtension.Options.Driver;
            ConnectionString = SixpenceORMBuilderExtension.Options.ConnectionString;
            CommandTimeout = SixpenceORMBuilderExtension.Options.CommandTimeout;
        }
        
        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <returns></returns>
        public static IEntityManager GetManager()
        {
            if (Driver == null)
            {
                throw new Exception("数据库驱动未初始化");
            }
            
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new Exception("数据库连接字符串未初始化");
            }

            return new EntityManager(ConnectionString, Driver, CommandTimeout);
        }

        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="driverType">数据库驱动类型</param>
        /// <returns>EntityManager</returns>
        public static IEntityManager GetManager(string connectionString, string driverName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("数据库连接字符串不能为空");
            }

            if (string.IsNullOrEmpty(driverName))
            {
                throw new Exception("数据库驱动名不能为空");
            }

            var driver = ServiceContainer.ResolveAll<IDbDriver>().FirstOrDefault(item => item.Name.ToUpper() == driverName);
            return new EntityManager(connectionString, driver, CommandTimeout);
        }

        /// <summary>
        /// 获取 EntityManager
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="driver">数据库驱动</param>
        /// <returns>EntityManager</returns>
        public static IEntityManager GetManager(string connectionString, IDbDriver driver)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("数据库连接字符串不能为空");
            }

            if (driver == null)
            {
                throw new Exception("数据库驱动不能为空");
            }
            
            return new EntityManager(connectionString, driver, CommandTimeout);
        }
    }
}
