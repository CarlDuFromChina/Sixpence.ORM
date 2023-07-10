using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Sixpence.ORM.Postgres
{
	public static class SormServiceCollectionExtensions
	{
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

