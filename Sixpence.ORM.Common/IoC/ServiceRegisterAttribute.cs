using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Common.IoC
{
    /// <summary>
    /// IoC服务注册
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class ServiceRegisterAttribute : Attribute
    {
    }
}
