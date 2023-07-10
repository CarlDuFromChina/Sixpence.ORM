using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    /// <summary>
    /// 数据库批量操作
    /// </summary>
    public interface IDbBatch
    {
        /// <summary>
        /// 批量复制数据进数据库
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="dataTable"></param>
        /// <param name="tableName"></param>
        void BulkCopy(IDbConnection conn, DataTable dataTable, string tableName);
    }
}
