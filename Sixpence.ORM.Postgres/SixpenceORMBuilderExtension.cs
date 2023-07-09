using System;
using Microsoft.AspNetCore.Builder;
using static Sixpence.ORM.SixpenceORMBuilderExtension;

namespace Sixpence.ORM.Postgres
{
	public static class SixpenceORMBuilderExtension
	{
        public static IApplicationBuilder UsePostgres(this IApplicationBuilder app, string connectionString, int commandTimeout)
        {
            if (Options != null)
            {
                Options.ConnectionString = connectionString;
                Options.CommandTimeout = commandTimeout;
                Options.Driver = new PostgresDriver();
            }

            return app;
        }
    }
}

