using Sixpence.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.EntityManager
{
    public static class EntityManagerQueryExtension
    {
        /// <summary>
        /// 通过lambda表达式的方式执行数据库事务
        /// </summary>
        public static void ExecuteTransaction(this IEntityManager manager, Action func)
        {
            try
            {
                manager.DbClient.Open();
                manager.DbClient.BeginTransaction();

                func?.Invoke();

                manager.DbClient.CommitTransaction();
            }
            catch (Exception ex)
            {
                manager.DbClient.Rollback();
                throw ex;
            }
            finally
            {
                manager.DbClient.Close();
            }
        }

        /// <summary>
        /// 通过lambda表达式的方式执行数据库事务
        /// </summary>
        public static T ExecuteTransaction<T>(this IEntityManager manager, Func<T> func, string transId = null)
        {
            try
            {
                manager.DbClient.Open();
                manager.DbClient.BeginTransaction();

                var t = default(T);

                if (func != null)
                {
                    t = func();
                }

                manager.DbClient.CommitTransaction();

                return t;
            }
            catch (Exception ex)
            {
                manager.DbClient.Rollback();
                throw ex;
            }
            finally
            {
                manager.DbClient.Close();
            }

        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="manager"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static IEnumerable<T> Query<T>(this IEntityManager manager, string sql, IDictionary<string, object> paramList = null)
        {
            return manager.DbClient.Query<T>(sql, paramList);
        }

        /// <summary>
        /// 查询数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="manager"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static int QueryCount(this IEntityManager manager, string sql, IDictionary<string, object> paramList = null)
        {
            return ConvertUtil.ConToInt(manager.ExecuteScalar(sql, paramList));
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static DataTable Query(this IEntityManager manager, string sql, IDictionary<string, object> paramList = null)
        {
            return manager.DbClient.Query(sql, paramList);
        }
    }
}
