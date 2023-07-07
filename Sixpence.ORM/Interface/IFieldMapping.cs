using Sixpence.Common.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM
{
    /// <summary>
    /// C# 数据类型与数据库数据类型映射
    /// </summary>
    [ServiceRegister]
    public interface IFieldMapping
    {
        Dictionary<Type, string> GetFieldMappings();
    }
}
