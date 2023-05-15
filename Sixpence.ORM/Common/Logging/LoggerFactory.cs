using Sixpence.ORM.Common.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sixpence.ORM.Common.Logging
{
    public static class LoggerFactory
    {
        private static ILoggerFactory loggerFactory;
        static LoggerFactory()
        {
            loggerFactory = ServiceContainer.Resolve<ILoggerFactory>();
        }

        public static ILogger GetLogger()
        {
            return loggerFactory?.GetLogger() ?? new DefaultLogger();
        }

        public static ILogger GetLogger(string name)
        {
            return loggerFactory?.GetLogger(name) ?? new DefaultLogger();
        }
    }
}
