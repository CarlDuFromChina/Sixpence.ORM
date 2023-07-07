using System;
using Microsoft.AspNetCore.Builder;
using Sixpence.ORM.PostgreSql;
using static Sixpence.ORM.SixpenceORMBuilderExtension;

namespace Sixpence.ORM.Postgres
{
	public static class SixpenceORMBuilderExtension
	{
        public static IApplicationBuilder UsePostgres(this IApplicationBuilder app, string connectionString, int commandTimeout)
        {
            Options.ConnectionString = connectionString;
            Options.CommandTimeout = commandTimeout;
            Options.Driver = new PostgresDriver();
            return app;
        }
    }
}

