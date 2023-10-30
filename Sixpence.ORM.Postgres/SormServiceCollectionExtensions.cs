using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Sixpence.ORM.Postgres
{
	public static class SormServiceCollectionExtensions
	{
        /// <summary>
        /// Use Postgres
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="commandTimeout">执行超时时间</param>
        /// <returns></returns>
        public static ServiceCollectionOptions UsePostgres(this ServiceCollectionOptions options, string connectionString, int commandTimeout)
        {
            options.DbSetting = new DbSetting()
            {
                ConnectionString = connectionString,
                CommandTimeout = commandTimeout,
                Driver = new PostgresDriver()
            };

            return options;
        }
    }
}

