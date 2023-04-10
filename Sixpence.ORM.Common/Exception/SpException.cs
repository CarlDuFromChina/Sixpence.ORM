using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sixpence.ORM.Common
{
    /// <summary>
    /// 自定义错误，方便系统异常统一捕获处理
    /// </summary>
    public class SpException : ApplicationException
    {
        public SpException() { }
        public SpException(string message)
        {
            this.message = message;
        }

        /// <summary>
        /// 错误提示
        /// </summary>
        private string message;
        public override string Message => message;
    }
}
