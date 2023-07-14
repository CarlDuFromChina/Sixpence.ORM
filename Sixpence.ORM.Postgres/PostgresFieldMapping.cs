using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Postgres
{
    public class PostgresFieldMapping : IFieldMapping
    {
        public Dictionary<Type, string> GetFieldMappings()
        {
            return new Dictionary<Type, string>()
            {
                { typeof(short), "int2vector" },
                { typeof(short?), "int2vector" },
                { typeof(int), "int4" },
                { typeof(int?), "int4" },
                { typeof(long), "int8" },
                { typeof(long?), "int8" },
                { typeof(decimal), "numeric" },
                { typeof(decimal?), "numeric" },
                { typeof(double), "double precision" },
                { typeof(DateTime), "timestamp" },
                { typeof(DateTime?), "timestamp" },
                { typeof(bool), "bool" },
                { typeof(bool?), "bool" },
                { typeof(object), "jsonb" },
                { typeof(string), "text" }
            };
        }
    }
}
