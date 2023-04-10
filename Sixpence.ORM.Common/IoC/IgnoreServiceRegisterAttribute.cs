using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Common.IoC
{
    /// <summary>
    /// IoC服务忽略注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IgnoreServiceRegisterAttribute : Attribute
    {
    }
}
