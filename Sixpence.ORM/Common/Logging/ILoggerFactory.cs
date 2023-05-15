using Sixpence.ORM.Common.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Common.Logging
{
    [ServiceRegister]
    public interface ILoggerFactory
    {
        ILogger GetLogger();
        ILogger GetLogger(string name);
    }
}
