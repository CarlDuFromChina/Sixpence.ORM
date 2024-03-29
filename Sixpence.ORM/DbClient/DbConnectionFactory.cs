﻿using Npgsql;
using Sixpence.ORM.Driver;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.DbClient
{
    public class DbConnectionFactory
    {
        public static DbConnection GetDbConnection(DriverType driverType, string connectionString)
        {
            switch (driverType)
            {
                case DriverType.Postgresql:
                    return new NpgsqlConnection(connectionString);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
