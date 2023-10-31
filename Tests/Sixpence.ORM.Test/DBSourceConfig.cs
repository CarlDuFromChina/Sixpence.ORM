using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Test
{
    public class DBSourceConfig
    {
        public static readonly string ConnectionString = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123123;";
        public static readonly int CommandTimeOut = 30;
    }
}
