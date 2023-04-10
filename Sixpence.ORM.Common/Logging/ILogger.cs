using Sixpence.ORM.Common.IoC;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Common.Logging
{
    [ServiceRegister]
    public interface ILogger
    {
       void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(string message, Exception ex);
        void Error(Exception ex);
    }
}
