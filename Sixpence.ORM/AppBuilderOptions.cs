using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    public class AppBuilderOptions
    {
        /// <summary>
        /// 是否自动迁移
        /// </summary>
        public bool MigrateDb { get; set; } = false;

        /// <summary>
        /// 是否日志 SQL 执行记录
        /// </summary>
        public bool EnableLogging = false;
    }
}
